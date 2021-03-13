using System.Collections.Generic;
using System.Linq;

namespace LongestPathProblem.Models
{
    public record GraphPath
    {
        public List<int> Vertices { get; init; } = new();

        public int Length => Vertices.Count;

        public override string ToString()
        {
            return $"[{string.Join(",", Vertices)}]";
        }

        public virtual bool Equals(GraphPath other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Vertices.SequenceEqual(other.Vertices);
        }

        public override int GetHashCode()
        {
            return (Vertices != null ? Vertices.GetHashCode() : 0);
        }
    }
}