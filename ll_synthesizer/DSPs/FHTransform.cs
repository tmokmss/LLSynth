using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer.DSPs
{
    class FHTransform
    {
        private int kWindowSize;
        public static int kOverlapCount = 4;

        internal static readonly double twopi = 2 * Math.PI;
        internal static readonly double invsqrt2 = 0.70710678118654752440084436210485;
        internal static readonly double nodivbyzero = 0.000000000000001;

        private Overlap overlap;
        private int overlapSize;
        public int OverlapSize {
            set
            {
                if (value == overlapSize) return;
                if (value > 0)
                {
                    overlapSize = value;
                    overlap = new Overlap(kOverlapCount, overlapSize);
                }
            }
            get { return overlapSize; }
        }

        public void ComputeFHT(ref double[] A, int nPoints, bool enableOverlap = false)
        {
            kWindowSize = nPoints;
            var mSineTab = FHTArrays.GetHalfSineTable(nPoints);
            int i, n, n2, theta_inc;

            // 1, 2 round
            for (i = 0; i < nPoints; i += 4)
            {
                double x0 = A[i];
                double x1 = A[i + 1];
                double x2 = A[i + 2];
                double x3 = A[i + 3];

                double y0 = x0 + x1;
                double y1 = x0 - x1;
                double y2 = x2 + x3;
                double y3 = x2 - x3;

                A[i] = y0 + y2;
                A[i + 2] = y0 - y2;
                A[i + 1] = y1 + y3;
                A[i + 3] = y1 - y3;
            }

            // 3 round
            for (i = 0; i < nPoints; i += 8)
            {
                double alpha, beta;
                alpha = A[i + 0];
                beta = A[i + 4];

                A[i + 0] = alpha + beta;
                A[i + 4] = alpha - beta;

                alpha = A[i + 2];
                beta = A[i + 6];
                A[i + 2] = alpha + beta;
                A[i + 6] = alpha - beta;

                alpha = A[i + 1];

                double beta1 = invsqrt2 * (A[i + 5] + A[i + 7]);
                double beta2 = invsqrt2 * (A[i + 5] - A[i + 7]);

                A[i + 1] = alpha + beta1;
                A[i + 5] = alpha - beta1;
                alpha = A[i + 3];
                A[i + 3] = alpha + beta2;
                A[i + 7] = alpha - beta2;
            }
            n = 16;
	        n2 = 8;
	        theta_inc = nPoints >> 4;

            while (n <= nPoints)
            {
                for (i = 0; i < nPoints; i += n)
                {
                    int j;
                    int theta = theta_inc;
                    double alpha, beta;
                    int n4 = n2 >> 1;

                    alpha = A[i];
                    beta = A[i + n2];

                    A[i] = alpha + beta;
                    A[i + n2] = alpha - beta;

                    alpha = A[i + n4];
                    beta = A[i + n2 + n4];

                    A[i + n4] = alpha + beta;
                    A[i + n2 + n4] = alpha - beta;

                    for (j = 1; j < n4; j++)
                    {
                        double sinval = mSineTab[theta];
                        double cosval = mSineTab[theta + (nPoints >> 2)];

                        double alpha1 = A[i + j];
                        double alpha2 = A[i - j + n2];
                        double beta1 = A[i + j + n2] * cosval + A[i - j + n] * sinval;
                        double beta2 = A[i + j + n2] * sinval - A[i - j + n] * cosval;

                        theta += theta_inc;

                        A[i + j] = alpha1 + beta1;
                        A[i + j + n2] = alpha1 - beta1;
                        A[i - j + n2] = alpha2 + beta2;
                        A[i - j + n] = alpha2 - beta2;
                    }
                }

                n *= 2;
                n2 *= 2;
                theta_inc >>= 1;
            }

            if (!enableOverlap) return;
            OverlapSize = nPoints / kOverlapCount;
            var postWindow = FHTArrays.GetPostWindow(kWindowSize);
            for (i = 0; i < kWindowSize; i++)
            {
                A[i] *= postWindow[i];
            }
            overlap.AddOverlap(ref A);
        }

        public void ComputeFHT(short[] input, out double[] output, bool overlapEnable)
        {
            var length = FHTArrays.CeilingPow2(input.Length);
            var mBitRev = FHTArrays.GetBitRevTable(length);
            var mPreWindow = FHTArrays.GetPreWindow(length);
            output = new double[length];

            for (var i = 0; i < input.Length; ++i)
            {
                output[i] = input[mBitRev[i]] * mPreWindow[mBitRev[i]];
            }
            ComputeFHT(ref output, length, overlapEnable);
        }

        public void ComputeFHT(double[] re, double[] im, out double[] output, bool overlapEnable = false)
        {
            var length = re.Length * 2;
            var mBitRev = FHTArrays.GetBitRevTable(length);
            output = new double[length];
            for (var i = 0; i < length / 2; i++)
            {
                output[mBitRev[i]] = re[i] + im[i];
                output[mBitRev[length - 1 - i]] = re[i] - im[i];
            }
            ComputeFHT(ref output, length, overlapEnable);
        }

        public void ComputeFHT(double[] input, out double[] re, out double[] im, bool overlapEnable = false)
        {
            var length = input.Length;
            ComputeFHT(ref input, length, overlapEnable);
            re = new double[length / 2];
            im = new double[length / 2];
            for (var i = 0; i < length / 2; i++)
            {
                re[i] = input[i] + input[length - i - 1];
                im[i] = input[i] - input[length - i - 1];
            }
        }

        public double[] test()
        {
            var mBitRev = FHTArrays.GetBitRevTable(kWindowSize);
            double[] sinArray = new double[kWindowSize];
            double omega = 2 * Math.PI * 20;
            for (int i = 0; i < kWindowSize; i++)
            {
                sinArray[mBitRev[i]] = Math.Sin(omega * i/1000);
            }
            ComputeFHT(ref sinArray, kWindowSize);
            return sinArray;
        }
    }
}
