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
        private short prePrevValy;
        private short prePrevValx;
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
            //dataout[0] = CalcFilteredValue(data[0], prevValx, prevValy);
            dataout[0] = CalcFilteredValue(data[0], prevValx, prePrevValx, prevValy, prePrevValy);
            dataout[1] = CalcFilteredValue(data[1], data[0], prevValx, dataout[1], prevValy);
            for (int i = 2; i < size; i++)
            {
                //dataout[i] = CalcFilteredValue(data[i], data[i - 1], dataout[i-1]);
                dataout[i] = CalcFilteredValue(data[i], data[i - 1], data[i - 2], dataout[i - 1], dataout[i - 2]);
            }
            prevValy = dataout[size / kOverlapCount - 1];
            prevValx = data[size / kOverlapCount - 1];
            prePrevValy = dataout[size / kOverlapCount - 2];
            prePrevValx = data[size / kOverlapCount - 2];

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
            a = -2 / (T * tau + 2);
            c = -a;
            b = -(T * tau - 2) / (T * tau + 2);

            fs = mSampleRate;
            fc = cutoffFrequency;
            var fb = cutoffFrequency + 100;
            var pi = Math.PI;
            c = (Math.Tan(pi * fb / fs) - 1) / (Math.Tan(2 * pi * fb / fs) + 1);
            a = -Math.Cos(2 * pi * fc / fs);
        }

        private short CalcFilteredValue(short x1, short x0, short y0)
        {
            return Convert.ToInt16(b * y0 + c * x1 + a * x0);
        }

        private short CalcFilteredValue(short x2, short x1, short x0, short y1, short y0)
        {
            double val = -c * x2 + a * (1 - c) * x1 + x0 - a * (1 - c) * y1 + c * y0;
            return Convert.ToInt16(val);
        }
    }
}
