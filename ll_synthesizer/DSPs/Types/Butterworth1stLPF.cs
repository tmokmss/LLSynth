using System;

namespace ll_synthesizer.DSPs.Types
{
    /// <summary>
    /// 1次バタワースフィルタによるLPF
    /// </summary>1
    class Butterworth1stLPF : DSP
    {
        private Butterworth1stLPF dspr, dspl;
        private short prevValy;
        private short prevValx;
        private double a, b, c;
        private double cutoffFrequency;

        public override DSPType Type
        {
            get
            {
                return DSPType.Butterworth1stLPF;
            }
        }

        public double CutoffFrequency
        {
            set
            {
                cutoffFrequency = value;
                if (dspr != null)
                {
                    dspr.CutoffFrequency = dspl.CutoffFrequency = value;
                }
                DefineCoefficients();
            }
            get { return cutoffFrequency; }
        }

        public Butterworth1stLPF()
        {
            CutoffFrequency = 1000;
        }

        public override void Process(ref short[] left, ref short[] right)
        {
            if (dspr == null)
            {
                dspr = new Butterworth1stLPF();
                dspl = new Butterworth1stLPF();
            }

            dspl.Process(left, out left);
            dspr.Process(right, out right);
        }

        private void Process(short[] data, out short[] dataout)
        {
            int size = data.Length;
            dataout = new short[size];
            dataout[0] = CalcFilteredValue(data[0], prevValx, prevValy);
            for (int i = 1; i < size; i++)
            {
                dataout[i] = CalcFilteredValue(data[i], data[i - 1], dataout[i-1]);// Convert.ToInt16(0.9 * dataout[i - 1] + 0.1 * data[i]);
            }
            prevValy = dataout[size/kOverlapCount-1];
            prevValx = data[size / kOverlapCount - 1];

        }

        private void DefineCoefficients()
        {
            var fs = mSampleRate;
            var fc = cutoffFrequency;
            var alpha = 2 * Math.PI * fc / fs;
            a = alpha / (2 + alpha);
            c = a;
            b = (2 - alpha) / (2 + alpha);

            
            var T = 1.0 / mSampleRate;
            var tau = 1.0 / (2 * Math.PI * cutoffFrequency);
            a = - 2 / (T * tau + 2);
            c = -a;
            b = -(T * tau - 2) / (T * tau + 2);
            
            
        }

        private short CalcFilteredValue(short x1, short x0, short y0)
        {
            return Convert.ToInt16(b * y0 + c * x1 + a * x0);
        }
    }
}
