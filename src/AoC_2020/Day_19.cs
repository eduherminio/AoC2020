using AoCHelper;
using FileParser;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC_2020
{
    public class Day_19 : BaseDay
    {
        public override string Solve_1()
        {
            (List<Rule> rules, List<string> messages) = ParseInput();

            rules = rules
                .OrderBy(r => !r.IsLiteral)
                .ThenBy(r => r.Content.Length).ToList();

            Dictionary<int, Rule> readyRules = ReplaceNestedRules(rules);

            var rule0 = readyRules[0];

            var regexPattern = "^" + rule0.RegexExpression.ToString() + "$";

            var regex = new Regex(regexPattern, RegexOptions.Compiled);

            return messages
                .Count(line => regex.IsMatch(line))
                .ToString();
        }

        public override string Solve_2()
        {
            (List<Rule> rules, List<string> messages) = ParseInput();

            var rule8 = rules.First(r => r.Id == 8);
            rule8.Content = "42 | 42 8";

            var rule11 = rules.First(r => r.Id == 11);
            rule11.Content = "42 31 | 42 11 31";

            IEnumerable<Regex> regexes = GenerateCombinations(rule8, rule11, rules);

            var matches = new ConcurrentDictionary<string, object?>();  // No concurrent set :(

            Parallel.ForEach(regexes, (regex) =>
            {
                Parallel.ForEach(messages, (message) =>
                {
                    if (regex.IsMatch(message))
                    {
                        matches.TryAdd(message, null);
                    }
                });
            });

            return matches.Count.ToString();
        }

        private static Dictionary<int, Rule> ReplaceNestedRules(List<Rule> originalRules)
        {
            var replacedRules = new Dictionary<int, Rule>();

            var index = 0;
            while (originalRules.Count > 0)
            {
                var current = originalRules[index];

                if (current.IsLiteral)
                {
                    current.RegexExpression = new StringBuilder(current.Content);
                    replacedRules.Add(current.Id, current);
                    originalRules.Remove(current);
                    continue;
                }

                if (!current.RelatedRules.All(replacedRules.ContainsKey))
                {
                    index = ++index % originalRules.Count;
                    continue;
                }

                foreach (var item in current.Content.Split('|', StringSplitOptions.TrimEntries))
                {
                    var rules = item.Split(' ');

                    foreach (var rule in rules.Select(int.Parse))
                    {
                        var alreadyReplacedRule = replacedRules[rule];

                        if (alreadyReplacedRule.IsLiteral)
                        {
                            current.RegexExpression.Append(alreadyReplacedRule.RegexExpression);
                        }
                        else
                        {
                            current.RegexExpression.Append("(?:");
                            current.RegexExpression.Append(alreadyReplacedRule.RegexExpression);
                            current.RegexExpression.Append(')');
                        }
                    }

                    current.RegexExpression.Append('|');
                }

                current.RegexExpression.Remove(current.RegexExpression.Length - 1, 1);
                replacedRules.Add(current.Id, current);
                originalRules.Remove(current);

                if (originalRules.Count == 0)
                {
                    break;
                }

                index = ++index % originalRules.Count;
            }

            return replacedRules;
        }

        private static IEnumerable<Regex> GenerateCombinations(Rule rule8, Rule rule11, List<Rule> rules)
        {
            var rule0 = rules.First(r => r.Id == 0);

            var readyRules = ReplaceNestedRules(rules.Except(new[] { rule0, rule8, rule11 }).ToList());
            var rule42 = readyRules[42];
            var rule31 = readyRules[31];

            var patterns = new HashSet<string>();

            for (int i42 = 2; i42 <= 10; ++i42)
            {
                for (int i31 = 1; i31 < i42; ++i31)
                {
                    var sb = new StringBuilder("^");

                    for (int i = 0; i < i42; ++i)
                    {
                        sb.Append("(?:").Append(rule42.RegexExpression).Append(')');
                    }

                    for (int i = 0; i < i31; ++i)
                    {
                        sb.Append("(?:").Append(rule31.RegexExpression).Append(')');
                    }

                    sb.Append('$');

                    patterns.Add(sb.ToString());
                }
            }

            return patterns.Select(pattern => new Regex(pattern, RegexOptions.Compiled)).ToList();
        }

        private (List<Rule> rules, List<string> messages) ParseInput()
        {
            var rules = new List<Rule>();
            var messages = new List<string>();

            var groups = ParsedFile.ReadAllGroupsOfLines(InputFilePath);
            Debug.Assert(groups.Count == 2);

            foreach (var rule in groups[0])
            {
                var split = rule.Split(":", StringSplitOptions.TrimEntries);
                Debug.Assert(split.Length == 2);

                rules.Add(new Rule(int.Parse(split[0]), split[1]));
            }

            messages.AddRange(groups[1]);

            return (rules, messages);
        }

        public class Rule
        {
            public int Id { get; }

            public string Content { get; set; }

            public bool IsLiteral { get; set; }

            public HashSet<int> RelatedRules { get; }

            public StringBuilder RegexExpression { get; set; }

            public Rule(int id, string content)
            {
                Id = id;
                IsLiteral = content.StartsWith("\"");
                Content = content.Replace("\"", string.Empty);
                RegexExpression = new StringBuilder();

                RelatedRules = IsLiteral
                    ? new HashSet<int>(0)
                    : new HashSet<int>(Content
                            .Split(' ', StringSplitOptions.TrimEntries)
                            .Where(str => str != "|")
                            .Select(int.Parse));
            }
        }
    }
}
