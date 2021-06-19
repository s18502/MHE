using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LongestPathProblem.Models;

namespace LongestPathProblem.Heuristics.Genetic
{
    public class GeneticIslandsLoop : IGeneticGraphHeuristic
    {
        private readonly int _initialPopulationCount;
        private readonly int _islandsCount;
        private readonly int _generations;

        public GeneticIslandsLoop(
            int initialPopulationCount,
            int islandsCount,
            int generations
        )
        {
            _initialPopulationCount = initialPopulationCount;
            _islandsCount = islandsCount;
            _generations = generations;
        }

        public GraphPath Solve(Graph g)
        {
            var islands = Enumerable.Range(0, _islandsCount).Select(_ =>
                new GeneticProgramIterator(
                    1,
                    0.2,
                    0.05,
                    CalculateFitnessFunctions.SequentialAndParallelCompare,
                    GeneticSelectionAlgorithms.Roulette,
                    CrossoverPairingAlgorithms.SingleEdgeDistance,
                    GeneticCrossoverAlgorithms.SingleEdgeCrossover,
                    MutationAlgorithms.ModifyVertices,
                    Console.WriteLine
                )).Cast<IGeneticProgramIterator>().ToArray();

            var model = new IslandsModel(_initialPopulationCount, _generations, 0.1, _ => true, islands, g);
            var bestSolution = model.Solve();
            
            return bestSolution;
        }
    }
}