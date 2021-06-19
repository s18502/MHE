using System;
using System.Collections.Generic;
using System.Linq;
using LongestPathProblem.Models;

namespace LongestPathProblem.Heuristics.Genetic
{
    public class GeneticProgramIterator : IGeneticProgramIterator
    {
        private static readonly Random Random = new();

        private readonly double _crossoverProbability;
        private readonly double _mutationProbability;
        private readonly double _elite;
        private readonly CalculateFitness _calculateFitness;

        private readonly SelectionAlgorithm _selectionAlgorithm;
        private readonly CrossOverPairingAlgorithm _crossOverPairingAlgorithm;
        private readonly CrossOverAlgorithm _crossOverAlgorithm;
        private readonly MutationAlgorithm _mutationAlgorithm;
        private readonly Logger _logger;

        public delegate IEnumerable<GraphPath> SelectionAlgorithm(Graph g, IReadOnlyCollection<(GraphPath, int)> scoredPopulation);

        public delegate IEnumerable<GraphPath> CrossOverAlgorithm(Graph g, GraphPath parent1, GraphPath parent2);

        public delegate GraphPath MutationAlgorithm(Graph g, GraphPath genome);
        public delegate IEnumerable<int> CalculateFitness(IEnumerable<GraphPath> population);

        public delegate IEnumerable<(GraphPath, GraphPath)>
            CrossOverPairingAlgorithm(Graph g ,IReadOnlyCollection<GraphPath> population);

        public delegate void Logger(string output);

        public GeneticProgramIterator(
            double crossoverProbability,
            double mutationProbability,
            double elite,
            CalculateFitness calculateFitness,
            SelectionAlgorithm selectionAlgorithm,
            CrossOverPairingAlgorithm crossOverPairingAlgorithm,
            CrossOverAlgorithm crossOverAlgorithm,
            MutationAlgorithm mutationAlgorithm,
            Logger logger
        )
        {
            _crossoverProbability = crossoverProbability;
            _mutationProbability = mutationProbability;
            _elite = elite;
            _calculateFitness = calculateFitness;
            _selectionAlgorithm = selectionAlgorithm;
            _crossOverPairingAlgorithm = crossOverPairingAlgorithm;
            _crossOverAlgorithm = crossOverAlgorithm;
            _mutationAlgorithm = mutationAlgorithm;
            _logger = logger;
        }
        
        public List<GraphPath> DoGeneticAlgorithmIteration(Graph g, int currentGeneration, List<GraphPath> population)
        {
            var populationFitness = _calculateFitness(population);
            var populationWithFitness = population.Zip(populationFitness).ToList();
            var parentPopulation = _selectionAlgorithm(g, populationWithFitness).ToList();

            var eliteCount = Math.Max(1, (int) Math.Round(_elite * population.Count));
            var elite = populationWithFitness
                .OrderByDescending(x => x.Second)
                .Take(eliteCount)
                .Select(x => x.First);

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

            _logger(
                $"[GEN: {currentGeneration}] Parent population: {parentPopulation.Count}, Population: {population.Count}, Elite: {eliteCount}");

            return population;
        }
    }
}