using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace ll_synthesizer.Sound
{
    class MP3Reader: SoundFileReader
    {
        byte[] data;
        int position;
        int length;
        public MP3Reader(String path)
        {
            Mp3FileReader fr = new Mp3FileReader(path);
            length = (int)fr.Length;
            data = new byte[length];
            fr.Read(data, 0, length);
            fr.Close();
        }

        public int Position
        {
            set { position = value; }
            get { return position; }
        }

        public int Length
        {
            get { return length; }
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            Array.Copy(data, position, buffer, offset, count); 
        }

        public void Dispose()
        {
            data = null;
        }

    }
}
