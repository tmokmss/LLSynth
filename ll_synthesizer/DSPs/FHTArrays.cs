using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ll_synthesizer.DSPs
{
    class FHTArrays
    {
        private static Dictionary<int, double[]> mSineTabs = new Dictionary<int, double[]>();
        private static Dictionary<int, double[]> mPreWindows = new Dictionary<int, double[]>();
        private static Dictionary<int, double[]> mPostWindows = new Dictionary<int, double[]>();
        private static Dictionary<int, uint[]> mBitRevs = new Dictionary<int, uint[]>();

        internal static readonly double twopi = 2 * Math.PI;
        internal static readonly double invsqrt2 = 0.70710678118654752440084436210485;
        internal static readonly double nodivbyzero = 0.000000000000001;
        private const double kOverlapCount = 2;
        private const int kPostWindowPower = 3;

        public static double[] GetPreWindow(int windowSize)
        {
            double[] dst;
            if (!mPreWindows.TryGetValue(windowSize, out dst))
            {
                dst = new double[windowSize];
                var mBitRev = GetBitRevTable(windowSize);
                var mSineTab = GetHalfSineTable(windowSize);

                double[] tmp = CreateRaisedCosineWindow(windowSize, 1.0);
                Parallel.For(0, windowSize, i =>
                {
                    // The correct Hartley<->FFT conversion is:
                    //
                    //	Fr(i) = 0.5(Hr(i) + Hi(i))
                    //	Fi(i) = 0.5(Hr(i) - Hi(i))
                    //
                    // We omit the 0.5 in both the forward and reverse directions,
                    // so we have a 0.25 to put here.

                    dst[i] = tmp[mBitRev[i]] * 0.5 * (2.0 / (double)kOverlapCount);
                });
                mPreWindows.Add(windowSize, dst);
            }
            return dst;
        }

        public static double[] GetPostWindow(int windowSize, int power=kPostWindowPower)
        {
            double[] dst;
            if (!mPostWindows.TryGetValue(windowSize, out dst))
            {
                var powerIntegrals = new double[] { 1.0, 1.0/2.0, 3.0/8.0, 5.0/16.0, 35.0/128.0,
									                 63.0/256.0, 231.0/1024.0, 429.0/2048.0 };
                double scalefac = windowSize * (powerIntegrals[1] / powerIntegrals[power + 1]);
                dst = CreateRaisedCosineWindow(windowSize, (double)power);
                Parallel.For(0, windowSize, i => dst[i] *= scalefac);
                mPostWindows.Add(windowSize, dst);
            }
            return dst;
        }

        public static double[] GetHalfSineTable(int windowSize)
        {
            double[] dst;
            if (!mSineTabs.TryGetValue(windowSize, out dst))
            {
                double twopi_over_n = twopi / windowSize;
                dst = new double[windowSize];
                Parallel.For(0, windowSize, i => dst[i] = Math.Sin(twopi_over_n * i));
                mSineTabs.Add(windowSize, dst);
            }
            return dst;
        }

        public static uint[] GetBitRevTable(int windowSize)
        {
            uint[] dst;
            if (!mBitRevs.TryGetValue(windowSize, out dst))
            {
                uint bits = IntegerLog2((uint)windowSize);
                dst = new uint[windowSize];
                Parallel.For(0, windowSize, i => dst[i] = RevBits((uint)i, bits));
                mBitRevs.Add(windowSize, dst);
            }
            return dst;
        }

        static double[] CreateRaisedCosineWindow(int windowSize, double power)
        {
            double twopi_over_n = twopi / windowSize;
            double scalefac = 1.0 / windowSize;
            var dst = new double[windowSize];
            Parallel.For(0, windowSize, i => dst[i] = scalefac * Math.Pow(0.5 * (1.0 - Math.Cos(twopi_over_n * (i + 0.5))), power));
            return dst;
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

    }
}
