using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using ll_synthesizer.DSPs.Config;

namespace ll_synthesizer.DSPs
{
    abstract class DSP
    {
        protected int mSampleRate = 44100;
        private static double shiftRate = 4.0 / 5;//Math.Pow(1/ONEDEG,3);
        protected ConfigWindow window;

        abstract public DSPType Type { get; }
        public abstract void Process(ref short[] left, ref short[] right);

        public void ShowConfigWindow(string title)
        {
            if (window == null || window.IsDisposed)
            {
                var factory = ConfigWindowFactory.GetInstance();
                window = factory.CreateConfigWindow(this);
            }
            window.Visible = !window.Visible;
            window.Text = title;
        }

        protected static double[] Stretch(double[] datain, double ratio)
        {
            return Stretch(datain, ratio, (int)Math.Round(datain.Length * ratio));
        }

        protected static double[] Stretch(double[] datain, double ratio, int size)
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

        public void LowPassFiltering(short[] datain, out short[] dataout)
        {
            int size = datain.Length;
            dataout = new short[size];
            for (int i = 1; i < size; i++)
            {
                dataout[i] = Convert.ToInt16(0.9*dataout[i - 1] + 0.1*datain[i]);
            }
        }

        protected static short[] ToShort(double[] array, int size)
        {
            // size is for performance reason
            short[] newarr = new short[size];
            for (int i = 0; i < size; i++)
            {
                newarr[i] = ToShort(array[i]);
            }
            return newarr;
        }

        protected static short ToShort(double val) {
            if (val > short.MaxValue)
                return short.MaxValue;
            else if (val < short.MinValue)
                return short.MinValue;
            else if (Double.IsNaN(val))
                return 0;
            return Convert.ToInt16(val);
        }

        protected static double[] ToDouble(short[] array, int size)
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
