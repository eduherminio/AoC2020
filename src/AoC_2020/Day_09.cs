using AoC_2020.Algorithms;
using AoCHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_09 : BaseDay
    {
        private const int Preamble = 25;

        private readonly List<long> _input;

        public Day_09()
        {
            _input = File.ReadAllLines(InputFilePath).Select(long.Parse).ToList();
        }

        public override string Solve_1()
        {
            return FirstWrongNumber(Preamble).ToString();
        }

        public override string Solve_2()
        {
            long number = FirstWrongNumber(Preamble);

            var set = _input.ContiguousNumbersThatSumN(number);

            return $"{set.Min() + set.Max()}";
        }

        private long FirstWrongNumber(int preamble)
        {
            for (int i = preamble; i < _input.Count; ++i)
            {
                if (!_input.GetRange(i - preamble, preamble).PairOfNumbersThatSumN(_input[i]).Any())
                {
                    return _input[i];
                }
            }

            throw new SolvingException();
        }
    }
}
