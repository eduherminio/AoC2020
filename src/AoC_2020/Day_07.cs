using AoCHelper;
using FileParser;
using SheepTools.Extensions;
using SheepTools.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC_2020
{
    public class Day_07 : BaseDay
    {
        private readonly Dictionary<string, Node> _input;

        public Day_07()
        {
            _input = ParseInput();
        }

        public override string Solve_1()
        {
            return BagsThatCanContainABag("shiny gold")
                .Count
                .ToString();
        }

        private HashSet<Node> BagsThatCanContainABag(string bagId, HashSet<Node>? result = null)
        {
            if (result is null)
            {
                result = new HashSet<Node>();
            }

            foreach (var bag in _input.Values.Where(node => node.Children.Any(ch => ch.Id == bagId)))
            {
                result.Add(bag);

                result.AddRange(BagsThatCanContainABag(bag.Id, result));
            }

            return result;
        }

        public override string Solve_2()
        {
            var solution = string.Empty;

            return solution;
        }

        private readonly Regex InputParsingRegex = new Regex(@"(?:(?:[\d].*?)) bag+", RegexOptions.Compiled);

        private Dictionary<string, Node> ParseInput()
        {
            var result = new Dictionary<string, Node>();

            foreach (var line in File.ReadAllLines(InputFilePath))
            {
                var split = line.Split("contain");
                var id = split[0].Replace("bags", "").Trim();

                if (!result.ContainsKey(id))
                {
                    result.Add(id, new Node(id));
                }
                foreach (Match child in InputParsingRegex.Matches(split[1]))
                {
                    var childId = child.Value.Replace("bag", "").Trim();

                    var n = childId[0..childId.IndexOf(' ')];
                    childId = childId[(childId.IndexOf(' ') + 1)..];
                    if (!result.TryGetValue(childId, out var existingChild))
                    {
                        existingChild = new Node(childId);
                        result.Add(childId, existingChild);
                    }
                    result[id].Children.Add(existingChild);
                }
            }

            return result;
        }

    }
}
