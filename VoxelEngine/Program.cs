using System;

namespace VoxelEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            using (VoxelGame game = new VoxelGame(800, 600, "Voxel Engine"))
            {
                game.Run();
            }
        }
    }
}
