using AoCHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_03 : BaseDay
    {
        private readonly List<string> _map;

        public Day_03()
        {
            _map = File.ReadAllLines(InputFilePath).ToList();
        }

        public override string Solve_1()
        {
            return TransverseMap(
                new[] { (x: 3, y: 1) },
                ch => ch == '#')
                .Single().ToString();
        }

        public override string Solve_2()
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

        public IEnumerable<int> TransverseMap(ICollection<(int x, int y)> slopes, Func<char, bool> predicate)
        {
            var matches = new Dictionary<(int x, int y), int>(
                slopes.Select(slope => new KeyValuePair<(int x, int y), int>(slope, 0)));

            for (int level = 1; level < _map.Count; ++level)
            {
                var mapLine = _map[level];
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
    }
}
