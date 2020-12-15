using AoCHelper;
using FileParser;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_15 : BaseDay
    {
        private readonly List<int> _input;

        public Day_15()
        {
            _input = File.ReadAllText(InputFilePath).Split(',').Select(int.Parse).ToList();
        }

        public override string Solve_1() => PlayMemoryGame(2020);

        public override string Solve_2() => PlayMemoryGame(30000000);

        private string PlayMemoryGame(int lastTurn)
        {
            var history = new Dictionary<int, int>();
            for (int index = 0; index < _input.Count - 1; ++index)
            {
                history.Add(_input[index], index + 1);
            }

            var turn = _input.Count + 1;
            var current = _input.Last();

            while (turn <= lastTurn)
            {
                var n = history.TryGetValue(current, out var previousTurn)
                    ? turn - 1 - previousTurn
                    : 0;

                history[current] = turn - 1;
                current = n;
                ++turn;
            }

            return current.ToString();
        }
    }
}
