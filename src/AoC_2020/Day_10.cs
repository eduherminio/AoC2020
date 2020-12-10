#pragma warning disable S125 // Sections of code should not be commented out - They're not code sonar :( just explanations

using AoCHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_10 : BaseDay
    {
        private readonly List<int> _input;

        public Day_10()
        {
            _input = File.ReadAllLines(InputFilePath).Select(int.Parse).OrderBy(_ => _).ToList();
        }

        public override string Solve_1()
        {
            var ones = 0;
            var threes = 0;

            var previous = 0;
            for (int i = 0; i < _input.Count; i++)
            {
                var current = _input[i];

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

            // Note: Previous has the last value of current, our bigger adapter
            if (previous + 1 >= _input.Max() + 3)
            {
                ++ones;
            }
            else
            {
                ++threes;
            }

            return $"{ones * threes}";
        }

        public override string Solve_2()
        {
            _input.Reverse();

            ulong total = 1;
            var numberOfExtraOptionsWhenRemovingKey = new Dictionary<int, ulong>();

            var previous = _input.Max() + 3;

            for (int i = 0; i < _input.Count; i++)
            {
                var current = _input[i];
                var next = i < _input.Count - 1
                    ? _input[i + 1]
                    : 0;

                if (previous - current == 1 && previous - next <= 3)    // Removing current
                {
                    ulong sum = 1;

                    foreach (var pair in numberOfExtraOptionsWhenRemovingKey)
                    {
                        // While we are removing {current}:
                        // If both {current-1} and {current-2} have been removed, not all removals from {current-1} will be valid when removing {current}
                        // In our case, {current+1} and {current+2}, due to _input being inverted (ordered by descending)
                        // Example:
                        //      46 - 47 - 48 - 49 - 52
                        //      * We remove 48
                        //      * We remove 47 -> 48 was also removed but 49 not
                        //          -> else
                        //      * We remove 46 -> 47 and 48 were also removed
                        //          -> Those 47 'extra options' where 48 has been removed aren't valid
                        //          -> sum += dic[47].Value - dic[48].Value
                        if (pair.Key == current + 1 && numberOfExtraOptionsWhenRemovingKey.TryGetValue(pair.Key + 1, out var value))
                        {
                            sum += pair.Value - value;
                        }
                        else
                        {
                            sum += pair.Value;
                        }
                    }

                    total += sum;
                    numberOfExtraOptionsWhenRemovingKey.Add(current, sum);
                }

                previous = current;
            }

            return total.ToString();
        }
    }
}

#pragma warning restore S125