using AoCHelper;

namespace AoC_2020
{
    public class Day_15 : BaseDay
    {
        private readonly List<int> _input;

        public Day_15()
        {
            _input = File.ReadAllText(InputFilePath).Split(',').Select(int.Parse).ToList();
        }

        public override ValueTask<string> Solve_1() => new(PlayMemoryGame_Array(2020));

        public override ValueTask<string> Solve_2() => new(PlayMemoryGame_Array(30_000_000));

        /// <summary>
        /// ~2s
        /// </summary>
        /// <param name="targetTurn"></param>
        /// <returns></returns>
        internal string PlayMemoryGame_Dictionary(int targetTurn)
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

        /// <summary>
        /// ~850ms.
        /// Using each element's index as number and its value as saved turn.
        /// </summary>
        /// <param name="targetTurn"></param>
        /// <returns></returns>
        internal string PlayMemoryGame_List(int targetTurn)
        {
            var history = new List<int>(new int[targetTurn + 1]);
            for (int index = 0; index < _input.Count - 1; ++index)
            {
                history[_input[index]] = index + 1;
            }

            var turn = _input.Count;
            var previousTurnNumber = _input.Last();

            while (++turn <= targetTurn)
            {
                (history[previousTurnNumber], previousTurnNumber) = (
                    turn - 1,
                    history[previousTurnNumber] != 0
                        ? turn - 1 - history[previousTurnNumber]
                        : 0);
            }

            return previousTurnNumber.ToString();
        }

        /// <summary>
        /// ~750ms.
        /// Using each element's index as number and its value as saved turn
        /// </summary>
        /// <param name="targetTurn"></param>
        /// <returns></returns>
        internal string PlayMemoryGame_Array(int targetTurn)
        {
            var history = new int[targetTurn + 1];
            for (int index = 0; index < _input.Count - 1; ++index)
            {
                history[_input[index]] = index + 1;
            }

            var turn = _input.Count;
            var previousTurnNumber = _input.Last();

            while (++turn <= targetTurn)
            {
                (history[previousTurnNumber], previousTurnNumber) = (
                    turn - 1,
                    history[previousTurnNumber] != 0
                        ? turn - 1 - history[previousTurnNumber]
                        : 0);
            }

            return previousTurnNumber.ToString();
        }
    }
}
