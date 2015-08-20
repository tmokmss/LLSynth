using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer.DSPs.Types
{
    class CenterCut : DSP
    {
        bool mBassToSides = true;
        FHTransform fhtr = new FHTransform();
        FHTransform fhtl = new FHTransform();
        FHTransform fhtc = new FHTransform();
            
        public override DSPType Type
        {
            get { return DSPType.CenterCut; }
        }

        public override void Process(ref short[] left, ref short[] right)
        {
            double[] leftd, rightd;
            int size = left.Length;
            Process(ToDouble(left, size), ToDouble(right, size), out leftd, out rightd);
            left = ToShort(leftd, size);
            right = ToShort(rightd, size);
        }

        private void Process(double[] leftin, double[] rightin, out double[] leftout, out double[] rightout)
        {
            int length = leftin.Length;
            int freqBelowToSides = (int)((200.0 / ((double)mSampleRate / length)) + 0.5);
            int freqAboveToSides = (int)((300.0 / ((double)mSampleRate / length)) + 0.5);
            var mBitRev = FHTArrays.GetBitRevTable(length);
            var mPreWindow = FHTArrays.GetPreWindow(length);
            var mPostWindow = FHTArrays.GetPostWindow(length);
            double[] tempLeft = new double[length];
            double[] tempRight = new double[length];

            for (uint i = 0; i < length; ++i)
            {
                uint j = (uint)(mBitRev[i]);
                uint k = (uint)(j & (length - 1));
                tempLeft[i] = leftin[k] * mPreWindow[k];
                tempRight[i] = rightin[k] * mPreWindow[k];
            }

            fhtl.ComputeFHT(ref tempLeft, length);
            fhtr.ComputeFHT(ref tempRight, length);

            double[] tempC = new double[length];
            for (uint i = 0; i < length / 2; i++)
            {
                double lR = tempLeft[i] + tempLeft[length - 1 - i];
                double lI = tempLeft[i] - tempLeft[length - 1 - i];
                double rR = tempRight[i] + tempRight[length - 1 - i];
                double rI = tempRight[i] - tempRight[length - 1 - i];

                double sumR = lR + rR;
                double sumI = lI + rI;
                double diffR = lR - rR;
                double diffI = lI - rI;

                double sumSq = sumR * sumR + sumI * sumI;
                double diffSq = diffR * diffR + diffI * diffI;
                double alpha = 0.0;

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
                tempC[mBitRev[length - 1 - i]] = cR - cI;
            }
            fhtc.ComputeFHT(ref tempC, length, true);
            for (var i = 0; i < length; i++)
            {
                tempLeft[i] = leftin[i] - tempC[i];
                tempRight[i] = rightin[i] - tempC[i];
            }

            leftout = tempC;
            rightout = tempC;
            leftout = tempLeft;
            rightout = tempRight;
        }
    }
}
