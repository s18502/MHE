using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using LongestPathProblem.Helpers;

// ReSharper disable PossibleMultipleEnumeration

namespace LongestPathProblem.Models
{
    public record Graph
    {
        public HashSet<Vertex> Vertices { get; private init; }
        private readonly Dictionary<int, Vertex> _verticiesById; 

        public Graph(IEnumerable<Vertex> vertices)
        {
            Vertices = new HashSet<Vertex>(vertices);
            _verticiesById = Vertices.ToDictionary(x => x.Id, x => x); 
        }

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

        private static readonly ObjectPool<Queue<int>> QueuePool = new(() => new Queue<int>());
        public bool IsPathValid(GraphPath path)
        {
            var verticesQueue = QueuePool.Get();
            verticesQueue.Clear();

            try
            {
                if (path.Vertices.Length < 2)
                    //Nie jest sciezka
                    return false;

                if (path.Vertices.Distinct().Count() != path.Vertices.Count())
                    return false;
                
                foreach (var vertex in path.Vertices)
                {
                    verticesQueue.Enqueue(vertex);
                }

                if (verticesQueue.Count == 0)
                    return true;

                var currentVertexId = verticesQueue.Dequeue();

                while (verticesQueue.TryDequeue(out var nextVertexId))
                {
                    if (!_verticiesById.TryGetValue(currentVertexId, out var currentVertex) 
                        || !currentVertex.Neighbours.Contains(nextVertexId))
                        return false;

                    currentVertexId = nextVertexId;
                }

                return true;
            }
            finally
            {
                QueuePool.Return(verticesQueue);
            }
        }

        public GraphPath GetNextDeterministicPath(GraphPath path)
        {
            var radix = Vertices.Count + 1;

            var maxPath = Enumerable.Range(1, Vertices.Count)
                .Reverse()
                .ToDecimalArbitrarySystem(radix);
            
            var pathAsNumber = path.Vertices.Select(v => v + 1).ToDecimalArbitrarySystem(radix);
            var nextPathAsNumber = (pathAsNumber + 1) % maxPath;

            while (true)
            {
                var nextPathVertices = nextPathAsNumber
                    .ToArbitrarySystem(radix)
                    .ToArray();

                nextPathAsNumber = (nextPathAsNumber + 1) % maxPath;
                
                if (nextPathVertices.Contains(0))
                    continue;

                nextPathVertices = nextPathVertices.Select(x => x - 1).ToArray();
                if(nextPathVertices.Any(x=>!_verticiesById.ContainsKey(x)))
                    continue;
                
                var nextPath = new GraphPath(nextPathVertices);
                if (IsPathValid(nextPath))
                    return nextPath;
            }
        }   
        public GraphPath RandomModifyPath(GraphPath path, int maxModifyVertices = 1)
        {
            var vertices = path.Vertices.Select(vId => _verticiesById[vId]).ToList();
            var verticesReversed = new List<Vertex>(vertices);
            verticesReversed.Reverse();

            var partialPaths = Enumerable.Range(0, path.Vertices.Length)
                .Select(x => path.Vertices.Take(x).ToList())
                .ToList();
            
            var startingVertex = verticesReversed
                .Zip(partialPaths)
                .Where(tpl=>
                {
                    var (vertex, pathSoFar) = tpl;
                    return vertex.Neighbours.Except(pathSoFar).Any();
                })
                .Skip(maxModifyVertices)
                .Select(x=>x.First)
                .FirstOrDefault();

            if (startingVertex == default)
                return GetRandomPath();

            var startingVertexIdx = vertices.IndexOf(startingVertex);

            var newPath = path.Vertices.Take(startingVertexIdx + 1).ToList();
            var currentVertex = startingVertex;
            var r = new Random();

            while (currentVertex.Neighbours.Any())
            {
                var possibleNeighbours = currentVertex.Neighbours
                    .Except(newPath)
                    .OrderBy(_ => r.Next());

                if ((r.Next() % (path.Length * 2) == 0 && newPath.Count() > 1) || possibleNeighbours.Any() == false)
                {
                    return new GraphPath(newPath.ToArray());
                }

                var randomNeighbour = possibleNeighbours.First();
                newPath.Add(randomNeighbour);
                currentVertex = _verticiesById[randomNeighbour];
            }

            return new GraphPath(newPath.ToArray());
        }

        public IEnumerable<GraphPath> AllPaths()
        {
            var radix = Vertices.Count + 1;

            var maxPath = Enumerable.Range(1, Vertices.Count)
                .Reverse()
                .ToDecimalArbitrarySystem(radix);

            for (BigInteger pathAsNumber = 1 ; pathAsNumber <= maxPath; pathAsNumber++)
            {
                var pathVertices = pathAsNumber
                    .ToArbitrarySystem(radix)
                    .Select(x=> x - 1)
                    .ToArray();

                var nextPath = new GraphPath(pathVertices);
                if (IsPathValid(nextPath))
                    yield return nextPath;
            }
        }

        private static GraphPath ToGraphPath(List<int> visitedVertices)
        {
            return new(visitedVertices.ToArray());
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