using System;
using System.Threading.Tasks;

namespace ll_synthesizer
{
    class RandomizedFactor
    {
        public int[,] Factors { get { FactorsCalced = false; return factors; } }
        public bool FactorsCalced { private set; get; }

        public static int[] MaxMin { set; get; }
        public static double RemovalRatio { set; get; }
        public static double AllowedError { set; get; }

        Random r = new Random();
        int[,] factors;
        int length;
        bool calcInProgress = false;

        public RandomizedFactor(int length)
        {
            this.length = length;
            FactorsCalced = false;
        }

        public async void AsyncCalcRandomizedFactor()
        {
            if (calcInProgress) return;
            await Task.Run(()=>CalcRandomizedFactor());
        }

        private void CalcRandomizedFactor()
        {
            calcInProgress = true;
            int num = length;
            if (num == 1)
                return; // there is no answer
            FactorsCalced = false;
            factors = new int[num, 2];

            int count = 0;
            double targetSaved = RemovalRatio;
            int max = 500000;
            while (!IsAllowedFactors(factors) && count < max)
            {
                for (int i = 0; i < num; i++)
                {
                    factors[i, 0] = r.Next(MaxMin[0], MaxMin[1]);   // LRFactor
                    factors[i, 1] = r.Next(-MaxMin[3], MaxMin[3]);  // TotalFactor
                    //factors[i, 1] = r.Next(maxmin[2], maxmin[3]);
                }
                count++;
            }
            FactorsCalced = true;
            calcInProgress = false;
        }

        private bool IsAllowedFactors(int[,] orgfac)
        {
            double sumR = 0;
            double sumL = 0;
            double sumAbsR = 0;
            double sumAbsL = 0;
            for (int i = 0; i < orgfac.GetLength(0); i++)
            {
                int val = orgfac[i, 0];
                double fac = (orgfac[i, 1] < 0) ? 1 : 1;
                double facR = Math.Cos(Math.PI / 40 * val + Math.PI / 4) * orgfac[i, 1] * fac;
                double facL = Math.Sin(Math.PI / 40 * val + Math.PI / 4) * orgfac[i, 1] * fac;
                sumR += facR;
                sumL += facL;
                sumAbsR += Math.Abs(facR);
                sumAbsL += Math.Abs(facL);
            }
            if (sumAbsR * sumAbsL == 0)
                return false;

            //double R = Math.Abs(sumR/sumAbsR);
            //double L = Math.Abs(sumL/sumAbsL);
            double R = sumR / sumAbsR;
            double L = sumL / sumAbsL;

            var target = RemovalRatio;
            if (IsApproxTarget(R, target) && IsApproxTarget(L, target))
                return true;
            return false;
        }

        private bool IsApproxTarget(double val, double target)
        {
            if (val > target - AllowedError && val < target + AllowedError)
                return true;
            return false;
        }
    }
}
