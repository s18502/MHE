using System.Collections.Generic;
using LongestPathProblem.Models;

namespace LongestPathProblem.Heuristics
{
    public interface IGraphClimbingHeuristic
    {
        (IEnumerable<HeuristicLog> log, GraphPath currentBest) Solve(Graph graph);
    }
}