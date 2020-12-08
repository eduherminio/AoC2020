using AoC_2020.AssemblyComputer;
using AoCHelper;
using FileParser;
using System.Collections.Generic;
using System.Linq;

namespace AoC_2020
{
    public class Day_08 : BaseDay
    {
        private readonly List<AssemblyInstruction> _instructions;

        public Day_08()
        {
            _instructions = ParseInput().ToList();
        }

        public override string Solve_1()
        {
            try
            {
                Computer.ExecuteInstructions(_instructions);
            }
            catch (InfiniteLoopException e)
            {
                return e.LastAccumulatorValue.ToString();
            }

            throw new SolvingException();
        }

        public override string Solve_2()
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
                    return Computer.ExecuteInstructions(_instructions).ToString();
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
            var file = new ParsedFile(InputFilePath);

            while (!file.Empty)
            {
                var line = file.NextLine();

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
