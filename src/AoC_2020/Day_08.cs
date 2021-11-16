using AoC_2020.AssemblyComputer;
using AoCHelper;
using FileParser;

namespace AoC_2020
{
    public class Day_08 : BaseDay
    {
        private readonly List<AssemblyInstruction> _instructions;

        public Day_08()
        {
            _instructions = ParseInput().ToList();
        }

        public override ValueTask<string> Solve_1()
        {
            try
            {
                Computer.ExecuteInstructions(_instructions);
            }
            catch (InfiniteLoopException e)
            {
                return new(e.LastAccumulatorValue.ToString());
            }

            throw new SolvingException();
        }

        public override ValueTask<string> Solve_2()
        {
            for (int index = 0; index < _instructions.Count; ++index)
            {
                var instruction = _instructions[index];

                if (instruction is Acc)
                {
                    continue;
                }

                // Replace instruction with its alternative
                AssemblyInstruction replacement = instruction switch
                {
                    Nop => new Jmp(instruction.Argument),
                    Jmp => new Nop(instruction.Argument),
                    _ => throw new SolvingException()
                };

                _instructions.RemoveAt(index);
                _instructions.Insert(index, replacement);

                // Run the code
                try
                {
                    return new(Computer.ExecuteInstructions(_instructions).ToString());
                }
                catch (InfiniteLoopException) { /* Expected */ }

                // Restore original instruction
                _instructions.RemoveAt(index);
                _instructions.Insert(index, instruction);
            }

            throw new SolvingException();
        }

        private IEnumerable<AssemblyInstruction> ParseInput()
        {
            foreach (var line in new ParsedFile(InputFilePath))
            {
                yield return line.NextElement<string>() switch
                {
                    "acc" => new Acc(line.NextElement<int>()),
                    "jmp" => new Jmp(line.NextElement<int>()),
                    "nop" => new Nop(line.NextElement<int>()),
                    _ => throw new SolvingException()
                };
            }
        }
    }
}
