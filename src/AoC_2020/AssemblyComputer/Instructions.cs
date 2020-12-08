namespace AoC_2020.AssemblyComputer
{
    public record InstructionInput(long Accumulator, int InstructionPointer);

    public record InstructionOutput(long Accumulator, int InstructionPointer);

    public abstract record AssemblyInstruction(int Argument)
    {
        public abstract InstructionOutput Run(InstructionInput input);
    }

    public record Nop(int Argument) : AssemblyInstruction(Argument)
    {
        public override InstructionOutput Run(InstructionInput input)
        {
            return new InstructionOutput(input.Accumulator, input.InstructionPointer + 1);
        }
    }

    public record Acc(int Argument) : AssemblyInstruction(Argument)
    {
        public override InstructionOutput Run(InstructionInput input)
        {
            return new InstructionOutput(input.Accumulator + Argument, input.InstructionPointer + 1);
        }
    }

    public record Jmp(int Argument) : AssemblyInstruction(Argument)
    {
        public override InstructionOutput Run(InstructionInput input)
        {
            return new InstructionOutput(input.Accumulator, input.InstructionPointer + Argument);
        }
    }
}
