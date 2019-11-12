using BattleSnake.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleSnake
{
    public class GameEngine
    {
        private MatchType _MatchType = MatchType.Full;
        private Mode _Mode = Mode.StepWise;
        private int _InitialSnakeLength = 1;
        private int _GrowthFactor = 1;
        private int _TimeLimit = 5;
        private int _MaxTimeLimit = 10;
        private int _MinTimeLimit = 1;

        private int _MinSpeed = 250;
        private int _MaxSpeed = 10;
        private int _Speed = 130;

        public static int Width = 49;
        public static int Height = 49;
        private int _LeftEdge = 0;
        private int _TopEdge = 0;
        private int _RightEdge = 0;
        private int _BottomEdge = 0;
        private int _Round = 0;
        private DateTime _RoundStartTime = DateTime.MinValue;
        private int _TotalRounds = 0;

        private bool _GameStarted = false;

        private GamePieces[][] _GameMatrix;
        private Snake[] _Snakes;
        private Point _ApplePosition;

        private Random _Random = new Random();
        private Dictionary<Type, int> _RoundRobin = new Dictionary<Type, int>();
        private DateTime _LastTick = DateTime.MinValue;
        private int _LastMoveCount = 0;

        /// <summary>
        /// Each step of the game, where every submitted snake wil have CheckForDirections called
        /// </summary>
        private void RunStep()
        {
            if (DateTime.Now.Subtract(_LastTick).Milliseconds < _Speed)
            {
                return;
            }
            _LastTick = DateTime.Now;

            Dictionary<Snake, Vector> snakeVectors = new Dictionary<Snake, Vector>();
            foreach (Snake snake in _Snakes.Where(s => !s.IsDead).OrderBy(o => _Random.Next()).ToArray())
            {
                snakeVectors.Add(snake, new Vector(snake.Points[snake.Points.Length - 1], snake.Direction));
            }

            foreach (Snake snake in _Snakes.Where(s => !s.IsDead).OrderBy(o => _Random.Next()).ToArray())
            {
                snake.Direction = snake.SnakeNavigator.CheckForDirections(
                    new GameParameters(
                        new Rectangle(_LeftEdge, _RightEdge, _TopEdge, _BottomEdge),
                        _ApplePosition,
                        _Snakes.SelectMany(p => p.Points).ToArray(),
                        new Library.Snake(snakeVectors[snake], snake.Points),
                        snakeVectors.Where(sv => sv.Key != snake).Select(sv => new Library.Snake(sv.Value, sv.Key.Points)).ToArray(),
                        _Mode,
                        _InitialSnakeLength,
                        _GrowthFactor
                        )
                    );

                //stepwise, snakes will move one at a time, which is why this happens in the loop.
                //on the next iteration, CheckForDirections will have an updated state of the game...
                if (_Mode == Mode.StepWise)
                {
                    StepSnake(snake);
                }
            }

            //simultaneous mode, each snake will have received their next direction
            //we still have to step the snakes one at a time,
            //which means the first snake may not realize it's collided, so we'll call Evaluate again once everyone's moved.
            //except this will cause all to fail, because their second evaluation they'll die on their own heads...
            if (_Mode == Mode.Simultaneous)
            {
                foreach (Snake snake in _Snakes.Where(s => !s.IsDead).ToArray())
                {
                    StepSnake(snake);
                }

                foreach (Snake snake in _Snakes.Where(s => !s.IsDead).ToArray())
                {
                    Snake collidingSnake = _Snakes.Where(s => s != snake).FirstOrDefault(s => s.Points[s.Points.Length - 1].X == snake.Points[snake.Points.Length - 1].X && s.Points[s.Points.Length - 1].Y == snake.Points[snake.Points.Length - 1].Y);
                    if (collidingSnake != null)
                    {
                        KillSnake(snake);
                        KillSnake(collidingSnake);
                    }
                }
            }

            //re-render the dead snakes (some collisions can overlap, so we want to make sure we show the fresh state)
            foreach (Snake snake in _Snakes.Where(s => s.IsDead))
            {
                KillSnake(snake);
            }

            _LastMoveCount++;
            if (_LastMoveCount > 500)
            {
                if (_GameMatrix[_ApplePosition.X][_ApplePosition.Y] == GamePieces.Apple)
                {
                    _GameMatrix[_ApplePosition.X][_ApplePosition.Y] = GamePieces.Blank;
                    Console.ForegroundColor = ConsoleColor.Black;
                    SetCursorPosition(_ApplePosition.X, _ApplePosition.Y);
                    Console.Write((char)GamePieces.Blank);
                    MoveApple();
                }
            }
        }

        private void StepSnake(Snake Snake)
        {
            _GameMatrix[Snake.Points[0].X][Snake.Points[0].Y] = GamePieces.Blank;
            SetCursorPosition(Snake.Points[0].X, Snake.Points[0].Y);
            Console.Write((char)GamePieces.Blank);
            Snake.Increment();
            RenderSnake(Snake);
        }

        private void MoveApple()
        {
            int x = _Random.Next(2, Width - 2);
            int y = _Random.Next(2, Height - 2);
            while (_GameMatrix[x][y] != GamePieces.Blank)
            {
                x = _Random.Next(2, Width - 2);
                y = _Random.Next(2, Height - 2);
            }

            _LastMoveCount = 0;
            _ApplePosition = new Point(x, y);
            _GameMatrix[_ApplePosition.X][_ApplePosition.Y] = GamePieces.Apple;
            Console.ForegroundColor = ConsoleColor.Magenta;
            SetCursorPosition(_ApplePosition.X, _ApplePosition.Y);
            Console.Write((char)GamePieces.Apple);
        }

        private void RenderSnake(Snake Snake)
        {
            if (_GameMatrix[Snake.Points[Snake.Points.Length - 1].X][Snake.Points[Snake.Points.Length - 1].Y] == GamePieces.Blank || _GameMatrix[Snake.Points[Snake.Points.Length - 1].X][Snake.Points[Snake.Points.Length - 1].Y] == GamePieces.Apple)
            {
                if (_GameMatrix[Snake.Points[Snake.Points.Length - 1].X][Snake.Points[Snake.Points.Length - 1].Y] == GamePieces.Apple)
                {
                    Snake.Lengthen(_GrowthFactor);

                    if (Snake.CollectingPoints)
                    {
                        _RoundRobin[Snake.SnakeNavigator.GetType()] += _GrowthFactor;
                    }

                    MoveApple();
                }

                char head = (char)GamePieces.Snake;
                if (Snake.Direction == Direction.Up)
                {
                    head = '▲';
                }
                else if (Snake.Direction == Direction.Down)
                {
                    head = '▼';
                }
                else if (Snake.Direction == Direction.Left)
                {
                    head = '◄';
                }
                else if (Snake.Direction == Direction.Right)
                {
                    head = '►';
                }
                for (int i = 0; i < Snake.Points.Length; i++)
                {
                    Console.ForegroundColor = Snake.Color;
                    SetCursorPosition(Snake.Points[i].X, Snake.Points[i].Y);
                    _GameMatrix[Snake.Points[i].X][Snake.Points[i].Y] = GamePieces.Snake;
                    Console.Write(i == Snake.Points.Length - 1 ? head : (char)GamePieces.Snake);
                }
            }
            else
            {
                KillSnake(Snake);
            }
        }

        private void KillSnake(Snake Snake)
        {
            Snake.IsDead = true;

            for (int i = 0; i < Snake.Points.Length; i++)
            {
                Console.ForegroundColor = i == Snake.Points.Length - 1 ? Snake.Color : ConsoleColor.DarkGray;
                _GameMatrix[Snake.Points[i].X][Snake.Points[i].Y] = GamePieces.Collision;
                SetCursorPosition(Snake.Points[i].X, Snake.Points[i].Y);
                Console.Write((char)GamePieces.Collision);
            }
        }

        private void SetCursorPosition(int X, int Y)
        {
            Console.SetCursorPosition((X * 2) + 1, Y);
        }

        public static GameEngine Start(Type[] SnakeNavigators)
        {
            return new GameEngine(SnakeNavigators);
        }

        #region SplashScreen
        private bool _AlreadyDrawn = false;
        private void RenderSplashScreen()
        {
            if (!_AlreadyDrawn)
            {
                _AlreadyDrawn = true;
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
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("                          ██████╗  █████╗ ████████╗████████╗██╗     ███████╗    ███████╗███╗   ██╗ █████╗ ██╗  ██╗███████╗");
                Console.WriteLine("                          ██╔══██╗██╔══██╗╚══██╔══╝╚══██╔══╝██║     ██╔════╝    ██╔════╝████╗  ██║██╔══██╗██║ ██╔╝██╔════╝");
                Console.WriteLine("                          ██████╔╝███████║   ██║      ██║   ██║     █████╗      ███████╗██╔██╗ ██║███████║█████╔╝ █████╗  ");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("                          ██╔══██╗██╔══██║   ██║      ██║   ██║     ██╔══╝      ╚════██║██║╚██╗██║██╔══██║██╔═██╗ ██╔══╝  ");
                Console.WriteLine("                          ██████╔╝██║  ██║   ██║      ██║   ███████╗███████╗    ███████║██║ ╚████║██║  ██║██║  ██╗███████╗");
                Console.WriteLine("                          ╚═════╝ ╚═╝  ╚═╝   ╚═╝      ╚═╝   ╚══════╝╚══════╝    ╚══════╝╚═╝  ╚═══╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝");

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("");
                Console.WriteLine("                                                                an epic battle game");
                Console.WriteLine("");

                Console.WriteLine("                                                                    written by:");
                Console.WriteLine("");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("                                 ██   █    ▄█    ▄▄▄▄▄    ▄  █ ██    ▄  █        ▄   ████▄     ▄   ▄█    ▄   ");
                Console.WriteLine("                                 █ █  █    ██   █     ▀▄ █   █ █ █  █   █         █  █   █      █  ██     █  ");
                Console.WriteLine("                                 █▄▄█ █    ██ ▄  ▀▀▀▀▄   ██▀▀█ █▄▄█ ██▀▀█     ██   █ █   █ █     █ ██ ██   █ ");
                Console.WriteLine("                                 █  █ ███▄ ▐█  ▀▄▄▄▄▀    █   █ █  █ █   █     █ █  █ ▀████  █    █ ▐█ █ █  █ ");
                Console.WriteLine("                                    █     ▀ ▐               █     █    █      █  █ █         █  █   ▐ █  █ █ ");
                Console.WriteLine("                                   █                       ▀     █    ▀       █   ██          █▐      █   ██ ");
                Console.WriteLine("                                  ▀                             ▀                             ▐              ");

                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("                                                             with snakes guided by you.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(@"                             _______ _______ _______ _______ _______     _______ _______ _______ _______ _______   ");
                Console.WriteLine(@"                            |   _   |   _   |   _   |   _   |   _   |   |   _   |   _   |   _   |   _   |   _   |");
                Console.WriteLine(@"                            |.  1   |.  l   |.  1___|   1___|   1___|   |   1___|.  1   |.  1   |.  1___|.  1___|");
                Console.WriteLine(@"                            |.  ____|.  _   |.  __)_|____   |____   |   |____   |.  ____|.  _   |.  |___|.  __)_ ");
                Console.WriteLine(@"                            |:  |   |:  |   |:  1   |:  1   |:  1   |   |:  1   |:  |   |:  |   |:  1   |:  1   |");
                Console.WriteLine(@"                            |::.|   |::.|:. |::.. . |::.. . |::.. . |   |::.. . |::.|   |::.|:. |::.. . |::.. . |");
                Console.WriteLine(@"                            `---'   `--- ---`-------`-------`-------'   `-------`---'   `--- ---`-------`-------'");
                Console.WriteLine(@"                                         _______ _______     _______  _______ _______ ___ ______                 ");
                Console.WriteLine(@"                                        |       |   _   |   |   _   \|   _   |   _   |   |   _  \                ");
                Console.WriteLine(@"                                        |.|   | |.  |   |   |.  1   /|.  1___|.  |___|.  |.  |   |               ");
                Console.WriteLine(@"                                        `-|.  |-|.  |   |   |.  _   \|.  __)_|.  |   |.  |.  |   |               ");
                Console.WriteLine(@"                                          |:  | |:  1   |   |:  1    |:  1   |:  1   |:  |:  |   |               ");
                Console.WriteLine(@"                                          |::.| |::.. . |   |::.. .  |::.. . |::.. . |::.|::.|   |               ");
                Console.WriteLine(@"                                          `---' `-------'   `-------'`-------`-------`---`--- ---'               ");

                Console.WriteLine(@"");
            }

            Console.SetCursorPosition(1, 47);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("            Mode (M): ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(_Mode.ToString());

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Battle (B): ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(_MatchType.ToString());

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Speed (Up/Down): ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(string.Format("{0}%", Convert.ToInt32(100.0 * (_Speed - _MinSpeed) / (_MaxSpeed - _MinSpeed))));

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Initial Size (S): ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(_InitialSnakeLength);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Growth Size (G): ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(_GrowthFactor);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Time Limit (T): ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(_TimeLimit > _MaxTimeLimit ? "Unlimited" : _TimeLimit.ToString());
            if (_TimeLimit == 1)
            {
                Console.WriteLine("min       ");
            }
            else if (_TimeLimit<=_MaxTimeLimit)
            {
                Console.WriteLine("mins      ");
            }

            //while on the splash screen, let the user set game conditions
            while (!_GameStarted)
            {
                KeyChecker();
                Thread.Sleep(1);
            }
        }
        #endregion

        internal GameEngine(Type[] SnakeNavigators)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.CursorVisible = false;
            Console.Title = "Battle Snake";

            _RightEdge = Width - 1;
            _BottomEdge = Height - 1;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.CursorVisible = false;
            Console.Title = "Battle Snake";
            Console.SetWindowSize((GameEngine.Width * 2) + 50, GameEngine.Height + 1);
            Console.SetBufferSize((GameEngine.Width * 2) + 50, GameEngine.Height + 1);
            Console.SetWindowPosition(0, 0);
            Console.CursorVisible = false;
            Console.Clear();

            RenderSplashScreen();

            foreach (Type type in SnakeNavigators)
            {
                if (!type.IsInterface && typeof(SnakeNavigator).IsAssignableFrom(type) && type != typeof(SnakeNavigator))
                {
                    _RoundRobin.Add(type, 0);
                }
            }

            if (_RoundRobin.Count == 0)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine(" No snakes provided...");
            }
            else
            {
                SetupRoundRobin();
            }
        }

        private bool _InsideStack = false;
        internal void SetupRoundRobin()
        {
            foreach (Type key in _RoundRobin.Keys.ToList())
            {
                _RoundRobin[key] = 0;
            }

            _Round = 0;
            _InsideStack = true;
            List<Type> roundRobin = _RoundRobin.Keys.ToList();
            if (roundRobin.Count == 1)
            {
                _TotalRounds = 1;
                if (_Random.Next(0, 4) == 1)
                {
                    StartRound(new Snake[1] { Snake.Create(roundRobin[0], _InitialSnakeLength, new Vector(new Point(_LeftEdge + 1, _TopEdge + 1), Direction.Down), ConsoleColor.Yellow, ConsoleColor.DarkYellow) });
                }
                else if (_Random.Next(0, 4) == 1)
                {
                    StartRound(new Snake[1] { Snake.Create(roundRobin[0], _InitialSnakeLength, new Vector(new Point(_RightEdge - 1, _BottomEdge - 1), Direction.Up), ConsoleColor.Green, ConsoleColor.DarkGreen) });
                }
                else if (_Random.Next(0, 4) == 1)
                {
                    StartRound(new Snake[1] { Snake.Create(roundRobin[0], _InitialSnakeLength, new Vector(new Point(_RightEdge - 1, _TopEdge + 1), Direction.Left), ConsoleColor.Red, ConsoleColor.DarkRed) });
                }
                else
                {
                    StartRound(new Snake[1] { Snake.Create(roundRobin[0], _InitialSnakeLength, new Vector(new Point(_LeftEdge + 1, _BottomEdge - 1), Direction.Right), ConsoleColor.Cyan, ConsoleColor.DarkCyan) });
                }
            }
            else if (this._MatchType == MatchType.HeadToHead || roundRobin.Count < 4)
            {
                _TotalRounds = Convert.ToInt32(Math.Ceiling(_RoundRobin.Count * (_RoundRobin.Count - 1) / 2.0));
                for (int i = 0; i < roundRobin.Count; i++)
                {
                    for (int j = i + 1; j < roundRobin.Count; j++)
                    {
                        List<Snake> snakes = new List<Snake>();
                        snakes.Add(Snake.Create(roundRobin[i], _InitialSnakeLength, new Vector(new Point(_LeftEdge + 1, _TopEdge + 1), Direction.Down), ConsoleColor.Yellow, ConsoleColor.DarkYellow));
                        snakes.Add(Snake.Create(roundRobin[j], _InitialSnakeLength, new Vector(new Point(_RightEdge - 1, _BottomEdge - 1), Direction.Up), ConsoleColor.Green, ConsoleColor.DarkGreen));
                        StartRound(snakes.ToArray());
                    }
                }
            }
            else
            {
                List<Type[]> snakeTypes = GetPowerSet<Type>(roundRobin).Where(c => c.Count() == 4).Select(i => i.OrderBy(o => _Random.Next()).ToArray()).ToList();
                _TotalRounds = snakeTypes.Count;
                foreach (Type[] types in snakeTypes)
                {
                    List<Snake> snakes = new List<Snake>();
                    snakes.Add(Snake.Create(types[0], _InitialSnakeLength, new Vector(new Point(_LeftEdge + 1, _TopEdge + 1), Direction.Down), ConsoleColor.Yellow, ConsoleColor.DarkYellow));
                    snakes.Add(Snake.Create(types[1], _InitialSnakeLength, new Vector(new Point(_RightEdge - 1, _BottomEdge - 1), Direction.Up), ConsoleColor.Green, ConsoleColor.DarkGreen));
                    snakes.Add(Snake.Create(types[2], _InitialSnakeLength, new Vector(new Point(_RightEdge - 1, _TopEdge + 1), Direction.Left), ConsoleColor.Red, ConsoleColor.DarkRed));
                    snakes.Add(Snake.Create(types[3], _InitialSnakeLength, new Vector(new Point(_LeftEdge + 1, _BottomEdge - 1), Direction.Right), ConsoleColor.Cyan, ConsoleColor.DarkCyan));
                    StartRound(snakes.ToArray());
                }
            }

            _InsideStack = false;
            while (!_InsideStack)
            {
                KeyChecker();
                Thread.Sleep(1);
            }
        }

        private IEnumerable<IEnumerable<T>> GetPowerSet<T>(List<T> list)
        {
            return from m in Enumerable.Range(0, 1 << list.Count)
                   select
                       from i in Enumerable.Range(0, list.Count)
                       where (m & (1 << i)) != 0
                       select list[i];
        }

        private void StartRound(Snake[] Snakes)
        {
            _Round++;
            _RoundStartTime = DateTime.Now;
            _Snakes = Snakes.ToArray();

            _GameMatrix = new GamePieces[Width][];

            for (int w = 0; w < Width; w++)
            {
                _GameMatrix[w] = new GamePieces[Height];
            }

            for (int x = 0; x <= _RightEdge; x++)
            {
                for (int y = 0; y <= _BottomEdge; y++)
                {
                    if (y == _TopEdge || y == _BottomEdge || x == _LeftEdge || x == _RightEdge)
                    {
                        _GameMatrix[x][y] = GamePieces.Wall;
                    }
                    else
                    {
                        _GameMatrix[x][y] = GamePieces.Blank;
                    }
                }
            }

            if (_Snakes.Length > 0)
            {
                _GameMatrix[_Snakes[0].Points[0].X][_Snakes[0].Points[0].Y] = GamePieces.Snake;
                if (_Snakes.Length > 1)
                {
                    _GameMatrix[_Snakes[1].Points[0].X][_Snakes[1].Points[0].Y] = GamePieces.Snake;
                    if (_Snakes.Length > 2)
                    {
                        _GameMatrix[_Snakes[2].Points[0].X][_Snakes[2].Points[0].Y] = GamePieces.Snake;
                        if (_Snakes.Length > 3)
                        {
                            _GameMatrix[_Snakes[3].Points[0].X][_Snakes[3].Points[0].Y] = GamePieces.Snake;
                        }
                    }
                }
            }

            _ApplePosition = new Point(Convert.ToInt32(Math.Ceiling(_RightEdge / 2.0)), Convert.ToInt32(Math.Ceiling(_BottomEdge / 2.0)));
            _GameMatrix[_ApplePosition.X][_ApplePosition.Y] = GamePieces.Apple;

            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(98, 1);
            Console.WriteLine(@"  ___       _   _   _     ___           _ ");
            Console.SetCursorPosition(98, 2);
            Console.WriteLine(@" | _ ) __ _| |_| |_| |___/ __|_ _  __ _| |_____ ");
            Console.SetCursorPosition(98, 3);

            Console.WriteLine(@" | _ \/ _` |  _|  _| / -_)__ \ ' \/ _` | / / -_)");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(98, 4);
            Console.WriteLine(@" |___/\__,_|\__|\__|_\___|___/_||_\__,_|_\_\___|");



            Console.SetCursorPosition(99, 31);
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write(@"Up/Down: ");
            Console.ForegroundColor = ConsoleColor.White; Console.Write("Change Speed");
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("  K:");
            Console.ForegroundColor = ConsoleColor.White; Console.Write(" Kill All Snakes");
            Console.SetCursorPosition(99, 32);

            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write(@"M: ");
            Console.ForegroundColor = ConsoleColor.White; Console.Write("Change Mode");
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("   B:");
            Console.ForegroundColor = ConsoleColor.White; Console.Write(" Change Match Type");

            Console.SetCursorPosition(99, 33);
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write(@"S: ");
            Console.ForegroundColor = ConsoleColor.White; Console.Write("Change Initial Size");
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("  G: ");
            Console.ForegroundColor = ConsoleColor.White; Console.Write("Change Growth Factor");

            Console.SetCursorPosition(99, 34);
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write(@"T: ");
            Console.ForegroundColor = ConsoleColor.White; Console.Write("Change Match Time Limit");
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write(@"  Esc: ");
            Console.ForegroundColor = ConsoleColor.White; Console.Write("Restart");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(115, 35);
            Console.WriteLine(@"       ---_ ......._-_--.        ");
            Console.SetCursorPosition(115, 36);
            Console.Write(@"      ("); Console.ForegroundColor = ConsoleColor.White; Console.Write(@"|"); Console.ForegroundColor = ConsoleColor.Green; Console.Write(@"\ /      / /"); Console.ForegroundColor = ConsoleColor.White; Console.Write(@"|"); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(@" \  \       ");
            Console.SetCursorPosition(115, 37);
            Console.WriteLine(@"      /  /     .'  -=-'   `.     ");
            Console.SetCursorPosition(115, 38);
            Console.WriteLine(@"     /  /    .'             )    ");
            Console.SetCursorPosition(115, 39);
            Console.WriteLine(@"   _/  /   .'        _.)   /     ");
            Console.SetCursorPosition(115, 39);
            Console.WriteLine(@"  / o   o        _.-' /  .'      ");
            Console.SetCursorPosition(115, 40);
            Console.WriteLine(@"  \          _.-'    / .'*|      ");
            Console.SetCursorPosition(115, 41);
            Console.ForegroundColor = ConsoleColor.Green; Console.Write(@"   \______.-'//    .'.'"); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(@" \"); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(@"*|      ");
            Console.SetCursorPosition(115, 42);
            Console.ForegroundColor = ConsoleColor.White; Console.Write(@"    \|  \ |"); Console.ForegroundColor = ConsoleColor.Green; Console.Write(@" //   .'.'"); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(@" _ |"); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(@"*|      ");
            Console.SetCursorPosition(115, 43);
            Console.ForegroundColor = ConsoleColor.White; Console.Write(@"     `   \|"); Console.ForegroundColor = ConsoleColor.Green; Console.Write(@"//  .'.'"); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(@"_ _ _|"); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(@"*|      ");
            Console.SetCursorPosition(115, 44);
            Console.ForegroundColor = ConsoleColor.White; Console.Write(@"      .  ."); Console.ForegroundColor = ConsoleColor.Green; Console.Write(@"// .'.' "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(@"| _ _ \"); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(@"*|      ");
            Console.SetCursorPosition(115, 45);
            Console.ForegroundColor = ConsoleColor.White; Console.Write(@"      \`"); Console.ForegroundColor = ConsoleColor.Green; Console.Write(@"-"); Console.ForegroundColor = ConsoleColor.White; Console.Write(@"|\"); Console.ForegroundColor = ConsoleColor.Green; Console.Write(@"_/ /"); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(@"    \ _ _ \"); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(@"*\     ");
            Console.SetCursorPosition(115, 46);
            Console.ForegroundColor = ConsoleColor.Green; Console.Write(@"       `"); Console.ForegroundColor = ConsoleColor.Red; Console.Write(@"/"); Console.ForegroundColor = ConsoleColor.Green; Console.Write(@"'\__/      "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(@"\ _ _ \"); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(@"*\    ");
            Console.SetCursorPosition(115, 47);
            Console.ForegroundColor = ConsoleColor.Red; Console.Write(@"      /^|"); Console.ForegroundColor = ConsoleColor.Green; Console.Write(@"            "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(@"\ _ _ \"); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(@"*    ");
            Console.SetCursorPosition(115, 48);
            Console.ForegroundColor = ConsoleColor.Red; Console.Write(@"     '  `             "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(@"\ _ _ \"); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(@"  ");
            Console.SetCursorPosition(115, 49);
            Console.ForegroundColor = ConsoleColor.Green; Console.Write(@"                       "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(@"\_"); Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(@"  ");

            for (int y = 0; y <= _BottomEdge; y++)
            {
                for (int x = 0; x <= _RightEdge; x++)
                {
                    SetCursorPosition(x, y);
                    if (_GameMatrix[x][y] == GamePieces.Wall)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else if (_GameMatrix[x][y] == GamePieces.Apple)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                    }
                    Console.Write((char)_GameMatrix[x][y]);
                }
            }

            GameLoop();

            if (_Speed == _MaxSpeed)
            {
                Thread.Sleep(500);
            }
            else
            {
                Thread.Sleep(2000);
            }
        }

        private void GameLoop()
        {
            while (!_Snakes.All(snk => snk.IsDead))
            {
                RenderDetails();
                RunStep();
                KeyChecker();
                Thread.Sleep(5);
                if (_TimeLimit <= _MaxTimeLimit && _RoundStartTime.AddSeconds(_TimeLimit * 60).Subtract(DateTime.Now).TotalSeconds <= 0)
                {
                    foreach (Snake snake in _Snakes)
                    {
                        KillSnake(snake);
                    }
                }
            }
        }

        private void RenderDetails()
        {
            Console.SetCursorPosition(99, 5);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Mode: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(_Mode.ToString());

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Battle: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(_MatchType.ToString());

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Speed: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(string.Format("{0}%", Convert.ToInt32(100.0 * (_Speed - _MinSpeed) / (_MaxSpeed - _MinSpeed))));

            Console.SetCursorPosition(99, 6);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Initial Size: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(_InitialSnakeLength);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Growth Size: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(_GrowthFactor);

            if (_TimeLimit <= _MaxTimeLimit)
            {
                Console.SetCursorPosition(99, 7);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Time: ");

                TimeSpan timeLeft = _RoundStartTime.AddSeconds(_TimeLimit * 60).Subtract(DateTime.Now);
                if (timeLeft.TotalSeconds < 1)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else if (timeLeft.TotalSeconds > 31)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (timeLeft.TotalSeconds < 16)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (timeLeft.TotalSeconds < 31)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                Console.Write(timeLeft.TotalMinutes >= 1 ? timeLeft.ToString(@"m'm'\ ss's'") : timeLeft.ToString("ss's'"));
            }

            Console.SetCursorPosition(99, 9);
            Console.ForegroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Scores:");
            Console.ForegroundColor = ConsoleColor.White;
            int count = 1;
            foreach (var kvp in _RoundRobin.OrderByDescending(r => r.Value))
            {
                Console.SetCursorPosition(101, 9 + count);

                Snake snake = _Snakes.FirstOrDefault(snk => snk.SnakeNavigator.GetType().Equals(kvp.Key));

                Console.ForegroundColor = snake == null || !snake.CollectingPoints ? ConsoleColor.White : snake.Color;

                string label = kvp.Key.Name.Length > 15 ? string.Format("{0}...", kvp.Key.Name) : kvp.Key.Name;
                string score = kvp.Value.ToString();
                string padding = string.Empty;
                for (int i = 0; i < 20 - (label.Length + score.Length); i++)
                {
                    padding += ".";
                }
                Console.WriteLine(string.Format("{0}. {1} {3} {2}      ", count, label, score, padding));
                count++;
                if (count > 10)
                {
                    break;
                }
            }

            Console.ForegroundColor = ConsoleColor.White;

            Console.SetCursorPosition(99, 9 + count + 1);
            Console.WriteLine("--------------------------------------------");
            Console.SetCursorPosition(99, 9 + count + 2);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Round: ");
            Console.Write(string.Format("{0} of {1}", _Round, _TotalRounds));
        }

        private void KeyChecker()
        {
            if (Keyboard.IsKeyDown(KeyCode.Up) && _Speed > _MaxSpeed)
            {
                _Speed -= 10;
                if (_GameStarted)
                {
                }
                else
                {
                    RenderSplashScreen();
                }
            }
            else if (Keyboard.IsKeyDown(KeyCode.T))
            {
                _TimeLimit++;
                if (_TimeLimit > _MaxTimeLimit+1) { _TimeLimit = _MinTimeLimit; }
                if (_GameStarted)
                {
                    foreach (Snake snake in _Snakes)
                    {
                        KillSnake(snake);
                    }
                    SetupRoundRobin();
                }
                else
                {
                    RenderSplashScreen();
                }
            }
            else if (Keyboard.IsKeyDown(KeyCode.Down) && _Speed < _MinSpeed)
            {
                _Speed += 10;

                if (_GameStarted)
                {
                }
                else
                {
                    RenderSplashScreen();
                }
            }
            else if (Keyboard.IsKeyDown(KeyCode.Escape))
            {
                foreach (Snake snake in _Snakes)
                {
                    KillSnake(snake);
                }
                SetupRoundRobin();
            }
            else if (Keyboard.IsKeyDown(KeyCode.M))
            {
                _Mode = (_Mode == Mode.Simultaneous) ? Mode.StepWise : Mode.Simultaneous;
                if (_GameStarted)
                {
                    foreach (Snake snake in _Snakes)
                    {
                        KillSnake(snake);
                    }
                    SetupRoundRobin();
                }
                else
                {
                    RenderSplashScreen();
                }
            }
            else if (Keyboard.IsKeyDown(KeyCode.B))
            {
                _MatchType = _MatchType == MatchType.HeadToHead ? MatchType.Full : MatchType.HeadToHead;
                if (_GameStarted)
                {
                    foreach (Snake snake in _Snakes)
                    {
                        KillSnake(snake);
                    }
                    SetupRoundRobin();
                }
                else
                {
                    RenderSplashScreen();
                }
            }
            else if (Keyboard.IsKeyDown(KeyCode.S))
            {
                _InitialSnakeLength++;
                if (_InitialSnakeLength > 10) { _InitialSnakeLength = 1; }
                if (_GameStarted)
                {
                    foreach (Snake snake in _Snakes)
                    {
                        KillSnake(snake);
                    }
                    SetupRoundRobin();
                }
                else
                {
                    RenderSplashScreen();
                }
            }
            else if (Keyboard.IsKeyDown(KeyCode.G))
            {
                _GrowthFactor++;
                if (_GrowthFactor > 10) { _GrowthFactor = 1; }
                if (_GameStarted)
                {
                    foreach (Snake snake in _Snakes)
                    {
                        KillSnake(snake);
                    }
                    SetupRoundRobin();
                }
                else
                {
                    RenderSplashScreen();
                }
            }
            else if (Keyboard.IsKeyDown(KeyCode.K))
            {
                foreach (Snake snake in _Snakes)
                {
                    KillSnake(snake);
                }
            }
            else if (Keyboard.IsKeyDown(KeyCode.Spacebar) && !_GameStarted)
            {
                _GameStarted = true;
            }
        }
    }

    internal enum GamePieces
    {
        Wall = '■',
        Snake = 'o',
        Blank = ' ',
        Collision = 'x',
        Apple = '@',
    }

    internal enum MatchType
    {
        HeadToHead = 2,
        Full = 4
    }
}
