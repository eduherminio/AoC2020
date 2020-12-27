using AoCHelper;
using FastHashSet;
using FileParser;
using Nito.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var winnerDeck = PlayCombatGame(_player1Cards, _player2Cards, isRecursive: false).winnerDeck;

            var counter = 1;
            return
                winnerDeck.Reverse().Aggregate((long)0, (result, item) => result + (item * counter++))
                .ToString();
        }

        public override string Solve_2()
        {
            var winnerDeck = PlayCombatGame(_player1Cards, _player2Cards, isRecursive: true).winnerDeck;

            var counter = 1;
            return
                winnerDeck.Reverse().Aggregate((long)0, (result, item) => result + (item * counter++))
                .ToString();
        }

        private static string DeckHashCode(Deque<int> deck) => string.Join('|', deck);

        /// <summary>
        /// Plays a Recursive Combate game (part 2)
        /// </summary>
        /// <param name="player1InitialDeck"></param>
        /// <param name="player2InitialDeck"></param>
        /// <returns>A boolean that is true if Player 1 is the winner, and the winner deck</returns>
        private static (bool isPlayer1Winner, Deque<int> winnerDeck) PlayCombatGame(IEnumerable<int> player1InitialDeck, IEnumerable<int> player2InitialDeck, bool isRecursive)
        {
            var previousDecks = new FastHashSet<(string, string)>();

            var player1Deck = new Deque<int>(player1InitialDeck);
            var player2Deck = new Deque<int>(player2InitialDeck);

            while (player1Deck.Count > 0 && player2Deck.Count > 0)
            {
                // Infinite game prevention rule
                if (isRecursive && !previousDecks.Add((DeckHashCode(player1Deck), DeckHashCode(player2Deck))))
                {
                    player2Deck.Clear();
                    break;
                }

                var p1 = player1Deck.RemoveFromFront();
                var p2 = player2Deck.RemoveFromFront();

                bool isPlayer1ThisRoundWinner = isRecursive && player1Deck.Count >= p1 && player2Deck.Count >= p2
                    ? PlayCombatGame(player1Deck.Take(p1), player2Deck.Take(p2), isRecursive).isPlayer1Winner
                    : p1 > p2;

                if (isPlayer1ThisRoundWinner)
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

            return player2Deck.Count == 0
                ? (true, player1Deck)
                : (false, player2Deck);
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
