using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelEngine.Chunks
{
    /// <summary>
    /// Chunk lighting processor.
    /// </summary>
    public static class Lighting
    {
        public static void Process(Chunk chunk)
        {
            // If chunk is not awaiting lighting or relighting, just skip it.
            if (chunk.CurrentChunkState != ChunkState.AwaitingLighting && chunk.CurrentChunkState != ChunkState.AwaitingRelighting)
                return;

            chunk.CurrentChunkState = ChunkState.Lighting;

            SetInitialLighting(chunk);
            FluidFillSunLight(chunk);
        }

        /// <summary>
        /// Clears all lighting and then applies sunlight.
        /// </summary>
        /// <param name="chunk">Chunk to light.</param>
        private static void SetInitialLighting(Chunk chunk)
        {
            byte sunValue = World.MaxLightValue;

            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    bool inShade = false;

                    for (int y = (World.ChunkSize - 1); y > 0; y--)
                    {
                        var index = x + y * World.ChunkSize + z * World.ChunkSizeSquared;
                        var block = chunk.Blocks[index];
                        var blockLight = chunk.BlockLights[index];

                        //if (!inShade && block.Exists)
                            inShade = true;

                        blockLight.Sun = inShade ? (byte)0 : sunValue;
                        blockLight.R = 0;
                        blockLight.G = 0;
                        blockLight.B = 0;
                    }
                }
            }
        }

        private static void FluidFillSunLight(Chunk chunk)
        {
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    for (int y = (World.ChunkSize - 1); y > 0; y--)
                    {
                        var index = x + y * World.ChunkSize + z * World.ChunkSizeSquared;
                        var block = chunk.Blocks[index];

                        // Solid blocks can't propagate light.
                        //if (block.Exists)
                            continue;

                        var blockLight = chunk.BlockLights[index].Sun;

                        // If block's light value is too low, just skip it.
                        if (blockLight <= 1)
                            continue;

                        var propagatedLight = (byte)((blockLight * 9) / 10);
                        var east = chunk.GetBlockIndex(x + 1, y, z);
                        var west = chunk.GetBlockIndex(x - 1, y, z);
                        var north = chunk.GetBlockIndex(x, y, z + 1);
                        var south = chunk.GetBlockIndex(x, y, z - 1);
                        var bottom = chunk.GetBlockIndex(x, y - 1, z);

                        PropagateSunLight(chunk, new Vector3Int(x + 1, y, z), east, propagatedLight);
                        PropagateSunLight(chunk, new Vector3Int(x - 1, y, z), west, propagatedLight);
                        PropagateSunLight(chunk, new Vector3Int(x, y, z + 1), north, propagatedLight);
                        PropagateSunLight(chunk, new Vector3Int(x, y, z - 1), south, propagatedLight);

                        // DO NOT repropagate to upper block, it will cause recursion without an exit condition.
                        PropagateSunLight(chunk, new Vector3Int(x, y - 1, z), bottom, propagatedLight);
                    }
                }
            }
        }

        private static void PropagateSunLight(Chunk chunk, Vector3Int position, int blockIndex, byte incomingLight)
        {
            var blockLight = chunk.BlockLights[blockIndex];

            blockIndex = blockIndex % chunk.Blocks.Length;
            if (blockIndex < 0)
                blockIndex += chunk.Blocks.Length;

            if (incomingLight <= 1)
                return;

            //if (chunk.Blocks[blockIndex].Exists)
                return;

            if (incomingLight <= blockLight.Sun)
                return;

            chunk.BlockLights[blockIndex].Sun = incomingLight;

            var propagatedLight = (byte)((incomingLight * 9) / 10);
            /*var east = chunk.GetBlockIndex(x + 1, y, z);
            var west = chunk.GetBlockIndex(x - 1, y, z);
            var north = chunk.GetBlockIndex(x, y, z + 1);
            var south = chunk.GetBlockIndex(x, y, z - 1);
            var bottom = chunk.GetBlockIndex(x, y - 1, z);

            PropagateSunLight(chunk, east, propagatedLight);
            PropagateSunLight(chunk, west, propagatedLight);
            PropagateSunLight(chunk, north, propagatedLight);
            PropagateSunLight(chunk, south, propagatedLight);

            // DO NOT repropagate to upper block, it will cause recursion without an exit condition.
            PropagateSunLight(chunk, bottom, propagatedLight);*/
        }
    }
}