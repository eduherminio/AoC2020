using AoC_2020.GameOfLife;
using AoCHelper;
using SheepTools;
using SheepTools.Extensions;
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

            var blackTiles = timesByVisitedPoint.Count(p => p.Value % 2 != 0);

            return blackTiles.ToString();
        }

        public override string Solve_2()
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

            var blackTiles = timesByVisitedPoint.Where(p => p.Value % 2 != 0);
            HashSet<Point> state = blackTiles.Select(p => p.Key).ToHashSet();

            var game = new GameOfLife24(state);

            while (game.Iterations < 100)
            {
                game.Mutate();
            }

            return game.AliveCells.Count.ToString();


            var solution = string.Empty;

            return solution;
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
        }
    }

    public class GameOfLife24
    {
        private readonly Dictionary<Point, Point[]> _neighboursCache;

        public int Iterations { get; private set; }

        public HashSet<Point> AliveCells { get; }

        public GameOfLife24(HashSet<Point> initialCells)
        {
            AliveCells = initialCells;
            Iterations = 0;
            _neighboursCache = new Dictionary<Point, Point[]>();
        }

        public void Mutate()
        {
            ++Iterations;

            var cellsToBorn = new HashSet<Point>(AliveCells.Count);
            var cellsToDie = new HashSet<Point>(AliveCells.Count);
            var neighbours = new Dictionary<Point, int>(10 * AliveCells.Count);

            foreach (var cell in AliveCells)
            {
                var cellNeighbours = MutateCell(cell, true, AliveCells, cellsToBorn, cellsToDie);

                cellNeighbours.ForEach(p =>
                {
                    if (neighbours.TryGetValue(p.cell, out var value))
                    {
                        neighbours[p.cell] = ++value;
                    }
                    else
                    {
                        neighbours[p.cell] = 1;
                    }
                });
            }

            foreach (var neighbourPair in neighbours.Where(pair => pair.Value >= 2 && !AliveCells.Contains(pair.Key)))
            {
                MutateCell(neighbourPair.Key, false, AliveCells, cellsToBorn, cellsToDie);
            }

            cellsToBorn.ForEach(p => AliveCells.Add(p));
            cellsToDie.ForEach(p => AliveCells.Remove(p));

            static IEnumerable<(Point cell, bool isAlive)> MutateCell(Point cell, bool isAlive,
                HashSet<Point> aliveCells, HashSet<Point> cellsToBorn, HashSet<Point> cellsToDie)
            {
                var neighboursWithStatus = cell.Neighbours()
                    .Select(p => (cell: p, aliveCells.Contains(p)));

                GameOfLife24.MutateCell(cell, isAlive, neighboursWithStatus, cellsToBorn, cellsToDie);

                return neighboursWithStatus;
            }
        }

        private static void MutateCell(Point cell, bool isAlive, IEnumerable<(Point cell, bool isActive)> neighbours,
            HashSet<Point> toBorn, HashSet<Point> toDie)
        {
            var activeNeighboursCount = neighbours.Count(pair => pair.isActive);

            if (isAlive)
            {
                if (activeNeighboursCount == 0 || activeNeighboursCount > 2)
                {
                    toDie.Add(cell);
                }
            }
            else
            {
                if (activeNeighboursCount == 2)
                {
                    toBorn.Add(cell);
                }
            }
        }
    }
}
