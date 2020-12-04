using AoCHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC_2020
{
    public class Day_04 : BaseDay
    {
        private readonly List<Dictionary<string, string>> _input;

        private readonly Regex _hclRegex = new Regex("^#[a-f0-9]{6}$", RegexOptions.Compiled);
        private readonly Regex _pidRegex = new Regex("^[0-9]{9}$", RegexOptions.Compiled);
        private readonly IReadOnlyCollection<string> _validEcl = new List<string> { "amb", "blu", "brn", "gry", "grn", "hzl", "oth" };

        public Day_04()
        {
            _input = ParseInput();
        }

        public override string Solve_1()
        {
            return _input.Count(dict =>
            {
                return dict.ContainsKey("byr")
                    && dict.ContainsKey("iyr")
                    && dict.ContainsKey("eyr")
                    && dict.ContainsKey("hgt")
                    && dict.ContainsKey("hcl")
                    && dict.ContainsKey("ecl")
                    && dict.ContainsKey("pid");
            }).ToString();
        }

        public override string Solve_2()
        {
            return _input.Count(hash =>
            {
                if (!hash.TryGetValue("byr", out var byrStr)
                    || !int.TryParse(byrStr, out var byr)
                    || byr < 190
                    || byr > 2002)
                {
                    return false;
                }

                if (!hash.TryGetValue("iyr", out var iyrStr)
                    || !int.TryParse(iyrStr, out var iyr)
                    || iyr < 2010
                    || iyr > 2020)
                {
                    return false;
                }

                if (!hash.TryGetValue("eyr", out var eyrStr)
                    || !int.TryParse(eyrStr, out var eyr)
                    || eyr < 2020
                    || eyr > 2030)
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

                if (!hash.TryGetValue("hcl", out var hclStr) || !_hclRegex.IsMatch(hclStr))
                {
                    return false;
                }

                if (!hash.TryGetValue("ecl", out var eclStr) || !_validEcl.Contains(eclStr))
                {
                    return false;
                }

                if (!hash.TryGetValue("pid", out var pidStr) || !_pidRegex.IsMatch(pidStr))
                {
                    return false;
                }

                return true;
            }).ToString();
        }

        private List<Dictionary<string, string>> ParseInput()
        {
            var groups = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>()
            };

            int index = 0;
            foreach (var line in File.ReadAllLines(InputFilePath))
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

            return groups;
        }
    }
}
