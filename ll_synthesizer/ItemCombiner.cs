using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;
using NAudio.Wave;

namespace ll_synthesizer
{
    class ItemCombiner: Streamable
    {
        private ArrayList list = new ArrayList();
        private string name = "Synthesized";
        private static WavPlayer wp;
        private int baseLength;
        private Stopwatch sw = new Stopwatch();
        private Thread factorCalculateThread;
        private System.Timers.Timer timer;
        private Dictionary<int, RandomizedFactor> randomFactors = new Dictionary<int, RandomizedFactor>();
        private Form1 form;

        // settings
        private static bool avoidAllMute = true;
        private static int compareSpan = 30; // in index +-
        private static short threashold = 50;
        private static double[] searchRegion = new double[] { 0, 0.01 };  // region of offset search
        private static double allowedError = 0.005;
        private static int minimumRefreshIntervalMs = 1000;
        private static double iconRefreshIntervalInMs = 500;
        private static int numOfElementsCompared = 2000;
        private static bool normalize = false;

        public double MelodyRemovalRatio
        {
            set { RandomizedFactor.RemovalRatio = value; }
        }

        public bool NormalizeEnabled
        {
            set { normalize = value; }
        }

        public bool AdjustWhenMuted { set; get; } = false;

        public ItemCombiner(Form1 form)
        {
            this.form = form;
            sw.Start();
            timer = new System.Timers.Timer(iconRefreshIntervalInMs);
            timer.Elapsed += new ElapsedEventHandler(RegularyRefreshIcons);
        }

        static public void SetWavPlayer(WavPlayer wp)
        {
            ItemCombiner.wp = wp;
        }

        static public void ApplySettings()
        {
            Settings settings = Settings.GetInstance();
            avoidAllMute = settings.AvoidAllMute;
            compareSpan = settings.CompareSpanIdx;
            threashold = settings.ThreasholdLevel;
            searchRegion[1] = settings.SearchRegionEnd;
            allowedError = settings.VocalReduceAllowedError;
            minimumRefreshIntervalMs = settings.MinimumRefreshInterval;
            iconRefreshIntervalInMs = settings.IconRefreshInterval;
            numOfElementsCompared = settings.NumOfElementsCompared;
            normalize = settings.NormalizeLRVolume;
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
                //baseLength = (baseLength > item.GetLength()) ? item.GetLength() : baseLength;
            }
            Subscribe(item);
            ResetRandomizedFactor();
            timer.Enabled = true;
        }

        void Subscribe(ItemSet item)
        {
            item.PlotRefreshed += this.RefleshAllPlot;
            item.Suicided += this.RemoveSuicider;
            item.FactorChanged += this.RefreshRequestReceived;
            item.MuteChanged += this.RandomizeFactorsOnMute;
        }

        void UnSubscribe(ItemSet item)
        {
            item.PlotRefreshed -= this.RefleshAllPlot;
            item.Suicided -= this.RemoveSuicider;
            item.FactorChanged -= this.RefreshRequestReceived;
            item.MuteChanged -= this.RandomizeFactorsOnMute;
        }

        public void Dispose()
        {
            timer.Stop();
            if (factorCalculateThread != null)
            {
                factorCalculateThread.Abort();
                factorCalculateThread = null;
            }
            int num = list.Count;
            for (int i = num-1; i >= 0; i--)
            {
                Dispose(i);
            }
            GraphPanel.ResetCounter();
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
            if (idx == 0 && list.Count > 0)
            {
                baseLength = GetItem(0).GetLength();
            }
        }

        public string[] GetLRStrength()
        {
            double[] facss = ComputeFactorSum(true);
            double[] facss2 = ComputeFactorSum(false);

            string[] str = new string[2];
            str[0] = (facss2[0] / facss[0]).ToString();
            str[1] = (facss2[1] / facss[1]).ToString();
            return str;
        }

        private void ResetRandomizedFactor()
        {
            if (RandomizedFactor.MaxMin == null)
            {
                RandomizedFactor.MaxMin = ((ItemSet)list[0]).GetFacsMaxMin();
                RandomizedFactor.RemovalRatio = 0.2;
                RandomizedFactor.AllowedError = allowedError;
            }
            var length = GetCount();
            for (var i=1; i<=length; i++)
            {
                RandomizedFactor fac;
                randomFactors.TryGetValue(i, out fac);
                if (fac == null)
                {
                    fac = new RandomizedFactor(i);
                    randomFactors.Add(i, fac);
                }
                if (!fac.FactorsCalced)
                {
                    fac.AsyncCalcRandomizedFactor();
                }
            }
        }

        public void ApplyRandomizedFactor()
        {
            ApplyRandomizedFactor(false);
        }

        public void ApplyRandomizedFactor(bool asap)
        {
            var unmuted = GetUnmutedItems();
            int num = unmuted.Count;
            if (num == 1) return;
            RandomizedFactor fac;
            randomFactors.TryGetValue(num, out fac);
            if (fac == null)
            {
                ResetRandomizedFactor();
                return;
            }
            if (!fac.FactorsCalced)
            {
                fac.AsyncCalcRandomizedFactor();
                return;
            }
            var factors = fac.Factors;
            for (int i = 0; i < num; i++)
            {
                ItemSet item = (ItemSet)unmuted[i];
                /* CenterCut版
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
                */
                // 逆位相版
                if (asap || ItemSet.FadingTime == 0)
                {
                    item.SetFactors(factors[i, 1], factors[i, 0]);
                }
                else
                {
                    item.AsyncSetLRBalance(factors[i, 0]);
                    item.AsyncSetTotalFactor(factors[i, 1]);
                }
            }
            fac.AsyncCalcRandomizedFactor();
        }

        private ArrayList GetUnmutedItems()
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

        private void RefleshAllPlot(object sender, EventArgs e)
        {
            foreach (ItemSet item in list) {
                item.PlotLR();
            }
        }

        private void RefreshAllIcon(bool forceRefresh = false)
        {
            if (sw.ElapsedMilliseconds > minimumRefreshIntervalMs || forceRefresh)
            {
                foreach (ItemSet item in list)
                {
                    item.Refresh();
                }
                if (!forceRefresh)
                    sw.Restart();
            }
        }

        delegate void generalDelegate(bool boolvalue);

        private void RegularyRefreshIcons(object sender, ElapsedEventArgs e)
        {
            form.BeginInvoke(new generalDelegate(RefreshAllIcon), new object[] { true });
        }

        private void RefreshRequestReceived(object sender, EventArgs e)
        {
            RefreshAllIcon();
        }

        private void RandomizeFactorsOnMute(object sender, EventArgs e)
        {
            if (AdjustWhenMuted)
                ApplyRandomizedFactor(true);
        }

        private void RemoveSuicider(object sender, EventArgs e)
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

        private double CompareDiff(short[] data1, short[] data2, int startidx1, int startidx2)
        {
            double diff2 = 0;
            int count = 0;
            int start = 0;
            if (startidx2 < 0 || startidx1 < 0) start = Math.Min(startidx1, startidx2) * -1;
            for (var i=start; i<numOfElementsCompared; i++)
            {
                diff2 += Math.Pow(data1[i + startidx1] - data2[i + startidx2], 2);
                count++;
            }
            return diff2 / count;
        }

        private int GetThresholdIdx(int itemidx, out short[] data)
        {
            ItemSet item1 = GetItem(itemidx);
            return GetThresholdIdx(item1, out data);
        }

        private int GetThresholdIdx(ItemSet item, out short[] data)
        {
            item.PrepareAdjustOffset();
            short[] left1 = item.GetData().GetLeft(searchRegion[0], searchRegion[1]);
            double newSearchEnd = searchRegion[1];
            int i1 = 0;
            bool loopFlag = true;
            while (true)
            {
                for (int i = 0; i < left1.Length; i++)
                {
                    if (left1[i] > threashold)
                    {
                        i1 = i;
                        loopFlag = false;
                        break;
                    }
                }
                if (!loopFlag)
                {
                    if (left1.Length - i1 < numOfElementsCompared)
                    {
                        left1 = item.GetData().GetLeft(searchRegion[0], newSearchEnd + searchRegion[1]);
                    }
                    break;
                }
                Console.WriteLine("Not enough");
                newSearchEnd *= 2;
                left1 = item.GetData().GetLeft(searchRegion[0], newSearchEnd);
            }
            data = left1;
            item.BackToPreparation();
            return i1;
        }

        private void SetBestOffset(int refidx, int idxOfTarget, short[] data1)
        {
            // refidx is the first threshold idx of the reference item.
            ItemSet item2 = GetItem(idxOfTarget);
            if (item2.OffsetAdjusted)
                return;
            short[] data2;
            int i2 = GetThresholdIdx(item2, out data2);
            int newi2 = i2;
            double diff = CompareDiff(data1, data2, refidx, i2 - compareSpan);
            for (int i = -compareSpan + 1; i <= compareSpan; i++)
            {
                double newdiff = CompareDiff(data1, data2, refidx, i2 + i);
                if (newdiff < diff)
                {
                    diff = newdiff;
                    newi2 = i2 + i;
                }
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
            Parallel.For(1, list.Count,  i=> SetBestOffset(idx1, i, data1));
            
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

        public ItemSet GetItem(int idx)
        {
            if (idx < list.Count && idx >= 0)
            {
                return (ItemSet)list[idx];
            }
            return null;
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

        string Streamable.GetTitle()
        {
            return name;
        }

        int Streamable.GetMaxTimeSeconds()
        {
            ItemSet item = GetLastItem();
            if (item == null)
            {
                return 0;
            }
            return (int)item.GetData().IdxToTime(baseLength);
        }

        short[] ComputeMean(int idx)
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
                }
                catch (ArgumentOutOfRangeException)
                {
                    // sometimes thrown when item removed while playing
                    // can be ignored
                }
                catch (NullReferenceException)
                {
                    continue;
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
            short newLeft, newRight;
            if (factorL > 1 || normalize)
            {
                newLeft = ToShortDevidedBy(left, factorL);
            }
            else newLeft = Convert.ToInt16(left);
            if (factorR > 1 || normalize)
            {
                newRight = ToShortDevidedBy(right, factorR); ;
            }
            else newRight = Convert.ToInt16(right);
            return new short[] {newLeft, newRight};
        }

        static short ToShortDevidedBy(int value, double factor)
        {
            if (factor < 0.000000000000001) return 0;
            int newval = (int)(value / factor);
            if (newval > short.MaxValue) return short.MaxValue;
            if (newval < short.MinValue) return short.MinValue;
            return Convert.ToInt16(newval);
        }

        public double[] ComputeFactorSum(bool isAbs)
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

        void Streamable.GetLRBuffer(int start, int size, out short[] left, out short[] right)
        {
            left = new short[size];
            right = new short[size];
            for (int i = 0; i < size; i++)
            {
                short[] mean = ComputeMean(i+start);
                left[i] = mean[0];
                right[i] = mean[1];
            }
        }

        bool Streamable.IsReady()
        {
            ItemSet item = GetLastItem();
            if (item == null)
                return false;
            return true;
        }

        WaveFormat Streamable.GetWaveFormat()
        {
            return GetLastItem().GetData().WaveFormat;
        }
    }
}
