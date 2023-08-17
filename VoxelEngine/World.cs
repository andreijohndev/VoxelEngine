using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using VoxelEngine.Blocks;
using VoxelEngine.Chunks;

namespace VoxelEngine
{
    public sealed class World
    {
        /*public const int ChunkSize = 32;
        public const int ChunkSizeSquared = 1024;
        public const int ChunkSizeCubed = 32768;*/
        public const int ChunkSize = 64;
        public const int ChunkSizeSquared = 4096;
        public const int ChunkSizeCubed = 262144;
        public const byte MaxLightValue = 16;
        public const byte CacheRange = 3;
        public const byte ViewRange = 2;

        /// <summary>
        /// Bounding box for view range.
        /// </summary>
        public BoundingBox ViewRangeBoundingBox { get; set; }

        /// <summary>
        /// Bounding box for cache range.
        /// </summary>
        public BoundingBox CacheRangeBoundingBox { get; set; }

        /// <summary>
        /// Chunks drawn statistics.
        /// </summary>
        public int ChunksDrawn { get; private set; }

        public bool UpdateThreadStarted { get; private set; }

        public Dictionary<ChunkState, int> StateStatistics { get; private set; }

        private ConcurrentDictionary<Vector2Int, ChunkColumn> chunkColumns;

        private IChunkVertexBuilder _chunkVertexBuilder;
        private TerrainGenerator _terrainGenerator;
        private Camera _camera;
        private Queue<Chunk> _chunksToGenerateBuffers;

        public World(Camera camera)
        {
            if (ViewRange > CacheRange)
                throw new ChunkRangeException();

            UpdateThreadStarted = false;
            _chunkVertexBuilder = new GreedyChunkVertexBuilder();
            _terrainGenerator = new FlatDebugTerrain();
            _camera = camera;
            _camera.World = this;

            _chunksToGenerateBuffers = new Queue<Chunk>();

            chunkColumns = new ConcurrentDictionary<Vector2Int, ChunkColumn>();

            StateStatistics = new Dictionary<ChunkState, int>
            {
                { ChunkState.AwaitingGeneration, 0 },
                { ChunkState.Generating, 0 },
                { ChunkState.AwaitingLighting, 0 },
                { ChunkState.Lighting, 0 },
                { ChunkState.AwaitingBuild, 0 },
                { ChunkState.Building, 0 },
                { ChunkState.AwaitingBufferGeneration, 0 },
                { ChunkState.GeneratingBuffers, 0 },
                { ChunkState.Ready, 0 },
                { ChunkState.AwaitingRelighting, 0 },
                { ChunkState.AwaitingRebuild, 0 },
                { ChunkState.AwaitingRemoval, 0 }
            };

            for (int z = -CacheRange; z <= CacheRange; z++)
            {
                for (int x = -CacheRange; x <= CacheRange; x++)
                {
                    var relativePosition = new Vector2Int(x, z);
                    var chunkColumn = new ChunkColumn(relativePosition);
                    chunkColumns[relativePosition] = chunkColumn;

                    if (chunkColumn.RelativePosition == Vector2.Zero)
                        camera.CurrentChunkColumn = chunkColumn;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block GetBlock(int x, int y, int z)
        {
            var relativePosition = new Vector2Int(x / ChunkSize, z / ChunkSize);
            var position = new Vector3(x % ChunkSize, y, z % ChunkSize);

            return chunkColumns[relativePosition].GetBlock(position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block GetBlock(Vector3 position)
        {
            var relativePosition = new Vector2Int((int)position.X / ChunkSize, (int)position.Z / ChunkSize);
            var relativeBlockPosition = new Vector3((int)position.X % ChunkSize, (int)position.Y, (int)position.Z % ChunkSize);

            return chunkColumns[relativePosition].GetBlock(relativeBlockPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block GetBlock(Vector3Int position)
        {
            var relativePosition = new Vector2Int(position.X / ChunkSize, position.Z / ChunkSize);
            var relativeBlockPosition = new Vector3(position.X % ChunkSize, position.Y, position.Z % ChunkSize);

            return chunkColumns[relativePosition].GetBlock(relativeBlockPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(int x, int y, int z, Block block)
        {
            var relativePosition = new Vector2Int(x / ChunkSize, z / ChunkSize);
            var position = new Vector3(x % ChunkSize, y, z % ChunkSize);

            chunkColumns[relativePosition].SetBlock(position, block);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(Vector3 position, Block block)
        {
            var relativePosition = new Vector2Int((int)position.X / ChunkSize, (int)position.Z / ChunkSize);
            var relativeBlockPosition = new Vector3((int)position.X % ChunkSize, (int)position.Y, (int)position.Z % ChunkSize);

            chunkColumns[relativePosition].SetBlock(relativeBlockPosition, block);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(Vector3Int position, Block block)
        {
            var relativePosition = new Vector2Int(position.X / ChunkSize, position.Z / ChunkSize);
            var relativeBlockPosition = new Vector3(position.X % ChunkSize, position.Y, position.Z % ChunkSize);

            chunkColumns[relativePosition].SetBlock(relativeBlockPosition, block);
        }

        /// <summary>
        /// Returns a boolean stating if chunk is currently in cache range.
        /// </summary>
        /// <param name="chunk">Chunk to check.</param>
        /// <returns></returns>
        public bool IsChunkInCacheRange(Chunk chunk)
        {
            return CacheRangeBoundingBox.Contains(chunk.ChunkBoundingBox) == ContainmentType.Intersects;
        }

        /// <summary>
        /// Returns a boolean stating if chunk is currently in view range. 
        /// </summary>
        /// <param name="chunk">Chunk to check.</param>
        /// <returns></returns>
        public bool IsChunkInViewRange(Chunk chunk)
        {
            return ViewRangeBoundingBox.Contains(chunk.ChunkBoundingBox) == ContainmentType.Intersects;
        }

        public void Update()
        {
            ViewRangeBoundingBox = new BoundingBox(
                new Vector3(_camera.CurrentChunkColumn.WorldPosition.X - (ViewRange * ChunkSize), 0,
                            _camera.CurrentChunkColumn.WorldPosition.Y - (ViewRange * ChunkSize)),
                new Vector3(_camera.CurrentChunkColumn.WorldPosition.X + (ViewRange * ChunkSize), ChunkColumn.MaxChunks * ChunkSize,
                            _camera.CurrentChunkColumn.WorldPosition.Y + (ViewRange * ChunkSize))
                );

            CacheRangeBoundingBox = new BoundingBox(
                new Vector3(_camera.CurrentChunkColumn.WorldPosition.X - (CacheRange * ChunkSize), 0,
                            _camera.CurrentChunkColumn.WorldPosition.Y - (CacheRange * ChunkSize)),
                new Vector3(_camera.CurrentChunkColumn.WorldPosition.X + (CacheRange * ChunkSize), ChunkColumn.MaxChunks * ChunkSize,
                            _camera.CurrentChunkColumn.WorldPosition.Y + (CacheRange * ChunkSize))
                );

            while (_chunksToGenerateBuffers.Count > 0)
            {
                _chunksToGenerateBuffers.Dequeue().GenerateBuffers();
            }

            if (UpdateThreadStarted)
                return;

            var updateThread = new Thread(new ThreadStart(UpdateThread)) { IsBackground = true };
            updateThread.Start();

            UpdateThreadStarted = true;
        }

        public void Draw(bool wireframe)
        {
            foreach (var chunkColumn in chunkColumns.Values)
            {
                foreach (var chunk in chunkColumn.Chunks)
                {
                    if (chunk.IsEmpty)
                        continue;

                    if (chunk.Vertices.Count == 0 || chunk.Indices.Count == 0)
                        continue;

                    if (!IsChunkInViewRange(chunk))
                        continue;

                    if (chunk.CurrentChunkState != ChunkState.Ready)
                        continue;

                    chunk.Draw(_camera, wireframe);
                }
            }
        }

        public void DeleteBuffers()
        {
            foreach(var chunkColumn in chunkColumns.Values)
            {
                foreach(var chunk in chunkColumn.Chunks)
                {
                    chunk.DeleteBuffers();
                }
            }
        }

        private void UpdateThread()
        {
            while (true)
            {
                if (_camera.CurrentChunkColumn == null)
                    continue;

                Process();
            }
        }

        private void Process()
        {
            foreach (var chunkColumn in chunkColumns.Values)
            {
                foreach (var chunk in chunkColumn.Chunks)
                {
                    if (IsChunkInViewRange(chunk))
                        ProcessChunkInViewRange(chunk);
                    else
                    {
                        if (IsChunkInCacheRange(chunk))
                            ProcessChunkInCacheRange(chunk);
                        else
                        {
                            chunk.CurrentChunkState = ChunkState.AwaitingRemoval;
                            chunkColumns.Remove(chunkColumn.RelativePosition, out _);
                            chunk.Dispose();
                        }
                    }
                }
            }

            //MakeNewChunks();
        }

        private void MakeNewChunks()
        {
            _camera.CurrentChunkColumn = chunkColumns[new Vector2Int((int)Math.Floor(_camera.Position.X / ChunkSize), (int)Math.Floor(_camera.Position.Z / ChunkSize))];

            if (_camera.CurrentChunkColumn == null)
                return;

            for (int z = -CacheRange; z <= CacheRange; z++)
            {
                for (int x = -CacheRange; x <= CacheRange; x++)
                {
                    var relativePosition = new Vector2Int(_camera.CurrentChunkColumn.RelativePosition.X + x, _camera.CurrentChunkColumn.RelativePosition.Y + z);

                    if (chunkColumns.ContainsKey(relativePosition))
                        continue;

                    var chunkColumn = new ChunkColumn(relativePosition);
                    chunkColumns[relativePosition] = chunkColumn;
                }
            }
        }

        private void ProcessChunkInCacheRange(Chunk chunk)
        {
            if (chunk.CurrentChunkState != ChunkState.AwaitingGeneration && chunk.CurrentChunkState != ChunkState.AwaitingLighting)
                return;

            switch(chunk.CurrentChunkState)
            {
                case ChunkState.AwaitingGeneration:
                    _terrainGenerator.Generate(chunk);
                    break;
                default:
                    break;
            }
        }

        private void ProcessChunkInViewRange(Chunk chunk)
        {
            if (chunk.CurrentChunkState == ChunkState.Ready || chunk.CurrentChunkState == ChunkState.AwaitingRemoval)
                return;

            switch(chunk.CurrentChunkState)
            {
                case ChunkState.AwaitingGeneration:
                    _terrainGenerator.Generate(chunk);
                    break;
                case ChunkState.AwaitingBuild:
                    _chunkVertexBuilder.Build(chunk);
                    break;
                case ChunkState.AwaitingRebuild:
                    _chunkVertexBuilder.Rebuild(chunk);
                    break;
                case ChunkState.AwaitingBufferGeneration:
                    _chunksToGenerateBuffers.Enqueue(chunk);
                    break;
                default:
                    break;
            }
        }

        public class ChunkRangeException : Exception
        {
            public ChunkRangeException() : base("Cache Range cannot be more than View Range.")
            { }
        }
    }
}
