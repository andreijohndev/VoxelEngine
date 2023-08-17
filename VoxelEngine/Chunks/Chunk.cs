using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelEngine.Blocks;

namespace VoxelEngine.Chunks
{
    public sealed class Chunk : IDisposable
    {
        /// <summary>
        /// The chunk's world position.
        /// </summary>
        public Vector3Int WorldPosition { get; set; }

        /// <summary>
        /// The chunk's relative position.
        /// </summary>
        public Vector3Int RelativePosition { get; set; }

        /// <summary>
        /// The chunk column the chunk is in.
        /// </summary>
        public ChunkColumn ChunkColumn { get; private set; }

        /// <summary>
        /// The current chunk state.
        /// </summary>
        public ChunkState CurrentChunkState { get; set; }

        /// <summary>
        /// The chunk bounding box.
        /// </summary>
        public BoundingBox ChunkBoundingBox { get; set; }

        /// <summary>
        /// The blocks inside the chunk.
        /// </summary>
        public byte[] Blocks;
        public BlockLight[] BlockLights;

        /// <summary>
        /// Is chunk full of air?
        /// </summary>
        public bool IsEmpty { get; set; }

        public List<BlockVertex> Vertices { get; private set; }

        public List<uint> Indices { get; private set; }

        private int vao;
        private int vbo;
        private int ebo;
        private Shader shader;

        public Chunk(ChunkColumn chunkColumn, Vector3Int relativePosition)
        {
            // Set initial state to awaiting generation.
            CurrentChunkState = ChunkState.AwaitingGeneration;

            // Set relative position, and calculate real world position.
            RelativePosition = relativePosition;
            WorldPosition = relativePosition * World.ChunkSize;
            ChunkColumn = chunkColumn;

            ChunkBoundingBox = new BoundingBox(WorldPosition, WorldPosition + World.ChunkSize);

            IsEmpty = true;
            
            // Create block array.
            Blocks = new byte[World.ChunkSizeCubed];
            BlockLights = new BlockLight[World.ChunkSizeCubed];

            // Create vertex and index lists.
            Vertices = new List<BlockVertex>();
            Indices = new List<uint>();
        }

        public void Draw(Camera camera, bool wireframe)
        {
            GL.BindVertexArray(vao);

            shader.Use();

            shader.SetInt("wireframe", wireframe ? 1 : 0);
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            var model = Matrix4.Identity;
            shader.SetMatrix4("model", model);

            GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);
        }

        public void GenerateBuffers()
        {
            if (CurrentChunkState != ChunkState.AwaitingBufferGeneration)
                return;

            CurrentChunkState = ChunkState.GeneratingBuffers;

            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Count * BlockVertex.SizeInBytes, Vertices.ToArray(), BufferUsageHint.DynamicDraw);

            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Count * sizeof(uint), Indices.ToArray(), BufferUsageHint.DynamicDraw);

            shader = Shader.LoadShaderFromResource("VoxelEngine.Shaders.BlockShader.vert", "VoxelEngine.Shaders.BlockShader.frag");
            shader.Use();

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            var pos = shader.GetAttribLocation("position");
            GL.VertexAttribPointer(pos, 3, VertexAttribPointerType.Float, false, BlockVertex.SizeInBytes, 0);
            GL.EnableVertexArrayAttrib(vao, pos);

            var color = shader.GetAttribLocation("vertexColor");
            GL.VertexAttribPointer(color, 3, VertexAttribPointerType.Float, false, BlockVertex.SizeInBytes, Vector3.SizeInBytes);
            GL.EnableVertexArrayAttrib(vao, color);

            CurrentChunkState = ChunkState.Ready;
        }

        public void RefreshBuffers()
        {
            if (CurrentChunkState != ChunkState.AwaitingBufferRefresh)
                return;

            CurrentChunkState = ChunkState.RefreshingBuffers;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Count * BlockVertex.SizeInBytes, Vertices.ToArray(), BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Count * sizeof(uint), Indices.ToArray(), BufferUsageHint.DynamicDraw);

            CurrentChunkState = ChunkState.Ready;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeleteBuffers()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(ebo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block GetBlock(int x, int y, int z)
        {
            var index = x + y * World.ChunkSize + z * World.ChunkSizeSquared;

            return new Block((BlockType)Blocks[index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block GetBlock(Vector3 position)
        {
            return GetBlock((int)position.X, (int)position.Y, (int)position.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block GetBlock(Vector3Int position)
        {
            return GetBlock(position.X, position.Y, position.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(int x, int y, int z, Block block)
        {
            var index = x + y * World.ChunkSize + z * World.ChunkSizeSquared;

            if (block.Exists)
                IsEmpty = false;

            Blocks[index] = (byte)block.Type;

            CurrentChunkState = ChunkState.AwaitingRebuild;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlockIndex(int x, int y, int z)
        {
            return x + y * World.ChunkSize + z * World.ChunkSizeSquared;
        }

        #region Deconstructor

        /// <summary>
        /// Is the region disposed already?
        /// </summary>
        public bool Disposed = false;

        private void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                Indices.Clear();
                Indices = null;
                Vertices.Clear();
                Vertices = null;

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.DeleteBuffer(vbo);
                GL.DeleteBuffer(vao);
            }

            Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Chunk() { Dispose(false); }

        #endregion
    }

    /// <summary>
    /// The chunk state.
    /// </summary>
    public enum ChunkState : byte
    {
        /// <summary>
        /// Chunk awaits initial generation.
        /// </summary>
        AwaitingGeneration,

        /// <summary>
        /// Chunk is being generated.
        /// </summary>
        Generating,

        /// <summary>
        /// Chunk awaits initial lighting.
        /// </summary>
        AwaitingLighting,

        /// <summary>
        /// Chunk is being lightened.
        /// </summary>
        Lighting,

        /// <summary>
        /// Chunk awaits initial build.
        /// </summary>
        AwaitingBuild,

        /// <summary>
        /// Chunk is being built.
        /// </summary>
        Building,

        /// <summary>
        /// Chunk awaits buffer generation.
        /// </summary>
        AwaitingBufferGeneration,

        /// <summary>
        /// Chunk is generating buffers.
        /// </summary>
        GeneratingBuffers,

        /// <summary>
        /// Chunk is ready to be rendered.
        /// </summary>
        Ready,

        /// <summary>
        /// Chunk awaits a buffer refresh.
        /// </summary>
        AwaitingBufferRefresh,

        /// <summary>
        /// Chunk is refreshing buffers.
        /// </summary>
        RefreshingBuffers,

        /// <summary>
        /// Chunk awaits a rebuild.
        /// </summary>
        AwaitingRebuild,

        /// <summary>
        /// Chunk awaits relighting.
        /// </summary>
        AwaitingRelighting,

        /// <summary>
        /// Chunk awaits for removal.
        /// </summary>
        AwaitingRemoval
    }
}
