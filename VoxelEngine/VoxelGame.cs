using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelEngine.Blocks;

namespace VoxelEngine
{
    public class VoxelGame : GameWindow
    {
        private bool wireframe;
        private Camera camera;
        private World world;
        private bool firstMove = true;
        private Vector2 lastPos;

        private const float cameraSpeed = 10.5f;
        private const float sensitivity = 0.2f;

        public VoxelGame(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            CursorState = CursorState.Grabbed;
            camera = new Camera(Vector3.UnitY * 5f, Size.X / (float)Size.Y);
            world = new World(camera);

            GL.ClearColor(Color4.CornflowerBlue);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            camera.AspectRatio = Size.X / (float)Size.Y;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
                return;

            var keyboardState = KeyboardState;
            if (keyboardState.IsKeyPressed(Keys.Z))
                wireframe = !wireframe;

            if (keyboardState.IsKeyDown(Keys.Escape))
                Close();

            if (keyboardState.IsKeyDown(Keys.W))
                camera.Position += camera.Front * cameraSpeed * (float)e.Time;
            if (keyboardState.IsKeyDown(Keys.S))
                camera.Position -= camera.Front * cameraSpeed * (float)e.Time;
            if (keyboardState.IsKeyDown(Keys.D))
                camera.Position += camera.Right * cameraSpeed * (float)e.Time;
            if (keyboardState.IsKeyDown(Keys.A))
                camera.Position -= camera.Right * cameraSpeed * (float)e.Time;
            if (keyboardState.IsKeyDown(Keys.Space))
                camera.Position += camera.Up * cameraSpeed * (float)e.Time;
            if (keyboardState.IsKeyDown(Keys.LeftShift))
                camera.Position -= camera.Up * cameraSpeed * (float)e.Time;

            int positionX = (int)camera.Position.X;
            int positionY = (int)camera.Position.Y / World.ChunkSize;
            int positionZ = (int)camera.Position.Z;

            //if (camera.World != null)
            //    camera.FindAimedBlock();

            MouseState mouse = MouseState;

            if (firstMove)
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                var deltaX = mouse.X;
                var deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                camera.Yaw += deltaX * sensitivity;
                camera.Pitch -= deltaY * sensitivity;
            }

            if (mouse.IsButtonPressed(MouseButton.Right) && camera.AimedSolidBlock.Block.Exists)
            {
                world.SetBlock(camera.AimedEmptyBlock.Position.X, camera.AimedEmptyBlock.Position.Y, camera.AimedEmptyBlock.Position.Z, new Block(BlockType.Stone));
            }

            if (mouse.IsButtonPressed(MouseButton.Left) && camera.AimedSolidBlock.Block.Exists)
            {
                world.SetBlock(camera.AimedSolidBlock.Position.X, camera.AimedSolidBlock.Position.Y, camera.AimedSolidBlock.Position.Z, Block.Empty);
            }

            if (!wireframe)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            else
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            world.Update();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            world.Draw(wireframe);

            Context.SwapBuffers();
        }
        
        protected override void OnUnload()
        {
            base.OnUnload();

            world.DeleteBuffers();
        }
    }
}