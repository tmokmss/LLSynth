using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace ll_synthesizer
{
    interface Streamable
    {
        /// <summary>
        /// Gets both left and right buffer.
        /// </summary>
        /// <param name="start">start index</param>
        /// <param name="size">size of the buffers</param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        void GetLRBuffer(int start, int size, out short[] left, out short[] right);

        /// <summary>
        /// Gets the end index of this stream.
        /// </summary>
        /// <returns>index of the end</returns>
        int GetLength();

        /// <summary>
        /// Gets the total time of this stream.
        /// </summary>
        /// <returns>total time in seconds</returns>
        int GetMaxTimeSeconds();

        /// <summary>
        /// returns if this stream is ready to give buffers.
        /// </summary>
        /// <returns>whether if it is ready</returns>
        bool IsReady();

        /// <summary>
        /// returns the title of this stream.
        /// </summary>
        /// <returns>the title</returns>
        string GetTitle();

        /// <summary>
        /// returns NAudio's WaveFormat of this stream.
        /// </summary>
        /// <returns>WaveFormat of this stream</returns>
        WaveFormat GetWaveFormat();
    }
}
