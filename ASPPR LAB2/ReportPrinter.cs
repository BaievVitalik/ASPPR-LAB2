using System;
using System.Globalization;

namespace SimplexMJE_Modular
{
    static class ReportPrinter
    {
        public static void PrintProblemStatement(double[,] rawConstraints, bool[] isGreaterOrEqual, double[] rawZ, bool isMax, int m, int n)
        {
            Console.WriteLine("Постановка задачі:\n");

            string zFunc = "Z = ";
            bool first = true;
            for (int j = 0; j < n; j++)
            {
                double coef = rawZ[j];
                if (Math.Abs(coef) > 1e-9)
                {
                    if (!first && coef > 0) zFunc += "+";
                    if (coef == 1 && first) zFunc += $"x{j + 1}";
                    else if (coef == 1) zFunc += $"x{j + 1}";
                    else if (coef == -1) zFunc += $"-x{j + 1}";
                    else zFunc += $"{coef}x{j + 1}";
                    first = false;
                }
            }
            if (first) zFunc += "0";
            zFunc += isMax ? " -> max\n" : " -> min\n";
            Console.WriteLine(zFunc);

            Console.WriteLine("при обмеженнях:\n");
            for (int i = 0; i < m; i++)
            {
                string constraint = "";
                first = true;
                for (int j = 0; j < n; j++)
                {
                    double coef = rawConstraints[i, j];
                    if (Math.Abs(coef) > 1e-9)
                    {
                        if (!first && coef > 0) constraint += "+";
                        if (coef == 1 && first) constraint += $"x{j + 1}";
                        else if (coef == 1) constraint += $"x{j + 1}";
                        else if (coef == -1) constraint += $"-x{j + 1}";
                        else constraint += $"{coef}x{j + 1}";
                        first = false;
                    }
                }
                double b = rawConstraints[i, n];
                string sign = isGreaterOrEqual[i] ? ">=" : "<=";
                constraint += $"{sign}{b}";
                Console.WriteLine(constraint);
            }
            Console.WriteLine($"x[j]>=0, j=1,{n}\n");
        }

public static void PrintX(List<double> solution, int m, int n)
        {
            string[] xStrs = new string[n];
            for (int j = 0; j < n; j++)
            {
                double v = solution[j];
                if (Math.Abs(v) < 1e-9) v = 0.0;
                xStrs[j] = v.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
            }
            Console.WriteLine($"X = ({string.Join("; ", xStrs)})\n");
        }

        public static void PrintFinalZ(double value, bool isMax, int m, int n)
        {
            double optimalZValue = value;
            if (Math.Abs(optimalZValue) < 1e-9) optimalZValue = 0.0;
            string optimalZStr = optimalZValue.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
            Console.WriteLine($"{(isMax ? "Max" : "Min")} (Z) = {optimalZStr}");
        }

        public static void PrintTable(double[,] A, string[] rowH, string[] colH, int m, int n)
        {
            Console.Write("     ");
            for (int j = 0; j <= n; j++) Console.Write($"{colH[j],9}");
            Console.WriteLine();
            Console.WriteLine(new string('-', 5 + 9 * (n + 1)));

            for (int i = 0; i <= m; i++)
            {
                string prefix = (i == m) ? "Z  =" : $"{rowH[i],2} =";
                Console.Write(prefix);
                for (int j = 0; j <= n; j++)
                {
                    double v = A[i, j];
                    if (Math.Abs(v) < 1e-9) v = 0.0;

                    string val = v.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
                    Console.Write($"{val,9}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static void PrintX(double[,] A, string[] rowH, int m, int n)
        {
            double[] X = new double[n];
            for (int i = 0; i < m; i++)
            {
                if (rowH[i].StartsWith("x"))
                {
                    int index = int.Parse(rowH[i].Substring(1)) - 1;
                    X[index] = A[i, n];
                }
            }

            string[] xStrs = new string[n];
            for (int j = 0; j < n; j++)
            {
                double v = X[j];
                if (Math.Abs(v) < 1e-9) v = 0.0;
                xStrs[j] = v.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
            }
            Console.WriteLine($"X = ({string.Join("; ", xStrs)})\n");
        }

        public static void PrintFinalZ(double[,] A, bool isMax, int m, int n)
        {
            double optimalZValue = isMax ? A[m, n] : -A[m, n];
            if (Math.Abs(optimalZValue) < 1e-9) optimalZValue = 0.0;
            string optimalZStr = optimalZValue.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
            Console.WriteLine($"{(isMax ? "Max" : "Min")} (Z) = {optimalZStr}");
        }
    }
}
