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

        public Day_06()
        {
            _input = ParsedFile.ReadAllGroupsOfLines(InputFilePath);
        }

        public override string Solve_1()
        {
            return _input
                .Sum(group => group.SelectMany(str => str).Distinct().Count())
                .ToString();
        }

        public override string Solve_2() => Part2_IntersectAll();

        /// <summary>
        /// >2x faster than the original implementation
        /// </summary>
        /// <returns></returns>
        public string Part2_IntersectAll()
        {
            return _input.Sum(group => group.IntersectAll().Count).ToString();
        }

        /// <summary>
        /// Original implementation translated to Linq.
        /// ~ speed and allocations as Part2_Loop.
        /// </summary>
        /// <returns></returns>
        public string Part2_Linq()
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
        public string Part2_Loop()
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
    }
}
