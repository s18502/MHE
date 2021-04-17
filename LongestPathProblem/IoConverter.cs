using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LongestPathProblem.Heuristics;
using LongestPathProblem.Models;

namespace LongestPathProblem
{
    public static class IoConverter
    {
        public static Graph GraphFromLines(IEnumerable<string> inputLines)
        {
            var vertices =
                inputLines
                    .Select(x => x.Split(' '))
                    .Select(x =>
                    {
                        var vertexIdStr = x.First();
                        var vertexId = int.Parse(vertexIdStr);

                        var neighbours = x.Skip(1).Select(int.Parse);

                        return new Vertex
                        {
                            Id = vertexId,
                            Neighbours = neighbours.ToList()
                        };
                    });
            return new Graph(vertices);
        }

        public static GraphPath PathFromString(string input)
        {
            var verticies = input.Split(' ').Select(int.Parse);

            return new GraphPath
            {
                Vertices = verticies.ToList()
            };
        }

        public static void LogToFile(this IEnumerable<HeuristicLog> log, string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Iteration;Goal");
            
            foreach (var logItem in log)
            {
                sb.AppendLine($"{logItem.Iteration};{logItem.Goal}");
            }

            sb.AppendLine();
            
            File.WriteAllText(fileName, sb.ToString());
        }
    }
}