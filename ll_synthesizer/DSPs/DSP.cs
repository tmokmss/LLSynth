using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace ll_synthesizer.DSPs
{
    class DSP
    {
        bool enabled = false;
        int mSampleRate = 44100;
        private static double shiftRate = 1.0;
        bool mBassToSides = true;
        FHTransform fhtr = new FHTransform();
        FHTransform fhtl = new FHTransform();
        FHTransform fhtc = new FHTransform();
        FHTransform fht1 = new FHTransform();
        FHTransform fht = new FHTransform();
        
        public static double ShiftRate
        {
            set { shiftRate = value; }
            get { return shiftRate; }
        }
        public bool Enabled
        {
            set { enabled = value; }
            get { return enabled; }
        }

        public DSP()
        {

        }

        public void CenterCut(ref short[] left, ref short[] right)
        {
            if (!enabled)
            {
                return;
            }
            double[] leftd, rightd;
            int size = left.Length;
            CenterCut(ToDouble(left, size), ToDouble(right, size), out leftd, out rightd);
            left = ToShort(leftd, size);
            right = ToShort(rightd, size);
        }

        public void SetOverlapSize(int overlapSize)
        {
            //fhtr.OverlapSize = overlapSize;
            //fhtl.OverlapSize = overlapSize;
            fhtr.OverlapSize = 0;
            fhtl.OverlapSize = 0;
            fhtc.OverlapSize = overlapSize;
        }

        public void CenterCut(double[] leftin, double[] rightin, out double[] leftout, out double[] rightout)
        {
            int kWindowSize = leftin.Length;
            int freqBelowToSides = (int)((200.0 / ((double)mSampleRate / kWindowSize)) + 0.5);
            int freqAboveToSides = (int)((300.0 / ((double)mSampleRate / kWindowSize)) + 0.5);
            var mBitRev = FHTArrays.GetBitRevTable(kWindowSize);
            var mPreWindow = FHTArrays.GetPreWindow(kWindowSize);
            var mPostWindow = FHTArrays.GetPostWindow(kWindowSize);
            double[] tempLeft = new double[kWindowSize];
            double[] tempRight = new double[kWindowSize];

            for (uint i = 0; i < kWindowSize; ++i)
            {
                uint j = (uint)(mBitRev[i]);
                //uint k = j;
                uint k = (uint)(j & (kWindowSize - 1));
                tempLeft[i] = leftin[k] * mPreWindow[i];
                tempRight[i] = rightin[k] * mPreWindow[i]; 
            }

            fhtl.ComputeFHT(ref tempLeft, kWindowSize);
            fhtr.ComputeFHT(ref tempRight, kWindowSize);

            double[] tempC = new double[kWindowSize];
            for (uint i = 1; i < kWindowSize / 2; i++)
            {
                double lR = tempLeft[i] + tempLeft[kWindowSize - i];
                double lI = tempLeft[i] - tempLeft[kWindowSize - i];
                double rR = tempRight[i] + tempRight[kWindowSize - i];
                double rI = tempRight[i] - tempRight[kWindowSize - i];

                double sumR = lR + rR;
                double sumI = lI + rI;
                double diffR = lR - rR;
                double diffI = lI - rI;

                double sumSq = sumR * sumR + sumI * sumI;
                double diffSq = diffR * diffR + diffI * diffI;
                double alpha = 0.0;

                double lLen = Math.Sqrt(lR * lR + lI * lI);
                double rLen = Math.Sqrt(rR * rR + rI * rI);
                double cRd = lR / lLen + rR / rLen;
                double cId = lI / lLen + rI / rLen;

                double a = cRd * cRd + cId * cId;
                double b = -(cRd * (lR + rR) + cId * (lI + rI));
                double c = lR * rR + lI * rI;
                double D = b * b - 4 * a * c;

                if (D >= 0)
                {
                    alpha = (-b + Math.Sqrt(D)) / (2 * a);
                    if (Math.Abs(alpha) > 1)
                    {
                        alpha = (-b - Math.Sqrt(D)) / (2 * a);
                    }
                }

                //double cR = cRd * alpha;
                //double cI = cId * alpha;

                if (sumSq > FHTransform.nodivbyzero)
                {
                    alpha = 0.5 - Math.Sqrt(diffSq / sumSq) * 0.5;
                }

                double cR = sumR * alpha;
                double cI = sumI * alpha;


                if (mBassToSides && ((i < freqBelowToSides)))// && (i < freqAboveToSides)))
                {
                    cR = cI = 0.0;
                }

                tempC[mBitRev[i]] = cR + cI;
                tempC[mBitRev[kWindowSize - i]] = cR - cI;
            }
            fhtc.ComputeFHT(ref tempC, kWindowSize, true);
            for (var i=0; i<kWindowSize; i++)
            {
                tempLeft[i] = leftin[i] - tempC[i];
                tempRight[i] = rightin[i] - tempC[i];
            }

            leftout = tempC;
            rightout = tempC;
            //leftout = tempLeft;
            //rightout = tempRight;
        }


        public void PitchShift(short[] datain, out short[] dataout)
        {
            if (!enabled)
            {
                dataout = datain;
                return;
            }
            int kWindowSize = datain.Length;

            var mBitRev = FHTArrays.GetBitRevTable(kWindowSize);
            var mPreWindow = FHTArrays.GetPreWindow(kWindowSize);
            var mPostWindow = FHTArrays.GetPostWindow(kWindowSize);
            double[] temp = new double[kWindowSize];

            for (var i = 0; i < kWindowSize; ++i)
            {
                uint j = (uint)(mBitRev[i]);
                //uint k = j;
                uint k = (uint)(j & (kWindowSize - 1));
                temp[i] = datain[k] * mPreWindow[i];
            }

            var length = datain.Length;
            fht.OverlapSize = 0;// (int)(length / 2.3);

            fht.ComputeFHT(ref temp, kWindowSize);

            var temp_shift = new double[length*2];

            mBitRev = FHTArrays.GetBitRevTable(length * 2);

            var re = new double[length/2];
            var im = new double[length/2];
            for (var i = 0; i < length / 2; i++)
            {
                re[i] = temp[i] + temp[length - i-1];
                im[i] = temp[i] - temp[length - i-1];
            }
            var reshift = new double[length];
            var imshift = new double[length];
            reshift = Stretch(re, ShiftRate, length);
            imshift = Stretch(im, ShiftRate, length);
            for (var i=0; i < length; i++)
            {
                temp_shift[mBitRev[i]] = reshift[i] + imshift[i];
                temp_shift[mBitRev[2*length-1-i]] = reshift[i] - imshift[i];
            }
            /*
            var temp_shiftirr = Stretch(temp, shiftr);
            for (var i=0; i<temp_shiftirr.Length; i++)
            {
                temp_shift[mBitRev[i]] = temp_shiftirr[i];
            }
            */
            //fht1.OverlapSize = (int)(length * 2 / 4);
            fht1.ComputeFHT(ref temp_shift, length*2, true);
            temp = Stretch(temp_shift, 0.5);

            dataout = ToShort(temp, length);
        }

        private const int region = (int)(44100 * 20e-3);
        public void PitchShiftTD(short[] datain, out short[] dataout)
        {
            if (!enabled)
            {
                dataout = datain;
                return;
            }
            var length = datain.Length;
            //var baseNum = (int)(44100 * 50e-3);
            var startIdx = SearchHeadZero(datain);

            var frame = new short[region];
            Array.Copy(datain, startIdx, frame, 0, region);

            var baseNum = CalcBaseCycle(frame);
            var ratio = 1.2;
            var newlen = (int)Math.Round(length / ratio);
            var temp = new double[newlen];
            var step = (int)(baseNum * ratio);
            var window = GetCrossFadeWindow(baseNum);

            Console.WriteLine(44100 *1.0 / baseNum);

            if (baseNum > 300)
            {
                dataout = datain;
                //return;
            }
            int j = 0, count = 0;
            for (var i= 0; i< newlen; i++)
            {
                var idx = j + step * count + startIdx;
                if (idx >= length) break;
                temp[i] = datain[idx]*window[j];
                j++;
                if (j >= baseNum)
                {
                    count++;
                    j = 0;
                }
            }
            datain = ToShort(Stretch(temp, ratio), length);
            //LowPassFiltering(datain, out dataout);
            dataout = datain;
        }

        private static double[] Stretch(double[] datain, double ratio)
        {
            return Stretch(datain, ratio, (int)Math.Round(datain.Length * ratio));
        }

        private static double[] Stretch(double[] datain, double ratio, int size)
        {
            var length = datain.Length;
            var lengthnew = size;
            var temp = new double[lengthnew];
            for (var i=0; i<lengthnew; i++)
            {
                double t = i / ratio;
                int t1 = (int)Math.Floor(t);
                if (t1 >= length) break;
                double x0 = (t1 >= 1) ? datain[t1 - 1] : datain[t1];
                double x1 = datain[t1];
                double x2 = (t1 < length - 1) ? datain[t1 + 1] : datain[t1];
                double x3 = (t1 < length - 2) ? datain[t1 + 2] : datain[t1];
                double newval = InterpolateHermite4pt3oX(x0, x1, x2, x3, t - t1);
                temp[i] = newval;
            }
            return temp;
        }

        private static double InterpolateHermite4pt3oX(double x0, double x1, double x2, double x3, double t)
        {
            double c0 = x1;
            double c1 = .5 * (x2 - x0);
            double c2 = x0 - (2.5 * x1) + (2 * x2) - (.5 * x3);
            double c3 = (.5 * (x3 - x0)) + (1.5 * (x1 - x2));
            return (((((c3 * t) + c2) * t) + c1) * t) + c0;
        }

        private static int SearchHeadZero(short[] datain)
        {
            int idx = 0;
            for (var i=0; i<datain.Length-1; i++)
            {
                if (datain[i] * datain[i + 1] <= 0) {
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
            for (var i=1; i< length-1; i++)
            {
                var diff = autocorr[i] - autocorr[i + 1];
                var diff2 = autocorr[i - 1] - autocorr[i];
                if (diff * diff2<= 0)
                {
                    peaks[peakidx++] = i;
                }
                if (peakidx >= peaks.Length) break;
            }
            for (var i=0; i<peakidx; i++)
            {
                var corr = Math.Abs(autocorr[peaks[i]]);
                if (corr>autocorrMax)
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
            var end = (int)(length*(1- dicline)); 
            for (var i=0; i<length; i++)
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
            for (var i=0; i< length; i++)
            {
                avg += datain[i];
            }
            avg /= length;
            for (var i=0; i< length; i++)
            {
                dataout[i] = (datain[i]- avg) * window[i];
            }
            return dataout;
        }

        private static double[] GetWindowCorrelation(int T)
        {
            var corr = new double[T];
            for (var i=0; i< T; i++)
            {
                corr[i] = (1 - i*1.0 / T) * (2.0 / 3 + 1.0 / 3 * Math.Cos(2 * Math.PI * i / T))
                    + 1.0 / 2 / Math.PI * Math.Sin(2 * (Math.PI * i * T));
            }
            return corr;
        }

        private static double CalcAutocorrelation(double[] datain, int diff)
        {
            double sum = 0;
            var length = datain.Length;
            for (var i=0; i<length-diff; i++)
            {
                sum += datain[i] * datain[i + diff];
            }
            return sum;
        }

        public void LowPassFiltering(short[] datain, out short[] dataout)
        {
            if (!enabled)
            {
                dataout = datain;
                return;
            }
            int size = datain.Length;
            dataout = new short[size];
            for (int i = 1; i < size; i++)
            {
                dataout[i] = Convert.ToInt16(0.9*dataout[i - 1] + 0.1*datain[i]);
            }
        }

        static short[] ToShort(double[] array, int size)
        {
            // size is for performance reason
            short[] newarr = new short[size];
            for (int i = 0; i < size; i++)
            {
                newarr[i] = ToShort(array[i]);
            }
            return newarr;
        }

        static short ToShort(double val) {
            if (val > short.MaxValue)
                return short.MaxValue;
            else if (val < short.MinValue)
                return short.MinValue;
            else if (Double.IsNaN(val))
                return 0;
            return Convert.ToInt16(val);
        }

        static double[] ToDouble(short[] array, int size)
        {
            double[] newarr = new double[size];
            for (var i=0; i< size; i++)
            {
                newarr[i] = (double)array[i];
            }
            return newarr;
        }

    }
}
