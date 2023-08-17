using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelEngine.Blocks
{
    [Serializable]
    public struct BlockLight
    {
        public byte Sun;
        public byte R;
        public byte G;
        public byte B;

        public BlockLight(byte sun)
        {
            this.Sun = sun;
            this.R = 0;
            this.G = 0;
            this.B = 0;
        }
    }
}
