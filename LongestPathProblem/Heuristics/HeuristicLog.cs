namespace LongestPathProblem.Heuristics
{
    public record HeuristicLog
    {
        public int Iteration { get; init; }
        public int Goal { get; init; }
    }
}