using AoCHelper;
using FileParser;
using Nito.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_22 : BaseDay
    {
        private readonly List<int> _player1Cards;
        private readonly List<int> _player2Cards;

        public Day_22()
        {
            (_player1Cards, _player2Cards) = ParseInput();
        }

        public override string Solve_1()
        {
            var totalCards = _player1Cards.Count + _player2Cards.Count;

            var player1Deck = new Deque<int>(_player1Cards);
            var player2Deck = new Deque<int>(_player2Cards);

            while (player1Deck.Count > 0 && player2Deck.Count > 0)
            {
                var p1 = player1Deck.RemoveFromFront();
                var p2 = player2Deck.RemoveFromFront();

                if (p1 > p2)
                {
                    player1Deck.AddToBack(p1);
                    player1Deck.AddToBack(p2);
                }
                else
                {
                    player2Deck.AddToBack(p2);
                    player2Deck.AddToBack(p1);
                }
            }

            var winnerDeck = player1Deck.Count == 0
                ? player2Deck
                : player1Deck;

            var counter = 1;
            return
                winnerDeck.Reverse().Aggregate((long)0, (result, item) => result + (item * counter++))
                .ToString();
        }

        public override string Solve_2()
        {
            var solution = string.Empty;

            return solution;
        }

        private (List<int>, List<int>) ParseInput()
        {
            var groups = ParsedFile.ReadAllGroupsOfLines(InputFilePath);
            Debug.Assert(groups.Count == 2);

            return (groups[0].Skip(1).Select(int.Parse).ToList(),
                    groups[1].Skip(1).Select(int.Parse).ToList());
        }
    }
}
