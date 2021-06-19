using System.Collections.Generic;
using LongestPathProblem.Models;

namespace LongestPathProblem.Heuristics.Genetic
{
    public interface IGeneticProgramIterator
    {
        List<GraphPath> DoGeneticAlgorithmIteration(Graph g, int currentGeneration, List<GraphPath> population);
    }
}