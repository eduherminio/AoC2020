using AoCHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_01 : BaseDay
    {
        private readonly ICollection<int> _input;

        public Day_01()
        {
            _input = File.ReadAllLines(InputFilePath).Select(i => int.Parse(i)).ToList();
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
            Dictionary<int, List<int>> existingGroups = new Dictionary<int, List<int>>();

            foreach (var input in _input)
            {
                if (input > 2020)
                {
                    continue;
                }

                var candidateGroups = existingGroups.Where(n => n.Key + input <= 2020 && n.Value.Count < 3);
                for (int i = 0; i < candidateGroups.Count(); ++i)
                {
                    var n = candidateGroups.ElementAt(i);

                    existingGroups[n.Key + input] = n.Value.Append(input).ToList();
                }

                existingGroups[input] = new[] { input }.ToList();
            }

            return existingGroups.First(p => p.Key == 2020 && p.Value.Count == 3)
                .Value.Aggregate(1, (total, n) => total * n)
                .ToString();
        }
    }
}
