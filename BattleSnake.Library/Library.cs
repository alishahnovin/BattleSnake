using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleSnake.Library
{
    public class Point
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Point(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    public class Snake
    {
        public Vector Head { get; private set; }
        public int Length
        {
            get
            {
                return Body.Length;
            }
        }
        public Point[] Body { get; private set; }

        public Snake(Vector Head, Point[] Body)
        {
            this.Head = Head;
            this.Body = Body;
        }
    }

    public class Vector
    {
        public Point Point { get; private set; }
        public Direction Direction { get; private set; }

        public Vector(Point Point, Direction Direction)
        {
            this.Point = Point;
            this.Direction = Direction;
        }
    }

    public class Rectangle
    {
        public int Left { get; private set; }
        public int Right { get; private set; }
        public int Top { get; private set; }
        public int Bottom { get; private set; }
        public Rectangle(int Left, int Right, int Top, int Bottom)
        {
            this.Left = Left;
            this.Right = Right;
            this.Top = Top;
            this.Bottom = Bottom;
        }
    }

    public enum Mode
    {
        Simultaneous,
        StepWise
    }

    public class GameParameters
    {
        public Point Apple { get; private set; }
        public Point[] Obstacles { get; private set; }
        public Rectangle Boundary { get; private set; }
        
        public Snake Self { get; private set; }
        public Snake[] Opponents { get; private set; }
        public Mode Mode { get; private set; }

        public int InitialSize { get; private set; }
        public int GrowthFactor { get; private set; }

        public GameParameters(Rectangle Boundary, Point Apple, Point[] Obstacles, Snake Self, Snake[] Opponents, Mode Mode, int InitialSize, int GrowthFactor)
        {
            this.Apple = Apple;
            this.Obstacles = Obstacles;
            this.Boundary = Boundary;

            this.Self = Self;
            this.Opponents = Opponents;

            this.Mode = Mode;
            this.InitialSize = InitialSize;
            this.GrowthFactor = GrowthFactor;
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public interface SnakeNavigator
    {
        Direction CheckForDirections(GameParameters GameParameters);
    }

}
