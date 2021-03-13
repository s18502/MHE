using LongestPathProblem.Models;

namespace LongestPathProblem
{
    public class Heuristic
    {
        public static int Goal(GraphPath path) => path.Length;
    }
}