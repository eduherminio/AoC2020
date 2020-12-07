using AoCHelper;
using System.Linq;

if (args.Length == 0)
{
    Solver.SolveLast(new SolverConfiguration { ClearConsole = false });
}
else if (args.Length == 1 && args[0].Contains("all", System.StringComparison.CurrentCultureIgnoreCase))
{
    Solver.SolveAll();
}
else
{
    var indexes = args.Select(arg => uint.TryParse(arg, out var index) ? index : uint.MaxValue);

    Solver.Solve(indexes.Where(i => i < uint.MaxValue), new SolverConfiguration { ShowOverallResults = false, ClearConsole = false });
}