using LongestPathProblem.Models;

namespace LongestPathProblem.Heuristics.Genetic
{
    public interface IGeneticGraphHeuristic
    {
        GraphPath Solve(Graph g);
    }
}