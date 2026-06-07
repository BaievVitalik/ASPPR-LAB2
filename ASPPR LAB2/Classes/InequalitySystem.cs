namespace SimplexMJE_Modular
{
    internal class InequalitySystem
    {
        private List<Inequality> _inequalities = [];
        public IReadOnlyList<Inequality> Inequalities => _inequalities;

        public int VariableCount
        {
            get
            {
                if (_inequalities.Count == 0) return 0;
                return _inequalities[0].Coefficients.Count;
            }
        }

        public InequalitySystem(List<Inequality> inequalities)
        {
            _inequalities = inequalities;
        }

        public InequalitySystem()
        {
        }

        public Matrix ConvertToMatrix()
        {
            var matrix = new Matrix(_inequalities.Count, VariableCount + 1);
            var varCount = 0;
            for (int i = 0; i < _inequalities.Count; i++)
            {
                var inequalityMatrix = _inequalities[i].ConvertToMatrix();
                for (int j = 0; j < VariableCount + 1; j++)
                    matrix[i, j] = inequalityMatrix[0, j];
                if (_inequalities[i].Sign == Sign.Equals)
                    matrix.RowMarkers[i] = "0";
                else
                    matrix.RowMarkers[i] = $"y{++varCount}";
            }
            matrix.ColMarkers[matrix.ColCount - 1] = "1";
            return matrix;
        }

        public void AddInequality(Inequality inequality)
        {
            if (inequality.Coefficients.Count != VariableCount && _inequalities.Count > 0)
                throw new Exception("Кількість змінних в нерівності не співпадає з кількістю змінних в системі");
            _inequalities.Add(inequality);
        }

        public override string ToString()
        {
            var str = "";
            foreach (var inequality in _inequalities)
                str += inequality.ToString() + "\n";
            return str;
        }

        public string ToStringWithZeroes()
        {
            var str = "";
            foreach (var inequality in _inequalities)
                str += inequality.ToStringWithZeroes() + "\n";
            return str;
        }
    }
}
