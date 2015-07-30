using System.Collections;
using System.Windows.Forms;

namespace ll_synthesizer
{
    class CharUtl
    {
        private static Keys Year1Key = Keys.D1;
        private static Keys Year2Key = Keys.D2;
        private static Keys Year3Key = Keys.D3;
        private static Keys AllKey = Keys.D9;
        private static Keys PrimptempsKey = Keys.P;
        private static Keys BiBiKey = Keys.B;
        private static Keys LilyWhiteKey = Keys.W;
        private static Keys NoneKey = Keys.D0;
        private static ArrayList keyList;

        static private void init()
        {
            keyList = new ArrayList();
            keyList.Add(Year1Key);
            keyList.Add(Year2Key);
            keyList.Add(Year3Key);
            keyList.Add(AllKey);
            keyList.Add(PrimptempsKey);
            keyList.Add(BiBiKey);
            keyList.Add(LilyWhiteKey);
            keyList.Add(NoneKey);
        }

        public static bool Is1Year(string name)
        {
            if (name == "HANAYO" | name == "MAKI" | name == "RIN")
                return true;
            return false;
        }

        public static bool Is2Year(string name)
        {
            if (name == "HONOKA" | name == "KOTORI" | name == "UMI")
                return true;
            return false;
        }

        public static bool Is3Year(string name)
        {
            if (name == "NOZOMI" | name == "ELI" | name == "NICO")
                return true;
            return false;
        }

        public static bool IsPrimtemps(string name)
        {
            if (name == "HONOKA" || name == "HANAYO" || name == "KOTORI")
                return true;
            return false;
        }

        public static bool IsBiBi(string name)
        {
            if (name == "ELI" || name == "NICO" || name == "MAKI")
                return true;
            return false;
        }

        public static bool IsLilyWhite(string name)
        {
            if (name == "UMI" || name == "RIN" || name == "NOZOMI")
                return true;
            return false;
        }

        public static bool CanProcess(Keys key)
        {
            if (keyList == null)
                init();
            if (keyList.Contains(key))
                return true;
            return false;
        }

        public static bool SelectIsMute(Keys key, string name)
        {
            if (key == Year1Key)
                return Is1Year(name);
            if (key == Year2Key)
                return Is2Year(name);
            if (key == Year3Key)
                return Is3Year(name);
            if (key == AllKey)
                return true;
            if (key == PrimptempsKey)
                return IsPrimtemps(name);
            if (key == BiBiKey)
                return IsBiBi(name);
            if (key == LilyWhiteKey)
                return IsLilyWhite(name);
            if (key == NoneKey)
                return false;
            return true;
        }
    }
}