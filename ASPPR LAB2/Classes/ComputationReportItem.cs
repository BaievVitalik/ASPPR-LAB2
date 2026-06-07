namespace SimplexMJE_Modular
{
    internal class ComputationReportItem
    {
        public string Title { get; set; } = string.Empty;
        public string Contents { get; set; } = string.Empty;
        public int TitleSize { get; set; } = 1;

        public ComputationReportItem()
        {
        }

        public ComputationReportItem(string title, string contents, int titlesize = 1)
        {
            Title = title;
            Contents = contents;
            TitleSize = titlesize;
        }
    }
}
