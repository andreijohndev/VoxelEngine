using OpenTK.Mathematics;

namespace VoxelEngine
{
    public struct BoundingBox
    {
        public Vector3 Min;
        public Vector3 Max;
        public const int CornerCount = 8;

        public BoundingBox(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }

        public ContainmentType Contains(BoundingBox box)
        {
            if (box.Max.X < Min.X ||
                box.Min.X > Max.X ||
                box.Max.Y < Min.Y ||
                box.Min.Y > Max.Y ||
                box.Max.Z < Min.Z ||
                box.Min.Z > Max.Z)
                return ContainmentType.Disjoint;

            if (box.Min.X >= Min.X &&
                box.Max.X <= Max.X &&
                box.Min.Y >= Min.Y &&
                box.Min.Z <= Max.Y &&
                box.Max.Z >= Max.Z)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            result = Contains(box);
        }
    }

    public enum ContainmentType
    {
        Disjoint,
        Contains,
        Intersects
    }
}
