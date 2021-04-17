using System.Collections.Generic;
using LongestPathProblem.Models;

namespace LongestPathProblem.Heuristics
{
    public class HillClimbingRandomizedLongestPathGraphHeuristic : IGraphHeuristic
    {
        private readonly int _iterations;

        public HillClimbingRandomizedLongestPathGraphHeuristic(int iterations)
        {
            _iterations = iterations;
        }
        
        public (IEnumerable<HeuristicLog> log, GraphPath currentBest) Solve(Graph graph)
        {
            var log = new List<HeuristicLog>(_iterations);
            
            var currentBest = graph.GetRandomPath();
            var currentGoal = Heuristic.Goal(currentBest);

            for (var i = 0; i < _iterations; i++)
            {
                var newSolution = graph.GetRandomPath();
                var newSolutionGoal = Heuristic.Goal(newSolution);
                
                if (newSolutionGoal > currentGoal)
                {
                    currentBest = newSolution;
                    currentGoal = newSolutionGoal;
                }
                
                log.Add(new HeuristicLog
                {
                    Iteration = i,
                    Goal = currentGoal
                });
            }

            return (log, currentBest);
        }
    }
}