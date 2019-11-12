using BattleSnake.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleSnake
{
    internal class Snake
    {
        internal Direction Direction = Direction.Left;
        internal Point[] Points;
        internal ConsoleColor Color;
        internal ConsoleColor DeadColor;

        internal bool IsDead = false;
        internal bool CollectingPoints = true;

        internal Vector InitialVector;
        internal SnakeNavigator SnakeNavigator;

        internal static Snake Create(Type SnakeNavigatorType, int InitialLength, Vector StartingVector, ConsoleColor Color, ConsoleColor DeadColor)
        {
            Snake snake = new Snake();
            snake.SnakeNavigator = (SnakeNavigator)Activator.CreateInstance(SnakeNavigatorType);
            snake.Points = new Point[InitialLength];
            snake.InitialVector = StartingVector;
            snake.Color = Color;
            snake.DeadColor = DeadColor;

            for (int i = 0; i < snake.Points.Length; i++)
            {
                snake.Points[i] = new Point(StartingVector.Point.X, StartingVector.Point.Y);
            }
            snake.Direction = StartingVector.Direction;
            return snake;
        }

        internal void Lengthen(int GrowthFactor)
        {
            Point[] points = new Point[Points.Length + GrowthFactor];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = Points[Math.Max(i - GrowthFactor, 0)];
            }
            Points = points;
        }

        internal void Increment()
        {
            if (IsDead)
            {
                return;
            }

            for (int i = 1; i < Points.Length; i++)
            {
                Points[i - 1] = Points[i];
            }

            if (Points.Length >= 2)
            {
                switch (Direction)
                {
                    case Direction.Left:
                        {
                            Points[Points.Length - 1] = new Point(Points[Points.Length - 2].X - 1, Points[Points.Length - 2].Y);
                            break;
                        }
                    case Direction.Right:
                        {
                            Points[Points.Length - 1] = new Point(Points[Points.Length - 2].X + 1, Points[Points.Length - 2].Y);
                            break;
                        }
                    case Direction.Up:
                        {
                            Points[Points.Length - 1] = new Point(Points[Points.Length - 2].X, Points[Points.Length - 2].Y - 1);
                            break;
                        }
                    case Direction.Down:
                        {
                            Points[Points.Length - 1] = new Point(Points[Points.Length - 2].X, Points[Points.Length - 2].Y + 1);
                            break;
                        }
                }
            }
            else if (Points.Length == 1)
            {
                switch (Direction)
                {
                    case Direction.Left:
                        {
                            Points[0] = new Point(Points[0].X - 1, Points[0].Y);
                            break;
                        }
                    case Direction.Right:
                        {
                            Points[0] = new Point(Points[0].X + 1, Points[0].Y);
                            break;
                        }
                    case Direction.Up:
                        {
                            Points[0] = new Point(Points[0].X, Points[0].Y - 1);
                            break;
                        }
                    case Direction.Down:
                        {
                            Points[0] = new Point(Points[0].X, Points[0].Y + 1);
                            break;
                        }
                }
            }
        }
    }
}
