﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace ll_synthesizer
{
    class ItemCombiner: Streamable
    {
        private ArrayList list = new ArrayList();
        private String name = "Synthesized";
        private static WavPlayer wp;
        private int baseLength;
        private Stopwatch sw = new Stopwatch();

        private static readonly short MAX_SHORT = 32767;
        private static readonly short MIN_SHORT = -32768;

        // settings
        private bool avoidAllMute = true;
        private static int compareSpan = 30; // in index +-
        private static short threashold = 50;
        private double[] searchRegion = new double[] { 0, 0.01 };  // region of offset search
        private double target = 0.2;    // targeted instrument/total ratio
        private double allowedError = 0.005;
        private int minimumRefreshIntervalMs = 100;

        public double MelodyRemovalRatio
        {
            set { target = value; }
        }

        public ItemCombiner()
        {
            sw.Start();
        }

        static public void SetWavPlayer(WavPlayer wp)
        {
            ItemCombiner.wp = wp;
        }

        public void AddItem(ItemSet item)
        {
            list.Add(item);
            if (list.Count == 1)
            {
                baseLength = item.GetLength();
            }
            else
            {
                baseLength = (baseLength > item.GetLength()) ? item.GetLength() : baseLength;
            }
            Subscribe(item);
            AsyncRandomizeFactor();
            RefreshAllIcon(true);
        }

        void Subscribe(ItemSet item)
        {
            item.PlotRefreshed += this.RefleshAllPlot;
            item.Suicided += this.RemoveSuicider;
            item.FactorChanged += this.RefreshRequestReceived;
        }

        void UnSubscribe(ItemSet item)
        {
            item.PlotRefreshed -= this.RefleshAllPlot;
            item.Suicided -= this.RemoveSuicider;
            item.FactorChanged -= this.RefreshRequestReceived;
        }

        public void Dispose()
        {
            int num = list.Count;
            for (int i = num-1; i >= 0; i--)
            {
                Dispose(i);
            }
            list = null;
            GraphPanel.reset();
        }

        public void Dispose(int idx)
        {
            ItemSet item = GetItem(idx);
            if (item == null)
                return;
            list.RemoveAt(idx);
            UnSubscribe(item);
            item.Dispose();
            item = null;
        }

        public String[] GetLRStrength()
        {
            double[] facss = GetFactorSum(true);
            double[] facss2 = GetFactorSum(false);
            
            String[] str = new String[2];
            str[0] = (facss2[0] / facss[0]).ToString();
            str[1] = (facss2[1] / facss[1]).ToString();
            return str;
        }

        System.Random r = new System.Random();
        int[,] factors;
        bool factorsCalced = false;
        bool isFirst = true;
        private void CalcRandomizedFactor()
        {
            ArrayList unmuted = GetUnmutedItems();
            int num = unmuted.Count;
            if (num == 1)
                return; // there is no answer
            factorsCalced = false;
            factors = new int[num, 2];
            int[] maxmin = ((ItemSet)unmuted[0]).GetFacsMaxMin();

            int count = 0;
            double targetSaved = target;
            while (!IsAllowedFactors(factors))
            {
                for (int i = 0; i < num; i++)
                {
                    factors[i, 0] = r.Next(maxmin[0], maxmin[1]);
                    factors[i, 1] = r.Next(-maxmin[3], maxmin[3]);
                    //factors[i, 1] = r.Next(maxmin[2], maxmin[3]);
                }
                count++;
            }
            Console.WriteLine(count);
            factorsCalced = true;
            isFirst = false;
        }

        private void AsyncRandomizeFactor()
        {
            Thread tr = new Thread(new ThreadStart(CalcRandomizedFactor));
            tr.IsBackground = true;
            tr.Start();
        }

        public void ApplyRandomizedFactor()
        {
            if (isFirst)
                AsyncRandomizeFactor();
            if (!factorsCalced)
                return;
            ArrayList unmuted = GetUnmutedItems();
            int num = unmuted.Count;
            int numfac = factors.GetLength(0);
            num = (numfac < num) ? numfac : num;
            for (int i = 0; i < num; i++)
            {
                ItemSet item = (ItemSet)unmuted[i];
                item.AsyncSetLRBalance(factors[i, 0]);
                /*
                item.AsyncSetTotalFactor(factors[i, 1]);
                if (i < num*target)
                {
                    item.DSPEnabled = true;
                }
                else
                {
                    item.DSPEnabled = false;
                }
                */

                if (factors[i, 1] < 0)
                {
                    item.DSPEnabled = true;
                    item.AsyncSetTotalFactor(-factors[i, 1]);
                }
                else
                {
                    item.DSPEnabled = false;
                    item.AsyncSetTotalFactor(factors[i, 1]);
                }
            }
            AsyncRandomizeFactor();
        }

        int ccc = 0;
        bool IsAllowedFactors(int[,] orgfac)
        {
            if (ccc++ > 1)
            {
                ccc = 0;
                //return true;
            }
            bool isOK = false;
            for (int i = 0; i < orgfac.GetLength(0); i++)
            {
                if (orgfac[i, 1] > 0) {
                    isOK = true;
                    break;
                }
            }
            //return isOK;

            double sumR = 0;
            double sumL = 0;
            double sumAbsR = 0;
            double sumAbsL = 0;
            for (int i = 0; i < orgfac.GetLength(0); i++)
            {
                int val = orgfac[i, 0];
                double fac = (orgfac[i, 1] < 0) ? 1 : 1;
                double facR = Math.Cos(Math.PI / 40 * val + Math.PI / 4) * orgfac[i, 1] * fac;
                double facL = Math.Sin(Math.PI / 40 * val + Math.PI / 4) * orgfac[i, 1] * fac;
                sumR += facR;
                sumL += facL;
                sumAbsR += Math.Abs(facR);
                sumAbsL += Math.Abs(facL);
            }
            if (sumAbsR * sumAbsL == 0)
                return false;

            //double R = Math.Abs(sumR/sumAbsR);
            //double L = Math.Abs(sumL/sumAbsL);
            double R = sumR / sumAbsR;
            double L = sumL / sumAbsL;

            double target = this.target;
            if (R > target - allowedError && R < target + allowedError)
            {
                if (L > target - allowedError && L < target + allowedError)
                    return true;
            }
            return false;
        }

        ArrayList GetUnmutedItems()
        {
            ArrayList unmutedList = new ArrayList();
            foreach (ItemSet item in list) {
                if (!item.Muted) {
                    unmutedList.Add(item);
                }
            }
            if (unmutedList.Count == 0 && avoidAllMute)
            {
                unmutedList.Add(GetLastItem());
            }
            return unmutedList;
        }

        void RefleshAllPlot(object sender, EventArgs e)
        {
            foreach (ItemSet item in list) {
                item.PlotLR();
            }
        }

        void RefreshAllIcon(bool forceRefresh)
        {
            if (sw.ElapsedMilliseconds > minimumRefreshIntervalMs || forceRefresh)
            {
                foreach (ItemSet item in list)
                {
                    item.Refresh();
                }
                sw.Restart();
            }
        }

        void RefreshAllIcon()
        {
            RefreshAllIcon(false);
        }

        void RefreshRequestReceived(object sender, EventArgs e)
        {
            RefreshAllIcon();
        }

        void RemoveSuicider(object sender, EventArgs e)
        {
            if (list.Count == 1)
            {
                wp.Stop();
            }
            ItemSet item = (ItemSet)sender;
            int idx = list.IndexOf(item);
            Dispose(idx);
            if (idx == 0 && list != null)
            {
                foreach (ItemSet item1 in list)
                {
                    item1.OffsetAdjusted = false;
                }
            }
        }

        double CompareDiff(short[] data1, short[] data2, int startidx1, int startidx2)
        {
            double diff2 = 0;
            if (startidx1 < 0 || startidx2 < 0)
                return 0;
            for (int i = 0; i + startidx1 < data1.Length && i + startidx2 < data2.Length; i++)
            {
                diff2 += Math.Pow(data1[i + startidx1] - data2[i + startidx2], 2);
            }
            return diff2;
        }

        int GetThresholdIdx(int itemidx, out short[] data)
        {
            ItemSet item1 = GetItem(itemidx);
            return GetThresholdIdx(item1, out data);
        }

        int GetThresholdIdx(ItemSet item, out short[] data)
        {
            item.PrepareAdjustOffset();
            short[] left1 = item.GetData().GetLeft(searchRegion[0], searchRegion[1]);
            int i1 = 0;
            for (int i = 0; i < left1.Length; i++)
            {
                if (left1[i] > threashold)
                {
                    i1 = i;
                    break;
                }
            }
            data = left1;
            item.BackToPreparation();
            return i1;
        }

        void SetBestOffset(int refidx, int idx2, short[] data1)
        {
            // refidx is the first threshold idx of the reference item.
            ItemSet item2 = GetItem(idx2);
            if (item2.OffsetAdjusted)
                return;
            short[] data2;
            int i2 = GetThresholdIdx(item2, out data2);
            int newi2 = i2;
            if (true)
            {
                double diff = CompareDiff(data1, data2, refidx, i2 - compareSpan);
                Console.WriteLine(item2.GetData().GetName());
                Console.WriteLine(i2);
                for (int i = -compareSpan; i <= compareSpan; i++)
                {
                    double newdiff = CompareDiff(data1, data2, refidx, i2 + i);
                    if (newdiff < diff)
                    {
                        diff = newdiff;
                        newi2 = i2 + i;
                    }
                }
                Console.WriteLine(newi2);
                Console.WriteLine(newi2 - i2);
            }
            item2.SetOffset(newi2 - refidx);
            item2.OffsetAdjusted = true;
        }
        
        public void AdjustOffset()
        {
            if (GetCount() == 0)
                return;
            short[] data1;
            wp.Pause();
            int idx1 = GetThresholdIdx(0, out data1);
            for (int i=1; i<list.Count; i++) {
                SetBestOffset(idx1, i, data1);
            }
            wp.Resume();
        }

        public void AsyncAdjustOffset()
        {
            Thread tr = new Thread(new ThreadStart(AdjustOffset));
            tr.IsBackground = true;
            tr.Start();
        }

        public int GetCount()
        {
            return list.Count;
        }

        public int GetAvailable()
        {
            return list.Count;
        }

        public ItemSet GetItem(int idx)
        {
            ItemSet item;
            try
            {
                item = (ItemSet)list[idx];
            }
            catch (ArgumentOutOfRangeException)
            {
                if (list.Count > 0)
                {
                    item = (ItemSet)list[list.Count];
                }
                else
                {
                    item = null;
                }
            }
            return item;
        }

        public ItemSet GetLastItem()
        {
            return GetItem(list.Count - 1);
        }

        int Streamable.GetLength()
        {
            //return GetItem(0).GetLength();
            return baseLength;
        }

        String Streamable.GetTitle()
        {
            return name;
        }

        int Streamable.GetMaxTimeSeconds()
        {
            return (int)GetLastItem().GetData().IdxToTime(baseLength);
        }

        short[] GetMean(int idx)
        {
            int left = 0;
            int right = 0;
            ArrayList listUnmuted = GetUnmutedItems();
            int num = listUnmuted.Count;
            double factorL = 0;
            double factorR = 0;
            for (int i = 0; i < num; i++)
            {
                try
                {
                    WavData item = ((ItemSet)listUnmuted[i]).GetData();
                    left += item.GetLeft(idx);
                    right += item.GetRight(idx);
                    factorL += Math.Abs(item.GetFactor(idx, WavData.LEFT));
                    factorR += Math.Abs(item.GetFactor(idx, WavData.RIGHT));
                    //factorR += (item.GetFactor(idx, WavData.RIGHT));
                }
                catch (ArgumentOutOfRangeException) {
                // sometimes thrown when item removed while playing
                // can be ignored
                }
            }
            if (factorL == 0 && factorR == 0 && avoidAllMute)
            {
                WavData item = GetLastItem().GetData();
                item.IsDefault = true;
                left = item.GetLeft(idx);
                right = item.GetRight(idx);
                item.IsDefault = false;
                factorL = 1;
                factorR = 1;
            }
            short newLeft = ConvertToShort(left, factorL);
            short newRight = ConvertToShort(right, factorR);//Math.Abs(factorR)*num*2);
            return new short[] {newLeft, newRight};
        }

        static short ConvertToShort(int value, double factor)
        {
            if (factor < 0.000000000000001) return 0;
            int newval = (int)(value / factor);
            if (newval > MAX_SHORT) return MAX_SHORT;
            if (newval < MIN_SHORT) return MIN_SHORT;
            return Convert.ToInt16(newval);
        }

        public double[] GetFactorSum(bool isAbs)
        {
            double factorL = 0;
            double factorR = 0;
            foreach (ItemSet item in list)
            {
                if (isAbs)
                {
                    factorL += Math.Abs(item.GetData().GetFactor(0, WavData.LEFT));
                    factorR += Math.Abs(item.GetData().GetFactor(0, WavData.RIGHT));
                }
                else
                {
                    int fac = (item.DSPEnabled) ? -1 : 1;
                    factorL += item.GetData().GetFactor(0, WavData.LEFT) * fac;
                    factorR += item.GetData().GetFactor(0, WavData.RIGHT) * fac;
                }
            }
            return new double[] { factorL, factorR };
        }

        public void Combine()
        {
            short[] left = new short[baseLength];
            short[] right = new short[baseLength];
            for (int i = 0; i < baseLength; i++)
            {
                short[] mean = GetMean(i);
                left[i] = mean[0];
                right[i] = mean[1];
            }
        }

        void Streamable.GetLRBuffer(int start, int size, out short[] left, out short[] right)
        {
            left = new short[size];
            right = new short[size];
            for (int i = 0; i < size; i++)
            {
                short[] mean = GetMean(i+start);
                left[i] = mean[0];
                right[i] = mean[1];
            }
        }
    }
}
