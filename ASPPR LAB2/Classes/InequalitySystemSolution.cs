namespace SimplexMJE_Modular
{
    internal class InequalitySystemSolution
    {
        public List<double> SolutionCoefficients { get; set; }
        public List<double> SolutionCoefficientsDual { get; set; }
        public double GoalFunctionValue { get; set; }
        public GoalFunctionType GoalFunctionType { get; set; }
        public bool IsOptimal { get; set; }
        public bool IsUnbounded { get; set; }
        public bool IsInfeasible { get; set; }
        public bool isInteger { get; set; }
        public Matrix? SolutionMatrix { get; set; }

        public InequalitySystemSolution(List<double> solution, Matrix solutionMatrix, GoalFunctionType type, double goalFunctionValue, bool isOptimal, bool isUnbounded, bool isInfeasible)
        {
            SolutionCoefficients = solution;
            GoalFunctionValue = goalFunctionValue;
            GoalFunctionType = type;
            IsOptimal = isOptimal;
            IsUnbounded = isUnbounded;
            IsInfeasible = isInfeasible;
            SolutionMatrix = solutionMatrix;
            SolutionCoefficientsDual = new List<double>();
        }

        public InequalitySystemSolution(List<double> solution, List<double> solutionDual, Matrix solutionMatrix, GoalFunctionType type, double goalFunctionValue, bool isOptimal, bool isUnbounded, bool isInfeasible)
        {
            SolutionCoefficients = solution;
            GoalFunctionValue = goalFunctionValue;
            GoalFunctionType = type;
            IsOptimal = isOptimal;
            IsUnbounded = isUnbounded;
            IsInfeasible = isInfeasible;
            SolutionMatrix = solutionMatrix;
            SolutionCoefficientsDual = solutionDual;
        }

        public bool IsSolutionInteger()
        {
            return SolutionCoefficients.All(c => Math.Abs(c % 1) <= 0.01);
        }

        public override string ToString()
        {
            return $"X = ({string.Join("; ", SolutionCoefficients.Select((c, i) => $"{c}"))}) {(IsOptimal ? $"{GoalFunctionType}(Z) = {GoalFunctionValue}" : "")}";
        }

        public string ToStringDual()
        {
            return $"U = ({string.Join("; ", SolutionCoefficientsDual.Select((c, i) => $"{c}"))}) {(IsOptimal ? $"{(GoalFunctionType == GoalFunctionType.Maximize ? GoalFunctionType.Minimize : GoalFunctionType.Maximize)}(W) = {GoalFunctionValue}" : "")}";
        }
    }
}
