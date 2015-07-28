﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NAudio.Wave;
using ll_synthesizer.Sound;
using ll_synthesizer.DSPs;

namespace ll_synthesizer
{
    class WavData: Streamable
    {
        private String path;
        private SoundFileReader wfr;
        private DSP myDSP;
        private static int bufSizeDefault = WavPlayer.BufSize / 2;
        private int bufSize;  // size in short. twice when byte
        private int overlapSize;
        private short[] leftBuf;
        private short[] rightBuf; // buffer
        // can be accessed as leftBuf[idxWithOffset-startPosition], which is left[idxWithoutOffset]
        // left[idxWithOffset] = leftBuf[idxWithOffset - startPosition + offset]
        // e.g.) leftBuf[10]  = left[1], where offset = 9, startPosition = 0

        private int length;
        private int samplesPerSecond;
        private int offset = 0;
        private int startPositionR, startPositionL; //buffer start position WITH offset
        private bool muted = false;
        private bool isDefault = false;
        private double factorL = 1 / Math.Sqrt(2);
        private double factorR = 1 / Math.Sqrt(2);
        private double factor = 1;

        public const bool LEFT = true;
        public const bool RIGHT = false;

        public bool DSPEnabled
        {
            set { myDSP.Enabled = value; }
            get { return myDSP.Enabled; }
        }
        
        public static int BufSizeDefault
        {
            get { return bufSizeDefault; }
        }

        public int OverlapSize
        {
            set {
                if (value >= bufSize / 2)
                {
                    overlapSize = bufSize / 4;
                }
                else overlapSize = value;
            }
            get { return overlapSize; }
        }

        public bool Muted
        {
            set { muted = value; }
            get { return muted; }
        }
        public int Offset
        {
            set { offset = value; }
            get { return offset; }
        }
        public bool IsDefault
        {
            set { isDefault = value; }
        }
        public WaveFormat WaveFormat
        {
            get { return wfr.WaveFormat; }
        }

        public WavData(String path)
        {
            this.path = path;
            this.samplesPerSecond = 44100;
            bufSize = WavPlayer.BufSize / 2;
            if (path.EndsWith("wav")) {
                wfr = new WaveReader(path);
            }
            else if (path.EndsWith("mp3")) {
                wfr = new MP3Reader(path);
            }
            ChangeBufSize(bufSize);
            this.myDSP = new DSP();
            this.length = (int)wfr.Length/4;
            OverlapSize = (int)(bufSize / 2.2);
            myDSP.SetOverlapSize(overlapSize);
            FetchBuffer(0);
        }

        public bool isMP3()
        {
            return path.EndsWith("mp3");
        }

        public void ChangeBufSize(int bufSize)
        {
            this.bufSize = bufSize;
            leftBuf = new short[bufSize];
            rightBuf = new short[bufSize];
        }

        public void SetOffset(double ms)
        {
            this.offset = MsToIdx(ms);
        }

        String Streamable.GetTitle()
        {
            return GetName();
        }

        int Streamable.GetLength()
        {
            return GetLength();
        }

        WaveFormat Streamable.GetWaveFormat()
        {
            return WaveFormat;
        }

        int Streamable.GetMaxTimeSeconds()
        {
            return (int)IdxToTime(GetLength());
        }

        public int GetLength()
        {
            return length;
        }

        public void Dispose()
        {
            wfr.Dispose();
            wfr = null;
            leftBuf = null;
            rightBuf = null;
        }

        public short[] GetLeft()
        {
            return GetWithOffset(LEFT);
        }

        public short[] GetRight()
        {
            return GetWithOffset(RIGHT);
        }

        public short GetLeft(int i)
        {
            return GetWithOffset(i, LEFT);
        }
        
        public short GetRight(int i)
        {
            return GetWithOffset(i, RIGHT);
        }

        private short[] GetWithOffset(bool isLeft)
        {
            short[] withofs = new short[length];
            for (int i = 0; i < length; i++)
            {
                withofs[i] = GetWithOffset(i, isLeft);
            }
            return withofs;
        }

        public double GetFactor(int idx, bool isLeft)
        {
            if (isDefault) return 1;
            if (muted) return 0;
            if (isLeft) return factorL * factor;
            return factorR * factor;
        }

        public void SetFactor(double fac, bool isLeft)
        {
            if (isLeft) factorL = fac;
            else factorR = fac;
        }

        public void SetFactor(double fac)
        {
            factor = fac;
        }

        private int IdxOffset(int idx, bool checkBorder = true)
        {
            int newidx = idx + offset;
            if (checkBorder)
            {
                if (newidx < 0)
                    return 0;
                if (newidx > length-1)
                    return length - 1;
            }
            return newidx;
        }

        public short[] GetLeft(double start, double end)
        {
            int startIdx = RatioToIdx(start);
            int endIdx = RatioToIdx(end);
            return GetLeft(startIdx, endIdx);
        }

        public short[] GetRight(double start, double end)
        {
            int startIdx = RatioToIdx(start);
            int endIdx = RatioToIdx(end);
            return GetRight(startIdx, endIdx);
        }

        public short[] GetLeft(int start, int end)
        {
            return GetLeft(end - start, start, end);
        }

        public short[] GetRight(int start, int end)
        {
            return GetRight(end - start, start, end);
        }

        public short[] GetLeft(int sampleNum, double start, double end)
        {
            int startIdx = RatioToIdx(start);
            int endIdx = RatioToIdx(end);
            return GetLeft(sampleNum, startIdx, endIdx);
        }

        public short[] GetRight(int sampleNum, double start, double end)
        {
            int startIdx = RatioToIdx(start);
            int endIdx = RatioToIdx(end);
            return GetRight(sampleNum, startIdx, endIdx);
        }

        public short[] GetLeft(int sampleNum, int start, int end)
        {
            return extractArray(sampleNum, start, end, LEFT);
        }

        public short[] GetRight(int sampleNum, int start, int end)
        {
            return extractArray(sampleNum, start, end, RIGHT);
        }

        private short[] extractArray(int sampleNum, int start, int end, bool isLeft)
        {
            int di = (int)Math.Floor((end - start) * 1.0 / sampleNum);
            if (di == 0) di = 1;
            short[] array = new short[sampleNum];
            try
            {
                for (int i = 0; i < sampleNum; i++)
                {
                    array[i] = GetWithOffset(i * di + start, isLeft);
                }
            }
            catch (Exception) { Console.WriteLine("Gotcha!!"); }
            return array;
        }

        private short GetWithOffset(int idxWithoutOffset, bool isLeft)
        {
            if (idxWithoutOffset >= length)
                return 0;
            int idx = IdxOffset(idxWithoutOffset);
            if (!isAvailable(idx, isLeft))
            {
                FetchBuffer(idxWithoutOffset);
            }
            int startPosition = (isLeft) ? startPositionL : startPositionR;
            double value;
            try
            {
                if (isLeft) value = leftBuf[idx - startPosition] * GetFactor(idx, isLeft);
                else value = rightBuf[idx - startPosition] * GetFactor(idx, isLeft);
            }
            catch (Exception)
            {
                return 0;
            }
            return ToShort(value);
        }

        private short ToShort(double value)
        {
            if (value < short.MinValue) return short.MinValue;
            if (value > short.MaxValue) return short.MaxValue;
            return Convert.ToInt16(value);
        }

        private void FetchBuffer(int idxWithoutOffset)
        {
            try
            {
                int startPosition = IdxOffset(idxWithoutOffset) - overlapSize;
                if (startPosition + bufSize > length)
                {
                    startPosition = length - bufSize;
                }
                else if (startPosition < IdxOffset(0))
                    startPosition = IdxOffset(0);
                startPositionL = startPositionR = startPosition;

                byte[] buffer = new byte[bufSize * 4];
                wfr.Position = (startPosition) * 4;
                wfr.Read(buffer, 0, bufSize * 4);

                for (int i = 0; i < bufSize; i++)
                {
                    leftBuf[i] = BitConverter.ToInt16(buffer, i * 4);
                    rightBuf[i] = BitConverter.ToInt16(buffer, i * 4 + 2);
                }
                //myDSP.PitchShift(rightBuf, out rightBuf);
                //myDSP.PitchShift(leftBuf, out leftBuf);
                //myDSP.CenterCut(ref leftBuf, ref rightBuf);
                myDSP.PitchShiftTD(rightBuf, out rightBuf);
                myDSP.PitchShiftTD(leftBuf, out leftBuf);
                //myDSP.LowPassFiltering(leftBuf, out leftBuf);
                //myDSP.LowPassFiltering(rightBuf, out rightBuf);
                /*
                if (DSPEnabled)
                {
                    var leftnew = new short[bufSize];
                    var rightnew = new short[bufSize];
                    for (var i = 1; i < bufSize; i++)
                    {
                        leftnew[i] = (short)((leftBuf[i] + leftBuf[i - 1]) / 2);
                        rightnew[i] = (short)((rightBuf[i] + rightBuf[i - 1]) / 2);
                    }
                    leftnew[0] = leftBuf[0];
                    rightnew[0] = rightBuf[0];
                    leftBuf = leftnew;
                    rightBuf = rightnew;
                }
                 */
            }
            catch (Exception)
            {
                Console.WriteLine("Gotcha!!!");
            }
        }

        private bool isAvailable(int idxWithOffset, bool isLeft)
        {
            // if a value is obtained from the buffer
            int startPosition = (isLeft) ? startPositionL : startPositionR;
            if (idxWithOffset >= startPosition && idxWithOffset < startPosition + bufSize - overlapSize) return true;
            return false;
        }

        void Streamable.GetLRBuffer(int start, int size, out short[] left, out short[] right)
        {
            int itr = size;
            if (start + size > length)
            {
                itr = length - start;
            }
            left = new short[size];
            right = new short[size];
            for (int i = 0; i < itr; i++)
            {
                left[i] = GetLeft(start + i);
                right[i] = GetRight(start + i);
            }
        }
            
        public int RatioToIdx(double ratio)
        {
            int maxIdx = length - 1;
            if (ratio < 0)
            {
                return 0;
            }
            else if (ratio > 1)
            {
                return maxIdx;
            }
            return (int)Math.Round(maxIdx * ratio);
        }

        public double IdxToTime(int idx) {
            return IdxOffset(idx, false)*1.0 / samplesPerSecond;
        }

        public double TimeToRatio(double time)
        {
            return time / IdxToTime(length);
        }

        public double IdxToRatio(int idx)
        {
            return TimeToRatio(IdxToTime(idx));
        }

        public double RatioToTime(double ratio)
        {
            return IdxToTime(RatioToIdx(ratio));
        }

        private int MsToIdx(double ms)
        {
            return (int)Math.Round(samplesPerSecond*ms/1000);
        }

        public String GetName()
        {
            return System.IO.Path.GetFileName(path);
        }

        bool Streamable.IsReady()
        {
            return true;
        }
    }
}
