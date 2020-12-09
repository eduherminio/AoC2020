using AoCHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_09 : BaseDay
    {
        private readonly List<long> _input;

        public Day_09()
        {
            _input = File.ReadAllLines(InputFilePath).Select(long.Parse).ToList();
        }

        public override string Solve_1()
        {
            const int preamble = 25;

            return FirstWrongNumber(preamble).ToString();
        }

        public override string Solve_2()
        {
            long number = FirstWrongNumber(25);

            var set = FirstContiguousSetToSumN(number);

            return $"{set.Min() + set.Max()}";
        }

        private long FirstWrongNumber(int preamble)
        {
            for (int i = preamble; i < _input.Count; ++i)
            {
                if (!AnyPairSumsN(_input.GetRange(i - preamble, preamble), _input[i]))
                {
                    return _input[i];
                }
            }

            throw new SolvingException();

            // Algorithm used in Day 01_1
            static bool AnyPairSumsN(List<long> candidates, long total)
            {
                var sum = new HashSet<long>();

                foreach (var current in candidates)
                {
                    if (current > total)
                    {
                        continue;
                    }

                    foreach (var n in sum)
                    {
                        if (n + current == total)
                        {
                            return true;
                        }
                    }
                    sum.Add(current);
                }

                return false;
            }
        }

        private List<long> FirstContiguousSetToSumN(long number)
        {
            var set = new List<long>();
            bool condition(long sum) => sum == number && set.Count > 1;

            foreach (var n in _input)
            {
                set.Add(n);

                var sum = set.Sum();
                if (condition(sum))
                {
                    return set;
                }

                while (set.Count > 0 && sum > number)
                {
                    set.Remove(set[0]);

                    sum = set.Sum();
                    if (condition(sum))
                    {
                        return set;
                    }
                }
            }

            throw new SolvingException();
        }
    }
}
