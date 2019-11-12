using BattleSnake.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snakes
{
    public class JellyShake : SnakeNavigator
    {
        private Direction _LastDirection;
        private bool _HasZigged = false;

        public Direction CheckForDirections(GameParameters GameParameters)
        {
            Direction direction = _LastDirection;
            if (GameParameters.Apple.X < GameParameters.Self.Head.Point.X && _LastDirection != Direction.Right)
            {
                direction = Direction.Left;
                if (_HasZigged)
                {
                    _HasZigged = false;
                }
                else
                {
                    _HasZigged = true;
                    if (GameParameters.Apple.Y < GameParameters.Self.Head.Point.Y && _LastDirection != Direction.Down)
                    {
                        direction = Direction.Up;
                    }
                    else if (GameParameters.Apple.Y > GameParameters.Self.Head.Point.Y && _LastDirection != Direction.Up)
                    {
                        direction = Direction.Down;
                    }
                }
            }
            else if (GameParameters.Apple.X > GameParameters.Self.Head.Point.X && _LastDirection != Direction.Left)
            {
                direction = Direction.Right;
                if (_HasZigged)
                {
                    _HasZigged = false;
                }
                else
                {
                    _HasZigged = true;
                    if (GameParameters.Apple.Y < GameParameters.Self.Head.Point.Y && _LastDirection != Direction.Down)
                    {
                        direction = Direction.Up;
                    }
                    else if (GameParameters.Apple.Y > GameParameters.Self.Head.Point.Y && _LastDirection != Direction.Up)
                    {
                        direction = Direction.Down;
                    }
                }
            }
            else if (GameParameters.Apple.Y < GameParameters.Self.Head.Point.Y && _LastDirection != Direction.Down)
            {
                direction = Direction.Up;
                if (_HasZigged)
                {
                    _HasZigged = false;
                }
                else
                {
                    _HasZigged = true;
                    if (GameParameters.Apple.X < GameParameters.Self.Head.Point.X && _LastDirection != Direction.Right)
                    {
                        direction = Direction.Left;
                    }
                    else if (GameParameters.Apple.X > GameParameters.Self.Head.Point.X && _LastDirection != Direction.Left)
                    {
                        direction = Direction.Right;
                    }
                }
            }
            else if (GameParameters.Apple.Y > GameParameters.Self.Head.Point.Y && _LastDirection != Direction.Up)
            {
                direction = Direction.Down;
                if (_HasZigged)
                {
                    _HasZigged = false;
                }
                else
                {
                    _HasZigged = true;
                    if (GameParameters.Apple.X < GameParameters.Self.Head.Point.X && _LastDirection != Direction.Right)
                    {
                        direction = Direction.Left;
                    }
                    else if (GameParameters.Apple.X > GameParameters.Self.Head.Point.X && _LastDirection != Direction.Left)
                    {
                        direction = Direction.Right;
                    }
                }
            }

            _LastDirection = UpdateDirection(GameParameters, direction);
            return _LastDirection;
        }

        private Direction UpdateDirection(GameParameters GameParameters, Direction NewDirection, int TryCount = 0)
        {
            Random r = new Random();
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

            return updatedDirection == NewDirection || TryCount > 5 ? NewDirection : UpdateDirection(GameParameters, updatedDirection, ++TryCount);
        }
    }
}