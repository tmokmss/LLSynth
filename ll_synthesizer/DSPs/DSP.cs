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
        bool mBassToSides = false;
        FHTransform fhtr = new FHTransform();
        FHTransform fhtl = new FHTransform();
        FHTransform fhtc = new FHTransform();

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
            fhtr.OverlapSize = 1;
            fhtl.OverlapSize = 1;
            fhtc.OverlapSize = overlapSize;
        }

        public void CenterCut(double[] leftin, double[] rightin, out double[] leftout, out double[] rightout)
        {
            int kWindowSize = leftin.Length;
            int freqBelowToSides = (int)((0 / ((double)mSampleRate / kWindowSize)) + 0.5);
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


                if (mBassToSides && ((i > freqBelowToSides) && (i < freqAboveToSides)))
                {
                    cR = cI = 0.0;
                }

                tempC[mBitRev[i]] = cR + cI;
                tempC[mBitRev[kWindowSize - i]] = cR - cI;
            }
            fhtc.ComputeFHT(ref tempC, kWindowSize);
            Parallel.For(0, kWindowSize, i => tempC[i] *= mPostWindow[i]);
            Parallel.For(0, kWindowSize, i =>
            {
                tempLeft[i] = leftin[i] - tempC[i];
                tempRight[i] = rightin[i] - tempC[i];
            });

            //leftout = tempC;
            //rightout = tempC;
            leftout = tempLeft;
            rightout = tempRight;
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

            var fht = new FHTransform();
            var length = datain.Length;
            fht.OverlapSize = (int)(length/2.3);

            fht.ComputeFHT(ref temp, kWindowSize);

            var temp_shift = new double[length*2];
            int shift = (int)((20 / ((double)mSampleRate / kWindowSize)) + 0.5);
            double shiftr = 0.8;
            /*
            for (var i = 0; i < length-shift; i++)
            {
                temp_shift[mBitRev[i+shift]] = temp[i];
            }
            */

            mBitRev = FHTArrays.GetBitRevTable(length * 2);
            mPostWindow = FHTArrays.GetPostWindow(length * 2);

            var re = new double[length/2];
            var im = new double[length/2];
            for (var i = 0; i < length / 2; i++)
            {
                re[i] = temp[i] + temp[length - i-1];
                im[i] = temp[i] - temp[length - i-1];
            }
            var reshift = new double[length];
            var imshift = new double[length];

            for (var i = 0; i < length; i++)
            {
                double orgidx = i / shiftr;
                int orgidxn = (int)Math.Floor(orgidx);
                int orgidxn1 = (int)Math.Ceiling(orgidx);
                if (orgidxn1 >= length / 2)
                    break;
                short newvalre = (short)((re[orgidxn] - re[orgidxn1]) * (orgidx - orgidxn) + re[orgidxn]);
                short newvalim = (short)((im[orgidxn] - im[orgidxn1]) * (orgidx - orgidxn) + im[orgidxn]);
                reshift[i] = newvalre;
                imshift[i] = newvalim;
            }

            for (var i=0; i < length; i++)
            {
                temp_shift[mBitRev[i]] = reshift[i] + imshift[i];
                temp_shift[mBitRev[2*length-1-i]] = reshift[i] - imshift[i];
            }

            /*
            for (var i = 0; i < length; i++)
            {
                double orgidx = i / shiftr;
                int orgidxn = (int)Math.Floor(orgidx);
                int orgidxn1 = (int)Math.Ceiling(orgidx);
                short newval = (short)((temp[orgidxn] - temp[orgidxn1]) * (orgidx - orgidxn) + temp[orgidxn]);
                temp_shift[mBitRev[i]] = newval;
                //temp_shift[mBitRev[i]] = temp[i];
            }
            for (var i = length; i < length * 2; i++)
            {
                double orgidx = i / shiftr;
                int orgidxn = (int)Math.Floor(orgidx);
                int orgidxn1 = (int)Math.Ceiling(orgidx);
                if (orgidxn1 >= length/2)
                    break;
                short newval = (short)((temp[orgidxn] - temp[orgidxn1]) * (orgidx - orgidxn) + temp[orgidxn]);
                temp_shift[mBitRev[i]] = newval;
                //temp_shift[mBitRev[i]] = temp[i];
            }
            */

            var fht1 = new FHTransform();
            fht1.OverlapSize = (int)(length * 2 / 2.3);
            fht1.ComputeFHT(ref temp_shift, length*2);
            for (var i = 0; i < length * 2; i++)
            {
                temp_shift[i] *= mPostWindow[i];
            }
            for (var i = 0; i < length; i++)
            {
                temp[i] = (temp_shift[2 * i] + temp_shift[2 * i + 1]) / 2;
            }

            dataout = ToShort(temp, length);
        }

        private const int region = (int)(44100 * 30e-3);
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
            var baseNum = CalcBaseCycle(datain);
            var ratio = 1.1;
            var newlen = (int)(length / ratio);
            var temp = new double[newlen];
            var step = (int)(baseNum * ratio);
            var window = GetCrossFadeWindow(baseNum);

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
            for (var i=0; i<length - 1; i++)
            {
                double orgidx = i / ratio;
                int orgidxn = (int)Math.Floor(orgidx);
                int orgidxn1 = (int)Math.Ceiling(orgidx);
                short newval = (short)((temp[orgidxn] - temp[orgidxn1]) * (orgidx - orgidxn) + temp[orgidxn]);
                datain[i] = newval;
            }
            LowPassFiltering(datain, out dataout);
        }

        private int SearchHeadZero(short[] datain)
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

        private int CalcBaseCycle(short[] datain)
        {
            double autcorrMax = 0;
            int diffSol = 0;
            for (var i=50; i < region; i++)
            {
                double autocorr = CalcAutocorrelation(datain, i);
                if (autocorr > autcorrMax)
                {
                    autcorrMax = autocorr;
                    diffSol = i;
                }
            }
            return diffSol;
        }

        private double[] GetCrossFadeWindow(int length)
        {
            //0.1~0.9 -> 1, other gradually goes 0
            var window = new double[length];
            var dicline = 0.0;
            var first = (int)(length * dicline);
            var end = (int)(length*(1- dicline)); 
            for (var i=0; i<length; i++)
            {
                double val = 1;
                if (i < first)
                {
                    val = 1.0 / first * i;
                }
                else if (i > end)
                {
                    val = 1.0 / (end - length) * (i - end) + 1;
                }
                window[i] = val;
            }
            return window;
        }

        private double CalcAutocorrelation(short[] datain, int diff)
        {
            int count = 0 ;
            double sum = 0;
            for (var i=0; i<region; i++)
            {
                sum += datain[i] * datain[i + diff];
                count++;
            }
            return sum / count;
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
                dataout[i] = Convert.ToInt16(0.8*dataout[i - 1] + 0.2*datain[i]);
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
            Parallel.For(0, size, i => newarr[i] = (double)array[i]);
            return newarr;
        }

    }
}
