using System;

namespace Facesketball
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ServerGame1 game = new ServerGame1())
            {
                game.Run();
            }
        }
    }
}

