namespace SimplexMJE_Modular
{
    internal class Inequality
    {
        private List<double> _coefficients = [];
        private double _constant = 0d;

        public Sign Sign { get; set; } = Sign.LessOrEqual;
        public double Constant => _constant;
        public IReadOnlyList<double> Coefficients => _coefficients;

        public Inequality(List<double> coefficients, double constant, Sign sign)
        {
            _coefficients = coefficients;
            _constant = constant;
            Sign = sign;
        }

        public Inequality()
        {
        }

        public Matrix ConvertToMatrix()
        {
            var matrix = new Matrix(1, _coefficients.Count + 1);
            var signMultiplier = Sign switch
            {
                Sign.LessOrEqual => 1,
                Sign.GreaterOrEqual => -1,
                Sign.LessStrict => 1,
                Sign.GreaterStrict => -1,
                Sign.Equals => 1,
                _ => 1
            };
            for (int i = 0; i < _coefficients.Count; i++)
                matrix[0, i] = _coefficients[i] * signMultiplier;
            matrix[0, _coefficients.Count] = _constant * signMultiplier;
            return matrix;
        }

        public override string ToString()
        {
            var str = "";
            for (int i = 0; i < _coefficients.Count; i++)
            {
                var coeff = _coefficients[i];
                if (Math.Abs(coeff) <= Double.Epsilon) continue;
                if (i > 0 && coeff > 0) str += "+";
                str += $"{coeff}*x{i + 1} ";
            }
            str += Sign switch
            {
                Sign.LessOrEqual => "<= ",
                Sign.GreaterOrEqual => ">= ",
                Sign.LessStrict => "< ",
                Sign.GreaterStrict => "> ",
                Sign.Equals => " = ",
                _ => throw new Exception("Invalid sign")
            };
            str += $"{_constant}";
            return str;
        }

        public string ToStringWithZeroes()
        {
            var str = "";
            for (int i = 0; i < _coefficients.Count; i++)
            {
                var coeff = _coefficients[i];
                if (Math.Abs(coeff) <= Double.Epsilon) continue;
                if (coeff >= 0) str += "+";
                str += $"{coeff}*x{i + 1} ";
            }
            if (_constant >= 0) str += "+";
            str += $"{_constant}";
            str += Sign switch
            {
                Sign.LessOrEqual => " <= ",
                Sign.GreaterOrEqual => " >= ",
                Sign.LessStrict => " < ",
                Sign.GreaterStrict => " > ",
                Sign.Equals => " = ",
                _ => throw new Exception("Invalid sign")
            };
            str += "0";
            return str;
        }
    }
}
