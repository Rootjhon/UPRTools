using System.IO;
using UnityEngine;
using UPRLuaProfiler;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

namespace UPRProfiler
{
    public sealed class UPRToolSetting
    {
        #region [Fields]
        public const string ResourcesDir = "Assets/UPRTools/Resources";
        public const string UPRSettingFile = "UPRToolSettings.bytes";

        private bool m_loadScene = false;
        private bool m_loadAsset = false;
        private bool m_loadAssetBundle = false;
        private bool m_instantiate = false;

        private bool m_enableLuaProfiler = false;

        public bool loadScene { get { return m_loadScene; } set { m_loadScene = value; } }
        public bool loadAsset { get { return m_loadAsset; } set { m_loadAsset = value; } }
        public bool loadAssetBundle { get { return m_loadAssetBundle; } set { m_loadAssetBundle = value; } }
        public bool instantiate { get { return m_instantiate; } set { m_instantiate = value; } }

        public bool enableLuaProfiler
        {
            get
            {
                return LuaDeepProfilerSetting.Instance.isDeepLuaProfiler;
            }
            set
            {
                LuaDeepProfilerSetting.Instance.isDeepLuaProfiler = value;
            }
        }
        public bool enableMonoProfiler
        {
            get
            {
                return LuaDeepProfilerSetting.Instance.isDeepMonoProfiler;
            }
            set
            {
                LuaDeepProfilerSetting.Instance.isDeepMonoProfiler = value;
            }
        }


        private static UPRToolSetting instance;
        public static UPRToolSetting Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UPRToolSetting().Load();
                }
                return instance;
            }
        }
        #endregion

        #region [API]
        [Conditional("UNITY_EDITOR")]
        public void Save()
        {
            string tempFilePath = Path.Combine(ResourcesDir, UPRSettingFile);
            if (!Directory.Exists(ResourcesDir))
            {
                Directory.CreateDirectory(ResourcesDir);
            }

            using (var output = new FileStream(tempFilePath, FileMode.Create))
            {
                using (var binaryWriter = new BinaryWriter(output))
                {
                    binaryWriter.Write(this.m_loadScene);
                    binaryWriter.Write(this.m_loadAsset);
                    binaryWriter.Write(this.m_loadAssetBundle);
                    binaryWriter.Write(this.m_instantiate);
                }
            }
        }

        public UPRToolSetting Load()
        {
            UPRToolSetting uprToolSetting = new UPRToolSetting();

            string tempFilePath = string.Empty;
            TextAsset tempAsset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(UPRSettingFile));
            var datas = tempAsset?.bytes;

            if (datas == null)
            {
                Debug.LogWarningFormat("[UPRToolSetting] can't load {0} at {1},just creat it.", UPRSettingFile, tempFilePath);
                Save();
                return uprToolSetting;
            }

            using (var memoryStream = new MemoryStream(datas))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    uprToolSetting.m_loadScene = binaryReader.ReadBoolean();
                    uprToolSetting.m_loadAsset = binaryReader.ReadBoolean();
                    uprToolSetting.m_loadAssetBundle = binaryReader.ReadBoolean();
                    uprToolSetting.m_instantiate = binaryReader.ReadBoolean();
                }
            }

            return uprToolSetting;
        }
        #endregion
    }
}