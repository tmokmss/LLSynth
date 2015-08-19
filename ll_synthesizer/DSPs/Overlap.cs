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
            
            /*
            // without windowning, just take average
            var end = overlapCount * overlapSize - A.Length;
            for (var i = 0; i < end; i++)
            {
                A[i] += overlap[overlapIdx, i];
                A[i] /= overlapCount;
                overlap[overlapIdx, i] = 0;
            }
            for (var i = end; i < overlapSize; i++)
            {
                A[i] += overlap[overlapIdx, i];
                A[i] /= (overlapCount-1);
                overlap[overlapIdx, i] = 0;
            }
            */

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
                    var index = j + (i + 1) * overlapSize;
                    if (index >= A.Length) break;
                    overlap[writeIdx, j] += A[index];
                }
            }
        }
    }
}
