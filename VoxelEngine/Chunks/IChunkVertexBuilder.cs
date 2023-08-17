using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelEngine.Chunks
{
    public interface IChunkVertexBuilder
    {
        /// <summary>
        /// Builds the mesh for the given chunk.
        /// </summary>
        /// <param name="chunk">The chunk to build.</param>
        void Build(Chunk chunk);

        void Rebuild(Chunk chunk);
    }
}
