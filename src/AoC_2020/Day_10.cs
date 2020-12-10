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

            var dic = new Dictionary<int, ulong>();
            ulong total = 1;

            var previous = _input.Max() + 3;

            for (int i = 0; i < _input.Count; i++)
            {
                var current = _input[i];
                var next = i < _input.Count - 1
                    ? _input[i + 1]
                    : 0;

                if (previous - current == 1 && previous - next <= 3)
                {
                    ulong sum = 1;

                    foreach (var pair in dic)
                    {
                        // If the previous one has been removed, and the previous-previous one as well,
                        // not all removals from the previous one will be valid
                        // Example:
                        //      46 - 47 - 48 - 49 - 52
                        //      * We remove 48
                        //      * We remove 47 -> 48 was also removed but 49 not
                        //          -> else
                        //      * We remove 46 -> 47 and 48 were also removed
                        //          -> 47 'options' where 48 has been removed aren't valid
                        //          -> sum += dic[47].Value - dic[48].Value
                        if (pair.Key == current + 1 && dic.TryGetValue(pair.Key + 1, out var value))
                        {
                            sum += pair.Value - value;
                        }
                        else
                        {
                            sum += pair.Value;
                        }
                    }

                    total += sum;
                    dic.Add(current, sum);
                }

                previous = current;
            }

            return total.ToString();
        }
    }
}
