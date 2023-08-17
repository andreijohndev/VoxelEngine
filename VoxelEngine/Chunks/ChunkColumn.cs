using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using VoxelEngine.Blocks;

namespace VoxelEngine.Chunks
{
    public class ChunkColumn
    {
        public const int MaxChunks = 8;

        public Vector2Int RelativePosition { get; private set; }
        public Vector2Int WorldPosition { get; private set; }
        public Chunk[] Chunks { get; private set; }

        public ChunkColumn(Vector2Int relativePosition)
        {
            RelativePosition = relativePosition;
            WorldPosition = relativePosition * World.ChunkSize;

            Chunks = new Chunk[MaxChunks];

            for (int y = 0; y < Chunks.Length; y++)
            {
                Chunks[y] = new Chunk(this, new Vector3Int(relativePosition.X, y, relativePosition.Y));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block GetBlock(int x, int y, int z)
        {
            int yChunkIndex = y / World.ChunkSize;
            int yRelativePosition = y % World.ChunkSize;

            return Chunks[yChunkIndex].GetBlock(x, yRelativePosition, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block GetBlock(Vector3 position)
        {
            int yChunkIndex = (int)position.Y / World.ChunkSize;
            int yRelativePosition = (int)position.Y % World.ChunkSize;

            return Chunks[yChunkIndex].GetBlock((int)position.X, yRelativePosition, (int)position.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block GetBlock(Vector3Int position)
        {
            int yChunkIndex = position.Y / World.ChunkSize;
            int yRelativePosition = position.Y % World.ChunkSize;

            return Chunks[yChunkIndex].GetBlock(position.X, yRelativePosition, position.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(int x, int y, int z, Block block)
        {
            int yChunkIndex = y / World.ChunkSize;
            int yRelativePosition = y % World.ChunkSize;

            Chunks[yChunkIndex].SetBlock(x, yRelativePosition, z, block);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(Vector3 position, Block block)
        {
            int yChunkIndex = (int)position.Y / World.ChunkSize;
            int yRelativePosition = (int)position.Y % World.ChunkSize;

            Chunks[yChunkIndex].SetBlock((int)position.X, yRelativePosition, (int)position.Z, block);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(Vector3Int position, Block block)
        {
            int yChunkIndex = position.Y / World.ChunkSize;
            int yRelativePosition = position.Y % World.ChunkSize;

            Chunks[yChunkIndex].SetBlock(position.X, yRelativePosition, position.Z, block);
        }
    }
}
