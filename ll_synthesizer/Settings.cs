using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;

namespace ll_synthesizer
{
    public class Settings
    {
        //[XmlIgnore]
        private string fileName = "..\\settings.config";
        //private const string filePath = basedir + fileName;
        private static Settings instance = new Settings();

        // ItemCombiner
        private bool avoidAllMute = true;
        private int compareSpan = 300; // in index +-
        private short threashold = 50;
        private double searchRegionEnd = 0.01;  // region of offset search
        //private double target = 0.2;    // targeted instrument/total ratio
        private double allowedError = 0.005;
        private int minimumRefreshIntervalMs = 1000;
        private double iconRefreshIntervalInMs = 500;
        private int numOfElementsCompared = 2000;
        private bool normalizeLRVolume = false;

        // WavPlayer
        private int m_StreamBufferSize = 262144 / 4;
        //private int m_numberOfSectorsInBuffer = 4;

        // GraphPanel
        private bool plotEnable = false;

        // Form1
        private static int randomizeInterval = 3;

        // General
        private string fontName = "Meiryo UI";
        private int fontSize = 9;

        public bool AvoidAllMute
        {
            set { avoidAllMute = value; }
            get { return avoidAllMute; }
        }
        public int CompareSpanIdx
        {
            set { compareSpan = value; }
            get { return compareSpan; }
        }
        public short ThreasholdLevel
        {
            set { threashold = value; }
            get { return threashold; }
        }
        public double SearchRegionEnd
        {
            set { searchRegionEnd = value; }
            get { return searchRegionEnd; }
        }
        public double VocalReduceAllowedError
        {
            set { allowedError = value; }
            get { return allowedError; }
        }
        public int MinimumRefreshInterval
        {
            set { minimumRefreshIntervalMs = value; }
            get { return minimumRefreshIntervalMs; }
        }
        public double IconRefreshInterval
        {
            set { iconRefreshIntervalInMs = value; }
            get { return iconRefreshIntervalInMs; }
        }
        public int NumOfElementsCompared
        {
            set { numOfElementsCompared = value; }
            get { return numOfElementsCompared; }
        }
        public int StreamBufferSize
        {
            set { m_StreamBufferSize = value; }
            get { return m_StreamBufferSize; }
        }
        public int AutoDJIntervalSecond
        {
            set { randomizeInterval = value; }
            get { return randomizeInterval; }
        }
        public string FontName
        {
            set { fontName = value; }
            get { return fontName; }
        }
        public int FontSize
        {
            set { fontSize = value; }
            get { return fontSize; }
        }
        public bool PlotEnable
        {
            set { plotEnable = value; }
            get { return plotEnable; }
        }
        public bool NormalizeLRVolume
        {
            set { normalizeLRVolume = value; }
            get { return normalizeLRVolume; }
        }

        private Settings()
        {

        }

        static public Settings GetInstance()
        {
            return instance;
        }

        public void SaveSettings()
        {
            StreamWriter sw = null;
            try
            {
                string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;
                string SettingPath = AppPath + fileName;

                sw = new StreamWriter(SettingPath, false, Encoding.Default);
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(sw, this);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }

        public void LoadSettings()
        {       
            StreamReader sr = null;
            try
            {
                // 環境設定ファイルのPATH設定
                string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;
                string SettingPath = AppPath + fileName;

                if (File.Exists(SettingPath))
                {
                    // 環境設定データ読込
                    sr = new StreamReader(SettingPath, Encoding.Default);
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    instance = (Settings)(serializer.Deserialize(sr));
                }
                else
                {
                    // 環境設定ファイルが存在しない時は作成
                    SaveSettings();
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
            finally
            {
                if (sr != null) sr.Close();
            }
        }
    }
}
