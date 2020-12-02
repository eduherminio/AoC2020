using AoCHelper;
using FileParser;
using System.Collections.Generic;
using System.Linq;

namespace AoC_2020
{
    public class Day_02 : BaseDay
    {
        private readonly List<Password> _input;

        public Day_02()
        {
            _input = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            return _input.Count(p =>
            {
                var charsCount = p.Content.Count(ch => ch == p.Policy);

                return charsCount <= p.Rule.Last && charsCount >= p.Rule.First;
            }).ToString();
        }

        public override string Solve_2()
        {
            return _input.Count(p =>
            {
                var charsCount = p.Content.Where((ch, index) =>
                    ch == p.Policy
                    && (index + 1 == p.Rule.First
                        || index + 1 == p.Rule.Last));

                return charsCount.Count() == 1;
            }).ToString();
        }

        private IEnumerable<Password> ParseInput()
        {
            foreach (var line in new ParsedFile(InputFilePath))
            {
                var range = line.NextElement<string>().Split('-').Select(int.Parse);
                var policy = line.NextElement<string>().TrimEnd(':');
                var content = line.NextElement<string>();

                yield return new Password(content, policy[0], (range.ElementAt(0), range.ElementAt(1)));
            }
        }

        private record Password(string Content, char Policy, (int First, int Last) Rule);
    }
}
