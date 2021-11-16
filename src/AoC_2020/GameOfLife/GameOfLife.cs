using SheepTools.Extensions;

namespace AoC_2020.GameOfLife
{
    public class GameOfLife
    {
        private static readonly Func<int, bool> DefaultToDieCondition =
            (activeNeighboursCount) => activeNeighboursCount != 2 && activeNeighboursCount != 3;

        private static readonly Func<int, bool> DefaultToBornCondition =
            (activeNeighboursCount) => activeNeighboursCount == 3;

        private static readonly Func<int, bool> DefaultNumberOfCellsWhichHaveAGivenCellAsNeighbourCondition =
            (neighboursCount) => neighboursCount >= 3;

        private readonly Func<int, bool> _toDieCondition;
        private readonly Func<int, bool> _toBornCondition;
        private readonly Func<int, bool> _numberOfCellsWhichHaveAGivenNeighbourAsNeighbourCondition;

        private readonly Dictionary<Point, Point[]> _neighboursCache;

        public int Iterations { get; private set; }

        public HashSet<Point> AliveCells { get; }

        /// <summary>
        /// Default Game of Life rules.
        /// </summary>
        /// <param name="initialCells"></param>
        public GameOfLife(HashSet<Point> initialCells)
            : this(initialCells, DefaultToDieCondition, DefaultToBornCondition, DefaultNumberOfCellsWhichHaveAGivenCellAsNeighbourCondition)
        {
        }

        /// <summary>
        /// Custom Game of Life rules
        /// </summary>
        /// <param name="initialCells"></param>
        /// <param name="toDieCondition">Condition for an alive cell to die. Based on the number of alive neighbours</param>
        /// <param name="toBornCondition">Condition for a dead cell to become alive. Based on the number of alive neighbours</param>
        /// <param name="numberOfCellsWhichHaveAGivenNeighbourAsNeighbourCondition">
        /// Optimization condition that filters what neighbours to attempt to become alive.
        /// Based on the total number of alive cells that has a given neighbour as neighbour.
        /// i.e., if it's an isolated neighbour (only one of the alive cells has it a neighbour), we probably don't even want to consider it.
        /// </param>
        public GameOfLife(HashSet<Point> initialCells, Func<int, bool> toDieCondition, Func<int, bool> toBornCondition, Func<int, bool> numberOfCellsWhichHaveAGivenNeighbourAsNeighbourCondition)
        {
            _toDieCondition = toDieCondition;
            _toBornCondition = toBornCondition;
            _numberOfCellsWhichHaveAGivenNeighbourAsNeighbourCondition = numberOfCellsWhichHaveAGivenNeighbourAsNeighbourCondition;

            AliveCells = initialCells;
            Iterations = 0;
            _neighboursCache = new Dictionary<Point, Point[]>();
        }

        public void Mutate()
        {
            ++Iterations;

            var cellsToBorn = new HashSet<Point>(AliveCells.Count);
            var cellsToDie = new HashSet<Point>(AliveCells.Count);
            var neighboursToNumberOfAliveCellsFromWhichItIsANeighbour = new Dictionary<Point, int>(10 * AliveCells.Count);

            foreach (var cell in AliveCells)
            {
                var cellNeighbours = LocalMutateCell(cell, true, AliveCells, cellsToBorn, cellsToDie);

                cellNeighbours.ForEach(p =>
                {
                    if (neighboursToNumberOfAliveCellsFromWhichItIsANeighbour.TryGetValue(p.cell, out var value))
                    {
                        neighboursToNumberOfAliveCellsFromWhichItIsANeighbour[p.cell] = ++value;
                    }
                    else
                    {
                        neighboursToNumberOfAliveCellsFromWhichItIsANeighbour[p.cell] = 1;
                    }
                });
            }

            foreach (var neighbourPair in neighboursToNumberOfAliveCellsFromWhichItIsANeighbour.Where(pair =>
                _numberOfCellsWhichHaveAGivenNeighbourAsNeighbourCondition.Invoke(pair.Value)
                && !AliveCells.Contains(pair.Key)))
            {
                LocalMutateCell(neighbourPair.Key, false, AliveCells, cellsToBorn, cellsToDie);
            }

            cellsToBorn.ForEach(p => AliveCells.Add(p));
            cellsToDie.ForEach(p => AliveCells.Remove(p));

            IEnumerable<(Point cell, bool isAlive)> LocalMutateCell(Point cell, bool isAlive,
                HashSet<Point> aliveCells, HashSet<Point> cellsToBorn, HashSet<Point> cellsToDie)
            {
                var neighboursWithStatus = cell.Neighbours()
                    .Select(p => (cell: p, aliveCells.Contains(p)));

                MutateCell(cell, isAlive, neighboursWithStatus, cellsToBorn, cellsToDie);

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
                var cellNeighbours = LocalMutateCell(cell, true);

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
                LocalMutateCell(neighbourPair.Key, false);
            }

            cellsToBorn.ForEach(p => AliveCells.Add(p));
            cellsToDie.ForEach(p => AliveCells.Remove(p));

            IEnumerable<(Point cell, bool isAlive)> LocalMutateCell(Point cell, bool isAlive)
            {
                if (!_neighboursCache.TryGetValue(cell, out var cellNeighbours))
                {
                    cellNeighbours = cell.Neighbours();
                    _neighboursCache[cell] = cellNeighbours;
                }

                var neighboursWithStatus = cellNeighbours
                    .Select(p => (cell: p, AliveCells.Contains(p)));

                MutateCell(cell, isAlive, neighboursWithStatus, cellsToBorn, cellsToDie);

                return neighboursWithStatus;
            }
        }

        private void MutateCell(Point cell, bool isAlive, IEnumerable<(Point cell, bool isActive)> neighbours,
            HashSet<Point> toBorn, HashSet<Point> toDie)
        {
            var activeNeighboursCount = neighbours.Count(pair => pair.isActive);

            if (isAlive)
            {
                if (_toDieCondition.Invoke(activeNeighboursCount))
                {
                    toDie.Add(cell);
                }
            }
            else
            {
                if (_toBornCondition.Invoke(activeNeighboursCount))
                {
                    toBorn.Add(cell);
                }
            }
        }
    }
}
