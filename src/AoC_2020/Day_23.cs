using AoCHelper;
using FileParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AoC_2020
{
    public class Day_23 : BaseDay
    {
        private readonly LinkedList<int> _input;

        public Day_23()
        {
            _input = ParseInput();
        }

        public override string Solve_1() => Part1_LinkedList();

        public override string Solve_2() => Part2_LinkedList();

        private string Part1_List()
        {
            var result = PlayCrubCups_List(ParseInput().ToList(), 100).ToList();

            var firstIndex = result.IndexOf(1);

            var solution = new StringBuilder();
            var count = firstIndex + result.Count;
            for (int i = firstIndex + 1; i < count; ++i)
            {
                solution.Append(result[i % result.Count]);
            }

            return solution.ToString();
        }

        private string Part1_LinkedList()
        {
            var result = PlayCrubCups_LinkedList_Enhanced(ParseInput(), 100).ToList();

            var firstIndex = result.IndexOf(1);

            var solution = new StringBuilder();
            var count = firstIndex + result.Count;
            for (int i = firstIndex + 1; i < count; ++i)
            {
                solution.Append(result[i % result.Count]);
            }

            return solution.ToString();
        }

        private string Part2_List()
        {
            for (int i = _input.Max() + 1; i <= 1_000_000; ++i)
            {
                _input.AddLast(i);
            }

            var result = PlayCrubCups_List(_input.ToList(), 10_000_000);

            var firstNode = result.IndexOf(1);

            return ((long)result[firstNode + 1] * result[firstNode + 2])
                .ToString();
        }

        private string Part2_LinkedList()
        {
            for (int i = _input.Max() + 1; i <= 1_000_000; ++i)
            {
                _input.AddLast(i);
            }

            var result = PlayCrubCups_LinkedList_Enhanced(_input, 10_000_000);

            var firstNode = result.Find(1)!;

            var next = firstNode.Next ?? result.First!;
            var nextNext = next.Next ?? result.First!;

            return ((long)next.Value * nextNext.Value)
                .ToString();
        }

        /// <summary>
        /// Similar to <see cref="PlayCrubCups_LinkedList(LinkedList{int}, int, int)"/>, but saving each node in a dictionary
        /// to avoid using <see cref="LinkedList{T}.Find(T)"/>
        /// </summary>
        /// <param name="initialLabelling"></param>
        /// <param name="numberOfCupsToRemove"></param>
        /// <param name="turns"></param>
        /// <returns></returns>
        private static LinkedList<int> PlayCrubCups_LinkedList_Enhanced(LinkedList<int> initialLabelling, int turns)
        {
            const int numberOfCupsToRemove = 3;
            var cups = initialLabelling;
            LinkedListNode<int> NextNode(LinkedListNode<int> node) => node.Next ?? cups?.First ?? throw new ArgumentNullException(nameof(initialLabelling));

            var nodeDictionary = new Dictionary<int, LinkedListNode<int>>();
            var currentNode = cups?.First ?? throw new ArgumentNullException(nameof(initialLabelling));
            for (int i = 0; i < cups.Count; ++i)
            {
                nodeDictionary.Add(currentNode!.Value, currentNode);
                currentNode = currentNode.Next;
            }

            var minLabel = cups.Min();
            var maxLabel = cups.Max();

            var current = cups.Last ?? throw new ArgumentNullException(nameof(initialLabelling));

            for (int turn = 1; turn <= turns; ++turn)
            {
                current = NextNode(current);

                var cupsToRemove = new List<LinkedListNode<int>>(numberOfCupsToRemove) { NextNode(current) };
                for (int i = 1; i <= numberOfCupsToRemove - 1; ++i)
                {
                    cupsToRemove.Add(NextNode(cupsToRemove.Last()));
                }

                cupsToRemove.ForEach(c => cups.Remove(c));

                var destinationCupLabel = (current.Value - 1) >= minLabel ? (current.Value - 1) : maxLabel;
                while (cupsToRemove.Select(n => n.Value).Contains(destinationCupLabel))
                {
                    if (--destinationCupLabel < minLabel)
                    {
                        destinationCupLabel = maxLabel;
                    }
                }

                var destinationCup = nodeDictionary[destinationCupLabel];

                for (int i = 0; i < cupsToRemove.Count; ++i)
                {
                    cups.AddAfter(destinationCup, cupsToRemove[i]);
                    destinationCup = cupsToRemove[i];
                }
            }

            return cups;
        }

        /// <summary>
        /// Bottleneck in <see cref="List{T}.Remove(T)(int, T)"/> (70%) and <see cref="List{T}.IndexOf(T)"/> (30%)
        /// </summary>
        /// <param name="initialLabelling"></param>
        /// <param name="numberOfCupsToRemove"></param>
        /// <param name="turns"></param>
        /// <returns></returns>
        private static List<int> PlayCrubCups_List(List<int> initialLabelling, int turns)
        {
            const int numberOfCupsToRemove = 3;
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

        /// <summary>
        /// Bottleneck in <see cref="LinkedList{T}.Find(T)"/> (100%)
        /// </summary>
        /// <param name="initialLabelling"></param>
        /// <param name="numberOfCupsToRemove"></param>
        /// <param name="turns"></param>
        /// <returns></returns>
        private static LinkedList<int> PlayCrubCups_LinkedList(LinkedList<int> initialLabelling, int turns)
        {
            const int numberOfCupsToRemove = 3;
            var cups = initialLabelling;
            LinkedListNode<int> NextNode(LinkedListNode<int> node) => node.Next ?? cups?.First ?? throw new ArgumentNullException(nameof(initialLabelling));

            var minLabel = cups.Min();
            var maxLabel = cups.Max();

            var current = cups.Last ?? throw new ArgumentNullException(nameof(initialLabelling));

            for (int turn = 1; turn <= turns; ++turn)
            {
                current = NextNode(current);

                var cupsToRemove = new List<LinkedListNode<int>>(numberOfCupsToRemove) { NextNode(current) };
                for (int i = 1; i <= numberOfCupsToRemove - 1; ++i)
                {
                    cupsToRemove.Add(NextNode(cupsToRemove.Last()));
                }

                cupsToRemove.ForEach(c => cups.Remove(c));

                var destinationCupLabel = (current.Value - 1) >= minLabel ? (current.Value - 1) : maxLabel;
                while (cupsToRemove.Select(n => n.Value).Contains(destinationCupLabel))
                {
                    if (--destinationCupLabel < minLabel)
                    {
                        destinationCupLabel = maxLabel;
                    }
                }

                var destinationCup = cups.Find(destinationCupLabel)!;

                for (int i = 0; i < cupsToRemove.Count; ++i)
                {
                    cups.AddAfter(destinationCup, cupsToRemove[i]);
                    destinationCup = cupsToRemove[i];
                }
            }

            return cups;
        }

        private LinkedList<int> ParseInput()
        {
            return new LinkedList<int>(File.ReadAllText(InputFilePath)
                .Trim()
                .Select(ch => int.Parse(ch.ToString())));
        }
    }
}
