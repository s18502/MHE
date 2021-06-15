using System;
using System.Collections.Generic;
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

    public class GeneticProgramHeuristic : IGeneticGraphHeuristic
    {
        private static readonly Random Random = new();

        private readonly int _initialPopulationCount;
        private readonly int _generations;
        private readonly double _crossoverProbability;
        private readonly double _mutationProbability;
        private readonly double _elite;

        private readonly SelectionAlgorithm _selectionAlgorithm;
        private readonly CrossOverPairingAlgorithm _crossOverPairingAlgorithm;
        private readonly CrossOverAlgorithm _crossOverAlgorithm;
        private readonly MutationAlgorithm _mutationAlgorithm;
        private readonly Logger _logger;

        public delegate IEnumerable<GraphPath> SelectionAlgorithm(Graph g, IReadOnlyCollection<(GraphPath, int)> scoredPopulation);

        public delegate IEnumerable<GraphPath> CrossOverAlgorithm(Graph g, GraphPath parent1, GraphPath parent2);

        public delegate GraphPath MutationAlgorithm(Graph g, GraphPath genome);

        public delegate IEnumerable<(GraphPath, GraphPath)>
            CrossOverPairingAlgorithm(Graph g ,IReadOnlyCollection<GraphPath> population);

        public delegate void Logger(string output);

        public GeneticProgramHeuristic(
            int initialPopulationCount,
            int generations,
            double crossoverProbability,
            double mutationProbability,
            double elite,
            SelectionAlgorithm selectionAlgorithm,
            CrossOverPairingAlgorithm crossOverPairingAlgorithm,
            CrossOverAlgorithm crossOverAlgorithm,
            MutationAlgorithm mutationAlgorithm,
            Logger logger
        )
        {
            _initialPopulationCount = initialPopulationCount;
            _generations = generations;
            _crossoverProbability = crossoverProbability;
            _mutationProbability = mutationProbability;
            _elite = elite;
            _selectionAlgorithm = selectionAlgorithm;
            _crossOverPairingAlgorithm = crossOverPairingAlgorithm;
            _crossOverAlgorithm = crossOverAlgorithm;
            _mutationAlgorithm = mutationAlgorithm;
            _logger = logger;
        }

        public GraphPath Solve(Graph g)
        {
            var population =
                Enumerable.Range(0, _initialPopulationCount)
                    .Select(_ => g.GetRandomPath(true))
                    .ToList();

            for (var currentGeneration = 0; currentGeneration < _generations; currentGeneration++)
            {
                var populationFitness = population.Select(Heuristic.Goal);
                var populationWithFitness = population.Zip(populationFitness).ToList();
                var parentPopulation = _selectionAlgorithm(g, populationWithFitness).ToList();

                var eliteCount = Math.Max(1, (int)Math.Round(_elite * population.Count));
                var elite = populationWithFitness
                    .OrderByDescending(x => x.Second)
                    .Take(eliteCount)
                    .Select(x=>x.First);
                
                population.Clear();
                population.AddRange(elite);
                
                foreach (var parents in _crossOverPairingAlgorithm(g, parentPopulation))
                {
                    var (p1, p2) = parents;

                    if (Random.NextDouble() <= _crossoverProbability)
                    {
                        population.AddRange(_crossOverAlgorithm(g, p1, p2));
                    }
                    else
                    {
                        population.AddRange(new[] {p1, p2});
                    }
                }

                // deduplicate
                population =
                    population
                        .GroupBy(x => string.Join("", x.Vertices.OrderBy(y => y)))
                        .Select(gr => gr.First())
                        .ToList();

                population = population.Select(p =>
                    Random.NextDouble() < _mutationProbability ? _mutationAlgorithm(g, p) : p
                ).ToList();

                _logger($"[GEN: {currentGeneration}] Parent population: {parentPopulation.Count}, Population: {population.Count}, Elite: {eliteCount}");

                if (population.Count == 1)
                    return population.Single();
            }

            return population.OrderByDescending(Heuristic.Goal).First();
        }
    }
}