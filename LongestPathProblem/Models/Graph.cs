using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable PossibleMultipleEnumeration

namespace LongestPathProblem.Models
{
    public record Graph
    {
        public HashSet<Vertex> Vertices { get; init; } = new();
        private Dictionary<int, Vertex> _verticiesById => 
            Vertices.ToDictionary(x => x.Id, x => x);

        public GraphPath GetRandomPath()
        {
            var random = new Random();
            var currentVertex = Vertices.OrderBy(_ => random.Next()).First();

            var visitedVertices = new List<int>(new[] {currentVertex.Id});

            while (currentVertex.Neighbours.Any())
            {
                var randomNeigbours = currentVertex.Neighbours
                    .Except(visitedVertices)
                    .OrderBy(_ => random.Next());

                if (!randomNeigbours.Any())
                    return ToGraphPath(visitedVertices);

                var nextVertexId = randomNeigbours.First();
                visitedVertices.Add(nextVertexId);

                currentVertex = _verticiesById[nextVertexId];
            }

            return ToGraphPath(visitedVertices);
        }

        public bool IsPathValid(GraphPath path)
        {
            if (path.Vertices.Count < 2)
                //Nie jest sciezka
                return false;
            
            if (path.Vertices.GroupBy(x => x).Any(x => x.Count() > 1))
                // ma powtarzajace sie wierzcholki
                return false;
            
            var verticesQueue = new Queue<Vertex>(
                path.Vertices.Select(x=>_verticiesById[x]));

            if (verticesQueue.Count == 0)
                return true;
            
            var currentVertex = verticesQueue.Dequeue();
            
            while (verticesQueue.TryDequeue(out var nextVertex))
            {
                if (!currentVertex.Neighbours.Contains(nextVertex.Id))
                    return false;

                currentVertex = nextVertex;
            }

            return true;
        }

        public GraphPath NextDeterministicPath(GraphPath path)
        {
            var radix = Vertices.Count;

            var maxPath = Enumerable.Range(1, Vertices.Count)
                .Reverse()
                .ToDecimalArbitrarySystem(radix);
            
            var pathAsNumber = path.Vertices.ToDecimalArbitrarySystem(radix);
            var nextPathAsNumber = (pathAsNumber + 1) % maxPath;

            while (true)
            {
                var nextPathVertices = nextPathAsNumber.ToArbitrarySystem(radix).ToList();
                var nextPath = new GraphPath {Vertices = nextPathVertices};
                if (IsPathValid(nextPath))
                    return nextPath;
                
                nextPathAsNumber = (nextPathAsNumber + 1) % maxPath;
            }
        }

        private static GraphPath ToGraphPath(List<int> visitedVertices)
        {
            return new()
            {
                Vertices = visitedVertices.ToList()
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