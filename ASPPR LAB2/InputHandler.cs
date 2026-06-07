using System;
using System.Globalization;

namespace SimplexMJE_Modular
{
    static class InputHandler
    {
        public static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? line = Console.ReadLine();
                if (int.TryParse(line, out int result) && result > 0)
                    return result;
                Console.WriteLine("Помилка: введіть коректне ціле додатне число.");
            }
        }

        public static bool ReadGoal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? line = Console.ReadLine();
                if (int.TryParse(line, out int result) && (result == 1 || result == 2))
                    return result == 1;
                Console.WriteLine("Помилка: введіть 1 або 2.");
            }
        }

        public static bool ReadBoolean(string prompt)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
            return input == "т" || input == "y" || input == "так" || input == "yes";
        }

        public static double[] ReadDoubleArray(int expectedLength, string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Replace(',', '.').Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != expectedLength)
                {
                    Console.WriteLine($"Помилка: очікується {expectedLength} чисел, ви ввели {parts.Length}.");
                    continue;
                }

                double[] result = new double[expectedLength];
                bool success = true;
                for (int i = 0; i < expectedLength; i++)
                {
                    if (!double.TryParse(parts[i], NumberStyles.Any, CultureInfo.InvariantCulture, out result[i]))
                    {
                        Console.WriteLine($"Помилка: '{parts[i]}' не є коректним числом.");
                        success = false;
                        break;
                    }
                }
                if (success) return result;
            }
        }
    }
}