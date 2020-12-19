using AoCHelper;
using FileParser;
using SheepTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC_2020
{
    public class Day_19 : BaseDay
    {
        private readonly List<string> _messages;
        private List<Rule> _rules;

        public Day_19()
        {
            (_rules, _messages) = ParseInput();
        }

        public override string Solve_1()
        {
            _rules = _rules
                .OrderBy(r => !r.Content.StartsWith("\""))
                .ThenBy(r => r.Content.Length).ToList();

            var readyRules = new Dictionary<int, Rule>();

            var index = 0;
            while (_rules.Count > 0)
            {
                var current = _rules[index];

                if (current.Content.StartsWith("\""))
                {
                    current.RegexExpression = new StringBuilder(current.Content.Replace("\"", string.Empty));
                    current.IsLiteral = true;
                    readyRules.Add(current.Id, current);
                    _rules.Remove(current);
                    continue;
                }

                if (current.RelatedRules is null)
                {
                    current.RelatedRules = new HashSet<int>(current.Content
                        .Split(' ', StringSplitOptions.TrimEntries)
                        .Where(str => str != "|")
                        .Select(int.Parse));
                }

                if (!current.RelatedRules.All(readyRules.ContainsKey))
                {
                    index = ++index % _rules.Count;
                    continue;
                }

                foreach (var item in current.Content.Split('|', StringSplitOptions.TrimEntries))
                {
                    var rules = item.Split(' ');

                    foreach (var rule in rules.Select(int.Parse))
                    {
                        var readyRule = readyRules[rule];

                        if (readyRule.IsLiteral)
                        {
                            current.RegexExpression.Append(readyRule.RegexExpression);
                        }
                        else
                        {
                            current.RegexExpression.Append("(?:");
                            current.RegexExpression.Append(readyRule.RegexExpression);
                            current.RegexExpression.Append(')');
                        }
                    }

                    current.RegexExpression.Append('|');
                }

                current.RegexExpression.Remove(current.RegexExpression.Length - 1, 1);
                readyRules.Add(current.Id, current);
                _rules.Remove(current);

                if (_rules.Count == 0)
                {
                    break;
                }

                index = ++index % _rules.Count;
            }

            var rule0 = readyRules[0];

            var regexPattern = "^" + rule0.RegexExpression.ToString() + "$";

            var regex = new Regex(regexPattern, RegexOptions.Compiled);

            return _messages
                .Count(line => regex.IsMatch(line))
                .ToString();
        }

        public override string Solve_2()
        {
            var solution = string.Empty;

            return solution;
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

            public HashSet<int>? RelatedRules { get; set; }

            public StringBuilder RegexExpression { get; set; }

            public Rule(int id, string content)
            {
                Id = id;
                Content = content;
                RegexExpression = new StringBuilder();
            }
        }
    }
}
