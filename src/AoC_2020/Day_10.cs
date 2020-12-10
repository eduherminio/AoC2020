using AoCHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public override string Solve_1()
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

            return $"{ones * threes}";
        }

        /// <summary>
        /// https://eduherminio.github.io/blog/solving-aoc-2020-day-10/
        /// </summary>
        /// <returns></returns>
        public override string Solve_2()
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

            return totalNumberOfWays.ToString();
        }

        /// <summary>
        /// While iterating through the reversed input, we think of 'removing' optional adapters.
        /// While we are removing {current}:
        /// If both {current-1} and {current-2} have been removed, not all removals from {current-2} will be valid when removing {current}
        /// In our case, {current+1} and {current+2}, due to _input being inverted (ordered by descending)
        /// Example:
        ///      46 - 47 - 48 - 49 - 52
        ///      * We remove 48
        ///      * We remove 47 -> 48 was also removed but 49 not
        ///          -> else
        ///      * We remove 46 -> 47 and 48 were also removed
        ///          -> Those 47 'extra options' where 48 has been removed aren't valid
        ///          -> sum += dic[47].Value - dic[48].Value
        /// </summary>
        /// <returns></returns>
        public string Part2_Original()
        {
            int inputSize = _descendingInput.Count;
            _descendingInput.Add(0);

            ulong totalNumberOfWays = 1;

            // Number of new ways that can be achieved by removing an item
            var numberOfExtraOptionsWhenRemovingKey = new Dictionary<int, ulong>();

            var previous = _descendingInput.Max() + 3;
            int current;
            int next;

            bool isCurrentRemovable() => previous - next <= 3;

            for (int i = 0; i < inputSize; i++)
            {
                current = _descendingInput[i];
                next = _descendingInput[i + 1];

                if (isCurrentRemovable())    // Removing current
                {
                    ulong newWays = 1;

                    foreach (var pair in numberOfExtraOptionsWhenRemovingKey)
                    {
                        newWays += pair.Value;
                        if (pair.Key == current + 1 && numberOfExtraOptionsWhenRemovingKey.TryGetValue(pair.Key + 1, out var value))
                        {
                            newWays -= value;
                        }
                    }

                    totalNumberOfWays += newWays;
                    numberOfExtraOptionsWhenRemovingKey.Add(current, newWays);
                }

                previous = current;
            }

            return totalNumberOfWays.ToString();
        }

        /// <summary>
        /// While iterating through the reversed input, we think of the how adding an optional adapter duplicates the number of arrangements
        /// </summary>
        /// <returns></returns>
        public string Part2_SlightlyDifferentApproach()
        {
            int inputSize = _descendingInput.Count;
            _descendingInput.Add(0);

            ulong totalNumberOfWays = 0;

            // Number of new ways that can be achieved by removing an item
            var numberOfNewWaysByRemovableItem = new Dictionary<int, ulong>();

            int previous = _descendingInput.Max() + 3;
            int current;
            int next;

            bool isCurrentRemovable() => previous - next <= 3;

            for (int i = 0; i < inputSize; i++)
            {
                current = _descendingInput[i];
                next = _descendingInput[i + 1];

                if (isCurrentRemovable())
                {
                    ulong newWays = 1 + totalNumberOfWays;

                    if (numberOfNewWaysByRemovableItem.ContainsKey(current + 1)
                        && numberOfNewWaysByRemovableItem.TryGetValue(current + 2, out var duplicated))
                    {
                        newWays -= duplicated;
                    }

                    totalNumberOfWays += newWays;
                    numberOfNewWaysByRemovableItem.Add(current, newWays);
                }

                previous = current;
            }

            ++totalNumberOfWays;    // Add full sequence

            return totalNumberOfWays.ToString();
        }
    }
}
