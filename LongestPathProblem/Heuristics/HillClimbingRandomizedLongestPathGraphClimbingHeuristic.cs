using System.Collections.Generic;
using LongestPathProblem.Models;

namespace LongestPathProblem.Heuristics
{
    public class HillClimbingRandomizedLongestPathGraphClimbingHeuristic : IGraphClimbingHeuristic
    {
        private readonly int _iterations;
        private double _randomness;

        public HillClimbingRandomizedLongestPathGraphClimbingHeuristic(int iterations, double randomness = 0.5)
        {
            _iterations = iterations;
            _randomness = randomness;
        }
        
        public (IEnumerable<HeuristicLog> log, GraphPath currentBest) Solve(Graph graph)
        {
            var log = new List<HeuristicLog>(_iterations);
            
            var currentBest = graph.GetRandomPath();
            var currentGoal = Heuristic.Goal(currentBest);

            for (var i = 0; i < _iterations; i++)
            {
                var newSolution = graph.GetNextDeterministicPath(currentBest, _randomness);
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