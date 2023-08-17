using System;
using OpenTK.Mathematics;

namespace VoxelEngine
{
    public struct Vector3Int : IEquatable<Vector3Int>
    {
        public int X;
        public int Y;
        public int Z;

        public static Vector3Int Zero => new Vector3Int();

        public Vector3Int(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static implicit operator Vector3(Vector3Int vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static bool operator !=(Vector3Int a, Vector3Int b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        }

        public static bool operator ==(Vector3Int a, Vector3Int b)
        {
            return a.Equals(b);
        }

        public static Vector3Int operator +(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3Int operator -(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3Int operator *(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3Int operator /(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3Int operator +(Vector3Int vector, int value)
        {
            return new Vector3Int(vector.X + value, vector.Y + value, vector.Z + value);
        }

        public static Vector3Int operator -(Vector3Int vector, int value)
        {
            return new Vector3Int(vector.X - value, vector.Y - value, vector.Z - value);
        }

        public static Vector3Int operator *(Vector3Int vector, int value)
        {
            return new Vector3Int(vector.X * value, vector.Y * value, vector.Z * value);
        }

        public static Vector3Int operator /(Vector3Int vector, int value)
        {
            return new Vector3Int(vector.X / value, vector.Y / value, vector.Z / value);
        }

        public static bool operator <(Vector3Int a, Vector3Int b)
        {
            return a.X < b.X || a.Y < b.Y || a.Z < b.Z;
        }

        public static bool operator >(Vector3Int a, Vector3Int b)
        {
            return a.X > b.X || a.Y > b.Y || a.Z > b.Z;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3Int)
                return Equals((Vector3Int)obj);
            
            return false;
        }

        public bool Equals(Vector3Int other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + X;
                hash = hash * 23 + Y;
                hash = hash * 23 + Z;
                return hash;
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
}
