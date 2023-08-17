using System;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace VoxelEngine.Blocks
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct BlockVertex : IEquatable<BlockVertex>
    {
        public Vector3 Position { get; set; }
        public Color4 Color { get; set; }
        //public float SunLight;
        //public Vector3 LightColor;

        public BlockVertex(Vector3 position, Color4 color)//, float sunLight, Vector3 lightColor)
        {
            Position = position;
            Color = color;
            //this.SunLight = sunLight;
            //this.LightColor = lightColor;
        }

        public static readonly int SizeInBytes = Marshal.SizeOf(new BlockVertex());

        public bool Equals(BlockVertex other) => Position.Equals(other.Position) && Color.Equals(other.Color);// && SunLight == other.SunLight && LightColor.Equals(other.LightColor);
    }
}