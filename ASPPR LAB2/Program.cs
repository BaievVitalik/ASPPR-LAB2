using System;
using System.Globalization;
using SimplexMJE_Modular.Classes.Static;

namespace SimplexMJE_Modular
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Програма для розв'язання задач ЛП методом МЖВ ===");

            int n = InputHandler.ReadInt("Введіть кількість змінних (n): ");
            int m = InputHandler.ReadInt("Введіть кількість обмежень (m): ");
            bool isMax = InputHandler.ReadGoal("Введіть тип цільової функції (1 - max, 2 - min): ");

            double[,] rawConstraints = new double[m, n + 1];
            bool[] isGreaterOrEqual = new bool[m];

            Console.WriteLine("\n--- Ввід системи обмежень ---");
            Console.WriteLine("Вводьте коефіцієнти кожного обмеження через пробіл (включаючи вільний член b).");

            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"\nОбмеження {i + 1}:");
                double[] rowInput = InputHandler.ReadDoubleArray(n + 1, $"Введіть {n} коефіцієнтів та число b: ");
                for (int j = 0; j <= n; j++)
                {
                    rawConstraints[i, j] = rowInput[j];
                }
                isGreaterOrEqual[i] = InputHandler.ReadBoolean("Змінити знак цього обмеження з <= на >= ? (т/н): ");
            }

            Console.WriteLine("\n--- Ввід цільової функції (Z) ---");
            double[] rawZ = InputHandler.ReadDoubleArray(n, $"Введіть {n} коефіцієнтів цільової функції через пробіл: ");

            Console.WriteLine("\n");
            Console.WriteLine("Обчислення:\n");
            ReportPrinter.PrintProblemStatement(rawConstraints, isGreaterOrEqual, rawZ, isMax, m, n);

            InequalitySystem inequalitySystem = new InequalitySystem();
            for (int i = 0; i < m; i++)
            {
                var coeffs = new List<double>();
                for (int j = 0; j < n; j++)
                    coeffs.Add(rawConstraints[i, j]);
                var sign = isGreaterOrEqual[i] ? Sign.GreaterOrEqual : Sign.LessOrEqual;
                var inequality = new Inequality(coeffs, rawConstraints[i, n], sign);
                inequalitySystem.AddInequality(inequality);
            }

            var goalFunctionCoeffs = new List<double>(rawZ);
            var goalFunctionType = isMax ? GoalFunctionType.Maximize : GoalFunctionType.Minimize;
            var goalFunction = new GoalFunction(goalFunctionCoeffs, goalFunctionType);

            var compiler = new ComputationReport();

            try
            {
                var solution = LinearInequalitySolver.Solve(inequalitySystem, goalFunction, compiler);

                if (!solution.IsInfeasible)
                {
                    Console.WriteLine(compiler.Compile());
                    Console.WriteLine("\nЗнайдено оптимальний розв'язок:\n");
                    ReportPrinter.PrintX(solution.SolutionCoefficients, m, n);
                    ReportPrinter.PrintFinalZ(solution.GoalFunctionValue, isMax, m, n);

                    if (solution.SolutionCoefficientsDual != null && solution.SolutionCoefficientsDual.Count > 0)
                    {
                        Console.WriteLine("\nДуальне розв'язання:");
                        Console.WriteLine(solution.ToStringDual());
                    }
                }
                else
                {
                    Console.WriteLine("\nСистема обмежень є суперечливою.\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nПомилка при розв'язанні: {ex.Message}\n");
            }

            Console.ReadLine();
        }
    }
}