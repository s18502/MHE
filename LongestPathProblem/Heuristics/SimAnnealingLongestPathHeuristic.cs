using System;
using System.Collections.Generic;
using System.Linq;
using LongestPathProblem.Models;

namespace LongestPathProblem.Heuristics
{
    public class SimAnnealingLongestPathHeuristic : IGraphHeuristic
    {
        private readonly int _iterations;
        private readonly Func<int, double> _annealingFunc;
        private readonly Random _random = new Random();

        public SimAnnealingLongestPathHeuristic(int iterations, Func<int, double> annealingFunc)
        {
            _iterations = iterations;
            _annealingFunc = annealingFunc;
        }
        
        public (IEnumerable<HeuristicLog> log, GraphPath currentBest) Solve(Graph graph)
        {
            var log = new List<HeuristicLog>(_iterations);
            
            var currentBest = graph.GetRandomPath();
            var currentGoal = Heuristic.Goal(currentBest);
            
            var v = new List<GraphPath>();

            for (var i = 0; i < _iterations; i++)
            {
                var modification = (int)Math.Min(RandomGaussian() * currentBest.Length, graph.Vertices.Count - 1) + 1;
                var newSolution = graph.RandomModifyPath(currentBest, modification);

                var newGoal = Heuristic.Goal(newSolution);
                
                if (newGoal > currentGoal)
                {
                    currentBest = newSolution;
                    currentGoal = Heuristic.Goal(currentBest);
                    v.Add(currentBest);
                }
                else
                {
                    var e = Math.Exp(-Math.Abs(newGoal - currentGoal) / _annealingFunc(i));
                    var u = _random.NextDouble();

                    if (u < e)
                    {
                        currentBest = newSolution;
                        currentGoal = Heuristic.Goal(currentBest);
                        v.Add(currentBest);
                    }
                }
                
                log.Add(new HeuristicLog
                {
                    Iteration = _iterations,
                    Goal = currentGoal
                });
            }

            currentBest = v.OrderByDescending(Heuristic.Goal).First();
            
            return (log, currentBest);
        }

        public double RandomGaussian()
        {
            var mean = 3;
            var stdDev = 1;
            
            var u1 = 1.0 - _random.NextDouble();
            var u2 = 1.0 - _random.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2); 
            var randNormal =
                mean + stdDev * randStdNormal;

            return Math.Abs(randNormal / 6.0);
        }
    }
}