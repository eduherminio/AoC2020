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

        private static readonly string[] FieldsToCheck = new[]
        {
            "byr" ,"iyr" ,"eyr", "hgt", "hcl", "ecl", "pid"
        };

        //  Part2_AsLittleRegexAsPossible solution
        private static readonly Regex HclRegex = new Regex("^#[a-f0-9]{6}$", RegexOptions.Compiled);
        private static readonly Regex PidRegex = new Regex("^[0-9]{9}$", RegexOptions.Compiled);
        private static readonly IReadOnlyCollection<string> ValidEcl = new List<string> { "amb", "blu", "brn", "gry", "grn", "hzl", "oth" };

        /// <summary>
        /// Regex patterns by @robertosanval and @mariomka (thanks!)
        /// Roberto's code: https://github.com/robertosanval/aoc2020/blob/master/src/day4/index.js
        /// Mario's code: https://github.com/mariomka/AdventOfCode2020 (TODO: add link to file when it's up)
        /// </summary>
        private static readonly Dictionary<string, Regex> CompiledRegexExpressions = new Dictionary<string, Regex>
        {
            ["byr"] = new Regex("^(19[2-8][0-9]|199[0-9]|200[0-2])$", RegexOptions.Compiled),
            ["iyr"] = new Regex("^(201[0-9]|2020)$", RegexOptions.Compiled),
            ["eyr"] = new Regex("^(202[0-9]|2030)$", RegexOptions.Compiled),
            ["hgt"] = new Regex("^((1[5-8][0-9]|19[0-3])cm|(59|6[0-9]|7[0-6])in)$", RegexOptions.Compiled),
            ["hcl"] = new Regex("^#[0-9a-f]{6}$", RegexOptions.Compiled),
            ["ecl"] = new Regex("^amb|blu|brn|gry|grn|hzl|oth$", RegexOptions.Compiled),
            ["pid"] = new Regex(@"^\d{9}$", RegexOptions.Compiled)
        };

        public Day_04()
        {
            _input = ParseInput();
        }

        public override string Solve_1()
        {
            return _input.Count(dict =>
                FieldsToCheck.All(key =>
                    dict.TryGetValue(key, out var str)))
            .ToString();
        }

        public override string Solve_2() => Part2_Regex();

        internal string Part2_Regex()
        {
            return _input.Count(dict =>
                FieldsToCheck.All(key =>
                    dict.TryGetValue(key, out var str) && CompiledRegexExpressions[key].IsMatch(str)))
            .ToString();
        }

        /// <summary>
        /// My initial solution after clean up.
        /// ~2 times slower than the regex (compiled) one
        /// </summary>
        /// <returns></returns>
        internal string Part2_AsLittleRegexAsPossible()
        {
            return _input.Count(dict =>
                       Validate_bhr(dict)
                    && Validate_iyr(dict)
                    && Validate_eyr(dict)
                    && Validate_hcl(dict)
                    && Validate_ecl(dict)
                    && Validate_pid(dict)
                    && Validate_hgt(dict))
                .ToString();

            static bool Validate_bhr(Dictionary<string, string> hash)
            {
                return hash.TryGetValue("byr", out var byrStr)
                    && int.TryParse(byrStr, out var byr)
                    && byr >= 1920
                    && byr <= 2002;
            }

            static bool Validate_iyr(Dictionary<string, string> dict)
            {
                return dict.TryGetValue("iyr", out var iyrStr)
                    && int.TryParse(iyrStr, out var iyr)
                    && iyr >= 2010
                    && iyr <= 2020;
            }

            static bool Validate_eyr(Dictionary<string, string> dict)
            {
                return dict.TryGetValue("eyr", out var eyrStr)
                    && int.TryParse(eyrStr, out var eyr)
                    && eyr >= 2020
                    && eyr <= 2030;
            }

            static bool Validate_hcl(Dictionary<string, string> dict)
            {
                return dict.TryGetValue("hcl", out var hclStr)
                    && HclRegex.IsMatch(hclStr);
            }

            static bool Validate_pid(Dictionary<string, string> dict)
            {
                return dict.TryGetValue("pid", out var pidStr)
                    && PidRegex.IsMatch(pidStr);
            }

            static bool Validate_ecl(Dictionary<string, string> dict)
            {
                return dict.TryGetValue("ecl", out var eclStr)
                    && ValidEcl.Contains(eclStr);
            }

            static bool Validate_hgt(Dictionary<string, string> dict)
            {
                return
                    dict.TryGetValue("hgt", out var hgtStr)
                    && int.TryParse(hgtStr[..^2], out var hgt)
                    && ((hgtStr.EndsWith("cm") && hgt >= 150 && hgt <= 193)
                        || (hgtStr.EndsWith("in") && hgt >= 59 && hgt <= 76));
            }
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
