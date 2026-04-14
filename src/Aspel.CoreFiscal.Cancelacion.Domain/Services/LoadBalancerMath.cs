namespace Aspel.CoreFiscal.Cancelacion.Domain.Services
{
    /// <summary>
    /// Servicio de dominio para el cálculo del Mínimo Común Múltiplo de las prioridades de los PACs.
    /// </summary>
    public static class McmCalculator
    {
        public static int Calculate(IEnumerable<int> numbers)
        {
            var list = numbers.Where(n => n > 0).ToList();
            if (!list.Any()) return -1;

            return list.Aggregate(Lcm);
        }

        private static int Gcd(int a, int b) // Máximo Común Divisor
        {
            while (b != 0)
            {
                int t = b;
                b = a % b;
                a = t;
            }
            return a;
        }

        private static int Lcm(int a, int b) // Mínimo Común Múltiplo
        {
            if (a == 0 || b == 0) return 0;
            return (a / Gcd(a, b)) * b;
        }
    }
}
