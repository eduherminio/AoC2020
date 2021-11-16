// https://eduherminio.github.io/blog/solving-aoc-2020-day-10/

using AoCHelper;

namespace AoC_2020
{
    public class Day_10 : BaseDay
    {
        private readonly List<int> _ascendingInput;
        private readonly List<int> _descendingInput;

        public Day_10()
        {
            _ascendingInput = File.ReadAllLines(InputFilePath).Select(int.Parse).OrderBy(_ => _).ToList();
            _descendingInput = File.ReadAllLines(InputFilePath).Select(int.Parse).OrderByDescending(_ => _).ToList();
        }

        public override ValueTask<string> Solve_1()
        {
            var ones = 0;
            var threes = 0;

            var previous = 0;
            for (int i = 0; i < _ascendingInput.Count; i++)
            {
                var current = _ascendingInput[i];

                if (current - previous == 1)
                {
                    ++ones;
                }
                else
                {
                    ++threes;
                }

                previous = current;
            }

            ++threes;   // Last connection is highest adapter + 3 jolts

            return new($"{ones * threes}");
        }

        public override ValueTask<string> Solve_2()
        {
            _descendingInput.Add(0);
            // No need to add the max, since we know its the highest adapter +3

            ulong totalNumberOfWays = 1;

            IEnumerable<int> GetOptionalAdapters()
            {
                for (int i = 1; i < _descendingInput.Count - 1; ++i)
                {
                    if (_descendingInput[i - 1] - _descendingInput[i + 1] <= 3)
                    {
                        yield return _descendingInput[i];
                    }
                }
            }

            var optionalParametersList = GetOptionalAdapters().ToList();

            foreach (var optionalAdapter in optionalParametersList)
            {
                totalNumberOfWays += optionalParametersList.Contains(optionalAdapter + 1) && optionalParametersList.Contains(optionalAdapter + 2)
                    ? 3 * totalNumberOfWays / 4
                    : totalNumberOfWays;
            }

            return new(totalNumberOfWays.ToString());
        }
    }
}
