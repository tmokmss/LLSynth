using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ll_synthesizer
{
    class FileGetter
    {
        private string dirPath;
        private string[] paths;
        private static string[] exts = new string[] {".wav", ".mp3"};
        private static string wild = "*";

        public FileGetter(string dirPath)
        {
            this.dirPath = dirPath;
            try
            {
                paths = Directory.GetFiles(dirPath, wild + exts[0]);
                int wavLen = paths.Length;
                string[] mp3s = Directory.GetFiles(dirPath, wild + exts[1]);
                Array.Resize(ref paths, paths.Length + mp3s.Length);
                Array.Copy(mp3s, 0, paths, wavLen, mp3s.Length);
            }
            catch (DirectoryNotFoundException)
            {
                paths = new string[0];
            }
        }

        public static bool HasValidFileExtension(string path)
        {
            foreach (string ext in exts)
            {
                if (path.EndsWith(ext))
                    return true;
            }
            return false;
        }

        public string[] GetList() {
            return paths;
        }

        public int GetNumber()
        {
            return paths.Length;
        }

        public string GetFileName(int i)
        {
            return paths[i];
        }
    }
}

