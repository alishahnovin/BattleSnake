using BattleSnake.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snakes
{
    public class StarvingSnake : SnakeNavigator
    {
        public Direction CheckForDirections(GameParameters GameParameters)
        {
            Direction direction = Direction.Up;
            if (GameParameters.Apple.X < GameParameters.Self.Head.Point.X && GameParameters.Self.Head.Direction != Direction.Right)
            {
                direction = Direction.Left;
            }
            else if (GameParameters.Apple.X > GameParameters.Self.Head.Point.X && GameParameters.Self.Head.Direction != Direction.Left)
            {
                direction = Direction.Right;
            }
            else if (GameParameters.Apple.Y < GameParameters.Self.Head.Point.Y && GameParameters.Self.Head.Direction != Direction.Down)
            {
                direction = Direction.Up;
            }
            else if (GameParameters.Apple.Y > GameParameters.Self.Head.Point.Y && GameParameters.Self.Head.Direction != Direction.Up)
            {
                direction = Direction.Down;
            }

            bool anythingAbove = GameParameters.Obstacles.Any(o => GameParameters.Self.Head.Point.Y - 1 == o.Y && GameParameters.Self.Head.Point.X == o.X) || GameParameters.Self.Head.Point.Y - 1 == GameParameters.Boundary.Top;
            bool anythingBelow = GameParameters.Obstacles.Any(o => GameParameters.Self.Head.Point.Y + 1 == o.Y && GameParameters.Self.Head.Point.X == o.X) || GameParameters.Self.Head.Point.Y + 1 == GameParameters.Boundary.Bottom;
            bool anythingLeft = GameParameters.Obstacles.Any(o => GameParameters.Self.Head.Point.X - 1 == o.X && GameParameters.Self.Head.Point.Y == o.Y) || GameParameters.Self.Head.Point.X - 1 == GameParameters.Boundary.Left;
            bool anythingRight = GameParameters.Obstacles.Any(o => GameParameters.Self.Head.Point.X + 1 == o.X && GameParameters.Self.Head.Point.Y == o.Y) || GameParameters.Self.Head.Point.X + 1 == GameParameters.Boundary.Right;

            if (anythingAbove && direction == Direction.Up)
            {
                direction = !anythingLeft ? Direction.Left : (!anythingRight ? Direction.Right : Direction.Down);
            }
            else if (anythingBelow && direction == Direction.Down)
            {
                direction = !anythingLeft ? Direction.Left : (!anythingRight ? Direction.Right : Direction.Up);
            }
            else if (anythingLeft && direction == Direction.Left)
            {
                direction = !anythingAbove ? Direction.Up : (!anythingBelow ? Direction.Down : Direction.Right);
            }
            else if (anythingRight && direction == Direction.Right)
            {
                direction = !anythingAbove ? Direction.Up : (!anythingBelow ? Direction.Down : Direction.Left);
            }

            return direction;
        }
    }
}
