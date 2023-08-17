using System;
using OpenTK.Mathematics;
using VoxelEngine.Blocks;
using VoxelEngine.Chunks;

namespace VoxelEngine
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public float AspectRatio { private get; set; }

        public Vector3 Front => front;
        public Vector3 Up => up;
        public Vector3 Right => right;
        public World World;
        public ChunkColumn CurrentChunkColumn;
        public PositionedBlock AimedEmptyBlock;
        public PositionedBlock AimedSolidBlock;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }
        public float Yaw
        {
            get => MathHelper.DegreesToRadians(yaw);
            set
            {
                yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 45f);
                fov = MathHelper.DegreesToRadians(angle);
            }
        }

        private Vector3 front = -Vector3.UnitZ;
        private Vector3 up = Vector3.UnitY;
        private Vector3 right = Vector3.UnitX;

        private float pitch;
        private float yaw = -MathHelper.PiOver2;
        private float fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        public void FindAimedBlock()
        {
            for (float z = 0.1f; z < 8f; z += 0.1f)
            {
                //var target = this.Position + (new Vector3(Math.Abs(Front.X), Math.Abs(Front.Y), Math.Abs(Front.Z)) * z);
                var target = this.Position + (Front * z);
                var block = World.GetBlock(target);

                if (!block.Exists)
                    this.AimedEmptyBlock = new PositionedBlock(new Vector3Int((int)target.X, (int)target.Y, (int)target.Z), block);
                else
                {
                    this.AimedSolidBlock = new PositionedBlock(new Vector3Int((int)target.X, (int)target.Y, (int)target.Z), block);
                    return;
                }
            }
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + front, up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, AspectRatio, 0.01f, 500f);
        }

        public void LookAt(Vector3 position)
        {
            Vector3 direction = (position - Position).Normalized();
            Pitch = (float)Math.Asin(direction.Y);
            Yaw = (float)Math.Atan2(direction.X, direction.Z);
        }

        private void UpdateVectors()
        {
            front.X = (float)Math.Cos(pitch) * (float)Math.Cos(yaw);
            front.Y = (float)Math.Sin(pitch);
            front.Z = (float)Math.Cos(pitch) * (float)Math.Sin(yaw);

            front = Vector3.Normalize(front);
            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }
    }
}
