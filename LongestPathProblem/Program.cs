using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            Console.WriteLine($"Ocena losowej sciezki: {Heuristic.Goal(randomPath)}");
            
            Console.WriteLine(graph.Vertices.Count);
        }
    }
}