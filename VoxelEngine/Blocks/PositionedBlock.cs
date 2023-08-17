using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelEngine.Blocks
{
    public struct PositionedBlock
    {
        public Vector3Int Position { get; private set; }
        public Block Block { get; private set; }

        public PositionedBlock(Vector3Int position, Block block)
        {
            Position = position;
            Block = block;
        }
    }
}
