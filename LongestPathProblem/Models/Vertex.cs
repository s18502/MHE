using System.Collections.Generic;

namespace LongestPathProblem.Models
{
    public record Vertex
    {
        public int Id { get; init; }
        public List<int> Neighbours { get; init; } = new();
    }
}