using OpenTK.Mathematics;

namespace VoxelEngine.Blocks
{
    public struct Block
    {
        /// <summary>
        /// The block type.
        /// </summary>
        public BlockType Type;

        private readonly static Color4 StoneColor = new Color4(130, 133, 143, 255);
        private readonly static Color4 DirtColor = new Color4(150, 103, 56, 255);
        private readonly static Color4 GrassColor = new Color4(62, 129, 73, 255);

        /// <summary>
        /// Returns true if a block exists, false it it's air.
        /// </summary>
        public bool Exists
        {
            get { return Type != 0; }
        }

        public Block(BlockType type)
        {
            Type = type;
        }

        public static readonly Block Empty = new Block(BlockType.None);

        public static Color4 GetDefaultColor(BlockType type)
        {
            switch (type)
            {
                case BlockType.None:
                    return Color4.Transparent;
                case BlockType.Stone:
                    return StoneColor;
                case BlockType.Dirt:
                    return DirtColor;
                case BlockType.Grass:
                    return GrassColor;
                default:
                    return Color4.Magenta;
            }
        }

        /// <summary>
        /// Returns a string that represents block's type.
        /// </summary>      
        /// <returns><see cref="string"/></returns>
        /// <remarks>Used by the Visual Studio debugger.</remarks>
        public override string ToString()
        {
            return Type.ToString();
        }

        public static bool operator ==(Block a, Block b)
        {
            return a.Type == b.Type;
        }

        public static bool operator !=(Block a, Block b)
        {
            return !(a == b);
        }
    }

    public enum BlockType : byte
    {
        None,
        Stone,
        Dirt,
        Grass
    }

    public enum BlockFaceDirection : byte
    {
        Top,
        Bottom,
        Front,
        Back,
        Left,
        Right
    }
}