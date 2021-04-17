using System.Collections.Generic;
using System.Linq;

namespace LongestPathProblem.Models
{
    public struct GraphPath
    {
        public GraphPath(int[] vertices)
        {
            Vertices = vertices;
        }

        public int[] Vertices { get; private set; }

        public int Length => Vertices.Length;

        public override string ToString()
        {
            return $"[{string.Join(",", Vertices)}]";
        }

        public bool Equals(GraphPath other)
        {
            return Vertices.SequenceEqual(other.Vertices);
        }

        public override int GetHashCode()
        {
            return (Vertices != null ? Vertices.GetHashCode() : 0);
        }
    }
}