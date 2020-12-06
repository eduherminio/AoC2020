using AoCHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_06 : BaseDay
    {
        private readonly List<List<string>> _input;

        public Day_06()
        {
            _input = ParseInput();
        }

        public override string Solve_1()
        {
            return _input
                .Sum(group => group.SelectMany(str => str).Distinct().Count())
                .ToString();
        }

        public override string Solve_2() => Part2_Linq();

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
        /// ~ speed and allocations as Part2_Linq
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

        private List<List<string>> ParseInput()
        {
            var result = new List<List<string>>()
            {
                new List<string>()
            };

            var currentIndex = 0;

            foreach (var line in File.ReadAllLines(InputFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    result.Add(new List<string>());
                    ++currentIndex;
                    continue;
                }

                result[currentIndex].Add(line);
            }

            return result;
        }
    }
}
