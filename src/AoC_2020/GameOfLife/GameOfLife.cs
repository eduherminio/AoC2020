using SheepTools.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AoC_2020.GameOfLife
{
    public class GameOfLife
    {
        private readonly Dictionary<Point, Point[]> _neighboursCache;

        public int Iterations { get; private set; }

        public HashSet<Point> AliveCells { get; }

        public GameOfLife(HashSet<Point> initialCells)
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

            foreach (var neighbourPair in neighbours.Where(pair => pair.Value >= 3 && !AliveCells.Contains(pair.Key)))
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

                GameOfLife.MutateCell(cell, isAlive, neighboursWithStatus, cellsToBorn, cellsToDie);

                return neighboursWithStatus;
            }
        }

        /// <summary>
        /// For Problem 17, this allocates 40% less memory than <see cref="Mutate"/>, but it's also 40% slower than it.
        /// </summary>
        public void MutateCachingNeighbours()
        {
            ++Iterations;

            var cellsToBorn = new HashSet<Point>(AliveCells.Count);
            var cellsToDie = new HashSet<Point>(AliveCells.Count);
            var neighbours = new Dictionary<Point, int>(10 * AliveCells.Count);

            foreach (var cell in AliveCells)
            {
                var cellNeighbours = MutateCell(cell, true);

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

            foreach (var neighbourPair in neighbours.Where(pair => pair.Value >= 3 && !AliveCells.Contains(pair.Key)))
            {
                MutateCell(neighbourPair.Key, false);
            }

            cellsToBorn.ForEach(p => AliveCells.Add(p));
            cellsToDie.ForEach(p => AliveCells.Remove(p));

            IEnumerable<(Point cell, bool isAlive)> MutateCell(Point cell, bool isAlive)
            {
                if (!_neighboursCache.TryGetValue(cell, out var cellNeighbours))
                {
                    cellNeighbours = cell.Neighbours();
                    _neighboursCache[cell] = cellNeighbours;
                }

                var neighboursWithStatus = cellNeighbours
                    .Select(p => (cell: p, AliveCells.Contains(p)));

                GameOfLife.MutateCell(cell, isAlive, neighboursWithStatus, cellsToBorn, cellsToDie);

                return neighboursWithStatus;
            }
        }

        private static void MutateCell(Point cell, bool isAlive, IEnumerable<(Point cell, bool isActive)> neighbours,
            HashSet<Point> toBorn, HashSet<Point> toDie)
        {
            var activeNeighboursCount = neighbours.Count(pair => pair.isActive);

            if (isAlive)
            {
                if (activeNeighboursCount != 2 && activeNeighboursCount != 3)
                {
                    toDie.Add(cell);
                }
            }
            else
            {
                if (activeNeighboursCount == 3)
                {
                    toBorn.Add(cell);
                }
            }
        }
    }
}
