using BattleSnake.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Snakes
{
    public class AStarrySnake : SnakeNavigator
    {
        public Direction CheckForDirections(GameParameters GameParameters)
        {
            Location current = null;
            Location start = new Location { X = GameParameters.Self.Head.Point.X, Y = GameParameters.Self.Head.Point.Y };
            Location target = new Location { X = GameParameters.Apple.X, Y = GameParameters.Apple.Y };
            List<Location> openList = new List<Location>();
            List<Location> closedList = new List<Location>();
            int g = 0;

            // start by adding the original position to the open list
            openList.Add(start);

            List<string> map = new List<string>();
            for (int y = GameParameters.Boundary.Top; y <= GameParameters.Boundary.Bottom; y++)
            {
                string row = "";
                for (int x = GameParameters.Boundary.Left; x <= GameParameters.Boundary.Right; x++)
                {
                    char c = ' ';
                    if (x == GameParameters.Boundary.Left || x == GameParameters.Boundary.Right || y == GameParameters.Boundary.Top || y == GameParameters.Boundary.Bottom)
                    {
                        c = '#';
                    }
                    else if (GameParameters.Obstacles.Any(o => o.X == x && o.Y == y))
                    {
                        c = '#';
                    }
                    else if (GameParameters.Apple.X == x && GameParameters.Apple.Y == y)
                    {
                        c = 'B';
                    }
                    else if (GameParameters.Self.Head.Point.X == x && GameParameters.Self.Head.Point.Y == y)
                    {
                        c = 'A';
                    }
                    row += c;
                }
                map.Add(row);
            }

            while (openList.Count > 0)
            {
                var lowest = openList.Min(l => l.F);
                current = openList.First(l => l.F == lowest);
                // add the current square to the closed list
                closedList.Add(current);

                // remove it from the open list
                openList.Remove(current);

                if (closedList.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null)
                    break;

                var adjacentSquares = GetWalkableAdjacentSquares(current.X, current.Y, map.ToArray());
                g++;

                foreach (var adjacentSquare in adjacentSquares)
                {
                    // if this adjacent square is already in the closed list, ignore it
                    if (closedList.FirstOrDefault(l => l.X == adjacentSquare.X
                            && l.Y == adjacentSquare.Y) != null)
                        continue;

                    // if it's not in the open list...
                    if (openList.FirstOrDefault(l => l.X == adjacentSquare.X
                            && l.Y == adjacentSquare.Y) == null)
                    {
                        // compute its score, set the parent
                        adjacentSquare.G = g;
                        adjacentSquare.H = ComputeHScore(adjacentSquare.X,
                            adjacentSquare.Y, target.X, target.Y);
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                        adjacentSquare.Parent = current;

                        // and add it to the open list
                        openList.Insert(0, adjacentSquare);
                    }
                    else
                    {
                        // test if using the current G score makes the adjacent square's F score
                        // lower, if yes update the parent because it means it's a better path
                        if (g + adjacentSquare.H < adjacentSquare.F)
                        {
                            adjacentSquare.G = g;
                            adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                            adjacentSquare.Parent = current;
                        }
                    }
                }
            }

            if (current != null)
            {
                Location startingPoint = current;
                Location nextPoint = current;
                while (startingPoint.Parent != null)
                {
                    nextPoint = startingPoint;
                    startingPoint = startingPoint.Parent;
                }

                if (nextPoint.X < GameParameters.Self.Head.Point.X && nextPoint.Y == GameParameters.Self.Head.Point.Y)
                {
                    return Direction.Left;
                }
                else if (nextPoint.X > GameParameters.Self.Head.Point.X && nextPoint.Y == GameParameters.Self.Head.Point.Y)
                {
                    return Direction.Right;
                }
                else if (nextPoint.X == GameParameters.Self.Head.Point.X && nextPoint.Y < GameParameters.Self.Head.Point.Y)
                {
                    return Direction.Up;
                }
                else if (nextPoint.X == GameParameters.Self.Head.Point.X && nextPoint.Y > GameParameters.Self.Head.Point.Y)
                {
                    return Direction.Down;
                }
            }
            return GameParameters.Self.Head.Direction;
        }

        private static List<Location> GetWalkableAdjacentSquares(int x, int y, string[] map)
        {
                var proposedLocations = new List<Location>()
        {
            new Location { X = x, Y = y - 1 },
            new Location { X = x, Y = y + 1 },
            new Location { X = x - 1, Y = y },
            new Location { X = x + 1, Y = y },
        };

                return proposedLocations.Where(
                    l => map[l.Y][l.X] == ' ' || map[l.Y][l.X] == 'B').ToList();
        }

        private static int ComputeHScore(int x, int y, int targetX, int targetY)
        {
            return Math.Abs(targetX - x) + Math.Abs(targetY - y);
        }

        private class Location
        {
            public int X;
            public int Y;
            public int F;
            public int G;
            public int H;
            public Location Parent;
        }
    }
}
