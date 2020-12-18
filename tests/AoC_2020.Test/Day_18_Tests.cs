using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AoC_2020.Test
{
    public class Day_18_Tests
    {
        [Theory]
        [InlineData("2 * 3 + 4 * 5", 50)]
        [InlineData("(2 * 3) + 4 * 5", 50)]
        [InlineData("(2 * 3 + 4) * 5", 50)]
        [InlineData("((2 * 3) + 4) * 5", 50)]
        [InlineData("2 * 3 + (4 * 5)", 26)]
        [InlineData("1 + (2 * 3) + (4 * (5 + 6))", 51)]
        [InlineData("5 + (8 * 3 + 9 + 3 * 4 * 3)", 437)]
        [InlineData("5 * 9 * (7 * 3 * 3 + 9 * 3 + (8 + 6 * 4))", 12240)]
        [InlineData("((2 + 4 * 9) * (6 + 9 * 8 + 6) + 6) + 2 + 4 * 2", 13632)]
        public void Part1(string line, long result)
        {
            var day = new Day_18();

            Assert.Equal(result, Day_18.CalculateLine(Day_18.ReverseLineAndAddSpaces(line)));
        }
    }
}
