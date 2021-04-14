using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            
            var incorrectPath = new GraphPath {Vertices = new List<int>(new[] {0, 1, 2})};
            Console.WriteLine($"Przyklad niepoprawnej sciezki: {graph.IsPathValid(incorrectPath)}");
            
            Console.WriteLine(graph.Vertices.Count);
            
            Console.WriteLine("Deterministyczne kolejne sciezki");
            var nextPath = randomPath;
            for (var i = 0; i < 100; i++)
            {
               nextPath = graph.NextDeterministicPath(nextPath);
               Console.WriteLine(nextPath);
            }

            var pathToBeRandomlyModified = new GraphPath(){Vertices = new[] {1,0,2,3,4}.ToList()};
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
        }
    }
}