using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace ll_synthesizer.Sound
{
    class WaveGenerator : SoundFileReader
    {
        private const int SAMPLERATE = 44100;
        private const int CHANNEL = 2;
        private const int LENGTH = SAMPLERATE * 3600;
        private int frequency = 100;
        private double[] sineUnit;

        public WaveGenerator(int frequency)
            : this(frequency, WaveType.Sine)
        {

        }

        public WaveGenerator(int frequency, WaveType type)
        {
            Frequency = frequency;
            WaveFormat = NAudio.Wave.WaveFormat.CreateALawFormat(SAMPLERATE, CHANNEL);
            WaveFormat = NAudio.Wave.WaveFormat.CreateCustomFormat(
                WaveFormatEncoding.Pcm,
                SAMPLERATE,   // sample rate
                CHANNEL,    // channel num
                SAMPLERATE*4,     // average byte per sec
                4,          // block align
                16);        // bits per sample
            Type = type;
        }

        public int Frequency {
            set
            {
                frequency = value;
                var oneCycle = SAMPLERATE / Frequency;
                sineUnit = new double[oneCycle];
                var omega = 2 * Math.PI / oneCycle;
                for (var i = 0; i < oneCycle; i++)
                {
                    sineUnit[i] = Math.Sin(omega * i);
                }
            }
            get { return frequency; }
        }

        public WaveType Type { set; get; }

        public int Position { set; get; }

        public int Length
        {
            get { return LENGTH; }
        }

        public WaveFormat WaveFormat { set; get; }

        public void Read(ref byte[] buffer, int offset, int count)
        {
            buffer = GenerateLRWave(Position / 4, count);
        }

        public void Dispose()
        {
        }

        private byte[] GenerateLRWave(int startTime, int size)
        {
            var uintlen = 2;
            var chanlen = size / uintlen / CHANNEL;
            double[] left, right;
            switch (Type)
            {
                case (WaveType.Sine):
                    left = GenerateSineWave(startTime, size);
                    break;
                case(WaveType.Square):
                    left = GenerateSquareWave(startTime, size);
                    break;
                case (WaveType.Sawtooth):
                    left = GenerateSawtoothWave(startTime, size);
                    break;
                case (WaveType.Triangle):
                    left = GenerateTriangleWave(startTime, size);
                    break;
                default:
                    left = GenerateSineWave(startTime, size);
                    break;
            }
            right = left;

            var wave = new byte[size];
            for (var i = 0; i < chanlen; i++)
            {
                var leftShort = (short)(left[i] * Int16.MaxValue);
                var rightShort = (short)(right[i] * Int16.MaxValue);
                var leftByte = BitConverter.GetBytes(leftShort);
                var rightByte = BitConverter.GetBytes(rightShort);
                Array.Copy(leftByte, 0, wave, i * uintlen * CHANNEL, uintlen);
                Array.Copy(rightByte, 0, wave, i * uintlen * CHANNEL + uintlen, uintlen);
            }
            return wave;
        }

        private double[] GenerateSineWave(int startTime, int size)
        {
            var oneCycle = SAMPLERATE / Frequency;
            var omega = 2 * Math.PI / oneCycle;
            var sine = new double[size];
            for (var i = 0; i < size; i++)
            {
                var omegat = omega * (i + startTime);
                sine[i] = Math.Sin(omegat);
                var tn = (i + startTime) % oneCycle;
                //sine[i] = sineUnit[tn];
            }
            return sine;
        }

        private double[] GenerateSineWaveCrip(int startTime, int size)
        {
            var oneCycle = SAMPLERATE / Frequency;
            var omega = 2 * Math.PI / oneCycle;
            var cycle = 0;
            var sine = new double[size];
            for (var i = 0; i < size; i++)
            {
                var omegat = omega * (i + startTime);
                sine[i] = Math.Sin(omegat);
                if ((i+startTime)%(2*oneCycle) > oneCycle)
                {
                    cycle++;
                    //sine[i] = 0;
                }
                
            }
            return sine;
        }

        private double[] GenerateSquareWave(int startTime, int size)
        {
            var oneCycle = SAMPLERATE / Frequency;
            var omega = 2 * Math.PI / oneCycle;
            var wave = new double[size];
            for (var i = 0; i < size; i++)
            {
                var omegat = omega * (i + startTime);
                if (Math.Sin(omegat) >= 0)
                    wave[i] = 1.0;
                else
                    wave[i] = (-1.0);
            }
            return wave;
        }

        private double[] GenerateSawtoothWave(int startTime, int size)
        {
            var oneCycle = SAMPLERATE / Frequency;
            var wave = new double[size];
            return wave;
        }

        private double[] GenerateTriangleWave(int startTime, int size)
        {
            var oneCycle = SAMPLERATE / Frequency;
            var wave = new double[size];
            return wave;
        }
    }
}