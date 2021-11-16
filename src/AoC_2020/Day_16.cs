using AoCHelper;
using FileParser;
using SheepTools;
using SheepTools.Extensions;

namespace AoC_2020
{
    public class Day_16 : BaseDay
    {
        private readonly Input _input;

        public Day_16()
        {
            _input = ParseInput();
        }

        public override ValueTask<string> Solve_1()
        {
            var errorRate = 0;
            foreach (var nearbyTicket in _input.NearbyTickets)
            {
                foreach (var fieldLength in nearbyTicket.FieldLengths)
                {
                    if (!_input.Rules.Any(rule => rule.Restrictions.Any(restriction => fieldLength >= restriction.Min && fieldLength <= restriction.Max)))
                    {
                        errorRate += fieldLength;
                        break;
                    }
                }
            }

            return new(errorRate.ToString());
        }

        public override ValueTask<string> Solve_2()
        {
            var validTickets = GetValidNearbyTickets().ToList();

            Dictionary<string, List<int>> candidateIndexesByFieldName = GetCandidates(validTickets);
            Dictionary<string, int> indexByFieldName = new Dictionary<string, int>();

            bool change = true;

            while (change)
            {
                var nonResolvedCandidates = candidateIndexesByFieldName
                    .Where(pair => !indexByFieldName.ContainsKey(pair.Key));

                // Assign as solution those candidates with a single possible index
                // and remove those indexes from all candidate pairs
                while (change)
                {
                    change = false;
                    foreach (var confirmed in nonResolvedCandidates.Where(pair => pair.Value.Count == 1))
                    {
                        change = true;

                        var value = confirmed.Value.Single();
                        indexByFieldName.Add(confirmed.Key, value);

                        nonResolvedCandidates.Where(pair => pair.Value.Contains(value))
                            .ForEach(pair => candidateIndexesByFieldName[pair.Key].Remove(value));
                    }
                }

                change = false;

                // Assign as solution those candidates that have a possible index which nobody else has
                var uniqueIndexes = nonResolvedCandidates
                    .Where(pair => pair.Value.Any(v =>
                        nonResolvedCandidates.Count(p => p.Value.Contains(v)) == 1))
                    .ToList();

                if (uniqueIndexes.Count > 0)
                {
                    foreach (var uniqueIndex in uniqueIndexes)
                    {
                        var uniqueValue = uniqueIndex.Value.Single(v => nonResolvedCandidates.Count(p => p.Value.Contains(v)) == 1);
                        indexByFieldName.Add(uniqueIndex.Key, uniqueValue);
                    }

                    change = true;
                }
            }

            return new(indexByFieldName
                .Where(pair => pair.Key.StartsWith("departure"))
                .Aggregate((double)1, (total, pair) => total * _input.MyTicket.FieldLengths[pair.Value])
                .ToString());
        }

        private IEnumerable<Ticket> GetValidNearbyTickets()
        {
            foreach (var nearbyTicket in _input.NearbyTickets)
            {
                bool isValid = true;
                foreach (var fieldLength in nearbyTicket.FieldLengths)
                {
                    if (!_input.Rules.Any(rule => rule.Restrictions.Any(restriction => fieldLength >= restriction.Min && fieldLength <= restriction.Max)))
                    {
                        isValid = false;
                        break;
                    }
                }
                if (isValid)
                {
                    yield return nearbyTicket;
                }
            }

            yield return _input.MyTicket;
        }

        private Dictionary<string, List<int>> GetCandidates(List<Ticket> validTickets)
        {
            var fieldNameByCandidateIndex = new Dictionary<string, List<int>>();

            foreach (var rule in _input.Rules)
            {
                var candidatePositions = new List<int>();

                for (int index = 0; index < _input.MyTicket.FieldLengths.Count; ++index)
                {
                    if (validTickets
                         .Select(ticket => ticket.FieldLengths[index])
                         .All(validLength => rule.Restrictions.Any(r => validLength >= r.Min && validLength <= r.Max)))
                    {
                        candidatePositions.Add(index);
                    }
                }

                if (candidatePositions.Count > 0)
                {
                    fieldNameByCandidateIndex.Add(rule.FieldName, candidatePositions);
                }
            }

            return fieldNameByCandidateIndex;
        }

        private Input ParseInput()
        {
            var groups = ParsedFile.ReadAllGroupsOfLines(InputFilePath);
            Ensure.Count(3, groups);

            var rules = new List<Rule>();
            foreach (var line in groups[0])
            {
                var split = line.Split(':', StringSplitOptions.TrimEntries);
                var restrictions = split[1]
                    .Split("or", StringSplitOptions.TrimEntries)
                    .Select(restrictions =>
                        {
                            var pair = restrictions.Split('-');
                            Ensure.Count(2, pair);
                            return (int.Parse(pair[0]), int.Parse(pair[1]));
                        })
                    .ToList();

                rules.Add(new Rule(split[0], restrictions));
            }

            var myTicket = new Ticket(ParseTickets(groups[1]).Single().ToList());

            var nearbyTickets = ParseTickets(groups[2]).ConvertAll(enumerable => new Ticket(enumerable.ToList()));

            return new Input(rules, myTicket, nearbyTickets);

            static List<IEnumerable<int>> ParseTickets(List<string> group)
            {
                return group
                    .Skip(1)
                    .Select(line => line.Split(',', StringSplitOptions.TrimEntries).Select(int.Parse))
                    .ToList();
            }
        }

        private record Rule(string FieldName, ICollection<(int Min, int Max)> Restrictions);

        private record Ticket(List<int> FieldLengths);

        private record Input(ICollection<Rule> Rules, Ticket MyTicket, ICollection<Ticket> NearbyTickets);
    }
}
