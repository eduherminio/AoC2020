using AoCHelper;
using SheepTools.Model;
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
            var timesByVisitedPoint = new Dictionary<IntPoint, int>();

            foreach (var hexagonIndications in _input)
            {
                var hexagon = new IntPoint(0, 0);
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

    public static class IntPointExtensions
    {
        /// <summary>
        ///
        ///     y
        ///      \
        ///       \
        ///     \  \  \  \
        ///     -.——.——.——.——
        ///     \ \  \  \  \
        ///      ——.——O——.——.——   ----> x
        ///      \  \  \  \  \
        ///     ——.——.——.——.——.
        ///        \  \  \  \  \
        /// </summary>
        /// <param name="currentPoint"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static IntPoint Move(this IntPoint currentPoint, HexDirection dir)
        {
            return dir switch
            {
                HexDirection.w => currentPoint with { X = currentPoint.X - 1 },
                HexDirection.e => currentPoint with { X = currentPoint.X + 1 },
                HexDirection.nw => currentPoint with { Y = currentPoint.Y + 1 },
                HexDirection.se => currentPoint with { Y = currentPoint.Y - 1 },
                HexDirection.ne => currentPoint with { X = currentPoint.X + 1, Y = currentPoint.Y + 1 },
                HexDirection.sw => currentPoint with { X = currentPoint.X - 1, Y = currentPoint.Y - 1 },
                _ => throw new SolvingException()
            };
        }
    }
}
