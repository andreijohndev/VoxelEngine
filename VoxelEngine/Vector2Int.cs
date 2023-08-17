using System;
using OpenTK.Mathematics;

namespace VoxelEngine
{
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int X;
        public int Y;

        public static Vector2Int Zero => new Vector2Int();

        public Vector2Int(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static implicit operator Vector2(Vector2Int vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static bool operator !=(Vector2Int a, Vector2Int b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static bool operator ==(Vector2Int a, Vector2Int b)
        {
            return a.Equals(b);
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2Int operator *(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2Int operator /(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X / b.X, a.Y / b.Y);
        }

        public static Vector2Int operator +(Vector2Int a, int value)
        {
            return new Vector2Int(a.X + value, a.Y + value);
        }

        public static Vector2Int operator -(Vector2Int a, int value)
        {
            return new Vector2Int(a.X - value, a.Y - value);
        }

        public static Vector2Int operator *(Vector2Int vector, int value)
        {
            return new Vector2Int(vector.X * value, vector.Y * value);
        }

        public static Vector2Int operator /(Vector2Int vector, int value)
        {
            return new Vector2Int(vector.X / value, vector.Y / value);
        }

        public static bool operator <(Vector2Int a, Vector2Int b)
        {
            return a.X < b.X || a.Y < b.Y;
        }

        public static bool operator >(Vector2Int a, Vector2Int b)
        {
            return a.X > b.X || a.Y > b.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2Int)
                return Equals((Vector2Int)obj);

            return false;
        }

        public bool Equals(Vector2Int other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + X;
                hash = hash * 23 + Y;
                return hash;
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
