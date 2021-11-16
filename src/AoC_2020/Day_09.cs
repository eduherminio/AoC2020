using AoC_2020.Algorithms;
using AoCHelper;

namespace AoC_2020
{
    public class Day_09 : BaseDay
    {
        private const int Preamble = 25;

        private readonly List<long> _input;

        private long _solutionPart1;

        public Day_09()
        {
            _input = File.ReadAllLines(InputFilePath).Select(long.Parse).ToList();
        }

        public override ValueTask<string> Solve_1()
        {
            _solutionPart1 = FirstWrongNumber(Preamble);
            return new(_solutionPart1.ToString());
        }

        public override ValueTask<string> Solve_2()
        {
            long sum = _solutionPart1 == default
                ? FirstWrongNumber(Preamble)
                : _solutionPart1;

            var set = _input.ContiguousNumbersThatSumN(sum);

            return new($"{set.Min() + set.Max()}");
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
