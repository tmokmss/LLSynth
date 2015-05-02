using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer.DSPs
{
    class DSP
    {
        bool enabled = false;
        int mSampleRate = 44100;
        bool mBassToSides = true;
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
            uint i;
            int kWindowSize = leftin.Length;
            int freqBelowToSides = (int)((0 / ((double)mSampleRate / kWindowSize)) + 0.5);
            int freqAboveToSides = (int)((200.0 / ((double)mSampleRate / kWindowSize)) + 0.5);
            uint[] mBitRev = FHTransform.mBitRev;
            double[] mPreWindow = FHTransform.mPreWindow;
            double[] mPostWindow = FHTransform.mPostWindow;
            double[] tempLeft = new double[kWindowSize];
            double[] tempRight = new double[kWindowSize];

            for (i = 0; i < kWindowSize; ++i)
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
            for (i = 1; i < kWindowSize / 2; i++)
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

                double lLen = Math.Sqrt(lR*lR+lI*lI);
                double rLen = Math.Sqrt(rR*rR+rI*rI);
                double cRd = lR / lLen + rR / rLen;
                double cId = lI / lLen + rI / rLen;

                double a = cRd * cRd + cId * cId;
                double b = -(cRd * (lR + rR) + cId * (lI + rI));
                double c = lR * rR + lI * rI;
                double D = b*b-4*a*c;

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
            for (i = 0; i < kWindowSize; i++)
            {
                tempC[i] *= mPostWindow[i];
            }

            for (i = 0; i < kWindowSize; i++)
            {
                tempLeft[i] = leftin[i] - tempC[i];
                tempRight[i] = rightin[i] - tempC[i];
            }
            leftout = tempC;
            rightout = tempC;
            //leftout = tempLeft;
            //rightout = tempRight;
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

        static double CalcAlpha(double[] L, double[] R, double[] C)
        {
            double a = Product(C, C);
            double b = Product(C, Add(L, R)) * -1.0;
            double c = Product(L, R);
            double D = b*b-4*a*c;
            if (D < 0) 
                return 0;
            if (a == 0)
                return 0;
            return (-b + Math.Sqrt(D)) / (2 * a);
        }
        static readonly short MAX_SHORT = 32767;
        static readonly short MIN_SHORT = -32768;

        static short[] ToShort(double[] array, int size)
        {
            short[] newarr = new short[size];
            for (int i = 0; i < size; i++)
            {
                double val = array[i];
                if (val > MAX_SHORT)
                    val = MAX_SHORT;
                else if (val < MIN_SHORT)
                    val = MIN_SHORT;
                else if (Double.IsNaN(val))
                    val = 0;
                newarr[i] = Convert.ToInt16(val);
            }
            return newarr;
        }

        static double[] ToDouble(short[] array, int size)
        {
            double[] newarr = new double[size];
            for (int i = 0; i < size; i++)
            {
                newarr[i] = (double)array[i];
            }
            return newarr;
        }

        static double Norm(double[] array)
        {
            double norm2 = 0;
            foreach (double val in array)
            {
                norm2 += val*val;
            }
            return Math.Sqrt(norm2);
        }

        static double[] Multiply(double[] fac, double b)
        {
            double[] newarray = new double[fac.Length];
            for (int i = 0; i < fac.Length; i++)
            {
                newarray[i] = fac[i] * b;
            }
            return newarray;
        }

        static double[] Add(double[] a, double[] b, double fac)
        {
            double[] newarray = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                newarray[i] = a[i] + b[i]* fac;
            }
            return newarray;
        }

        static double[] Add(double[] a, double[] b)
        {
            return Add(a, b, 1);
        }

        static double[] Normalize(double[] a)
        {
            double fac = Norm(a);
            if (fac == 0) return a;
            for (int i = 0; i < a.Length; i++)
            {
                a[i] /= fac;
            }
            return a;
        }

        static double Product(double[] a, double[] b)
        {
            double prod = 0;
            for (int i = 0; i < a.Length; i++)
            {
                prod += a[i] * b[i];
            }
            return prod;
        }
    }
}
