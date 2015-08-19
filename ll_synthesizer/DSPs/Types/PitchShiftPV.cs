﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ll_synthesizer.DSPs.Config;
using System.Windows.Forms;

namespace ll_synthesizer.DSPs.Types
{
    class PitchShiftPV: DSP
    {
        private int kOverlapCount = FHTransform.kOverlapCount;
        private static readonly double ONEDEG = Math.Pow(2, 1.0 / 12);
        private double shiftRate = 4.0 / 5;//Math.Pow(1/ONEDEG,3);
        private double[] mLastPhase = null;
        private double[] mSumPhase = null;
        private bool shiftChanged = true;
        FHTransform fht = new FHTransform();
        FHTransform ifht = new FHTransform();
        Overlap overlap;
        PitchShiftPV dspr, dspl;

        public override DSPType Type
        {
            get { return DSPType.PitchShiftPV; }
        }

        public double ShiftRate
        {
            set {
                shiftRate = value;
                shiftChanged = true;
                if (dspr != null)
                {
                    dspr.ShiftRate = dspl.ShiftRate = value;
                }
            }
            get { return shiftRate; }
        }

        public override void Process(ref short[] left, ref short[] right)
        {
            if (dspr == null)
            {
                dspr = new PitchShiftPV();
                dspl = new PitchShiftPV();
            }
            //dspl.Position = dspr.Position = Position;

            dspl.Processb(left, out left);
            dspr.Processb(right, out right);
            //Process(left, out left, true);
            //Process(right, out right, false);
        }

        public void Process(short[] datain, out short[] dataout)
        {
            var length = datain.Length;
            var mBitRev = FHTArrays.GetBitRevTable(length);
            var mPreWindow = FHTArrays.GetPreWindow(length);
            var mPostWindow = FHTArrays.GetPostWindow(length);
            var hopanal = length / kOverlapCount;
            var hopsyn = (int)(hopanal * ShiftRate);
            var temp = new double[length];

            if (shiftChanged || mLastPhase == null || mLastPhase.Length != length / 2)
            {
                mLastPhase = new double[length / 2];
                mSumPhase = new double[length / 2];
                var overlapCount = (int)Math.Ceiling((double)length / hopsyn);
                overlap = new Overlap(overlapCount, hopsyn);
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
           
            //var reshift = Stretch(re, ShiftRate, newlen / 2);
            //var imshift = Stretch(im, ShiftRate, newlen / 2);
            
            //AnalysisAndSynthesis(ref re, ref im);
            var reshift = re;
            var imshift = im;

            for (var i = 0; i < newlen / 2; i++)
            {
                temp_shift[mBitRev[i]] = reshift[i] + imshift[i];
                temp_shift[mBitRev[newlen - 1 - i]] = reshift[i] - imshift[i];
            }

            ifht.ComputeFHT(ref temp_shift, newlen, false);
            temp = temp_shift;
            for (var i = 0; i < length; i++)
            {
                temp_shift[i] = datain[i];
            }

            //dataout = ToShort(temp, length);
            //return;

            var window = FHTArrays.GetPostWindow(length);
            for (var i = 0; i < length; i++)
            {
                temp_shift[i] *= window[i];
            }
            overlap.AddOverlap(ref temp_shift);
            temp = new double[hopsyn];
            Array.Copy(temp_shift, temp, hopsyn);
            temp = Stretch(temp, 1/ShiftRate, length);

            dataout = ToShort(temp, length);
        }

        public void Processb(short[] datain, out short[] dataout)
        {
            // even without FFT, it works to some extent.
            var length = datain.Length;
            var hopanal = length / kOverlapCount;
            var hopsyn = (int)(hopanal * ShiftRate);
            var temp = new double[length];

            if (shiftChanged || overlap == null)
            {
                var overlapCount = (int)Math.Ceiling((double)length / hopsyn);
                overlap = new Overlap(overlapCount, hopsyn);
                shiftChanged = false;
            }

            var temp_shift = new double[length];
            for (var i = 0; i < length; i++)
            {
                temp_shift[i] = datain[i];
            }

            var window = FHTArrays.GetPostWindow(length);
            for (var i = 0; i < length; i++)
            {
                temp_shift[i] *= window[i] / 2 * ShiftRate;
            }
            overlap.AddOverlap(ref temp_shift);
            temp = new double[hopsyn];
            Array.Copy(temp_shift, temp, hopsyn);
            temp = Stretch(temp, 1 / ShiftRate, length);

            dataout = ToShort(temp, length);
        }

        private void AnalysisAndSynthesisa(ref double[] re, ref double[] im)
        {

            var length = re.Length;
            var osamp = FHTransform.kOverlapCount;
            var hopanal = length / kOverlapCount;
            var hopsyn = (int)(hopanal * ShiftRate);
            var dtanal = (double)length / kOverlapCount / mSampleRate;
            var dtsyn = dtanal * ShiftRate;

            var expct = 2 * Math.PI / osamp;
            var freqPerBin = mSampleRate / (length * 2);
            var omegaPerBin = (double)mSampleRate / (length * 2) * 2 * Math.PI;
            var anaMagn = new double[length];
            var anaFreq = new double[length];
            var synMagn = new double[length];
            var synFreq = new double[length];
            for (var i = 0; i < length; i++)
            {
                var magn = 2 * Math.Sqrt(re[i] * re[i] + im[i] * im[i]);
                var phase = Math.Atan2(im[i], re[i]);
                var omega = (phase - mLastPhase[i]) / dtanal - i * omegaPerBin;
                mLastPhase[i] = phase;
                var omegawrap = (omega + Math.PI) % (2 * Math.PI) - Math.PI;
                if (omegawrap < 0) omegawrap = 2 * Math.PI + omegawrap;
                var omegatrue = i * omegaPerBin + omegawrap;

                anaMagn[i] = magn;
                anaFreq[i] = omegatrue / (2 * Math.PI);
            }
            //synMagn = Stretch(anaMagn, shiftRate, length);
            //synFreq = Stretch(anaFreq, shiftRate, length);
            //for (var i = 0; i < length; i++) synFreq[i] *= shiftRate;
            /*
            for (var i = 0; i < length; i++)
            {
                var index = (int)(i * shiftRate);
                if (index >= length) break;
                synMagn[index] += anaMagn[i];
                synFreq[index] = anaFreq[i] * shiftRate;
            }
            */
            synMagn = anaMagn;
            synFreq = anaFreq;

            for (var i = 0; i < length; i++)
            {
                var magn = synMagn[i];
                var omegatrue = synFreq[i] * 2 * Math.PI;
                mSumPhase[i] += dtsyn * omegatrue;
                var phase = mSumPhase[i];
                re[i] = magn * Math.Cos(phase);
                im[i] = magn * Math.Sin(phase);
            }
        }

        private void AnalysisAndSynthesis(ref double[] re, ref double[] im)
        {
            var length = re.Length;
            var osamp = FHTransform.kOverlapCount;
            var expct = 2 * Math.PI / osamp;
            var freqPerBin = (double)mSampleRate / (length * 2);
            var anaMagn = new double[length];
            var anaFreq = new double[length];
            var synMagn = new double[length];
            var synFreq = new double[length];
            for (var i=0; i< length; i++)
            {
                var magn = 2*Math.Sqrt(re[i] * re[i] + im[i] * im[i]);
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
            //synMagn = Stretch(anaMagn, shiftRate, length);
            //synFreq = Stretch(anaFreq, shiftRate, length);
            //for (var i = 0; i < length; i++) synFreq[i] *= shiftRate;
            
            for (var i = 0; i < length; i++)
            {
                var index = (int)(i * shiftRate);
                if (index >= length) break;
                synMagn[index] += anaMagn[i];
                synFreq[index] = anaFreq[i] * shiftRate;
            }
            
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
