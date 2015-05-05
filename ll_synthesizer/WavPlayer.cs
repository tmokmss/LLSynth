using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.DirectX.DirectSound;

namespace ll_synthesizer
{
    class WavPlayer
    {
        private BufferDescription bufferDesc;
        private WaveFormat waveFormat;
        private SecondaryBuffer buffer;
        private Device device = null;
        private Notify notify;
        private Thread mThread;
        public delegate void ProcessEventHandler(object sender, ProcessEventArgs e);
        public event ProcessEventHandler PlayReachedBy;

        private static int m_StreamBufferSize = 262144/4;
        private static int m_numberOfSectorsInBuffer = 4;
        private static int m_SectorSize = m_StreamBufferSize / m_numberOfSectorsInBuffer;
        private short[] m_transferBuffer = new short[m_SectorSize/2];
        private int m_secondaryBufferWritePosition = 0;
        private int m_lastPlayingPosition = 0;
        private int volume = 0;
        private bool repeating = false;
        public bool Repeat
        {
            set { repeating = value; }
        }

        public static int BufSize
        {
            get { return m_SectorSize; }
        }

        public int Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        public WavPlayer(Form1 form)
        {
            device = new Device();
            device.SetCooperativeLevel(form, CooperativeLevel.Priority);
        }

        void setBufferAndWave()
        {
            waveFormat = new Microsoft.DirectX.DirectSound.WaveFormat();
            waveFormat.SamplesPerSecond = 44100;
            waveFormat.Channels = 2;
            waveFormat.FormatTag = WaveFormatTag.Pcm;
            waveFormat.BitsPerSample = 16;
            waveFormat.BlockAlign = (short)(waveFormat.Channels * waveFormat.BitsPerSample / 8);
            waveFormat.AverageBytesPerSecond = waveFormat.BlockAlign * waveFormat.SamplesPerSecond;

            bufferDesc = new BufferDescription(waveFormat);
            bufferDesc.DeferLocation = true;
            bufferDesc.ControlEffects = false;
            bufferDesc.ControlFrequency = true;
            bufferDesc.ControlPan = true;
            bufferDesc.ControlVolume = true;
            bufferDesc.ControlPositionNotify = true;
            bufferDesc.GlobalFocus = true;
            bufferDesc.BufferBytes = m_StreamBufferSize;
        }

        public void Stop()
        {
            isDoing = false;
            Array.Clear(m_transferBuffer, 0, m_transferBuffer.Length);
            stream = null;
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
            if (mThread != null)
            {
                mThread.Abort();
                mThread = null;
            }
        }

        public void Pause()
        {
            if (buffer != null)
            {
                try
                {
                    if (buffer.Status.Playing)
                        buffer.Stop();
                }
                catch (AccessViolationException) { }
                int trash;
                buffer.GetCurrentPosition(out m_lastPlayingPosition, out trash);
            }
        }

        public bool Resume()
        {
            // returns if resume action was successful
            if (buffer != null)
            {
                buffer.SetCurrentPosition(m_lastPlayingPosition);
                buffer.Play(0, BufferPlayFlags.Looping);
                return true;
            }
            return false;
        }

        public void Close()
        {
            if (mThread != null)
            {
                mThread.Abort();
                mThread = null;
            }
            if (buffer != null)
            {
                /*
                if (IsPlaying())
                    buffer.Stop();
                 */
                buffer.Dispose();
                buffer = null;
            }
            if (device != null)
            {
                device.Dispose();
                device = null;
            }
        }

        public bool IsPlaying()
        {
            if (buffer == null)
                return false;
            try
            {
                return buffer.Status.Playing;
            }
            catch (AccessViolationException)
            {
                return false;
            }
        }

        public Streamable GetCurrentStream()
        {
            return stream;
        }

        AutoResetEvent are;
        Boolean isPlaying, isDoing;
        Streamable stream;

        public void Play(WavData wd, int PlayPosition)
        {
            position = PlayPosition;
            Play(wd);
        }

        public void Seek(double ratio)
        {
            if (stream != null)
            {
                position = (int)(stream.GetLength() * ratio);
                progressSoFar = GetProgress();
            }
        }

        double reportInterval = 0.01;
        double progressSoFar = 0;

        void SetInterval()
        {
            reportInterval = 1.0/stream.GetMaxTimeSeconds();
            progressSoFar = -reportInterval;
        }

        double GetProgress()
        {
            return position * 1.0 / (stream.GetLength() - m_SectorSize- 1);
        }

        void ReportProgress()
        {
            double progress = GetProgress();
            int maxtime = stream.GetMaxTimeSeconds();
            if (progress > progressSoFar + reportInterval)
            {
                progressSoFar = progress;
                ProcessEventArgs e = new ProcessEventArgs();
                e.progress = progress;
                e.maxTimeSeconds = maxtime;
                e.title = stream.GetTitle();
                if (PlayReachedBy != null)
                    PlayReachedBy(this, e);
                if (Math.Round((maxtime * progress)) == maxtime)
                {
                    if (repeating)
                    {
                        //isDoing = false;
                        //Play(stream);
                        Seek(0);
                    }
                    else
                        Stop();
                }
            }
        }

        public void Play(Streamable stream)
        {
            Stop();

            m_secondaryBufferWritePosition = 0;
            position = 0;
            progressSoFar = 0;

            this.stream = stream;
            setBufferAndWave();
            SetInterval();
            buffer = new SecondaryBuffer(bufferDesc, device);

            mThread = new Thread(new ThreadStart(OutputEventTask));
            mThread.Name = "DataTransferThread";
            //mThread.IsBackground = true;
            are = new AutoResetEvent(false);

            notify = new Notify(buffer);
            BufferPositionNotify[] psa = new BufferPositionNotify[m_numberOfSectorsInBuffer];
            for (int i=0; i< m_numberOfSectorsInBuffer; i++) {
                psa[i].Offset = m_SectorSize*(i+1)-1;
                psa[i].EventNotifyHandle = are.SafeWaitHandle.DangerousGetHandle();
            }
            notify.SetNotificationPositions(psa, m_numberOfSectorsInBuffer);

            isDoing = true;
            mThread.Start();

            isPlaying = true;

            //TransferBuffer();
            buffer.Play(0, BufferPlayFlags.Looping);
        }


        int position = 0;

        void OutputEventTask()
        {
            while (isDoing)
            {
                buffer.Volume = volume;
                ReportProgress();
                are.WaitOne(Timeout.Infinite, true);
                TransferBuffer();
            }
        }

        void TransferBuffer()
        {
            short[] left, right;
            stream.GetLRBuffer(position, m_SectorSize / 4, out left, out right);
            Array.Clear(m_transferBuffer, 0, m_SectorSize / 2);
            MakeShortArrayFromRL(left, right, ref m_transferBuffer);

            buffer.Write(m_secondaryBufferWritePosition, m_transferBuffer, LockFlag.None);
            m_secondaryBufferWritePosition += m_SectorSize;
            m_secondaryBufferWritePosition %= m_StreamBufferSize;
            position += m_SectorSize / 4;
        }

        void MakeShortArrayFromRL(short[] left, short[] right, ref short[] output)
        {
            for (int i = 0; i < left.Length; i++)
            {
                output[2*i] = left[i];
                output[2 * i + 1] = right[i];
            }
        }
    }
}
