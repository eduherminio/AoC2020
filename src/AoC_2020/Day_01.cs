using AoCHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_01 : BaseDay
    {
        private readonly List<int> _input;

        public Day_01()
        {
            _input = File.ReadAllLines(InputFilePath).Select(int.Parse).ToList();
        }

        public override string Solve_1()
        {
            var sum = new HashSet<int>();
            foreach (var input in _input)
            {
                if (input > 2020)
                {
                    continue;
                }

                foreach (var n in sum)
                {
                    if (n + input == 2020)
                    {
                        return (n * input).ToString();
                    }
                }
                sum.Add(input);
            }

            throw new SolvingException();
        }

        public override string Solve_2()
        {
            return DictionaryApproach();
        }

        public string DictionaryApproach()
        {
            Dictionary<int, List<int>> existingGroups = new Dictionary<int, List<int>>();

            foreach (var input in _input)
            {
                if (input > 2020)
                {
                    continue;
                }

                var candidateGroups = existingGroups.Where(n => n.Key + input <= 2020 && n.Value.Count < 3).ToList();
                for (int i = 0; i < candidateGroups.Count; ++i)
                {
                    var entry = candidateGroups[i];

                    if (entry.Value.Count == 2 && entry.Value.Sum() + input == 2020)
                    {
                        return (input * entry.Value[0] * entry.Value[1]).ToString();
                    }

                    existingGroups[entry.Key + input] = entry.Value.Append(input).ToList();
                }

                existingGroups[input] = new[] { input }.ToList();
            }

            throw new SolvingException();
        }

        /// <summary>
        /// Over 2 x slower than dictionary approach, but almost 0 memory allocation.
        /// </summary>
        /// <returns></returns>
        public string NestedLoopsApproach()
        {
            for (int i = 0; i < _input.Count; ++i)
            {
                for (int j = i; j < _input.Count; ++j)
                {
                    for (int k = j; k < _input.Count; ++k)
                    {
                        if (_input[i] + _input[j] + _input[k] == 2020)
                        {
                            return (_input[i] * _input[j] * _input[k]).ToString();
                        }
                    }
                }
            }
            throw new SolvingException();
        }

        /// <summary>
        /// Slightly slower for part 1, significatively (> 500x) slower for part 2.
        /// Allocates a huge amount of memory.
        /// </summary>
        /// <param name="numberofItems"></param>
        /// <returns></returns>
        public string CombinationsApproach(int numberofItems)
        {
            return _input.DifferentCombinations(numberofItems)
                .First(en => en.Sum() == 2020)
                .Aggregate(1, (total, n) => total * n)
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
