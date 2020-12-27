// Everything about hexagonal coordinates: https://www.redblobgames.com/grids/hexagons/

using AoC_2020.GameOfLife;
using AoCHelper;
using SheepTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_24 : BaseDay
    {
        private readonly List<List<HexDirection>> _input;

        public Day_24()
        {
            _input = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            var blackTiles = GetInitialBlackTiles();

            return blackTiles.Count.ToString();
        }

        public override string Solve_2()
        {
            var blackTiles = GetInitialBlackTiles();

            HashSet<Point> state = blackTiles.ToHashSet();

            var game = new GameOfLife.GameOfLife(
                state,
                toDieCondition: (activeNeighboursCount) => activeNeighboursCount is 0 or > 2,
                toBornCondition: (activeNeighboursCount) => (activeNeighboursCount == 2),
                numberOfCellsWhichHaveAGivenNeighbourAsNeighbourCondition: (numberOfNeighbours) => numberOfNeighbours >= 2);

            while (game.Iterations < 100)
            {
                game.Mutate();
            }

            return game.AliveCells.Count.ToString();
        }

        private ICollection<Point> GetInitialBlackTiles()
        {
            var timesByVisitedPoint = new Dictionary<Point, int>();

            foreach (var hexagonIndications in _input)
            {
                var hexagon = new HexagonalPoint2D(0, 0);
                foreach (var hexDir in hexagonIndications)
                {
                    hexagon = hexagon.Move(hexDir);
                }

                if (timesByVisitedPoint.ContainsKey(hexagon))
                {
                    ++timesByVisitedPoint[hexagon];
                }
                else
                {
                    timesByVisitedPoint[hexagon] = 1;
                }
            }

            return timesByVisitedPoint
                .Where(p => p.Value % 2 != 0)
                .Select(pair => pair.Key)
                .ToList();
        }

        private IEnumerable<List<HexDirection>> ParseInput()
        {
            var validIdentifiers = Enum.GetNames(typeof(HexDirection));
            foreach (var line in File.ReadAllLines(InputFilePath).ToList())
            {
                var parsed = new List<HexDirection>();
                var str = string.Empty;

                foreach (var ch in line)
                {
                    str += ch;

                    if (validIdentifiers.Contains(str))
                    {
                        parsed.Add((HexDirection)Enum.Parse(typeof(HexDirection), str));
                        str = string.Empty;
                    }
                }

                yield return parsed;
            }
        }
    }

    public enum HexDirection { w, sw, se, e, ne, nw };

    public record HexagonalPoint2D(int X, int Y) : Point2D(X, Y)
    {
        public override Point2D[] Neighbours()
        {
            var result = NeighboursIncludingThis().ToList();

            result.Remove(this);
            result.Remove(new HexagonalPoint2D(X - 1, Y + 1));
            result.Remove(new HexagonalPoint2D(X + 1, Y - 1));

#if DEBUG
            Ensure.Count(6, result);
#endif

            return result.ToArray();
        }

        private IEnumerable<HexagonalPoint2D> NeighboursIncludingThis()
        {
            for (int x = X - 1; x <= X + 1; ++x)
            {
                for (int y = Y - 1; y <= Y + 1; ++y)
                {
                    yield return new HexagonalPoint2D(x, y);
                }
            }
        }

        public HexagonalPoint2D Move(HexDirection dir)
        {
#pragma warning disable S1121 // Assignments should not be made from within sub-expressions
            return dir switch
            {
                HexDirection.w => this with { X = this.X - 1 },
                HexDirection.e => this with { X = this.X + 1 },
                HexDirection.nw => this with { Y = this.Y + 1 },
                HexDirection.se => this with { Y = this.Y - 1 },
                HexDirection.ne => this with { X = this.X + 1, Y = this.Y + 1 },
                HexDirection.sw => this with { X = this.X - 1, Y = this.Y - 1 },
                _ => throw new SolvingException()
            };
#pragma warning restore S1121 // Assignments should not be made from within sub-expressions
        }
    }
}
