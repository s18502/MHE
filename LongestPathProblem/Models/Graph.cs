using System;
using System.Collections.Generic;
using System.Linq;

namespace LongestPathProblem.Models
{
    public record Graph
    {
        public HashSet<Vertex> Vertices { get; init; } = new();
        private Dictionary<int, Vertex> _verticiesById => Vertices.ToDictionary(x => x.Id, x => x);

        public GraphPath GetRandomPath()
        {
            var random = new Random();
            var currentVertex = Vertices.OrderBy(_ => random.Next()).First();

            var visitedVertices = new List<Vertex>(new[] {currentVertex});

            while (currentVertex.Neighbours.Any())
            {
                var nextVertex = currentVertex.Neighbours
                    .Except(visitedVertices)
                    .OrderBy(_ => random.Next())
                    .FirstOrDefault();

                if (nextVertex == default)
                    return ToGraphPath(visitedVertices);
                
                visitedVertices.Add(nextVertex);

                currentVertex = nextVertex;
            }

            return ToGraphPath(visitedVertices);
        }

        public bool IsPathValid(GraphPath path)
        {
            var verticesQueue = new Queue<Vertex>(
                path.Vertices.Select(x=>_verticiesById[x]));

            if (verticesQueue.Count == 0)
                return true;
            
            var currentVertex = verticesQueue.Dequeue();
            
            while (verticesQueue.TryDequeue(out var nextVertex))
            {
                if (!currentVertex.Neighbours.Contains(nextVertex))
                    return false;

                currentVertex = nextVertex;
            }

            return true;
        }
        
        private static GraphPath ToGraphPath(List<Vertex> visitedVertices)
        {
            return new GraphPath
            {
                Vertices = visitedVertices.Select(x=>x.Id).ToList()
            };
        }

        public virtual bool Equals(Graph other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Vertices.OrderBy(x => x.Id).SequenceEqual(other.Vertices.OrderBy(x => x.Id));
        }

        public override int GetHashCode()
        {
            return (Vertices != null ? Vertices.GetHashCode() : 0);
        }
    }
}