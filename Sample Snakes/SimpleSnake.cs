using BattleSnake.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snakes
{
    public class SimpleSnake : SnakeNavigator
    {
        private Direction _LastDirection;
        private Random _Random;

        public Direction CheckForDirections(GameParameters GameParameters)
        {
            _Random = new Random(DateTime.Now.Millisecond + GameParameters.Self.Head.Point.X + GameParameters.Self.Head.Point.Y);

            Direction direction = _LastDirection;
            if (GameParameters.Apple.X < GameParameters.Self.Head.Point.X && _LastDirection != Direction.Right)
            {
                direction = Direction.Left;
            }
            else if (GameParameters.Apple.X > GameParameters.Self.Head.Point.X && _LastDirection != Direction.Left)
            {
                direction = Direction.Right;
            }
            else if (GameParameters.Apple.Y < GameParameters.Self.Head.Point.Y && _LastDirection != Direction.Down)
            {
                direction = Direction.Up;
            }
            else if (GameParameters.Apple.Y > GameParameters.Self.Head.Point.Y && _LastDirection != Direction.Up)
            {
                direction = Direction.Down;
            }

            _LastDirection = UpdateDirection(GameParameters, direction);
            return _LastDirection;
        }

        private Direction UpdateDirection(GameParameters GameParameters, Direction NewDirection, int TryCount = 0)
        {
            Direction updatedDirection = NewDirection;
            bool anythingAbove = GameParameters.Obstacles.Any(o => GameParameters.Self.Head.Point.Y - 1 == o.Y && GameParameters.Self.Head.Point.X == o.X) || GameParameters.Self.Head.Point.Y - 1 == GameParameters.Boundary.Top;
            bool anythingBelow = GameParameters.Obstacles.Any(o => GameParameters.Self.Head.Point.Y + 1 == o.Y && GameParameters.Self.Head.Point.X == o.X) || GameParameters.Self.Head.Point.Y + 1 == GameParameters.Boundary.Bottom;
            bool anythingLeft = GameParameters.Obstacles.Any(o => GameParameters.Self.Head.Point.X - 1 == o.X && GameParameters.Self.Head.Point.Y == o.Y) || GameParameters.Self.Head.Point.X - 1 == GameParameters.Boundary.Left;
            bool anythingRight = GameParameters.Obstacles.Any(o => GameParameters.Self.Head.Point.X + 1 == o.X && GameParameters.Self.Head.Point.Y == o.Y) || GameParameters.Self.Head.Point.X + 1 == GameParameters.Boundary.Right;

            if (anythingAbove && NewDirection == Direction.Up)
            {
                updatedDirection = !anythingLeft ? Direction.Left : (!anythingRight ? Direction.Right : Direction.Down);
            }
            else if (anythingBelow && NewDirection == Direction.Down)
            {
                updatedDirection = !anythingLeft ? Direction.Left : (!anythingRight ? Direction.Right : Direction.Up);
            }
            else if (anythingLeft && NewDirection == Direction.Left)
            {
                updatedDirection = !anythingAbove ? Direction.Up : (!anythingBelow ? Direction.Down : Direction.Right);
            }
            else if (anythingRight && NewDirection == Direction.Right)
            {
                updatedDirection = !anythingAbove ? Direction.Up : (!anythingBelow ? Direction.Down : Direction.Left);
            }

            if (GameParameters.Opponents.Any(opp => opp.Head.Point.X == GameParameters.Self.Head.Point.X - 2 && opp.Head.Point.Y == GameParameters.Self.Head.Point.Y && opp.Head.Direction == Direction.Right) && updatedDirection == Direction.Left && _Random.Next(0, 3) == 0)
            {
                updatedDirection = !anythingAbove ? Direction.Up : (!anythingBelow ? Direction.Down : Direction.Right);
            }
            else if (GameParameters.Opponents.Any(opp => opp.Head.Point.X == GameParameters.Self.Head.Point.X + 2 && opp.Head.Point.Y == GameParameters.Self.Head.Point.Y && opp.Head.Direction == Direction.Left) && updatedDirection == Direction.Right && _Random.Next(0, 3) == 0)
            {
                updatedDirection = !anythingBelow ? Direction.Down : (!anythingAbove ? Direction.Up : Direction.Right);
            }
            else if (GameParameters.Opponents.Any(opp => opp.Head.Point.Y == GameParameters.Self.Head.Point.Y + 2 && opp.Head.Point.X == GameParameters.Self.Head.Point.X && opp.Head.Direction == Direction.Up) && updatedDirection == Direction.Down && _Random.Next(0, 3) == 0)
            {
                updatedDirection = !anythingLeft ? Direction.Left : (!anythingRight ? Direction.Right : Direction.Up);
            }
            else if (GameParameters.Opponents.Any(opp => opp.Head.Point.Y == GameParameters.Self.Head.Point.Y - 2 && opp.Head.Point.X == GameParameters.Self.Head.Point.X && opp.Head.Direction == Direction.Down) && updatedDirection == Direction.Up && _Random.Next(0, 3) == 0)
            {
                updatedDirection = !anythingRight ? Direction.Right : (!anythingLeft ? Direction.Left : Direction.Up);
            }

            return updatedDirection == NewDirection || TryCount > 5 ? NewDirection : UpdateDirection(GameParameters, updatedDirection, ++TryCount);
        }
    }
}
