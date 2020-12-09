using System.Collections.Generic;
using System.Linq;

namespace AoC_2020.Algorithms
{
    public static class Numbers
    {
        public static IEnumerable<long> PairOfNumbersThatSumN(this IEnumerable<long> candidates, long total)
        {
            var savedNumberList = new HashSet<long>();

            foreach (var current in candidates)
            {
                if (current > total)
                {
                    continue;
                }

                foreach (var savedNumber in savedNumberList)
                {
                    if (savedNumber + current == total)
                    {
                        return new List<long> { savedNumber, current };
                    }
                }
                savedNumberList.Add(current);
            }

            return Enumerable.Empty<long>();
        }

        public static IEnumerable<long> MultipleNumbersThatSumN(this IEnumerable<long> candidates, long total, long numberOfAddends)
        {
            if (numberOfAddends == 2)
            {
                return PairOfNumbersThatSumN(candidates, total);
            }

            var existingGroups = new Dictionary<long, List<long>>();

            foreach (var current in candidates)
            {
                if (current > total)
                {
                    continue;
                }

                var candidateGroups = existingGroups.Where(n => n.Key + current <= total && n.Value.Count < 3).ToList();
                for (int i = 0; i < candidateGroups.Count; ++i)
                {
                    var entry = candidateGroups[i];

                    if (entry.Value.Count == numberOfAddends - 1 && entry.Value.Sum() + current == total)
                    {
                        return new List<long> { current, entry.Value[0], entry.Value[1] };
                    }

                    existingGroups[entry.Key + current] = entry.Value.Append(current).ToList();
                }

                existingGroups[current] = new[] { current }.ToList();
            }

            return Enumerable.Empty<long>();
        }

        public static IEnumerable<long> ContiguousNumbersThatSumN(this IEnumerable<long> candidates, long total)
        {
            var list = new List<long>();
            bool condition(long sum) => sum == total && list.Count > 1;

            foreach (var n in candidates)
            {
                list.Add(n);

                var sum = list.Sum();
                if (condition(sum))
                {
                    return list;
                }

                while (list.Count > 0 && sum > total)
                {
                    list.Remove(list[0]);

                    sum = list.Sum();
                    if (condition(sum))
                    {
                        return list;
                    }
                }
            }

            return Enumerable.Empty<long>();
        }
    }
}
