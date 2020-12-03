using AoCHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_03 : BaseDay
    {
        private readonly List<string> _stringListInput;
        private readonly char[,] _charMapInput;
        private readonly List<List<char>> _charListInput;

        public Day_03()
        {
            _stringListInput = File.ReadAllLines(InputFilePath).ToList();
            _charMapInput = ParseInputAsCharMap();
            _charListInput = ParseInputAsCharList();
        }

        public override string Solve_1()
        {
            return TransverseMap(
                new[] { (x: 3, y: 1) },
                ch => ch == '#')
                .Single().ToString();
        }

        public override string Solve_2() => Part2_UnidimensionalStringList();

        internal string Part2_UnidimensionalStringList()
        {
            var results = TransverseMap(
                new[] {
                    (x: 1, y: 1), (x: 3, y: 1), (x: 5, y: 1), (x: 7, y: 1),
                    (x : 1, y : 2)},
                ch => ch == '#');

            return results
                .Aggregate(1.0, (total, n) => total * n)
                .ToString();
        }

        internal IEnumerable<int> TransverseMap(ICollection<(int x, int y)> slopes, Func<char, bool> predicate)
        {
            var matches = new Dictionary<(int x, int y), int>(
                slopes.Select(slope => new KeyValuePair<(int x, int y), int>(slope, 0)));

            for (int level = 1; level < _stringListInput.Count; ++level)
            {
                var mapLine = _stringListInput[level];
                foreach (var slope in slopes)
                {
                    if (level % slope.y == 0)
                    {
                        var y = level / slope.y;
                        var x = (y * slope.x) % mapLine.Length;

                        if (predicate.Invoke(mapLine[x]))
                        {
                            ++matches[slope];
                        }
                    }
                }
            }

            return matches.Select(t => t.Value);
        }

        /// <summary>
        /// Exact same results as UnidimensionalStringList, not worth the effort
        /// </summary>
        /// <returns></returns>
        internal string Part2_MultidimensionalCharArray()
        {
            var results = LocalTransverseMap(
                new[] {
                    (x: 1, y: 1), (x: 3, y: 1), (x: 5, y: 1), (x: 7, y: 1),
                    (x : 1, y : 2)},
                ch => ch == '#');

            return results
                .Aggregate(1.0, (total, n) => total * n)
                .ToString();

            IEnumerable<int> LocalTransverseMap(ICollection<(int x, int y)> slopes, Func<char, bool> predicate)
            {
                var matches = new Dictionary<(int x, int y), int>(
                    slopes.Select(slope => new KeyValuePair<(int x, int y), int>(slope, 0)));

                var xLength = _charMapInput.GetLength(1);
                var yLength = _charMapInput.GetLength(0);
                for (int yIndex = 1; yIndex < yLength; ++yIndex)
                {
                    foreach (var slope in slopes)
                    {
                        if (yIndex % slope.y == 0)
                        {
                            var y = yIndex / slope.y;
                            var x = (y * slope.x) % xLength;
                            if (predicate.Invoke(_charMapInput[yIndex, x]))
                            {
                                ++matches[slope];
                            }
                        }
                    }
                }

                return matches.Select(t => t.Value);
            }
        }

        /// <summary>
        /// Exact same results as UnidimensionalStringList, not worth the effort
        /// </summary>
        /// <returns></returns>
        internal string Part2_MultidimensionalCharList()
        {
            var results = LocalTransverseMap(
                new[] {
                    (x: 1, y: 1), (x: 3, y: 1), (x: 5, y: 1), (x: 7, y: 1),
                    (x : 1, y : 2)},
                ch => ch == '#');

            return results
                .Aggregate(1.0, (total, n) => total * n)
                .ToString();

            IEnumerable<int> LocalTransverseMap(ICollection<(int x, int y)> slopes, Func<char, bool> predicate)
            {
                var matches = new Dictionary<(int x, int y), int>(
                    slopes.Select(slope => new KeyValuePair<(int x, int y), int>(slope, 0)));

                var xLength = _charListInput[0].Count;
                var yLength = _charListInput.Count;
                for (int yIndex = 1; yIndex < yLength; ++yIndex)
                {
                    foreach (var slope in slopes)
                    {
                        if (yIndex % slope.y == 0)
                        {
                            var y = yIndex / slope.y;
                            var x = (y * slope.x) % xLength;
                            if (predicate.Invoke(_charListInput[yIndex][x]))
                            {
                                ++matches[slope];
                            }
                        }
                    }
                }

                return matches.Select(t => t.Value);
            }
        }

        private char[,] ParseInputAsCharMap()
        {
            var lines = File.ReadAllLines(InputFilePath);

            var map = new char[lines.Length, lines[0].Length];
            for (int i = 0; i < lines.Length; ++i)
            {
                for (int j = 0; j < lines[0].Length; ++j)
                {
                    map[i, j] = lines[i][j];
                }
            }

            return map;
        }

        private List<List<char>> ParseInputAsCharList()
        {
            var lines = File.ReadAllLines(InputFilePath);

            var list = new List<List<char>>(lines.Length);
            for (int i = 0; i < lines.Length; ++i)
            {
                list.Add(new List<char>(lines[0].Length));
                for (int j = 0; j < lines[0].Length; ++j)
                {
                    list[i].Add(lines[i][j]);
                }
            }

            return list;
        }
    }
}
