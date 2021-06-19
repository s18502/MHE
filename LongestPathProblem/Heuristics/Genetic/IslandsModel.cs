using System;
using System.Collections.Generic;
using System.Linq;
using LongestPathProblem.Models;

namespace LongestPathProblem.Heuristics.Genetic
{
    record IslandState
    {
        public IGeneticProgramIterator GeneticProgramIterator { get; set; } 
        public List<GraphPath> Population { get; set; }
        public int CurrentGeneration { get; set; } 
    }
    
    public class IslandsModel
    {
        private readonly Random Random = new();
        
        private readonly Graph _graph;
        private readonly int _generations;
        private readonly double _migrationRate;
        private readonly Predicate<int> _shouldMigrate;
        private readonly int _a;
        private readonly IslandState[][] _topology;
        private readonly int _islandsCount;

        public IslandsModel(int initialPopulationCount, int generations, double migrationRate, Predicate<int> shouldMigrate, IGeneticProgramIterator[] islands, Graph graph)
        {
            _graph = graph;
            _generations = generations;
            _migrationRate = migrationRate;
            _shouldMigrate = shouldMigrate;
            _islandsCount = islands.Length;
            
            _a = (int) Math.Ceiling(Math.Sqrt(islands.Length));
            _topology = new IslandState[_a][];
            for (var i = 0; i < _a; i++)
            {
                _topology[i] = new IslandState[_a];
            }

            for (var i = 0; i < islands.Length; i++)
            {
                int row = (int) Math.Floor((double) i / _a);
                int col = i % _a;

                var iterator = islands[i];
                
                var population =
                    Enumerable.Range(0, initialPopulationCount)
                        .Select(_ => _graph.GetRandomPath(true))
                        .ToList();

                var islandState = new IslandState
                {
                    GeneticProgramIterator = iterator,
                    CurrentGeneration = 0,
                    Population = population
                };

                _topology[row][col] = islandState;
            }
        }

        public GraphPath Solve()
        {
            var currentBest = new GraphPath();
            
            for (var generation = 0; generation < _generations; generation++)
            {
                for (var row = 0; row < _a; row++)
                for (var col = 0; col < _a; col++)
                {
                    var island = _topology[row][col];
                    var newPopulation = island.GeneticProgramIterator.DoGeneticAlgorithmIteration(_graph, generation, island.Population);

                    _topology[row][col] = island with
                    {
                        CurrentGeneration = island.CurrentGeneration + 1,
                        Population = newPopulation
                    };
                    
                    if(!_shouldMigrate(generation)) continue;

                    var migrationCount = (int) Math.Round(_migrationRate * newPopulation.Count);
                    if(migrationCount < 1) continue;
                    
                    var sharedPopulation = newPopulation
                        .OrderByDescending(Heuristic.Goal)
                        .Take(migrationCount)
                        .ToList();

                    if (Heuristic.Goal(currentBest) < Heuristic.Goal(sharedPopulation.First()))
                        currentBest = sharedPopulation.First();

                    var (neighbourRow, neighbourCol) = GetRandomNeighbour(row);

                    var neighbour = _topology[neighbourRow][neighbourCol];
                    var neighbourNewPopulation =
                        neighbour.Population
                            .Take(neighbour.Population.Count - migrationCount)
                            .Concat(sharedPopulation).ToList();

                    _topology[neighbourRow][neighbourCol] = neighbour with
                    {
                        Population = neighbourNewPopulation
                    };
                }    
            }

            return currentBest;
        }

        private (int neighbourRow, int neighbourCol) GetRandomNeighbour(int row)
        {
            var neighbourRow = 0;
            var neighbourCol = 0;

            do
            {
                var randomNeighbourRowDelta = Random.Next() % 2 == 0 ? -1 : 1;
                var randomNeighbourColDelta = Random.Next() % 2 == 0 ? -1 : 1;

                neighbourRow = row + randomNeighbourRowDelta;
                if (neighbourRow < 0) neighbourRow = _a - 1;
                if (neighbourRow >= _a) neighbourRow = 0;

                neighbourCol = row + randomNeighbourColDelta;
                if (neighbourCol < 0) neighbourCol = _a - 1;
                if (neighbourCol >= _a) neighbourCol = 0;
            } while (neighbourRow * _a + neighbourCol >= _islandsCount);
            
            return (neighbourRow, neighbourCol);
        }
    }
}