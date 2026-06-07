namespace SimplexMJE_Modular.Classes.Static
{
    internal static class LinearInequalitySolver
    {
        public static InequalitySystemSolution Solve(InequalitySystem A, GoalFunction Z, IComputationReportCompiler compiler = null)
        {
            if (A.Inequalities.Any(ineq => ineq.Sign == Sign.Equals))
                return SolveMixedSystem(A, Z, compiler);
            else
                return SolveNormal(A, Z, compiler);
        }

        public static InequalitySystemSolution SolveNormal(InequalitySystem A, GoalFunction Z, IComputationReportCompiler compiler = null)
        {
            compiler?.AddAction("Згенерований протокол обчислення", titleLevel: 4);
            compiler?.AddAction("Постановка задачі:", Z.ToString(), 0);
            compiler?.AddAction("При обмеженнях:", A.ToString(), 0);
            compiler?.AddAction("Перепишемо систему обмежень:", A.ToStringWithZeroes(), 0);

            var matrix = ConvertInputToMatrix(A, Z);
            matrix.RowMarkers[matrix.RowCount - 1] = "Z";
            compiler?.AddAction("Вхідна симлекс-таблиця:", matrix.ToStringWithDualMarkers(), 0);

            var solution1 = GetReferenceSolution(matrix, Z, compiler, A.VariableCount);

            if (solution1 == null)
                return new InequalitySystemSolution(new List<double>(), new Matrix(0, 0), Z.Type, 0, false, false, true);

            var solution2 = GetOptimalSolution(A, Z, solution1, compiler);
            if (solution2 == null)
                return solution1;
            compiler.AddAction("Знайдено оптимальний розв'язок для дуальної задачі:", solution2.ToStringDual(), titleLevel: 1);

            return solution2;
        }

        private static InequalitySystemSolution? GetReferenceSolution(Matrix matrix, GoalFunction Z, IComputationReportCompiler? compiler = null, int varCount = 0)
        {
            compiler?.AddAction("Пошук опорного розв'язку:", titleLevel: 2);

            var success = false;
            do
            {
                var negativeNumbersInConstantCol = FindNumberRowsInColumn(matrix, matrix.ColCount - 1, x => x < 0);
                if (!negativeNumbersInConstantCol.Any())
                {
                    success = true;
                    compiler?.AddAction("Опорний розв'язок знайдено!", titleLevel: 1);
                    break;
                }
                var firstNegNumPos = negativeNumbersInConstantCol.First();
                var solutionCols = FindNumberColsInRow(matrix, firstNegNumPos, x => x < 0).Where(col => col != matrix.ColCount - 1);
                if (!solutionCols.Any())
                {
                    success = false;
                    compiler?.AddAction("Помилка! Система обмежень є суперечливою!", titleLevel: 1);
                    throw new Exception("Помилка! Система обмежень є суперечливою!");
                }
                var firstSolutionCol = solutionCols.First();
                var col1 = matrix.GetColumnAsList(firstSolutionCol);
                var col2 = matrix.GetColumnAsList(matrix.ColCount - 1);
                var solutionRow = -1;
                var minimalNonNegativeRatio = double.MaxValue;
                for (int i = 0; i < matrix.RowCount - 1; i++)
                {
                    if (Math.Abs(col1[i]) <= Double.Epsilon) continue;
                    var ratio = col2[i] / col1[i];
                    var signum1 = double.IsNegative(col1[i]) ? -1 : 1;
                    var signum2 = double.Sign(col2[i]) == 0 ? 1 : double.Sign(col2[i]);
                    if (ratio < 0 || signum1 * signum2 < 0) continue;
                    if (ratio < minimalNonNegativeRatio)
                    {
                        minimalNonNegativeRatio = ratio;
                        solutionRow = i;
                    }
                }
                compiler?.AddAction($"Розв'язувальний рядок:{solutionRow} ({matrix.RowMarkers[solutionRow]})", titleLevel: 0);
                compiler?.AddAction($"Розв'язувальний стовпець:{firstSolutionCol} ({matrix.ColMarkers[firstSolutionCol]})", titleLevel: 0);

                matrix = matrix.JordanExcludeModified(solutionRow, firstSolutionCol);
                compiler?.AddAction("Таблиця після виконання МЖВ:", matrix.ToStringWithDualMarkers(), 0);
            } while (!success);
            if (!success) return null;
            var resultList = ParseSolution(matrix, varCount);
            var resultListDual = ParseSolutionDual(matrix, varCount);
            var result = new InequalitySystemSolution(resultList, resultListDual, matrix, Z.Type, Z.CalculateResultWithValues(resultList), false, false, false);
            compiler?.AddAction("Знайдено опорний розв'язок:", result.ToString(), titleLevel: 1);
            return result;
        }

        public static InequalitySystemSolution? GetReferenceSolutionStandalone(InequalitySystem A, GoalFunction Z, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Згенерований протокол обчислення", titleLevel: 4);
            compiler?.AddAction("Постановка задачі:", Z.ToString(), 0);
            compiler?.AddAction("При обмеженнях:", A.ToString(), 0);
            compiler?.AddAction("Перепишемо систему обмежень:", A.ToStringWithZeroes(), 0);
            var matrix = ConvertInputToMatrix(A, Z);
            matrix.RowMarkers[matrix.RowCount - 1] = "Z";
            compiler?.AddAction("Вхідна симлекс-таблиця:", matrix.ToStringWithMarkers(), 0);
            if (A.Inequalities.Any(ineq => ineq.Sign == Sign.Equals))
            {
                matrix = CrossOutZeroRows(matrix, compiler);
                matrix = CrossOutZeroCols(matrix);
            }
            var success = false;
            do
            {
                var negativeNumbersInConstantCol = FindNumberRowsInColumn(matrix, matrix.ColCount - 1, x => x < 0);
                if (!negativeNumbersInConstantCol.Any())
                {
                    success = true;
                    compiler?.AddAction("Опорний розв'язок знайдено!", titleLevel: 1);
                    break;
                }
                var firstNegNumPos = negativeNumbersInConstantCol.First();
                var solutionCols = FindNumberColsInRow(matrix, firstNegNumPos, x => x < 0).Where(col => col != matrix.ColCount - 1);
                if (!solutionCols.Any())
                {
                    success = false;
                    compiler?.AddAction("Помилка! Система обмежень є суперечливою!", titleLevel: 1);
                    throw new Exception("Помилка! Система обмежень є суперечливою!");
                }
                var firstSolutionCol = solutionCols.First();
                var col1 = matrix.GetColumnAsList(firstSolutionCol);
                var col2 = matrix.GetColumnAsList(matrix.ColCount - 1);
                var solutionRow = -1;
                var minimalNonNegativeRatio = double.MaxValue;
                for (int i = 0; i < matrix.RowCount - 1; i++)
                {
                    if (Math.Abs(col1[i]) <= Double.Epsilon) continue;
                    var ratio = col2[i] / col1[i];
                    var signum1 = double.IsNegative(col1[i]) ? -1 : 1;
                    var signum2 = double.Sign(col2[i]) == 0 ? 1 : double.Sign(col2[i]);
                    if (ratio < 0 || signum1 * signum2 < 0) continue;
                    if (ratio < minimalNonNegativeRatio)
                    {
                        minimalNonNegativeRatio = ratio;
                        solutionRow = i;
                    }
                }
                compiler?.AddAction($"Розв'язувальний рядок:{solutionRow} ({matrix.RowMarkers[solutionRow]})", titleLevel: 0);
                compiler?.AddAction($"Розв'язувальний стовпець:{firstSolutionCol} ({matrix.ColMarkers[firstSolutionCol]})", titleLevel: 0);

                matrix = matrix.JordanExcludeModified(solutionRow, firstSolutionCol);
                compiler?.AddAction("Таблиця після виконання МЖВ:", matrix.ToStringWithMarkers(), 0);
            } while (!success);
            if (!success) return null;
            var resultList = ParseSolution(matrix, A.VariableCount);
            var resultListDual = ParseSolutionDual(matrix, A.Inequalities.Count);
            var result = new InequalitySystemSolution(resultList, resultListDual, matrix, Z.Type, Z.CalculateResultWithValues(resultList), false, false, false);
            compiler?.AddAction("Знайдено опорний розв'язок:", result.ToString(), titleLevel: 1);
            return result;
        }

        public static InequalitySystemSolution? GetOptimalSolution(InequalitySystem A, GoalFunction Z, InequalitySystemSolution referenceSolution, IComputationReportCompiler? compiler = null)
        {
            var matrix = referenceSolution.SolutionMatrix;
            compiler?.AddAction("Пошук оптимального розв'язку:", titleLevel: 2);
            var success = false;
            do
            {
                var negativeNumbersInZRow = FindNumberColsInRow(matrix, matrix.RowCount - 1, x => x < 0 && Math.Abs(x) > 0.01);
                if (!negativeNumbersInZRow.Any())
                {
                    success = true;
                    compiler?.AddAction("Оптимальний розв'язок вже знайдено!", titleLevel: 1);
                    break;
                }
                var firstSolutionCol = negativeNumbersInZRow.First();

                var col1 = matrix.GetColumnAsList(firstSolutionCol);
                var col2 = matrix.GetColumnAsList(matrix.ColCount - 1);
                var solutionRow = -1;
                var minimalNonNegativeRatio = double.MaxValue;
                for (int i = 0; i < matrix.RowCount - 1; i++)
                {
                    if (Math.Abs(col1[i]) <= Double.Epsilon) continue;
                    var ratio = col2[i] / col1[i];
                    var signum1 = double.IsNegative(col1[i]) ? -1 : 1;
                    var signum2 = double.Sign(col2[i]) == 0 ? 1 : double.Sign(col2[i]);
                    if (ratio < 0 || signum1 * signum2 < 0) continue;
                    if (ratio < minimalNonNegativeRatio)
                    {
                        minimalNonNegativeRatio = ratio;
                        solutionRow = i;
                    }
                }

                if (solutionRow == -1)
                {
                    success = false;
                    compiler?.AddAction("Помилка! Функція не обмежена зверху!", titleLevel: 1);
                    throw new Exception("Помилка! Функція не обмежена зверху!");
                }
                compiler?.AddAction($"Розв'язувальний рядок:{solutionRow} ({matrix.RowMarkers[solutionRow]})", titleLevel: 0);
                compiler?.AddAction($"Розв'язувальний стовпець:{firstSolutionCol} ({matrix.ColMarkers[firstSolutionCol]})", titleLevel: 0);

                matrix = matrix.JordanExcludeModified(solutionRow, firstSolutionCol);
                compiler?.AddAction("Таблиця після виконання МЖВ:", matrix.ToStringWithDualMarkers(), 0);

            } while (!success);
            if (!success) throw new Exception("Оптимальне рішення не знайдено!");
            var resultList = ParseSolution(matrix, A.VariableCount);
            var resultListDual = ParseSolutionDual(matrix, A.Inequalities.Count);
            var result = new InequalitySystemSolution(resultList, resultListDual, matrix, Z.Type, Z.CalculateResultWithValues(resultList), true, false, false);
            compiler?.AddAction("Знайдено оптимальний розв'язок:", result.ToString(), titleLevel: 1);
            return result;
        }

        private static List<double> ParseSolution(Matrix matrix, int varCount)
        {
            var solution = new List<double>(Enumerable.Repeat(0d, varCount));
            matrix.RoundToDecimalPlaces(3);
            for (int i = 0; i < matrix.RowCount - 1; i++)
            {
                var rowMarker = matrix.RowMarkers[i];
                if (rowMarker.StartsWith("x"))
                {
                    var variableIndex = int.Parse(rowMarker.Substring(1));
                    solution[variableIndex - 1] = matrix[i, matrix.ColCount - 1];
                }
            }
            return solution;
        }

        private static List<double> ParseSolutionDual(Matrix matrix, int varCount)
        {
            var solution = new List<double>(Enumerable.Repeat(0d, varCount));
            matrix.RoundToDecimalPlaces(3);
            for (int i = 0; i < matrix.ColCount - 1; i++)
            {
                var colMarker = matrix.ColMarkersDual[i];
                if (colMarker.StartsWith("u"))
                {
                    var variableIndex = int.Parse(colMarker.Substring(1));
                    solution[variableIndex - 1] = matrix[matrix.RowCount - 1, i];
                }
            }
            return solution;
        }

        private static Matrix ConvertInputToMatrix(InequalitySystem A, GoalFunction Z)
        {
            var matrix = A.ConvertToMatrix();
            var zRow = Z.ConvertToMatrix();
            matrix.AddRow(zRow[0], "Z", "1");
            return matrix;
        }

        private static IEnumerable<int> FindNumberPositionsInList(List<double> list, Func<double, bool> predicate)
        {
            return list.Select((value, index) => new { value, index }).Where(x => predicate(x.value)).Select(x => x.index);
        }

        private static IEnumerable<int> FindNumberRowsInColumn(Matrix A, int col, Func<double, bool> predicate)
        {
            var column = A.GetColumnAsList(col);
            return FindNumberPositionsInList(column, predicate);
        }

        private static IEnumerable<int> FindNumberColsInRow(Matrix A, int row, Func<double, bool> predicate)
        {
            return FindNumberPositionsInList(A[row], predicate);
        }

        public static InequalitySystemSolution SolveMixedSystem(InequalitySystem A, GoalFunction Z, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Згенерований протокол обчислення", titleLevel: 4);
            compiler?.AddAction("Постановка задачі:", Z.ToString(), 0);
            compiler?.AddAction("При обмеженнях:", A.ToString(), 0);
            compiler?.AddAction("Перепишемо систему обмежень:", A.ToStringWithZeroes(), 0);
            var matrix = ConvertInputToMatrix(A, Z);
            matrix.RowMarkers[matrix.RowCount - 1] = "Z";
            compiler?.AddAction("Вхідна симлекс-таблиця:", matrix.ToStringWithDualMarkers(), 0);
            var crossedOutZeroRowsMatrix = CrossOutZeroRows(matrix, compiler);
            var dualFormulas = ParseDualFormulas(crossedOutZeroRowsMatrix, crossedOutZeroRowsMatrix.ColCount - 1);
            var crossedOutZeroColsMatrix = CrossOutZeroCols(crossedOutZeroRowsMatrix);
            var solution1 = GetReferenceSolution(crossedOutZeroColsMatrix, Z, compiler, A.VariableCount);

            if (solution1 == null)
                return new InequalitySystemSolution(new List<double>(), new Matrix(0, 0), Z.Type, 0, false, false, true);

            var solution2 = GetOptimalSolution(A, Z, solution1, compiler);
            if (solution2 == null)
                return solution1;
            var solutions = new List<double>(solution2.SolutionCoefficientsDual);
            solutions.Add(1);

            for (int i = dualFormulas.Count - 1; i >= 0; i--)
            {
                var formula = dualFormulas[i];
                var l = formula.Count;
                var sum = 0d;
                var str = $"U[{i + 1}] = ";
                for (int j = l - 1; j >= 0; j--)
                {
                    sum += formula[j] * solutions[j];
                    str += $"({Math.Round(formula[j], 3)} * ({Math.Round(solutions[j], 3)}))";
                    if (j == 0)
                    {
                        str += " = ";
                        continue;
                    }
                    str += " + ";
                }
                str += Math.Round(sum, 3);
                compiler?.AddAction(str, titleLevel: 0);
                if (sum != 0)
                    solutions[i] = sum;
                else
                    solutions[i] = solution2.SolutionCoefficientsDual[i];
            }
            solutions.RemoveAt(solutions.Count - 1);
            solution2.SolutionCoefficientsDual = solutions;
            compiler.AddAction("Знайдено оптимальний розв'язок для дуальної задачі:", solution2.ToStringDual(), titleLevel: 1);

            return solution2;
        }

        public static Matrix CrossOutZeroRows(InequalitySystem A, GoalFunction Z, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Згенерований протокол обчислення", titleLevel: 4);
            compiler?.AddAction("Постановка задачі:", Z.ToString(), 0);
            compiler?.AddAction("При обмеженнях:", A.ToString(), 0);
            compiler?.AddAction("Перепишемо систему обмежень:", A.ToStringWithZeroes(), 0);
            var matrix = ConvertInputToMatrix(A, Z);
            matrix.RowMarkers[matrix.RowCount - 1] = "Z";
            compiler?.AddAction("Вхідна симлекс-таблиця:", matrix.ToStringWithMarkers(), 0);
            var crossedOutZeroRowsMatrix = CrossOutZeroRows(matrix, compiler);
            compiler?.AddAction("Вихідна симлекс-таблиця:", crossedOutZeroRowsMatrix.ToStringWithMarkers(), 0);
            return crossedOutZeroRowsMatrix;
        }

        private static Matrix CrossOutZeroRows(Matrix matrix, IComputationReportCompiler? compiler = null)
        {
            matrix = matrix.DeepCopy();
            var success = false;
            do
            {
                var zeroRows = matrix.RowMarkers.Select((marker, index) => new { marker, index }).Where(x => x.marker.StartsWith("0")).Select(x => x.index);
                if (!zeroRows.Any())
                {
                    success = true;
                    break;
                }
                int firstSolutionCol = -1;

                foreach (var row in zeroRows)
                {
                    var positiveNumbersInCoefRow = FindNumberColsInRow(matrix, row, x => x > 0);
                    positiveNumbersInCoefRow = positiveNumbersInCoefRow.Where(positiveNumbersInCoefRow => !matrix.ColMarkers[positiveNumbersInCoefRow].StartsWith("0")).ToList();
                    firstSolutionCol = positiveNumbersInCoefRow.Any() ? positiveNumbersInCoefRow.First() : firstSolutionCol;
                }
                if (firstSolutionCol == -1)
                {
                    success = false;
                    compiler?.AddAction("Помилка! Система обмежень є суперечливою!", titleLevel: 1);
                    throw new Exception("Система обмежень є суперечливою!");
                }
                var col1 = matrix.GetColumnAsList(firstSolutionCol);
                var col2 = matrix.GetColumnAsList(matrix.ColCount - 1);
                var solutionRow = -1;
                var minimalNonNegativeRatio = double.MaxValue;
                for (int i = 0; i < matrix.RowCount - 1; i++)
                {
                    if (Math.Abs(col1[i]) <= Double.Epsilon) continue;
                    var ratio = col2[i] / col1[i];
                    var signum1 = double.IsNegative(col1[i]) ? -1 : 1;
                    var signum2 = double.Sign(col2[i]) == 0 ? 1 : double.Sign(col2[i]);
                    if (ratio < 0 || signum1 * signum2 < 0) continue;
                    if (ratio < minimalNonNegativeRatio)
                    {
                        minimalNonNegativeRatio = ratio;
                        solutionRow = i;
                    }
                }
                if (solutionRow == -1)
                {
                    success = true;
                    break;
                }

                compiler?.AddAction($"Розв'язувальний рядок:{solutionRow} ({matrix.RowMarkers[solutionRow]})", titleLevel: 0);
                compiler?.AddAction($"Розв'язувальний стовпець:{firstSolutionCol} ({matrix.ColMarkers[firstSolutionCol]})", titleLevel: 0);

                matrix = matrix.JordanExcludeModified(solutionRow, firstSolutionCol);
                compiler?.AddAction("Таблиця після виконання МЖВ:", matrix.ToStringWithDualMarkers(), 0);
            } while (!success);
            if (!success) return null;

            compiler?.AddAction("Вихідна симлекс-таблиця:", matrix.ToStringWithDualMarkers(), 0);
            return matrix;
        }

        private static Matrix CrossOutZeroCols(Matrix matrix)
        {
            matrix = matrix.DeepCopy();
            var zeroCols = matrix.ColMarkers.Select((marker, index) => new { marker, index }).Where(x => x.marker.StartsWith("0")).Select(x => x.index).ToList();
            while (zeroCols.Any())
            {
                var colToRemove = zeroCols.First();
                matrix.RemoveColumn(colToRemove);
                zeroCols = matrix.ColMarkers.Select((marker, index) => new { marker, index }).Where(x => x.marker.StartsWith("0")).Select(x => x.index).ToList();
            }
            return matrix;
        }

        private static List<List<double>> ParseDualFormulas(Matrix matrix, int varCount)
        {
            var solution = new List<List<double>>();
            for (int i = 0; i < varCount; i++)
                solution.Add(new List<double>(Enumerable.Repeat(0d, varCount + 1)));
            matrix.RoundToDecimalPlaces(3);
            for (int i = 0; i < matrix.ColCount - 1; i++)
            {
                var colMarker = matrix.ColMarkersDual[i];
                if (colMarker.StartsWith("u"))
                {
                    var variableIndex = int.Parse(colMarker.Substring(1));
                    for (int j = 0; j < matrix.RowCount; j++)
                    {
                        if (matrix.RowMarkersDual[j].StartsWith("u"))
                        {
                            var variableIndex2 = int.Parse(matrix.RowMarkersDual[j].Substring(1));
                            solution[variableIndex - 1][variableIndex2 - 1] = matrix[j, i];
                        }
                    }
                    solution[variableIndex - 1][varCount] = matrix[matrix.RowCount - 1, i];
                }
            }
            return solution;
        }
    }
}
