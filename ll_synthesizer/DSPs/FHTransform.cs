using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer.DSPs
{
    class FHTransform
    {
        private static int kWindowSize;
        private const int kOverlapCount = 1;
        private const int kPostWindowPower = 3;

        internal static readonly double twopi = 2 * Math.PI;
        internal static readonly double invsqrt2 = 0.70710678118654752440084436210485;
        internal static readonly double nodivbyzero = 0.000000000000001;
        internal static double[] mSineTab;
        internal static double[] mPreWindow;
        internal static double[] mPostWindow;
        internal static uint[] mBitRev;

        private double[] overlap;
        private int overlapSize;
        public int OverlapSize {
            set
            {
                if (value > 0)
                {
                    overlapSize = value;
                    overlap = new double[value];
                }
            }
        }

        public static void Initialize(int windowSize)
        {
            kWindowSize = windowSize;
            CreateBitRevTable(out mBitRev, kWindowSize);
            CreateHalfSineTable(out mSineTab, kWindowSize);

            double[] tmp;// = new double[kWindowSize];
            //if (!tmp) return false;
            CreateRaisedCosineWindow(out tmp, kWindowSize, 1.0);
            mPreWindow = new double[kWindowSize];
            for (uint i = 0; i < kWindowSize; ++i)
            {
                // The correct Hartley<->FFT conversion is:
                //
                //	Fr(i) = 0.5(Hr(i) + Hi(i))
                //	Fi(i) = 0.5(Hr(i) - Hi(i))
                //
                // We omit the 0.5 in both the forward and reverse directions,
                // so we have a 0.25 to put here.

                mPreWindow[i] = tmp[mBitRev[i]] * 0.5 * (2.0 / (double)kOverlapCount);
                
            }

            CreatePostWindow(out mPostWindow, kWindowSize, kPostWindowPower);
        }

        static void CreateRaisedCosineWindow(out double[] dst, int n, double power)
        {
            double twopi_over_n = twopi / n;
            double scalefac = 1.0 / n;
            dst = new double[n];
            for (int i = 0; i < n; ++i)
            {
                dst[i] = scalefac * Math.Pow(0.5 * (1.0 - Math.Cos(twopi_over_n * (i + 0.5))), power);
            }
        }

        static void CreatePostWindow(out double[] dst, int windowSize, int power)
        {
            double[] powerIntegrals = new double[] { 1.0, 1.0/2.0, 3.0/8.0, 5.0/16.0, 35.0/128.0,
									                 63.0/256.0, 231.0/1024.0, 429.0/2048.0 };
            double scalefac = windowSize * (powerIntegrals[1] / powerIntegrals[power + 1]);
            CreateRaisedCosineWindow(out dst, windowSize, (double)power);
            for (int i = 0; i < windowSize; ++i)
            {
                dst[i] *= scalefac;
                //dst[i] = 1;
            }
        }

        static void CreateHalfSineTable(out double[] dst, int n)
        {
            double twopi_over_n = twopi / n;
            dst = new double[n];
            for (int i = 0; i < n; ++i)
            {
                dst[i] = Math.Sin(twopi_over_n * i);
            }
        }

        static void CreateBitRevTable(out uint[] dst, int n)
        {
            uint bits = IntegerLog2((uint)n);
            dst = new uint[n];
            for (uint i = 0; i < n; ++i)
            {
                dst[i] = RevBits(i, bits);
            }
        }

        public void ComputeFHT(ref double[] A, int nPoints)
        {
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
            AddOverlap(ref A);
        }

        private void AddOverlap(ref double[] A)
        {
            for (var i = 0; i < overlapSize; i++)
            {
                A[i] += overlap[i];
            }
            Array.Copy(A, kWindowSize - overlapSize - 1, overlap, 0, overlapSize);
        }

        static uint IntegerLog2(uint v)
        {
            uint i = 0;
            while (v > 1)
            {
                ++i;
                v >>= 1;
            }
            return i;
        }

        static uint RevBits(uint x, uint bits)
        {
            uint y = 0;
            while (bits-- > 0)
            {
                y = (y + y) + (x & 1);
                x >>= 1;
            }
            return y;
        }

        public double[] test()
        {
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
