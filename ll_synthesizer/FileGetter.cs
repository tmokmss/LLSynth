using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ll_synthesizer
{
    class FileGetter
    {
        private String dirPath;
        private String[] paths;
        private static String[] exts = new String[] {".wav", ".mp3"};
        private static String wild = "*";

        public FileGetter(String dirPath)
        {
            this.dirPath = dirPath;
            try
            {
                paths = Directory.GetFiles(dirPath, wild + exts[0]);
                int wavLen = paths.Length;
                String[] mp3s = Directory.GetFiles(dirPath, wild + exts[1]);
                Array.Resize(ref paths, paths.Length + mp3s.Length);
                Array.Copy(mp3s, 0, paths, wavLen, mp3s.Length);
            }
            catch (DirectoryNotFoundException)
            {
                paths = new String[0];
            }
        }

        public static bool IsValidFile(String path)
        {
            foreach (String ext in exts)
            {
                if (path.EndsWith(ext))
                    return true;
            }
            return false;
        }

        public String[] GetList() {
            return paths;
        }

        public int GetNumber()
        {
            return paths.Length;
        }

        public String GetFileName(int i)
        {
            return paths[i];
        }
    }
}

