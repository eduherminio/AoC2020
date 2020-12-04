using AoCHelper;
using FileParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC_2020
{
    public class Day_04 : BaseDay
    {
        //private readonly List<> _input;
        private readonly string _rawInput;
        private readonly List<string> _inputLines;
        private readonly List<int> _inputLinesInt;

        public Day_04()
        {
            _rawInput = ParseInputAsText();
            _inputLines = ParseInputAsLines();
            //_inputLinesInt = _inputLines.Select(i => int.Parse(i)).ToList();
        }

        public override string Solve_1()
        {
            var groups = new List<HashSet<string>>()
            {
                new HashSet<string>()
            };

            int index = 0;
            foreach (var line in _inputLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    groups.Add(new HashSet<string>());
                    ++index;
                }

                foreach (var word in line.Split(' '))
                {
                    groups[index].Add(word.Split(':')[0]);
                }
            }


            return groups.Count(hash =>
            {
                return hash.Contains("byr")
                    && hash.Contains("iyr")
                    && hash.Contains("eyr")
                    && hash.Contains("hgt")
                    && hash.Contains("hcl")
                    && hash.Contains("ecl")
                    && hash.Contains("pid");

            }).ToString();
        }

        public override string Solve_2()
        {
            var groups = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>()
            };

            int index = 0;
            foreach (var line in _inputLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    groups.Add(new Dictionary<string, string>());
                    ++index;
                    continue;
                }

                foreach (var word in line.Split(' '))
                {
                    var pair = word.Split(':');
                    groups[index][pair[0]] = pair[1];
                }
            }
            var hclRegex = new Regex("^#[a-f0-9]{6}$");
            var pidRegex = new Regex("^[0-9]{9}$");
            var eclPos = new List<string> { "amb", "blu", "brn", "gry", "grn", "hzl", "oth" };

            return groups.Count(hash =>
            {
                if (!hash.TryGetValue("byr", out var byrStr)
                    || !int.TryParse(byrStr, out var byr)
                     || byr < 190 || byr > 2002)
                {
                    return false;
                }

                if (!hash.TryGetValue("iyr", out var iyrStr)
                    || !int.TryParse(iyrStr, out var iyr)
                    || iyr < 2010 || iyr > 2020)
                {
                    return false;
                }

                if (!hash.TryGetValue("eyr", out var eyrStr)
                    || !int.TryParse(eyrStr, out var eyr)
                    || eyr < 2020 || eyr > 2030)
                {
                    return false;
                }

                if (!hash.TryGetValue("hgt", out var hgtStr)
                    || (!hgtStr.EndsWith("cm")
                    && !hgtStr.EndsWith("in")))
                {
                    return false;
                }

                if (hgtStr.EndsWith("in"))
                {
                    var hgt = int.Parse(hgtStr[..^2]);
                    if (hgt < 59 || hgt > 76)
                    {
                        return false;
                    }
                }

                if (hgtStr.EndsWith("cm"))
                {
                    var hgt = int.Parse(hgtStr[..^2]);
                    if (hgt < 150 || hgt > 193)
                    {
                        return false;
                    }
                }

                if (!hash.TryGetValue("hcl", out var hclStr) || !(hclRegex.IsMatch(hclStr)))
                {
                    return false;
                }


                if (!hash.TryGetValue("ecl", out var eclStr) || !eclPos.Contains(eclStr))
                {
                    return false;
                }

                if (!hash.TryGetValue("pid", out var pidStr) || !pidRegex.IsMatch(pidStr))
                {
                    return false;
                }

                return true;
            }).ToString();
        }

        private string ParseInputAsText() => File.ReadAllText(InputFilePath);

        private List<string> ParseInputAsLines() => File.ReadAllLines(InputFilePath).ToList();

        private string ParseInput()
        {
            var file = new ParsedFile(InputFilePath);

            return "";
        }
    }
}
