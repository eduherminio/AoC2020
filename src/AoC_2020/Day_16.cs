using AoCHelper;
using FileParser;
using SheepTools;
using SheepTools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_16 : BaseDay
    {
        private readonly Input _input;

        public Day_16()
        {
            _input = ParseInput();
        }

        public override string Solve_1()
        {
            var errorRate = 0;
            foreach (var nearbyTicket in _input.NearbyTickets)
            {
                foreach (var fieldLength in nearbyTicket.FieldLengths)
                {
                    if (!_input.Rules.Any(rule => rule.restrictions.Any(restriction => fieldLength >= restriction.Min && fieldLength <= restriction.Max)))
                    {
                        errorRate += fieldLength;
                        break;
                    }
                }
            }

            return errorRate.ToString();
        }

        public override string Solve_2()
        {
            var validTickets = new List<Ticket>() { _input.MyTicket };

            foreach (var nearbyTicket in _input.NearbyTickets)
            {
                bool isValid = true;
                foreach (var fieldLength in nearbyTicket.FieldLengths)
                {
                    if (!_input.Rules.Any(rule => rule.restrictions.Any(restriction => fieldLength >= restriction.Min && fieldLength <= restriction.Max)))
                    {
                        isValid = false;
                        break;
                    }
                }
                if (isValid)
                {
                    validTickets.Add(nearbyTicket);
                }
            }

            //for (int index = 0; index < _input.MyTicket.FieldLengths.Count; ++index)
            //{
            //    var myTicketField = _input.MyTicket.FieldLengths[index];

            //    var validLengths = validTickets.Select(ticket => ticket.FieldLengths[index]);

            //    foreach (var rule in _input.Rules)
            //    {
            //        var candidates =
            //    }
            //    var rules = validLengths.Select(length =>
            //        _input.Rules.Where(r => r.restrictions.Any(r => r.))
            //}




            var fieldNameByCandidateIndex = new Dictionary<string, List<int>>();
            bool change = true;

            while (change)
            {
                change = false;

                //foreach (var rule in _input.Rules.Where(r => r.FieldName.StartsWith("departure")))
                foreach (var rule in _input.Rules.Where(r => !fieldNameByCandidateIndex.ContainsKey(r.FieldName)))
                {
                    var candidatePositions = new List<int>();

                    //foreach (var index in _input.MyTicket.FieldLengths.Where(index => !fieldNameByIndex.ContainsValue(index)))
                    for (int index = 0; index < _input.MyTicket.FieldLengths.Count; ++index)
                    {
                        //if(fieldNameByIndex.ContainsValue(r.FieldName))
                        //{
                        //    continue;
                        //}
                        var myTicketField = _input.MyTicket.FieldLengths[index];

                        if (!rule.restrictions.Any(r => myTicketField >= r.Min && myTicketField <= r.Max))
                        {
                            continue;
                        }

                        var validLengths = validTickets
                            .Select(ticket => ticket.FieldLengths[index])
                            .Count(validLength => rule.restrictions.Any(r => validLength >= r.Min && validLength <= r.Max));

                        if (validLengths == validTickets.Count)
                        {
                            candidatePositions.Add(index);
                        }
                    }

                    if (candidatePositions.Count > 0)
                    {
                        if (fieldNameByCandidateIndex.TryGetValue(rule.FieldName, out var existing))
                        {
                            fieldNameByCandidateIndex[rule.FieldName].AddRange(candidatePositions);
                        }
                        else
                        {
                            fieldNameByCandidateIndex.Add(rule.FieldName, candidatePositions);
                        }

                        change = true;
                    }

                    //if (candidatePositions.Count == 1)
                    //{
                    //    fieldNameByIndex.Add(rule.FieldName, candidatePositions);
                    //    change = true;
                    //}
                }
            }

            var fieldNameByIndex = new Dictionary<string, int>();
            change = true;

            while (change)
            {
                var nonResolvedRules = fieldNameByCandidateIndex
                    .Where(pair => !fieldNameByIndex.ContainsKey(pair.Key));

                // Single value
                while (change)
                {
                    change = false;
                    foreach (var confirmed in nonResolvedRules.Where(pair => pair.Value.Count == 1))
                    {
                        change = true;
                        var value = confirmed.Value.Single();
                        fieldNameByIndex.Add(confirmed.Key, value);

                        fieldNameByCandidateIndex.Where(pair => pair.Value.Contains(value))
                            .ForEach(pair => fieldNameByCandidateIndex[pair.Key].Remove(value));
                    }
                }

                change = false;
                // Unique value
                var uniqueIndexes = nonResolvedRules
                    .Where(pair => pair.Value.Any(v =>
                        fieldNameByCandidateIndex.Count(p => p.Value.Contains(v)) == 1))
                    .ToList();

                if (uniqueIndexes.Count > 0)
                {
                    foreach (var uniqueIndex in uniqueIndexes)
                    {
                        var uniqueValue = uniqueIndex.Value.Single(v => nonResolvedRules.Count(p => p.Value.Contains(v)) == 1);
                        fieldNameByIndex.Add(uniqueIndex.Key, uniqueValue);
                    }
                    change = true;
                }
            }

            return fieldNameByIndex
                .Where(pair => pair.Key.StartsWith("departure"))
                .Aggregate((double)1, (total, pair) => total * _input.MyTicket.FieldLengths[pair.Value])
                .ToString();
        }

        private Input ParseInput()
        {
            var groups = ParsedFile.ReadAllGroupsOfLines(InputFilePath);
            Ensure.Count(3, groups);

            var rules = new List<Rule>();
            foreach (var line in groups[0])
            {
                var split = line.Split(':', StringSplitOptions.TrimEntries);
                var fieldName = split[0];
                var rawRestrictions = split[1].Split("or", StringSplitOptions.TrimEntries);
                var restrictions = new List<(int, int)>();
                foreach (var rawRestriction in rawRestrictions)
                {
                    var pair = rawRestriction.Split('-');
                    restrictions.Add((int.Parse(pair[0]), int.Parse(pair[1])));
                }

                rules.Add(new Rule(fieldName, restrictions));
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

        private record Rule(string FieldName, ICollection<(int Min, int Max)> restrictions);

        private record Ticket(List<int> FieldLengths);

        private record Input(ICollection<Rule> Rules, Ticket MyTicket, ICollection<Ticket> NearbyTickets);
    }
}
