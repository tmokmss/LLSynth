using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ll_synthesizer.DSPs.Config;
using System.Windows.Forms;

namespace ll_synthesizer.DSPs.Types
{
    class PitchShiftPV: DSP
    {
        private static readonly double ONEDEG = Math.Pow(2, 1.0 / 12);
        private double shiftRate = 4.0 / 5;//Math.Pow(1/ONEDEG,3);
        private double[] mLastPhase = null;
        private double[] mSumPhase = null;
        private bool shiftChanged = true;
        FHTransform fhtr = new FHTransform();
        FHTransform ifhtr = new FHTransform();
        FHTransform fhtl = new FHTransform();
        FHTransform ifhtl = new FHTransform();

        public override DSPType Type
        {
            get { return DSPType.PitchShiftPV; }
        }

        public double ShiftRate
        {
            set {
                shiftRate = value;
                shiftChanged = true;
            }
            get { return shiftRate; }
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
            var mPostWindow = FHTArrays.GetPostWindow(length);
            var temp = new double[length];

            if (shiftChanged || mLastPhase == null || mLastPhase.Length != length / 2)
            {
                mLastPhase = new double[length / 2];
                mSumPhase = new double[length / 2];
                shiftChanged = false;
            }
                
            for (var i = 0; i < length; ++i)
            {
                var j = mBitRev[i];
                var k = (uint)(j & (length - 1));
                temp[i] = datain[k] * mPreWindow[k];
            }

            fht.ComputeFHT(ref temp, length);

            //var newlen = FHTArrays.CeilingPow2((int)(length * shiftRate));
            var newlen = length;
            var temp_shift = new double[newlen];
            mBitRev = FHTArrays.GetBitRevTable(newlen);
            
            var re = new double[length / 2];
            var im = new double[length / 2];
            for (var i = 0; i < length / 2; i++)
            {
                re[i] = temp[i] + temp[length - i - 1];
                im[i] = temp[i] - temp[length - i - 1];
            }
           
            var reshift = Stretch(re, ShiftRate, newlen / 2);
            var imshift = Stretch(im, ShiftRate, newlen / 2);
            
            //AnalysisAndSynthesis(ref re, ref im);
            //var reshift = re;
            //var imshift = im;

            for (var i = 0; i < newlen / 2; i++)
            {
                temp_shift[mBitRev[i]] = reshift[i] + imshift[i];
                temp_shift[mBitRev[newlen - 1 - i]] = reshift[i] - imshift[i];
            }

            ifht.ComputeFHT(ref temp_shift, newlen, true);
            temp = Stretch(temp_shift, length * 1.0 / newlen);

            dataout = ToShort(temp, length);
        }

        private void AnalysisAndSynthesis(ref double[] re, ref double[] im)
        {
            var length = re.Length;
            var osamp = FHTransform.kOverlapCount;
            var expct = 2 * Math.PI / osamp;
            var freqPerBin = mSampleRate / (length * 2);
            var anaMagn = new double[length];
            var anaFreq = new double[length];
            var synMagn = new double[length];
            var synFreq = new double[length];
            for (var i=0; i< length; i++)
            {
                var magn = Math.Sqrt(re[i] * re[i] + im[i] * im[i]);
                var phase = Math.Atan2(im[i], re[i]);
                var temp = phase - mLastPhase[i];
                mLastPhase[i] = phase;
                temp -= i * expct;
                var qpd = (int)(temp / Math.PI);
                if (qpd >= 0) qpd += qpd & 1;
                else qpd -= qpd & 1;
                temp -= Math.PI * qpd;
                temp = osamp * temp / (2 * Math.PI);
                temp = i * freqPerBin + temp * freqPerBin;
                anaMagn[i] = magn;
                anaFreq[i] = temp;
            }
            synMagn = Stretch(anaMagn, shiftRate, length);
            synFreq = Stretch(anaFreq, shiftRate, length);
            for (var i = 0; i < length; i++) synFreq[i] *= shiftRate;
            //synMagn = anaMagn;
            //synFreq = anaFreq;
            /*
            for (var i = 0; i < length; i++)
            {
                var index = (int)(i * shiftRate);
                if (index >= length) break;
                synMagn[index] += anaMagn[i];
                synFreq[index] = anaFreq[i] * shiftRate;
            }
            */
            for (var i = 0; i < length; i++)
            {
                var magn = synMagn[i];
                var tmp = synFreq[i];
                tmp -= i * freqPerBin;
                tmp /= freqPerBin;
                tmp = 2 * Math.PI * tmp / osamp;
                tmp += i * expct;
                mSumPhase[i] += tmp;
                var phase = mSumPhase[i];
                re[i] = magn * Math.Cos(phase);
                im[i] = magn * Math.Sin(phase);
            }
        }
    }
}
