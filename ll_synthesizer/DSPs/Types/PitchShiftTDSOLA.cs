using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ll_synthesizer.DSPs.Types
{
    class PitchShiftTDSOLA : DSP
    {
        private int kOverlapCount = FHTransform.kOverlapCount;
        private Overlap overlapr = null;
        private Overlap overlapl = null;
        private double[] window = null;
        private const int region = (int)(44100 * 20e-3);

        public double PitchShiftRate { set; get; }
        public double FormantShiftRate { set; get; }

        public PitchShiftTDSOLA()
        {
            PitchShiftRate = 0.8;
            FormantShiftRate = 1.0;
        }

        public override DSPType Type
        {
            get { return DSPType.PitchShiftTDSOLA; }
        }

        public override void Process(ref short[] left, ref short[] right)
        {
            PitchShiftTD(left, out left, ref overlapl);
            PitchShiftTD(right, out right, ref overlapr);
        }

        public void PitchShiftTD(short[] datain, out short[] dataout, ref Overlap overlap)
        {
            var length = datain.Length;
            var startIdx = SearchHeadZero(datain);

            var frame = new short[region];
            Array.Copy(datain, startIdx, frame, 0, region);

            var baseCycle = CalcBaseCycle(frame);
            var pratio = PitchShiftRate;
            var fratio = FormantShiftRate;
            var newlen = (int)Math.Round(length / fratio);
            var temp = new double[newlen];

            //Console.WriteLine(44100.0 / baseCycle);

            int j = 0, count = 0, idx = startIdx;
            var incrementFreq = (int)((length - startIdx) * (fratio - 1) / baseCycle);
            if (baseCycle == 0) incrementFreq = 0;
            var isIncrement = incrementFreq >= 0;
            incrementFreq = Math.Abs(incrementFreq);
            for (var i = 0; i < newlen; i++)
            {
                if (idx >= length) break;
                temp[i] = datain[idx];// * window[j];
                j++;
                idx++;
                if (j >= baseCycle && j>=3)
                {
                    // search for zero-cross point
                    if (datain[idx - 1] * datain[idx - 2] > 0) continue;
                    count++;
                    baseCycle = j;
                    j = 0;
                    if (incrementFreq != 0 && count % incrementFreq == 0)
                    {
                        idx -= j;
                    }
                }
                //if (i%500 == 0)
                //    Console.WriteLine(String.Format("{0}:{1}", i, idx));
            }

            var tempout = Stretch(temp, fratio, length);

            if (overlap == null) overlap = new Overlap(kOverlapCount, length / kOverlapCount);
            SetPrePostWindow(length);
            for (var i = 0; i < length; i++)
            {
                tempout[i] *= window[i];
            }
            overlap.AddOverlap(ref tempout);

            dataout = ToShort(tempout, length);
        }

        private static int SearchHeadZero(short[] datain)
        {
            int idx = 0;
            for (var i = 0; i < datain.Length - 1; i++)
            {
                if (datain[i] * datain[i + 1] <= 0)
                {
                    idx = i;
                    break;
                }
            }
            return idx;
        }

        private static int CalcBaseCycle(short[] datain)
        {
            double autocorrMax = 0;
            int diffSol = 0;
            var length = datain.Length;
            var a = ApplyWindow(datain);
            var autocorr = new double[length];
            var wincorr = GetWindowCorrelation(length);
            var a0 = CalcAutocorrelation(a, 0);
            Parallel.For(0, length, i =>
            {
                var corr = CalcAutocorrelation(a, i);
                autocorr[i] = corr / a0 / wincorr[i];
            });
            var peaks = new int[20];
            var peakidx = 0;
            for (var i = 1; i < length - 1; i++)
            {
                var diff = autocorr[i] - autocorr[i + 1];
                var diff2 = autocorr[i - 1] - autocorr[i];
                if (diff * diff2 <= 0)
                {
                    peaks[peakidx++] = i;
                }
                if (peakidx >= peaks.Length) break;
            }
            for (var i = peakidx-1; i>=0; i--)
            {
                var corr = autocorr[peaks[i]];
                if (corr >= autocorrMax*0.99)
                {
                    autocorrMax = corr;
                    diffSol = peaks[i];
                }
            }
            return diffSol;
        }

        private static double[] GetCrossFadeWindow(int length)
        {
            var window = new double[length];
            var dicline = 0.0;
            var first = (int)(length * dicline);
            var end = (int)(length * (1 - dicline));
            for (var i = 0; i < length; i++)
            {
                window[i] = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i * 1.0 / length);
            }
            return window;
        }

        private static double[] ApplyWindow(short[] datain)
        {
            var length = datain.Length;
            var dataout = new double[length];
            var window = GetCrossFadeWindow(length);
            double avg = 0;
            for (var i = 0; i < length; i++)
            {
                avg += datain[i];
            }
            avg /= length;
            for (var i = 0; i < length; i++)
            {
                dataout[i] = (datain[i] - avg) * window[i];
            }
            return dataout;
        }

        private static double[] GetWindowCorrelation(int T)
        {
            var corr = new double[T];
            for (var i = 0; i < T; i++)
            {
                corr[i] = (1 - i * 1.0 / T) * (2.0 / 3 + 1.0 / 3 * Math.Cos(2 * Math.PI * i / T))
                    + 1.0 / 2 / Math.PI * Math.Sin(2 * Math.PI * i / T);
            }
            return corr;
        }

        private void SetPrePostWindow(int length)
        {
            if (window == null || window.Length != length)
            {
                var preWindow = FHTArrays.GetPreWindow(length);
                var postWindow = FHTArrays.GetPostWindow(length);
                window = new double[length];
                for (int i = 0; i < length; i++)
                {
                    window[i] = preWindow[i] * postWindow[i] * length * 2 * Math.Sqrt(2);
                }
            }
        }

        private static double CalcAutocorrelation(double[] datain, int diff)
        {
            double sum = 0;
            var length = datain.Length;
            for (var i = 0; i < length - diff; i++)
            {
                sum += datain[i] * datain[i + diff];
            }
            return sum;
        }
    }
}
