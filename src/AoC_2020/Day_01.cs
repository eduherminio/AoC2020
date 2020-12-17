using AoC_2020.Algorithms;
using AoCHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_01 : BaseDay
    {
        private const int TwentyTwenty = 2020;
        private readonly List<long> _input;

        public Day_01()
        {
            _input = File.ReadAllLines(InputFilePath).Select(long.Parse).ToList();
        }

        public override string Solve_1()
        {
            return _input.PairOfNumbersThatSumN(2020)
                .Aggregate((long)1, (total, n) => total * n)
                .ToString();
        }

        public override string Solve_2() => Part2_Dictionary();

        internal string Part2_Dictionary()
        {
            return _input.MultipleNumbersThatSumN(2020, 3)
                .Aggregate((long)1, (total, n) => total * n)
                .ToString();
        }

        /// <summary>
        /// Over 2 x slower than <see cref="Part2_Dictionary"/>, but almost 0 memory allocation.
        /// </summary>
        /// <returns></returns>
        internal string Part2_NestedLoops()
        {
            for (int i = 0; i < _input.Count; ++i)
            {
                for (int j = i; j < _input.Count; ++j)
                {
                    for (int k = j; k < _input.Count; ++k)
                    {
                        if (_input[i] + _input[j] + _input[k] == TwentyTwenty)
                        {
                            return (_input[i] * _input[j] * _input[k]).ToString();
                        }
                    }
                }
            }
            throw new SolvingException();
        }

        /// <summary>
        /// Slighly faster than <see cref="Part2_NestedLoops"/> in exchange of allocating some (very little) memory.
        /// https://www.reddit.com/r/adventofcode/comments/k4e4lm/2020_day_1_solutions/ge95uga
        /// </summary>
        /// <returns></returns>
        internal string Part2_Linq()
        {
            return _input.Where(input1 =>
                 _input.Find(input2 => _input.Contains(TwentyTwenty - input1 - input2)) != default)
                 .Aggregate((long)1, (o, c) => o * c)
                 .ToString();
        }

        /// <summary>
        /// Slightly slower for part 1, significatively (> 500x) slower for part 2.
        /// Allocates a huge amount of memory.
        /// </summary>
        /// <param name="numberofItems"></param>
        /// <returns></returns>
        internal string Part2_Combinations(int numberofItems)
        {
            return _input.DifferentCombinations(numberofItems)
                .First(en => en.Sum() == TwentyTwenty)
                .Aggregate((long)1, (total, n) => total * n)
                .ToString();
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// https://stackoverflow.com/a/33336576/5459321
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> DifferentCombinations<T>(this IEnumerable<T> elements, int k)
        {
            return k == 0
                ? new[] { System.Array.Empty<T>() }
                : elements.SelectMany((e, i) =>
                    elements.Skip(i + 1).DifferentCombinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }
    }
}
