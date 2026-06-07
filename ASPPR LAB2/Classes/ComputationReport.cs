using System.Text;

namespace SimplexMJE_Modular
{
    internal class ComputationReport : IComputationReportCompiler
    {
        private List<ComputationReportItem> _items;

        public IReadOnlyList<ComputationReportItem> Items => _items;

        public ComputationReport()
        {
            _items = new List<ComputationReportItem>();
        }

        public void AddMatrix(string title, Matrix matrix, int t)
        {
            _items.Add(new ComputationReportItem()
            {
                Title = title,
                Contents = matrix.ToString(),
                TitleSize = t
            });
        }

        public void AddStep(int number, string description, int t)
        {
            _items.Add(new ComputationReportItem()
            {
                Title = $"Крок №{number}",
                Contents = description,
                TitleSize = t
            });
        }

        public void AddAction(string title, string description, int t)
        {
            _items.Add(new ComputationReportItem()
            {
                Title = title,
                Contents = description,
                TitleSize = t
            });
        }

        public string Compile()
        {
            var sB = new StringBuilder();
            foreach (var item in _items)
            {
                sB.AppendLine($"{new string('*', item.TitleSize * 2)}{item.Title}{new string('*', item.TitleSize * 2)}");
                sB.AppendLine();
                sB.AppendLine(item.Contents);
                sB.AppendLine();
            }
            return sB.ToString();
        }

        public void Flush()
        {
            _items = new List<ComputationReportItem>();
        }
    }
}
