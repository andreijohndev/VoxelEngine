using System;
using System.Diagnostics;
using VoxelEngine.Blocks;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace VoxelEngine.Chunks
{
    public class GreedyChunkVertexBuilder : IChunkVertexBuilder
    {
        /// <summary>
        /// Builds the mesh for the given chunk.
        /// </summary>
        /// <param name="chunk">The chunk to build.</param>
        public void Build(Chunk chunk)
        {
            // If chunk is not awaiting build, skip it.
            if (chunk.CurrentChunkState != ChunkState.AwaitingBuild)
                return;

            if (chunk.IsEmpty)
            {
                chunk.CurrentChunkState = ChunkState.Ready;
                return;
            }

            // Set current chunk state to building.
            chunk.CurrentChunkState = ChunkState.Building;

            // Build the mesh.
            // TODO: Thread this?
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            BuildMesh(chunk);

            stopwatch.Stop();
            Console.WriteLine($"Build: {stopwatch.ElapsedMilliseconds}ms");

            // Chunk should generate buffers now.
            chunk.CurrentChunkState = ChunkState.AwaitingBufferGeneration;
        }

        public void Rebuild(Chunk chunk)
        {
            // If chunk is not awaiting build, skip it.
            if (chunk.CurrentChunkState != ChunkState.AwaitingRebuild)
                return;

            if (chunk.IsEmpty)
            {
                chunk.CurrentChunkState = ChunkState.Ready;
                return;
            }

            // Set current chunk state to building.
            chunk.CurrentChunkState = ChunkState.Building;

            // Build the mesh.
            // TODO: Thread this?
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            BuildMesh(chunk);

            stopwatch.Stop();
            Console.WriteLine($"Rebuild: {stopwatch.ElapsedMilliseconds}ms");

            // Chunk should generate buffers now.
            chunk.CurrentChunkState = ChunkState.AwaitingBufferRefresh;
        }

        private void BuildMesh(Chunk chunk)
        {
            chunk.Vertices.Clear();
            chunk.Indices.Clear();

            int i, j, k, l, w, h, u, v, n = 0;

            int[] x = new int[] { 0, 0, 0 };
            int[] q = new int[] { 0, 0, 0 };
            int[] du = new int[] { 0, 0, 0 };
            int[] dv = new int[] { 0, 0, 0 };

            BlockFaceDirection side = BlockFaceDirection.Top;

            Block[] mask = new Block[World.ChunkSizeSquared];
            Block blockCurrent;
            Block blockCompare;

            for (bool backFace = true, b = false; b != backFace; backFace = backFace && b, b = !b)
            {
                for (int d = 0; d < 3; d++)
                {
                    u = (d + 1) % 3;
                    v = (d + 2) % 3;

                    x[0] = 0;
                    x[1] = 0;
                    x[2] = 0;

                    q[0] = 0;
                    q[1] = 0;
                    q[2] = 0;
                    q[d] = 1;

                    if (d == 0)
                        side = backFace ? BlockFaceDirection.Left : BlockFaceDirection.Right;
                    else if (d == 1)
                        side = backFace ? BlockFaceDirection.Bottom : BlockFaceDirection.Top;
                    else if (d == 2)
                        side = backFace ? BlockFaceDirection.Back : BlockFaceDirection.Front;


                    for (x[d] = -1; x[d] < World.ChunkSize;)
                    {
                        n = 0;

                        for (x[v] = 0; x[v] < World.ChunkSize; x[v]++)
                        {
                            for (x[u] = 0; x[u] < World.ChunkSize; x[u]++)
                            {
                                blockCurrent = (x[d] >= 0) ? new Block((BlockType)chunk.Blocks[x[0] + x[1] * World.ChunkSize + x[2] * World.ChunkSizeSquared]) : Block.Empty;
                                blockCompare = (x[d] < World.ChunkSize - 1) ? new Block((BlockType)chunk.Blocks[(x[0] + q[0]) + (x[1] + q[1]) * World.ChunkSize + (x[2] + q[2]) * World.ChunkSizeSquared]) : Block.Empty;

                                mask[n++] = (blockCurrent.Exists && blockCompare.Exists)
                                            ? Block.Empty
                                            : backFace ? blockCompare : blockCurrent;
                            }
                        }

                        x[d]++;
                        n = 0;

                        for (j = 0; j < World.ChunkSize; j++)
                        {
                            for (i = 0; i < World.ChunkSize;)
                            {
                                if (mask[n].Exists)
                                {
                                    for (w = 1; i + w < World.ChunkSize && mask[n + w].Exists && mask[n + w] == mask[n]; w++) { }

                                    bool done = false;

                                    for (h = 1; j + h < World.ChunkSize; h++)
                                    {
                                        for (k = 0; k < w; k++)
                                        {
                                            if (!mask[n + k + h * World.ChunkSize].Exists || mask[n + k + h * World.ChunkSize] != mask[n])
                                            {
                                                done = true;
                                                break;
                                            }
                                        }

                                        if (done)
                                            break;
                                    }

                                    x[u] = i;
                                    x[v] = j;

                                    du[0] = 0;
                                    du[1] = 0;
                                    du[2] = 0;
                                    du[u] = w;

                                    dv[0] = 0;
                                    dv[1] = 0;
                                    dv[2] = 0;
                                    dv[v] = h;

                                    uint index = (uint)chunk.Vertices.Count;
                                    var color = Block.GetDefaultColor(mask[n].Type);
                                    var bottomLeft = chunk.WorldPosition + new Vector3(x[0], x[1], x[2]);
                                    var topLeft = chunk.WorldPosition + new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                    var topRight = chunk.WorldPosition + new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                                    var bottomRight = chunk.WorldPosition + new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);

                                    switch (side)
                                    {
                                        case BlockFaceDirection.Front:
                                        case BlockFaceDirection.Left:
                                        case BlockFaceDirection.Right:
                                        case BlockFaceDirection.Back:
                                            color.R *= 0.8f;
                                            color.G *= 0.8f;
                                            color.B *= 0.8f;
                                            break;
                                        case BlockFaceDirection.Bottom:
                                            color.R *= 0.5f;
                                            color.G *= 0.5f;
                                            color.B *= 0.5f;
                                            break;
                                    }

                                    switch (side)
                                    {
                                        case BlockFaceDirection.Back:
                                        case BlockFaceDirection.Left:
                                        case BlockFaceDirection.Bottom:
                                            chunk.Vertices.Add(new BlockVertex(bottomRight, color));
                                            chunk.Vertices.Add(new BlockVertex(topRight, color));
                                            chunk.Vertices.Add(new BlockVertex(topLeft, color));
                                            chunk.Vertices.Add(new BlockVertex(bottomLeft, color));
                                            break;
                                        case BlockFaceDirection.Front:
                                        case BlockFaceDirection.Right:
                                        case BlockFaceDirection.Top:
                                            chunk.Vertices.Add(new BlockVertex(bottomLeft, color));
                                            chunk.Vertices.Add(new BlockVertex(topLeft, color));
                                            chunk.Vertices.Add(new BlockVertex(topRight, color));
                                            chunk.Vertices.Add(new BlockVertex(bottomRight, color));
                                            break;
                                    }

                                    chunk.Indices.Add(index + 0);
                                    chunk.Indices.Add(index + 1);
                                    chunk.Indices.Add(index + 2);
                                    chunk.Indices.Add(index + 2);
                                    chunk.Indices.Add(index + 3);
                                    chunk.Indices.Add(index + 0);

                                    for (l = 0; l < h; ++l)
                                        for (k = 0; k < w; ++k)
                                            mask[n + k + l * World.ChunkSize] = Block.Empty;

                                    i += w;
                                    n += w;
                                }
                                else
                                {
                                    i++;
                                    n++;
                                }
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddQuad(Chunk chunk, Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, BlockFaceDirection side, Block block)
        {
            
        }
    }
}