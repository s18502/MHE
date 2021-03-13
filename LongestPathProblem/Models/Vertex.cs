using System.Collections.Generic;

namespace LongestPathProblem.Models
{
    public record Vertex
    {
        public int Id { get; init; }
        public HashSet<Vertex> Neighbours { get; init; } = new();
    }
}