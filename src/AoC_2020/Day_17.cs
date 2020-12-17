using AoCHelper;
using FileParser;
using SheepTools;
using SheepTools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_17 : BaseDay
    {
        private readonly List<Point> _input;

        public Day_17()
        {
            _input = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            Dictionary<Point, bool> state = new(_input.Select(p => new KeyValuePair<Point, bool>(p, true)));
            //Print(state);
            for (int cycle = 1; cycle <= 6; ++cycle)
            {
                Mutate(state);
                //Print(state);
            }

            return state.Count(pair => pair.Value).ToString();
        }

        public override string Solve_2()
        {
            Dictionary<Point4D, bool> state = new(_input.Select(p =>
                new KeyValuePair<Point4D, bool>(new Point4D(p.X, p.Y, 0, 0), true)));

            //Print(state);
            for (int cycle = 1; cycle <= 6; ++cycle)
            {
                Mutate(state);
                //Print(state);
            }

            return state.Count(pair => pair.Value).ToString();
        }

        private static void Mutate(Dictionary<Point, bool> state)
        {
            var pointsToActivate = new HashSet<Point>();
            var pointsToDeactivate = new HashSet<Point>();

            var visitedPoints = new HashSet<Point>();
            foreach (var pair in state)
            {
                if (visitedPoints.Contains(pair.Key))
                {
                    continue;
                }

                var neighbours = pair.Key.Neighbours().Select(p =>
                {
                    return state.TryGetValue(p, out bool isActive)
                        ? (point: p, isActive)
                        : (point: p, isActive: false);
                });

                MutatePoint(pair, neighbours);
                visitedPoints.Add(pair.Key);

                foreach (var neighbourPair in neighbours)
                {
                    if (visitedPoints.Contains(neighbourPair.point))
                    {
                        continue;
                    }

                    var neighbourNeighbours = neighbourPair.point.Neighbours().Select(p =>
                    {
                        return state.TryGetValue(p, out bool isActive)
                            ? (point: p, isActive)
                            : (point: p, isActive: false);
                    });

                    MutatePoint(
                        new KeyValuePair<Point, bool>(neighbourPair.point, neighbourPair.isActive),
                        neighbourNeighbours);
                    if (!state.ContainsKey(neighbourPair.point))
                    {
                        visitedPoints.Add(neighbourPair.point);
                    }
                }
            }

            pointsToActivate.ForEach(p => state[p] = true);
            pointsToDeactivate.ForEach(p => state[p] = false);

            void MutatePoint(KeyValuePair<Point, bool> pair, IEnumerable<(Point point, bool isActive)> neighbours)
            {
                var activeNeighboursCount = neighbours.Count(pair => pair.isActive);
                if (pair.Value)
                {
                    if (activeNeighboursCount != 2 && activeNeighboursCount != 3)
                    {
                        pointsToDeactivate.Add(pair.Key);
                    }
                }
                else
                {
                    if (activeNeighboursCount == 3)
                    {
                        pointsToActivate.Add(pair.Key);
                    }
                }
            }
        }

        private static void Mutate(Dictionary<Point4D, bool> state)
        {
            var pointsToActivate = new HashSet<Point4D>();
            var pointsToDeactivate = new HashSet<Point4D>();

            var visitedPoints = new HashSet<Point4D>();
            foreach (var pair in state)
            {
                if (visitedPoints.Contains(pair.Key))
                {
                    continue;
                }

                var neighbours = pair.Key.Neighbours().Select(p =>
                {
                    return state.TryGetValue(p, out bool isActive)
                        ? (point: p, isActive)
                        : (point: p, isActive: false);
                });

                MutatePoint(pair, neighbours);
                visitedPoints.Add(pair.Key);

                foreach (var neighbourPair in neighbours)
                {
                    if (visitedPoints.Contains(neighbourPair.point))
                    {
                        continue;
                    }

                    var neighbourNeighbours = neighbourPair.point.Neighbours().Select(p =>
                    {
                        return state.TryGetValue(p, out bool isActive)
                            ? (point: p, isActive)
                            : (point: p, isActive: false);
                    });

                    MutatePoint(
                        new KeyValuePair<Point4D, bool>(neighbourPair.point, neighbourPair.isActive),
                        neighbourNeighbours);
                    if (!state.ContainsKey(neighbourPair.point))
                    {
                        visitedPoints.Add(neighbourPair.point);
                    }
                }
            }

            pointsToActivate.ForEach(p => state[p] = true);
            pointsToDeactivate.ForEach(p => state[p] = false);

            void MutatePoint(KeyValuePair<Point4D, bool> pair, IEnumerable<(Point4D point, bool isActive)> neighbours)
            {
                var activeNeighboursCount = neighbours.Count(pair => pair.isActive);
                if (pair.Value)
                {
                    if (activeNeighboursCount != 2 && activeNeighboursCount != 3)
                    {
                        pointsToDeactivate.Add(pair.Key);
                    }
                }
                else
                {
                    if (activeNeighboursCount == 3)
                    {
                        pointsToActivate.Add(pair.Key);
                    }
                }
            }
        }

        private static void Print(Dictionary<Point, bool> state)
        {
            var minX = state.Min(pair => pair.Key.X);
            var maxX = state.Max(pair => pair.Key.X);
            var minY = state.Min(pair => pair.Key.Y);
            var maxY = state.Max(pair => pair.Key.Y);

            foreach (var group in state.GroupBy(pair => pair.Key.Z).OrderBy(g => g.Key))
            {
                Console.WriteLine();
                Console.WriteLine($"Z = {group.Key}");

                for (int y = minY; y <= maxY; ++y)
                {
                    for (int x = minX; x <= maxX; ++x)
                    {
                        var p = group.FirstOrDefault(p => p.Key.X == x && p.Key.Y == y);

                        Console.Write(p.Value ? '#' : '.');
                    }
                    Console.WriteLine();
                }
            }

            Console.WriteLine("--------------------------------------");
        }

        private IEnumerable<Point> ParseInput()
        {
            var lines = File.ReadAllLines(InputFilePath);
            for (int y = 0; y < lines.Length; ++y)
            {
                for (int x = 0; x < lines[y].Length; ++x)
                {
                    if (lines[y][x] == '#')
                    {
                        yield return new Point(x, y, 0);
                    }
                }
            }
        }

        private record Point(int X, int Y, int Z)
        {
            public ICollection<Point> Neighbours()
            {
                var result = NeighboursIncludingThis().ToList();
                result.Remove(this);

#if DEBUG
                Ensure.Count(26, result);
#endif

                return result;
            }

            private IEnumerable<Point> NeighboursIncludingThis()
            {
                for (int x = X - 1; x <= X + 1; ++x)
                {
                    for (int y = Y - 1; y <= Y + 1; ++y)
                    {
                        for (int z = Z - 1; z <= Z + 1; ++z)
                        {
                            yield return new Point(x, y, z);
                        }
                    }
                }
            }

            public override string ToString() => $"[{X}, {Y}, {Z}]";
        }

        private record Point4D(int X, int Y, int Z, int W)
        {
            public ICollection<Point4D> Neighbours()
            {
                var result = NeighboursIncludingThis().ToList();
                result.Remove(this);

#if DEBUG
                Ensure.Count(80, result);
#endif

                return result;
            }

            private IEnumerable<Point4D> NeighboursIncludingThis()
            {
                for (int x = X - 1; x <= X + 1; ++x)
                {
                    for (int y = Y - 1; y <= Y + 1; ++y)
                    {
                        for (int z = Z - 1; z <= Z + 1; ++z)
                        {
                            for (int w = W - 1; w <= W + 1; ++w)
                            {
                                yield return new Point4D(x, y, z, w);
                            }
                        }
                    }
                }
            }

            public override string ToString() => $"[{X}, {Y}, {Z}, {W}]";
        }
    }
}
