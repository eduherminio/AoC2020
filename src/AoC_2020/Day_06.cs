using AoCHelper;
using FileParser;
using SheepTools.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AoC_2020
{
    public class Day_06 : BaseDay
    {
        private readonly List<List<string>> _input;
        private readonly List<List<int>> _inputForBinarySolution;

        public Day_06()
        {
            _input = ParsedFile.ReadAllGroupsOfLines(InputFilePath);
            _inputForBinarySolution = _input.ConvertAll(group => group.ConvertAll(LineToBinary));
        }

        public override string Solve_1() => Part1_Binary();

        public override string Solve_2() => Part2_Binary();

        /// <summary>
        /// Original implementation
        /// </summary>
        /// <returns></returns>
        internal string Part1_Linq()
        {
            return _input
                .Sum(group => group.SelectMany(str => str).Distinct().Count())
                .ToString();
        }

        /// <summary>
        /// >2x faster than the original implementation
        /// </summary>
        /// <returns></returns>
        internal string Part2_IntersectAll()
        {
            return _input.Sum(group => group.IntersectAll().Count).ToString();
        }

        /// <summary>
        /// Original implementation translated to Linq.
        /// ~ speed and allocations as Part2_Loop.
        /// </summary>
        /// <returns></returns>
        internal string Part2_Linq()
        {
            return _input.Sum(group =>
            {
                return group
                 .SelectMany(str => str)
                 .Distinct()
                 .Count(ch => group.All(str => str.Contains(ch)));
            }).ToString();
        }

        /// <summary>
        /// Original implementation.
        /// ~ speed and allocations as Part2_Linq.
        /// </summary>
        /// <returns></returns>
        internal string Part2_Loop()
        {
            var result = 0;

            foreach (var group in _input)
            {
                foreach (var ch in group.SelectMany(str => str).Distinct())
                {
                    if (group.All(g => g.Contains(ch)))
                    {
                        ++result;
                    }
                }
            }

            return result.ToString();
        }

        #region Binary solution

        /// <summary>
        /// Let's explain the magic here:
        /// * Converts each char to a number N from 0 to 27                 ---->   (ch - 'a')                      4
        /// * Creates a binary string S of length N with a leading '1'      ---->   1 << N                      10000
        /// * Uses | to add that new '1' to the existing binary string      ---->   binary | S          XXXXXXXX1XXXX
        /// The result is a max. 25 char length binary string (~34_000_000, fits in an int)
        /// Source: https://github.com/mariomka/AdventOfCode2020/blob/master/day6/src/lib.rs
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static int LineToBinary(string str)
        {
            return str.Aggregate(0, (binary, ch) => binary | 1 << (ch - 'a'));
        }

        /// <summary>
        /// 2x faster thatn Part1_Linq
        /// Source: https://github.com/mariomka/AdventOfCode2020/blob/master/day6/src/lib.rs
        /// </summary>
        /// <returns></returns>
        internal string Part1_Binary()
        {
            return _inputForBinarySolution
                .Sum(lines =>
                    System.Convert.ToString(
                        lines.Aggregate(0, (total, bin) => total | bin),
                        2)
                    .Count(ch => ch == '1'))
                .ToString();
        }

        /// <summary>
        /// > 2x faster thatn Part2_Linq
        /// Important to notice int.MaxValue as aggregator seed
        /// Source: https://github.com/mariomka/AdventOfCode2020/blob/master/day6/src/lib.rs
        /// </summary>
        /// <returns></returns>
        internal string Part2_Binary()
        {
            return _inputForBinarySolution
                .Sum(lines =>
                    System.Convert.ToString(
                        lines.Aggregate(int.MaxValue, (total, bin) => total & bin),
                        2)
                    .Count(ch => ch == '1'))
                .ToString();
        }

        #endregion
    }
}
