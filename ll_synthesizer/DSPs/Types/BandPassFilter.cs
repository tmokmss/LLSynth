using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer.DSPs.Types
{
    class BandPassFilter: DSP
    {
        FHTransform fhtr = new FHTransform();
        FHTransform ifhtr = new FHTransform();
        FHTransform fhtl = new FHTransform();
        FHTransform ifhtl = new FHTransform();

        public override DSPType Type
        {
            get { return DSPType.BandPassFilter; }
        }

        public double CutoffFrequencyDown { set; get; }
        public double CutoffFrequencyUp { set; get; }

        public BandPassFilter()
        {
            CutoffFrequencyDown = 200;
            CutoffFrequencyUp = 5000;
        }

        public override void Process(ref short[] left, ref short[] right)
        {
            Process(left, out left, true);
            Process(right, out right, false);
        }

        public void Process(short[] datain, out short[] dataout, bool isLeft)
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
            var temp = new double[length];
            var length2 = length / 2;
            var cutoffDown = (int)((CutoffFrequencyDown / ((double)mSampleRate / length2)) + 0.5);
            var cutoffUp = (int)((CutoffFrequencyUp / ((double)mSampleRate / length2)) + 0.5);
            for (var i = 0; i < length; ++i)
            {
                var j = mBitRev[i];
                var k = (uint)(j & (length - 1));
                temp[i] = datain[k] * mPreWindow[k];
            }

            double[] re, im;
            fht.ComputeFHT(temp, out re, out im);

            for (var i = 0; i < cutoffDown; i++)
            {
                re[i] = im[i] = 0;
            }
            for (var i = cutoffUp; i < length2; i++)
            {
                re[i] = im[i] = 0;
            }

            ifht.ComputeFHT(re, im, out temp, true);

            dataout = ToShort(temp, length);
        }
    }
}
