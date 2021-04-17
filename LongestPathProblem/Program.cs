using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LongestPathProblem.Heuristics;
using LongestPathProblem.Models;

namespace LongestPathProblem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var graphFileName = args.FirstOrDefault();
            var inputLines = await File.ReadAllLinesAsync(graphFileName);
            var graph = IoConverter.GraphFromLines(inputLines);

            var solutionFileName = args.Skip(1).FirstOrDefault();
            if (solutionFileName != default)
            {
                Console.WriteLine("Ladowanie dziedziny z pliku");
                var pathFromFile = IoConverter.PathFromString(await File.ReadAllTextAsync(solutionFileName));
                Console.WriteLine($"Zaladowana sciezka: {pathFromFile}");
                Console.WriteLine($"Ocena sciezki z pliku: {Heuristic.Goal(pathFromFile)}");
            }

            var randomPath = graph.GetRandomPath();
            Console.WriteLine($"Losowa sciezka: {randomPath}");
            Console.WriteLine($"Czy losowa sciezka jest poprawna? {graph.IsPathValid(randomPath)}");
            Console.WriteLine($"Ocena losowej sciezki: {Heuristic.Goal(randomPath)}");
            
            var incorrectPath = new GraphPath(new[] {1, 2});
            Console.WriteLine($"Przyklad niepoprawnej sciezki: {graph.IsPathValid(incorrectPath)}");
            
            Console.WriteLine(graph.Vertices.Count);
            
            Console.WriteLine("Deterministyczne kolejne sciezki");
            var nextPath = new GraphPath(new []{1,3});
            for (var i = 0; i < 100; i++)
            {
               nextPath = graph.GetNextDeterministicPath(nextPath);
               Console.WriteLine(nextPath);
            }

            var pathToBeRandomlyModified = new GraphPath(new[] {1,3,4});
            Console.WriteLine("Losowo modyfikowana sciezka, stopien 1");
            for (var i = 0; i < 10; i++)
            {
                var randomModifiedPath = graph.RandomModifyPath(pathToBeRandomlyModified, 1);
                Console.WriteLine(randomModifiedPath);
            }
            
            Console.WriteLine("Losowo modyfikowana sciezka, stopien 2");
            for (var i = 0; i < 10; i++)
            {
                var randomModifiedPath = graph.RandomModifyPath(pathToBeRandomlyModified, 2);
                Console.WriteLine(randomModifiedPath);
            }
            
            Console.WriteLine("Losowo modyfikowana sciezka, stopien 3");
            for (var i = 0; i < 10; i++)
            {
                var randomModifiedPath = graph.RandomModifyPath(pathToBeRandomlyModified, 3);
                Console.WriteLine(randomModifiedPath);
            }
            
            Console.WriteLine("Pelny przeglad");
            // foreach (var graphPath in graph.AllPaths())
            // {
            //     Console.WriteLine(graphPath);
            // }

            Console.WriteLine(graph.AllPaths().Count());

            var iterations = 10_000;
            Console.WriteLine("Algorytm wspinaczkowy, randomizowany, {0} iteracji", iterations);
            
            var randomizedHillClimbing = new HillClimbingRandomizedLongestPathGraphHeuristic(iterations); 
            var randomizedHillClimbingSolution = randomizedHillClimbing.Solve(graph);
            randomizedHillClimbingSolution.log.LogToFile("randomized_hill_climbing.csv");
            
            Console.WriteLine("Algorytm wspinaczkowy, randomizowany, {0} iteracji, wynik = {1} (cel: {2})", iterations, randomizedHillClimbingSolution, randomizedHillClimbingSolution.currentBest.Length);
            
            var deterministicHillClimbing = new HillClimbingDeterministicLongestPathGraphHeuristic(iterations);
            var deterministicHillClimbingSolution = deterministicHillClimbing.Solve(graph);
            deterministicHillClimbingSolution.log.LogToFile("deterministic_hill_climbing.csv");
            Console.WriteLine("Algorytm wspinaczkowy, deterministyczny, {0} iteracji, wynik = {1} (cel: {2})", iterations, deterministicHillClimbingSolution, deterministicHillClimbingSolution.currentBest.Length);
            
            var simAnnealing = new SimAnnealingLongestPathHeuristic(iterations, k=> 1000.0 / k); 
            var simAnnelingSolution = simAnnealing.Solve(graph);
            simAnnelingSolution.log.LogToFile("sim_annealing.csv");
            
            Console.WriteLine("Algorytm wyzarzania, {0} iteracji, wynik = {1} (cel: {2})", iterations, simAnnelingSolution, simAnnelingSolution.currentBest.Length);
        }
    }
}