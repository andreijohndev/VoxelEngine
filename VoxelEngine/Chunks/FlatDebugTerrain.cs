using System;
using VoxelEngine.Blocks;

namespace VoxelEngine.Chunks
{
    public class FlatDebugTerrain : TerrainGenerator
    {
        public FlatDebugTerrain(int seed)
            : base(seed)
        { }

        public FlatDebugTerrain()
            : base()
        { }

        protected override void GenerateChunkTerrain(Chunk chunk)
        {
            var stoneHeight = 5;
            var dirtHeight = stoneHeight + 3;
            var grassHeight = dirtHeight + 1;

            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    for (int y = World.ChunkSize - 1; y >= 0; y--)
                    {
                        if (chunk.WorldPosition.Y + y == grassHeight)
                        {
                            if (chunk.IsEmpty)
                                chunk.IsEmpty = false;

                            chunk.SetBlock(x, y, z, new Block(BlockType.Grass));
                        }
                        else if (chunk.WorldPosition.Y + y <= dirtHeight && chunk.WorldPosition.Y + y > stoneHeight)
                        {
                            if (chunk.IsEmpty)
                                chunk.IsEmpty = false;

                            chunk.SetBlock(x, y, z, new Block(BlockType.Dirt));
                        }
                        else if (chunk.WorldPosition.Y + y <= stoneHeight)
                        {
                            if (chunk.IsEmpty)
                                chunk.IsEmpty = false;

                            chunk.SetBlock(x, y, z, new Block(BlockType.Stone));
                        }
                    }
                }
            }
        }
    }
}