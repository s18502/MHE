using System.Collections.Generic;
using System.Linq;
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
            return new Graph
            {
                Vertices = new HashSet<Vertex>(vertices)
            };
        }

        public static GraphPath PathFromString(string input)
        {
            var verticies = input.Split(' ').Select(int.Parse);

            return new GraphPath
            {
                Vertices = verticies.ToList()
            };
        }
    }
}