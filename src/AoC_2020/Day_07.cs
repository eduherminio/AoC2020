using AoCHelper;
using FastHashSet;
using SheepTools.Extensions;
using SheepTools.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC_2020
{
    public class Day_07 : BaseDay
    {
        private const string MyBagName = "shiny gold";
        private readonly Dictionary<string, Bag> _input;

        public Day_07()
        {
            _input = ParseInput();
        }

        public override string Solve_1()
        {
            return BagsThatCanContainABag(MyBagName).Count.ToString();

            FastHashSet<Bag> BagsThatCanContainABag(string bagId, FastHashSet<Bag>? result = null)
            {
                result ??= new FastHashSet<Bag>();

                foreach (var bag in _input.Values.Where(node => node.Children.Any(ch => ch.Key.Id == bagId)))
                {
                    result.Add(bag);
                    result.AddRange(BagsThatCanContainABag(bag.Id, result));
                }

                return result;
            }
        }

        public override string Solve_2()
        {
            return BagsContainedByABag(_input[MyBagName]).ToString();

            static long BagsContainedByABag(Bag bag)
            {
                long result = bag.Children.Sum(pair => pair.Value);

                foreach (var child in bag.Children)
                {
                    result += child.Value * BagsContainedByABag(child.Key);
                }

                return result;
            }
        }

        private readonly Regex _inputParsingRegex = new Regex(@"(?:[\d].*?) bag+", RegexOptions.Compiled);

        private Dictionary<string, Bag> ParseInput()
        {
            var result = new Dictionary<string, Bag>();

            foreach (var line in File.ReadAllLines(InputFilePath))
            {
                var split = line.Split("contain");

                var id = split[0].Replace("bags", "").Trim();
                if (!result.ContainsKey(id))
                {
                    result.Add(id, new Bag(id));
                }

                foreach (Match match in _inputParsingRegex.Matches(split[1]))
                {
                    var child = match.Value.Replace("bag", "").Trim();

                    if (!int.TryParse(child[0..child.IndexOf(' ')], out int numberOfBags))
                    {
                        throw new SolvingException();
                    }

                    var childId = child[(child.IndexOf(' ') + 1)..];
                    if (!result.TryGetValue(childId, out var existingChild))
                    {
                        existingChild = new Bag(childId);
                        result.Add(childId, existingChild);
                    }

                    result[id].Children.Add(existingChild, numberOfBags);
                }
            }

            return result;
        }

        private record Bag(string Id) : Node(Id)
        {
            public new Dictionary<Bag, int> Children { get; set; } = new Dictionary<Bag, int>();

            public virtual bool Equals(Bag? other) => base.Equals(other);

#pragma warning disable RCS1132 // Remove redundant overriding member.
            public override int GetHashCode() => base.GetHashCode();
#pragma warning restore RCS1132 // Remove redundant overriding member.
        }
    }
}
