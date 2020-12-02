using AoCHelper;

if (args.Length > 0 && args[0] == "--all")
{
    Solver.SolveAll();
}
else
{
    Solver.SolveLast();
}