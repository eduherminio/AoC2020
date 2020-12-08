using AoCHelper;
using FileParser;
using System;
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
            return ExecuteInstructions(_instructions).accumulator.ToString();
        }

        public override string Solve_2()
        {
            var enumerable = _instructions
                .Select((instruction, index) => (instruction, index))
                .Where(pair => pair.instruction.GetType() == typeof(Nop) || pair.instruction.GetType() == typeof(Jmp))
                .ToList();

            foreach (var (instruction, index) in enumerable)
            {
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
                var (success, accumulator) = ExecuteInstructions(_instructions);
                if (success)
                {
                    return accumulator.ToString();
                }

                // Restore original instruction
                _instructions.RemoveAt(index);
                _instructions.Insert(index, instruction);
            }

            throw new SolvingException();
        }

        private static (bool success, long accumulator) ExecuteInstructions(List<AssemblyInstruction> instructions)
        {
            var executedInstructions = new HashSet<int>();

            long accumulator = 0;
            int instructionPointer = 0;
            while (true)
            {
                if (instructionPointer >= instructions.Count)
                {
                    return (true, accumulator);
                }
                if (!executedInstructions.Add(instructionPointer))
                {
                    return (false, accumulator);
                }

                (accumulator, instructionPointer) = instructions[instructionPointer].Run(new InstructionInput(accumulator, instructionPointer));
            }
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
