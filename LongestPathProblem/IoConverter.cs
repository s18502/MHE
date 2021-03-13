using System.Collections.Generic;
using System.Linq;
using LongestPathProblem.Models;

namespace LongestPathProblem
{
    public static class IoConverter
    {
        public static Graph GraphFromLines(IEnumerable<string> inputLines)
        {
            var verticesAndNeighbours =
                inputLines
                    .Select(x => x.Split(' '))
                    .Select(x =>
                    {
                        var vertexIdStr = x.First();
                        var vertexId = int.Parse(vertexIdStr);

                        var neighbours = x.Skip(1).Select(int.Parse);
                        
                        return (VertexId: vertexId, Neihbours: neighbours);
                    })
                    .ToDictionary(x => x.VertexId, x => x.Neihbours);

            var vertices = verticesAndNeighbours.Keys.Select(v => new Vertex
            {
                Id = v
            }).ToDictionary(x=>x.Id, x=>x);
         
            foreach (var vertex in verticesAndNeighbours)
            {
                var (vertexId, vertexNeighbours) = vertex;
                var neighbours = vertexNeighbours.Select(x => vertices[x]);
                var currentVertex = vertices[vertexId];
                
                foreach (var neighbour in neighbours)
                {
                    currentVertex.Neighbours.Add(neighbour);
                }
            }

            return new Graph
            {
                Vertices = new HashSet<Vertex>(vertices.Values)
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