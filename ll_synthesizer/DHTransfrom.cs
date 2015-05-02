using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer
{
    class DHTransform
    {
        public static double[] transform(short[] datain)
        {
            int N = datain.Length;
            double[] dataout = new double[N];
            for (int k = 0; k < N; k++)
            {
                double Hk = 0;
                for (int n = 0; n < N; n++)
                {
                    double fac = Math.Cos(2*Math.PI*k*n/N)+Math.Sin(2*Math.PI*k*n/N);
                    Hk += datain[n]*fac;
                }
                dataout[k] = Hk;
            }
            return dataout;
        }

        public static short[] transform(double[] datain)
        {
            int N = datain.Length;
            short[] dataout = new short[N];
            for (int k = 0; k < N; k++)
            {
                double Hk = 0;
                for (int n = 0; n < N; n++)
                {
                    double fac = Math.Cos(2 * Math.PI * k * n / N) + Math.Sin(2 * Math.PI * k * n / N);
                    Hk += datain[n] * fac;
                }
                dataout[k] = Convert.ToInt16(Hk/N);
            }
            return dataout;
        }

        public static double[] transform(double[] datain, bool isdouble)
        {
            int N = datain.Length;
            double[] dataout = new double[N];
            for (int k = 0; k < N; k++)
            {
                double Hk = 0;
                for (int n = 0; n < N; n++)
                {
                    double fac = Math.Cos(2 * Math.PI * k * n / N) + Math.Sin(2 * Math.PI * k * n / N);
                    Hk += datain[n] * fac;
                }
                dataout[k] = Hk / N;
            }
            return dataout;
        }

        static public double[] test()
        {
            int kWindowSize = 8192;
            double[] sinArray = new double[kWindowSize];
            double omega = 2 * Math.PI * 20;
            for (int i = 0; i < kWindowSize; i++)
            {
                sinArray[i] = Math.Sin(omega * i / 1000);
            }
            double[] from = transform(sinArray, true);
            return from;
        }

        /*
        public static void transform(short[] datain, out double[] real, out double[] imag)
        {
            double[] H = transform(datain);

        }
         */
    }
}
