using AoCHelper;
using FileParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AoC_2020
{
    public class Day_23 : BaseDay
    {
        private readonly List<int> _input;

        public Day_23()
        {
            _input = ParseInput();
        }

        public override string Solve_1()
        {
            var result = PlayCrubCups(_input, 3, 100);

            var firstIndex = result.IndexOf(1);

            var solution = new StringBuilder();
            var count = firstIndex + result.Count;
            for (int i = firstIndex + 1; i < count; ++i)
            {
                solution.Append(result[i % result.Count]);
            }

            return solution.ToString();
        }

        public static List<int> PlayCrubCups(List<int> initialLabelling, int numberOfCupsToRemove, int turns)
        {
            var cups = initialLabelling.ConvertAll(_ => _);

            var minLabel = cups.Min();
            var maxLabel = cups.Max();

            var cupIndexToSelect = 0;

            for (int turn = 1; turn <= turns; ++turn)
            {
                var selectedCup = cups[cupIndexToSelect];

                var cupsToRemove = new List<int>(numberOfCupsToRemove);
                for (int i = 1; i <= numberOfCupsToRemove; ++i)
                {
                    cupsToRemove.Add(cups[(cupIndexToSelect + i) % cups.Count]);
                }

                cupsToRemove.ForEach(c => cups.Remove(c));

                var destinationCupLabel = (selectedCup - 1) >= minLabel ? (selectedCup - 1) : maxLabel;
                while (cupsToRemove.Contains(destinationCupLabel))
                {
                    if (--destinationCupLabel < minLabel)
                    {
                        destinationCupLabel = maxLabel;
                    }
                }

                var destinationCupIndex = cups.IndexOf(destinationCupLabel);

                if (destinationCupIndex == -1) throw new SolvingException();

                for (int i = 0; i < cupsToRemove.Count; ++i)
                {
                    var pos = destinationCupIndex + i + 1;
                    if (pos == cups.Count)
                    {
                        cups.Add(cupsToRemove[i]);
                    }
                    else
                    {
                        cups.Insert(destinationCupIndex + i + 1, cupsToRemove[i]);
                    }
                }

                cupIndexToSelect = (cups.IndexOf(selectedCup) + 1) % cups.Count;
            }

            return cups;
        }

        public override string Solve_2()
        {
            var solution = string.Empty;

            return solution;
        }

        private List<int> ParseInput()
        {
            return File.ReadAllText(InputFilePath)
                .Trim()
                .Select(ch => int.Parse(ch.ToString()))
                .ToList();
        }
    }
}
