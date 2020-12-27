using FastHashSet;
using System.Collections.Generic;

namespace AoC_2020.AssemblyComputer
{
    public static class Computer
    {
        public static long ExecuteInstructions(List<AssemblyInstruction> instructions)
        {
            var executedInstructions = new FastHashSet<int>();

            long accumulator = 0;
            int instructionPointer = 0;
            while (true)
            {
                if (instructionPointer >= instructions.Count)
                {
                    return accumulator;
                }

                if (!executedInstructions.Add(instructionPointer))
                {
                    throw new InfiniteLoopException("Infinite loop detected") { LastAccumulatorValue = accumulator };
                }

                (accumulator, instructionPointer) = instructions[instructionPointer].Run(new InstructionInput(accumulator, instructionPointer));
            }
        }
    }
}
