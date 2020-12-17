using AoC_2020.GameOfLife;
using AoCHelper;
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
            HashSet<Point> state = new(_input.Select(p => new Point3D(p.X, p.Y, 0)));

            var game = new GameOfLife.GameOfLife(state);

            //Print(game.AliveCells);
            while (game.Iterations < 6)
            {
                game.Mutate();
                //Print(game.AliveCells);
            }

            return game.AliveCells.Count.ToString();
        }

        public override string Solve_2() => Part2_GameOfLife();

        /// <summary>
        /// 50% faster than <see cref="Part2_Set"/>
        /// </summary>
        /// <returns></returns>
        public string Part2_GameOfLife()
        {
            HashSet<Point> state = new(_input.Select(p => new Point4D(p.X, p.Y, 0, 0)));

            var game = new GameOfLife.GameOfLife(state);

            while (game.Iterations < 6)
            {
                game.Mutate();
            }

            return game.AliveCells.Count.ToString();
        }

        /// <summary>
        /// Allocates 40% less memory than <see cref="Part2_GameOfLife"/>, but it's also 40% slower than it.
        /// </summary>
        /// <returns></returns>
        public string Part2_GameOfLife_CachingNeighbours()
        {
            HashSet<Point> state = new(_input.Select(p => new Point4D(p.X, p.Y, 0, 0)));

            var game = new GameOfLife.GameOfLife(state);

            while (game.Iterations < 6)
            {
                game.MutateCachingNeighbours();
            }

            return game.AliveCells.Count.ToString();
        }

        /// <summary>
        /// Original implementation
        /// </summary>
        /// <returns></returns>
        public string Part2_Dictionary()
        {
            Dictionary<Point, bool> state = new(_input.Select(p =>
                new KeyValuePair<Point, bool>(new Point4D(p.X, p.Y, 0, 0), true)));

            for (int cycle = 1; cycle <= 6; ++cycle)
            {
                Mutate_Dictionary(state);
            }

            return state.Count(pair => pair.Value).ToString();

            static void Mutate_Dictionary(Dictionary<Point, bool> state)
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
        }

        /// <summary>
        /// First improvement, 40% faster than <see cref="Part2_Dictionary"/>
        /// </summary>
        /// <returns></returns>
        public string Part2_Set()
        {
            HashSet<Point> state = new(_input.Select(p => new Point4D(p.X, p.Y, 0, 0)));

            for (int cycle = 1; cycle <= 6; ++cycle)
            {
                Mutate_Set(state);
            }

            return state.Count.ToString();


            static void Mutate_Set(HashSet<Point> state)
            {
                var pointsToActivate = new HashSet<Point>();
                var pointsToDeactivate = new HashSet<Point>();

                var visitedPoints = new HashSet<Point>();
                foreach (var point in state)
                {
                    if (visitedPoints.Contains(point))
                    {
                        continue;
                    }

                    var neighbours = point.Neighbours()
                        .Select(p => (point: p, isActive: state.Contains(p)));

                    MutatePoint((point, true), neighbours);
                    visitedPoints.Add(point);

                    foreach (var neighbourPair in neighbours)
                    {
                        if (visitedPoints.Contains(neighbourPair.point))
                        {
                            continue;
                        }

                        var neighbourNeighbours = neighbourPair.point.Neighbours()
                            .Select(p => (point: p, isActive: state.Contains(p)));

                        MutatePoint(
                            (neighbourPair.point, neighbourPair.isActive),
                            neighbourNeighbours);

                        if (!state.Contains(neighbourPair.point))
                        {
                            visitedPoints.Add(neighbourPair.point);
                        }
                    }
                }

                pointsToActivate.ForEach(p => state.Add(p));
                pointsToDeactivate.ForEach(p => state.Remove(p));

                void MutatePoint((Point Point, bool isActive) pair, IEnumerable<(Point point, bool isActive)> neighbours)
                {
                    var activeNeighboursCount = neighbours.Count(pair => pair.isActive);
                    if (pair.isActive)
                    {
                        if (activeNeighboursCount != 2 && activeNeighboursCount != 3)
                        {
                            pointsToDeactivate.Add(pair.Point);
                        }
                    }
                    else
                    {
                        if (activeNeighboursCount == 3)
                        {
                            pointsToActivate.Add(pair.Point);
                        }
                    }
                }
            }
        }

        private static void Print(HashSet<Point> state)
        {
            if (state.First() is not Point3D)
            {
                return;
            }

            var state3D = new HashSet<Point3D>(state.Select(point => point as Point3D)!);

            var minX = state3D.Min(point => point.X);
            var maxX = state3D.Max(point => point.X);
            var minY = state3D.Min(point => point.Y);
            var maxY = state3D.Max(point => point.Y);

            foreach (var group in state3D.GroupBy(point => point.Z).OrderBy(g => g.Key))
            {
                Console.WriteLine();
                Console.WriteLine($"Z = {group.Key}");

                for (int y = minY; y <= maxY; ++y)
                {
                    for (int x = minX; x <= maxX; ++x)
                    {
                        var p = group.FirstOrDefault(p => p.X == x && p.Y == y);

                        Console.Write(p is null ? '.' : '#');
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
    }
}
