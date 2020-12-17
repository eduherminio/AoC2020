using AoCHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC_2020
{
    public class Day_05 : BaseDay
    {
        private readonly List<string> _input;

        public Day_05()
        {
            _input = File.ReadAllLines(InputFilePath).ToList();
        }

        public override string Solve_1()
        {
            return _input.Max(boardingPass => CalculateSeatId(ExtractSeat(boardingPass)))
                .ToString();
        }

        public override string Solve_2() => Part2_Linq();

        internal string Part2_Linq()
        {
            var ids = _input.ConvertAll(boardingPass => CalculateSeatId(ExtractSeat(boardingPass)));

            return $"{ids.First(id => !ids.Contains(id + 1) && ids.Contains(id + 2)) + 1}";
        }

        /// <summary>
        /// ~ speed and allocations as <see cref="Part2_Linq"/>
        /// </summary>
        /// <returns></returns>
        internal string Part2_Loop()
        {
            var ids = _input.ConvertAll(boardingPass => CalculateSeatId(ExtractSeat(boardingPass)));

            for (int i = 0; i < ids.Count; ++i)
            {
                var current = ids[i];
                if (!ids.Contains(current + 1) && ids.Contains(current + 2))
                {
                    return $"{current + 1}";
                }
            }

            throw new SolvingException();
        }

        private static (int row, int column) ExtractSeat(string input)
        {
            var row = input[0..7].Replace('F', '0').Replace('B', '1');
            var column = input[^3..].Replace('L', '0').Replace('R', '1');

            return (Convert.ToInt32(row, 2), Convert.ToInt32(column, 2));
        }

        private static int CalculateSeatId((int row, int column) seat) => (8 * seat.row) + seat.column;
    }
}
