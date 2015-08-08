using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer.DSPs
{
    class Overlap
    {
        private double[,] overlap;
        private int overlapIdx = 0;
        private int overlapSize;
        private int overlapCount;

        public Overlap(int overlapCount, int overlapSize)
        {
            this.overlapCount = overlapCount;
            this.overlapSize = overlapSize;
            overlap = new double[overlapCount, overlapSize];
        }

        public void AddOverlap(ref double[] A)
        {
            if (overlapSize <= 0) return;

            for (var i = 0; i < overlapSize; i++)
            {
                A[i] += overlap[overlapIdx, i];
                overlap[overlapIdx, i] = 0;
            }
            overlapIdx = (overlapIdx + 1) % overlapCount;

            for (var i = 0; i < overlapCount - 1; i++)
            {
                var writeIdx = (overlapIdx + i) % overlapCount;
                for (var j = 0; j < overlapSize; j++)
                {
                    overlap[writeIdx, j] += A[j + (i + 1) * overlapSize];
                }
            }
        }
    }
}
