using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LongestPathProblem.Models;

namespace LongestPathProblem.Heuristics.Genetic
{
    public record MutationAlgorithms
    {
        private static Random r = new();
        public static GraphPath ModifyVertices(Graph g, GraphPath path)
        {
            return g.RandomModifyPath(path, r.Next(1, path.Length / 2));
        }
    }

    public static class CalculateFitnessFunctions
    {
        public static IEnumerable<int> SequentialAndParallelCompare(IEnumerable<GraphPath> population)
        {
            var populationList = population.ToList();

            var sw = Stopwatch.StartNew();
            var fitnessParallel = populationList.AsParallel().Select(Heuristic.Goal).ToList();
            sw.Stop();
            Console.WriteLine($"Równoległe obliczanie fitness: {sw.ElapsedTicks} ticks");
            
            sw.Reset();
            sw.Start();
            var fitnessSequential = populationList.Select(Heuristic.Goal).ToList();
            sw.Stop();
            Console.WriteLine($"Sekwencyjne obliczanie fitness: {sw.ElapsedTicks} ticks");

            return fitnessParallel;
        }
    }
    
    public static class CrossoverPairingAlgorithms
    {
        public static IEnumerable<(GraphPath, GraphPath)> SingleEdgeDistance(Graph g, IReadOnlyCollection<GraphPath> graphPaths)
        {
            var pathsByVertex = graphPaths
                .SelectMany(p => p.Vertices.Select(v=>(Path: p, Vertex: v)))
                .GroupBy(x=>x.Vertex)
                .Select(gr => (Vertex: gr.Key, Paths: gr.Select(x=>x.Path)))
                .ToDictionary(x => x.Vertex, x => x.Paths);

            var introspectedVertices = new HashSet<int>();
            
            foreach (var graphPath in graphPaths)
            {
                var pathsWithinDistanceOne =
                    graphPath.Vertices
                        .SelectMany(g.NeighboursOf)
                        .Except(graphPath.Vertices)
                        .Where(v=> !introspectedVertices.Contains(v))
                        .Where(pathsByVertex.ContainsKey)
                        .SelectMany(v => pathsByVertex[v]);
                
                foreach (var pair in pathsWithinDistanceOne.Select(p => (graphPath, p)))
                {
                    yield return pair;
                }
                
                foreach (var v in graphPath.Vertices)
                    introspectedVertices.Add(v);
            }
        }
    }

    public static class GeneticCrossoverAlgorithms
    {
        private static readonly Random Random = new();

        public static IEnumerable<GraphPath> SingleEdgeCrossover(Graph g, GraphPath parent1, GraphPath parent2)
        {
            var result = new List<GraphPath>();
            result.AddRange(JoinPaths(parent1, parent2));

            for (var i = 1; i < parent1.Length - 1; i++)
            {
                var innerVertex = parent1.Vertices[i];   
                var intersections =
                    g.NeighboursOf(innerVertex).Intersect(parent2.Vertices);
                
                foreach (var intersection in intersections)
                {
                    var secondParentIntersectionIdx = Array.IndexOf(parent2.Vertices, intersection);

                    var firstParentIntersectionIdx = i;
                    var leftPartFrom1 = parent1.Vertices
                        .TakeWhile((_, idx) => idx <= firstParentIntersectionIdx)
                        .ToArray();
                    var leftPartFrom2 = parent2.Vertices
                        .TakeWhile((_, idx) => idx <= secondParentIntersectionIdx)
                        .ToArray();
                    
                    var rightPartFrom1 = parent1.Vertices
                        .TakeWhile((_, idx) => idx >= firstParentIntersectionIdx)
                        .ToArray();
                    var rightPartFrom2 = parent2.Vertices
                        .TakeWhile((_, idx) => idx >= secondParentIntersectionIdx)
                        .ToArray();
                    
                    result.AddRange(JoinPaths(new GraphPath(leftPartFrom1), new GraphPath(rightPartFrom2)));
                    result.AddRange(JoinPaths(new GraphPath(leftPartFrom2), new GraphPath(rightPartFrom1)));
                }
            }

            return result
                .Where(g.IsPathValid);
        }

        private static IEnumerable<GraphPath> JoinPaths(GraphPath path1, GraphPath path2)
        {
            yield return new GraphPath(path1.Vertices.Concat(path2.Vertices).ToArray());
            yield return new GraphPath(path1.Vertices.Reverse().Concat(path2.Vertices).ToArray());
            yield return new GraphPath(path1.Vertices.Concat(path2.Vertices.Reverse()).ToArray());
            yield return new GraphPath(path1.Vertices.Reverse().Concat(path2.Vertices.Reverse()).ToArray());
        }
    }

    public static class GeneticSelectionAlgorithms
    {
        private static readonly Random Random = new();

        private static double ToSShapedProbabbility(double x, int beta = 3)
        {
            var a = x / (1.0 - x);
            var b = Math.Pow(a, -1.0 * beta);
            return 1.0 / (1.0 + b);
        }
        
        public static IEnumerable<GraphPath> Roulette(Graph g, IReadOnlyCollection<(GraphPath, int)> scoredPopulation)
        {
            var maxFitness = g.Vertices.Count;
            
            return scoredPopulation.Where(scoredGenome =>
            {   
                var (_, fitness) = scoredGenome;
                var probability = ToSShapedProbabbility((double) fitness / maxFitness);

                return Random.NextDouble() <= probability;
            }).Select(x => x.Item1);
        }
    }

    public class GeneticProgramLoop : IGeneticGraphHeuristic
    {
        private readonly int _initialPopulationCount;
        private readonly int _generations;
        private readonly IGeneticProgramIterator _geneticProgramIterator;

        public GeneticProgramLoop(
            int initialPopulationCount,
            int generations,
            IGeneticProgramIterator geneticProgramIterator
        )
        {
            _initialPopulationCount = initialPopulationCount;
            _generations = generations;
            _geneticProgramIterator = geneticProgramIterator;
        }

        public GraphPath Solve(Graph g)
        {
            var population =
                Enumerable.Range(0, _initialPopulationCount)
                    .Select(_ => g.GetRandomPath(true))
                    .ToList();

            for (var currentGeneration = 0; currentGeneration < _generations; currentGeneration++)
            {
                population = _geneticProgramIterator.DoGeneticAlgorithmIteration(g, currentGeneration, population);
                if (population.Count == 1)
                    return population.First();
            }

            return population.OrderByDescending(Heuristic.Goal).First();
        }
    }
}