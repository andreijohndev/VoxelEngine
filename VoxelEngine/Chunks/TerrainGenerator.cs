using System;
using System.Diagnostics;

namespace VoxelEngine.Chunks
{
    /// <summary>
    /// Base class for terrain generators.
    /// </summary>
    public class TerrainGenerator
    {
        /// <summary>
        /// Gets or sets the current seed.
        /// </summary>
        protected int Seed { get; private set; }

        /// <summary>
        /// Creates a new terrain generator with given seed.
        /// </summary>
        /// <param name="seed">The seed to use.</param>
        public TerrainGenerator(int seed)
        {
            this.Seed = seed;
        }

        /// <summary>
        /// Creates a new terrain generator with the current DateTime seconds as the default seed.
        /// </summary>
        public TerrainGenerator()
            : this(DateTime.Now.Second)
        { }

        public void Generate(Chunk chunk)
        {
            // If chunk is not awaiting generation, just skip it.
            if (chunk.CurrentChunkState != ChunkState.AwaitingGeneration)
                return;

            // Set current chunk state to generating.
            chunk.CurrentChunkState = ChunkState.Generating;

            // Generate teh chunk terrain.
            // TODO: Thread this?
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            this.GenerateChunkTerrain(chunk);
            stopwatch.Stop();
            Console.WriteLine($"Generation: {stopwatch.ElapsedMilliseconds}ms");

            // Chunk should be built now.
            chunk.CurrentChunkState = ChunkState.AwaitingBuild;
        }

        /// <summary>
        /// Generates the chunk terrain for given chunk.
        /// </summary>
        /// <param name="chunk">The chunk to generate terrain for.</param>
        protected virtual void GenerateChunkTerrain(Chunk chunk)
        { }
    }
}