using AoCHelper;
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
        private readonly List<(int X, int Y)> _input;

        public Day_17()
        {
            _input = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            Dictionary<Point, bool> state = new(_input.Select(p =>
                new KeyValuePair<Point, bool>(new Point3D(p.X, p.Y, 0), true)));

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
            Dictionary<Point, bool> state = new(_input.Select(p =>
                new KeyValuePair<Point, bool>(new Point4D(p.X, p.Y, 0, 0), true)));

            for (int cycle = 1; cycle <= 6; ++cycle)
            {
                Mutate(state);
            }

            return state.Count(pair => pair.Value).ToString();
        }

        private static void Mutate(Dictionary<Point, bool> state3D)
        {
            var pointsToActivate = new HashSet<Point>();
            var pointsToDeactivate = new HashSet<Point>();

            var visitedPoints = new HashSet<Point>();
            foreach (var pair in state3D)
            {
                if (visitedPoints.Contains(pair.Key))
                {
                    continue;
                }

                var neighbours = pair.Key.Neighbours().Select(p =>
                {
                    return state3D.TryGetValue(p, out bool isActive)
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
                        return state3D.TryGetValue(p, out bool isActive)
                            ? (point: p, isActive)
                            : (point: p, isActive: false);
                    });

                    MutatePoint(
                        new KeyValuePair<Point, bool>(neighbourPair.point, neighbourPair.isActive),
                        neighbourNeighbours);
                    if (!state3D.ContainsKey(neighbourPair.point))
                    {
                        visitedPoints.Add(neighbourPair.point);
                    }
                }
            }

            pointsToActivate.ForEach(p => state3D[p] = true);
            pointsToDeactivate.ForEach(p => state3D[p] = false);

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

        private static void Print(Dictionary<Point, bool> state)
        {
            if (state.First().Key is not Point3D)
            {
                return;
            }

            var state3D = new Dictionary<Point3D, bool>(state.Select(pair =>
                new KeyValuePair<Point3D, bool>((pair.Key as Point3D)!, pair.Value)));

            var minX = state3D.Min(pair => pair.Key.X);
            var maxX = state3D.Max(pair => pair.Key.X);
            var minY = state3D.Min(pair => pair.Key.Y);
            var maxY = state3D.Max(pair => pair.Key.Y);

            foreach (var group in state3D.GroupBy(pair => pair.Key.Z).OrderBy(g => g.Key))
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

        private IEnumerable<(int X, int Y)> ParseInput()
        {
            var lines = File.ReadAllLines(InputFilePath);
            for (int y = 0; y < lines.Length; ++y)
            {
                for (int x = 0; x < lines[y].Length; ++x)
                {
                    if (lines[y][x] == '#')
                    {
                        yield return (x, y);
                    }
                }
            }
        }

        private abstract record Point()
        {
            public abstract Point[] Neighbours();
        }

        private record Point3D(int X, int Y, int Z) : Point
        {
            public override Point3D[] Neighbours()
            {
                var result = NeighboursIncludingThis().ToList();
                result.Remove(this);

#if DEBUG
                Ensure.Count(26, result);
#endif

                return result.ToArray();
            }

            private IEnumerable<Point3D> NeighboursIncludingThis()
            {
                for (int x = X - 1; x <= X + 1; ++x)
                {
                    for (int y = Y - 1; y <= Y + 1; ++y)
                    {
                        for (int z = Z - 1; z <= Z + 1; ++z)
                        {
                            yield return new Point3D(x, y, z);
                        }
                    }
                }
            }

            public override string ToString() => $"[{X}, {Y}, {Z}]";
        }

        private record Point4D(int X, int Y, int Z, int W) : Point3D(X, Y, Z)
        {
            public override Point4D[] Neighbours()
            {
                var result = NeighboursIncludingThis().ToList();
                result.Remove(this);

#if DEBUG
                Ensure.Count(80, result);
#endif

                return result.ToArray();
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
