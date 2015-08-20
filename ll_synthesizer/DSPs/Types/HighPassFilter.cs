using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ll_synthesizer.DSPs.Config;
using System.Windows.Forms;


namespace ll_synthesizer.DSPs.Types
{
    class HighPassFilter : DSP
    {
        FHTransform fhtr = new FHTransform();
        FHTransform ifhtr = new FHTransform();
        FHTransform fhtl = new FHTransform();
        FHTransform ifhtl = new FHTransform();

        public override DSPType Type
        {
            get { return DSPType.HighPassFilter; }
        }

        public double CutoffFrequency { set; get; }

        public HighPassFilter()
        {
            CutoffFrequency = 200;
        }

        public override void Process(ref short[] left, ref short[] right)
        {
            Process(left, out left, true);
            Process(right, out right, false);
        }

        private void Process(short[] datain, out short[] dataout, bool isLeft)
        {
            FHTransform fht, ifht;
            if (isLeft)
            {
                fht = fhtl; ifht = ifhtl;
            }
            else
            {
                fht = fhtr; ifht = ifhtr;
            }

            var length = datain.Length;
            var mBitRev = FHTArrays.GetBitRevTable(length);
            var mPreWindow = FHTArrays.GetPreWindow(length);
            var mPostWindow = FHTArrays.GetPostWindow(length);
            var temp = new double[length];
            int freqBelowToSides = (int)((CutoffFrequency / ((double)mSampleRate / length)) + 0.5);

            for (var i = 0; i < length; ++i)
            {
                var j = mBitRev[i];
                var k = (uint)(j & (length - 1));
                temp[i] = datain[k] * mPreWindow[k];
            }

            double[] re, im;
            fht.ComputeFHT(temp, out re, out im);

            for (var i = 0; i < freqBelowToSides; i++)
            {
                re[i] = im[i] = 0;
            }

            ifht.ComputeFHT(re, im, out temp, true);

            dataout = ToShort(temp, length);
        }


    }
}
