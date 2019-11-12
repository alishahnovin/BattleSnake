using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BattleSnake.Library;

namespace BattleSnake
{
    class Program
    {
        private static void Main(string[] args)
        {
            GameEngine.Start
            (
                InitializeAndLoadLocalDLLs()
            );
        }

        //I wouldn't recommend loading any old remote DLL unless you're sure of the source...
        private static Type[] InitializeAndLoadLocalDLLs()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.CursorVisible = false;
            Console.Title = "Battle Snake";
            Console.SetWindowSize((GameEngine.Width * 2) + 50, GameEngine.Height + 1);
            Console.SetBufferSize((GameEngine.Width * 2) + 50, GameEngine.Height + 1);
            Console.SetWindowPosition(0, 0);
            Console.CursorVisible = false;
            Console.Clear();

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  Welcome to BattleSnake...");
            Thread.Sleep(300);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("");
            Console.WriteLine("  Initializing...");
            Thread.Sleep(1000);

            string[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory(), "*.dll"); //get all files

            Console.WriteLine("  Loading DLLs...");

            Thread.Sleep(1000);

            List<Type> snakeNavigators = new List<Type>();
            foreach (string file in files)
            {
                if (file.ToLower().EndsWith("battlesnake.library.dll"))
                {
                    continue;
                }

                try
                {
                    bool loadedAny = false;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(string.Format("    > Found {0}", file.Contains(@"\") ? file.Substring(file.LastIndexOf(@"\") + 1) : file));
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(file); //load valid assembly into the main AppDomain
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (!type.IsInterface && typeof(SnakeNavigator).IsAssignableFrom(type) && type != typeof(SnakeNavigator))
                        {
                            loadedAny = true;
                            snakeNavigators.Add(type);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(string.Format("      > Loaded {0}", type.Name));
                            Thread.Sleep(500);
                        }
                    }

                    if (!loadedAny)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("        (No snakes defined in DLL)");
                    }
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("        (No snakes defined in DLL)");
                }
            }

            if (snakeNavigators.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("");
                Console.WriteLine("  No snakes found. Include your YourCompiledSnake.DLL inside the local folder.");
                Console.WriteLine("");
                return new Type[0];
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  -------------------");
            Thread.Sleep(500);
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            int cursorPosition = Console.CursorTop;
            Console.Write("  Starting... (3)");
            for (int i = 0; i < 3; i++)
            {
                Console.SetCursorPosition(15, cursorPosition);
                Console.Write(3 - i);
                Thread.Sleep(1000);
            }

            Console.Clear();
            Console.SetWindowPosition(0, 0);
            Console.SetCursorPosition(0, 0);

            return snakeNavigators.ToArray();
        }
    }
}