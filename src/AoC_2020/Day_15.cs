﻿using AoCHelper;
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

        public override string Solve_2() => PlayMemoryGame(30_000_000);

        private string PlayMemoryGame(int targetTurn)
        {
            var history = new Dictionary<int, int>(targetTurn);
            for (int index = 0; index < _input.Count - 1; ++index)
            {
                history.Add(_input[index], index + 1);
            }

            var turn = _input.Count;
            var previousTurnNumber = _input.Last();

            while (++turn <= targetTurn)
            {
                (history[previousTurnNumber], previousTurnNumber) = (
                    turn - 1,
                    history.TryGetValue(previousTurnNumber, out var oldTurn)
                        ? turn - 1 - oldTurn
                        : 0);
            }

            return previousTurnNumber.ToString();
        }
    }
}
