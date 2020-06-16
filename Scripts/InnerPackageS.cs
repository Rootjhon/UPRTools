using System;
using UnityEngine;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using UnityEngine.Profiling;

#if UNITY_2018_2_OR_NEWER
using Unity.Collections;
#endif

using Debug = UnityEngine.Debug;

namespace UPRProfiler
{
    public sealed class InnerPackageS : MonoBehaviour
    {
        #region [Fields]
        private static int pid = 0;
        private static int width;
        private static int height;
        private static byte[] rawBytes;

        public static int _Frequency = 4;
        public static int Frequency
        {
            get { return _Frequency; }
            set
            {
                if (_Frequency != value)
                {
                    _Frequency = value;
                    waitSeconds = new WaitForSeconds(value);
                }
            }
        }

        private static WaitForSeconds waitSeconds = new WaitForSeconds(Frequency);
        public static string packageVersion = "0.6.2";
        private static string cpuString = string.Empty;

        public static int screenCnt = 0;
        public static int memoryCnt = 0;

#if UNITY_2018_2_OR_NEWER
        private static NativeArray<byte> nativeRawBytes;
#endif
        #endregion

        #region [MonoBehaviour]
        private void Start()
        {
            height = Screen.height;
            width = Screen.width;

#if !UNITY_2018_2_OR_NEWER
            rawBytes = new byte[0];
#endif
            StartCoroutine(GetScreenShot());

#if UNITY_ANDROID && !UNITY_EDITOR
            //仅在真机上调用;
            DetectSystemInfo();
#endif
        }
        private void OnApplicationQuit()
        {
            NetworkServer.Close();
        }
        #endregion

        #region [API]
        public static void GetSystemDevice()
        {
            StringBuilder deviceInfo = new StringBuilder();
            deviceInfo.AppendFormat("{0}:{1}", "systemBrand", SystemInfo.deviceModel);
            deviceInfo.AppendFormat("& {0}:{1}MB", "systemTotalRam", SystemInfo.systemMemorySize.ToString());
            deviceInfo.AppendFormat("& {0}:{1}MHZ", "systemMaxCpuFreq", SystemInfo.processorFrequency);
            deviceInfo.AppendFormat("& {0}:{1}", "packageVersion", packageVersion);

            deviceInfo.AppendFormat("& {0}:{1}", "operatingSystem", SystemInfo.operatingSystem);
            deviceInfo.AppendFormat("& {0}:{1}", "graphicsDeviceID", SystemInfo.graphicsDeviceID);
            deviceInfo.AppendFormat("& {0}:{1}", "graphicsDeviceName", SystemInfo.graphicsDeviceName);
            deviceInfo.AppendFormat("& {0}:{1}", "graphicsDeviceType", SystemInfo.graphicsDeviceType);
            deviceInfo.AppendFormat("& {0}:{1}", "graphicsDeviceVendor", SystemInfo.graphicsDeviceVendor);
            deviceInfo.AppendFormat("& {0}:{1}", "graphicsDeviceVendorID", SystemInfo.graphicsDeviceVendorID);
            deviceInfo.AppendFormat("& {0}:{1}", "graphicsDeviceVersion", SystemInfo.graphicsDeviceVersion);
            deviceInfo.AppendFormat("& {0}:{1}", "graphicsMemorySize", SystemInfo.graphicsMemorySize);
            deviceInfo.AppendFormat("& {0}:{1}", "graphicsMultiThreaded", SystemInfo.graphicsMultiThreaded);
            deviceInfo.AppendFormat("& {0}:{1}", "graphicsShaderLevel", SystemInfo.graphicsShaderLevel);
            deviceInfo.AppendFormat("& {0}:{1}", "maxTextureSize", SystemInfo.maxTextureSize);
            deviceInfo.AppendFormat("& {0}:{1}", "npotSupport", SystemInfo.npotSupport);
            deviceInfo.AppendFormat("& {0}:{1}", "cpuCores", SystemInfo.processorCount);
            deviceInfo.AppendFormat("& {0}:{1}*{2}", "resolution", Screen.width, Screen.height);
            deviceInfo.AppendFormat("& {0}:{1}", "processorType", SystemInfo.processorType);
            NetworkServer.SendMessage(Encoding.ASCII.GetBytes(deviceInfo.ToString()), 2, width, height);
        }
        #endregion

        #region [Business]
        private IEnumerator GetScreenShot()
        {
            WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
            Texture2D shot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            Rect area = new Rect(0, 0, Screen.width, Screen.height);
            screenCnt = 0;
            while (true)
            {
                yield return waitForEndOfFrame;
                Profiler.BeginSample("Profiler.ScreenShotCoroutine");
                if (NetworkServer.isConnected && NetworkServer.enableScreenShot && !NetworkServer.screenFlag)
                {
                    if (width != Screen.width)
                    {
                        shot.Resize(Screen.width, Screen.height);
                        width = Screen.width;
                        height = Screen.height;
                        area = new Rect(0, 0, Screen.width, Screen.height);
                        yield return waitForEndOfFrame;
                    }

                    try
                    {
                        shot.ReadPixels(area, 0, 0, false);
                        shot.Apply();
#if UNITY_2018_2_OR_NEWER
                        nativeRawBytes = shot.GetRawTextureData<byte>();
                        NetworkServer.SendMessage(nativeRawBytes, 0, width, height);
#else
                        rawBytes = shot.GetRawTextureData();
                        NetworkServer.SendMessage(rawBytes, 0, width, height);
#endif
                        NetworkServer.screenFlag = true;
                        screenCnt++;
                    }
                    catch (Exception e)
                    {
                        NetworkServer.screenFlag = false;
                        Debug.LogErrorFormat("[PACKAGE] Screenshot Error {0}\n{1}", e.Message, e.StackTrace);
                    }
                }
                Profiler.EndSample();
                if (NetworkServer.isConnected && !NetworkServer.sendDeviceInfo)
                {
                    try
                    {
                        GetSystemDevice();
                        NetworkServer.sendDeviceInfo = true;
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("[PACKAGE] Send SystemDevice Error {0}\n{1}", e.Message, e.StackTrace);
                    }
                }

                yield return waitSeconds;
            }
        }

        #region [Android]
#if UNITY_ANDROID

        #region [Fields]
        private static AndroidJavaObject _MemoryInfo;
        private static AndroidJavaObject _Intent;


        private static Process cprocess;
        private static int cpuCores = 0;

        private static object[] tempeartureModel = new object[] { "temperature", 0 };
        private static string[] funcName = new string[]
        {
            "getTotalPss",
            "getTotalSwappablePss",
            "getTotalPrivateDirty",
            "getTotalSharedDirty",
            "getTotalPrivateClean",
            "getTotalSharedClean"
        };
        #endregion

        private void DetectSystemInfo()
        {
            using (var UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (var process = new AndroidJavaClass("android.os.Process"))
                    {
                        using (var intentFilter = new AndroidJavaObject("android.content.IntentFilter", "android.intent.action.BATTERY_CHANGED"))
                        {
                            _Intent = currentActivity.Call<AndroidJavaObject>("registerReceiver", new object[] { null, intentFilter });
                            pid = process.CallStatic<int>("myPid");
                            if (pid <= 0)
                            {
                                Debug.LogError("Get Device Pid Error");
                                return;
                            }

                            using (var memoryManager = currentActivity.Call<AndroidJavaObject>("getSystemService",
                                new AndroidJavaObject("java.lang.String", "activity")))
                            {
                                _MemoryInfo = memoryManager.Call<AndroidJavaObject[]>("getProcessMemoryInfo", new int[] { pid })[0];
                                cpuCores = SystemInfo.processorCount;
                                cprocess = new Process();
                                StartCoroutine(GetSystemMemory());
                                new Thread(new ThreadStart(CpuThread)).Start();
                            }
                        }
                    }
                }
            }
        }
        private IEnumerator GetSystemMemory()
        {
            memoryCnt = 0;
            while (true)
            {
                Profiler.BeginSample("Profiler.GetMemoryInfo");
                if (NetworkServer.isConnected)
                {
                    try
                    {
                        GetPSS();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("[PACKAGE] Send PSS Error {0}\n{1}", ex.Message, ex.StackTrace);
                        yield break;
                    }
                    memoryCnt++;
                }
                Profiler.EndSample();
                yield return waitSeconds;
            }
        }
        private void CpuThread()
        {
            cprocess.StartInfo.FileName = "sh";
            cprocess.StartInfo.Arguments = string.Format("-c top | grep ", pid);
            cprocess.StartInfo.UseShellExecute = false;
            cprocess.StartInfo.RedirectStandardOutput = true;
            cprocess.StartInfo.RedirectStandardError = true;
            while (true)
            {
                if (NetworkServer.isConnected)
                {
                    cpuString = GetCpuUsage(pid);
                }
                Thread.Sleep(3000);
            }
        }
        private static string GetCpuUsage(int pid)
        {
            string result = string.Empty;

            try
            {
                cprocess.Start();
                string strOutput = "S";
                for (int i = 0; i < 6; i++)
                    strOutput = cprocess.StandardOutput.ReadLine();
                strOutput = strOutput.Split('S')[1];
                int index = strOutput.IndexOf("   ", StringComparison.Ordinal);
                float cpuUsage = float.Parse(strOutput.Substring(1, index));
                result = (cpuUsage / cpuCores).ToString();
                cprocess.WaitForExit();
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Package GetCPUUsage Error {0}\n{1}", e.Message, e.StackTrace);
            }
            return result;
        }
        public static void GetPSS()
        {
            StringBuilder meminfo = new StringBuilder();
            meminfo.Append("{");
            for (int i = 0; i < funcName.Length; i++)
            {
                if (i > 0)
                    meminfo.Append(",");
                meminfo.AppendFormat("\"{0}\":\"{1}\"", funcName[i], _MemoryInfo.Call<int>(funcName[i]).ToString());
            }
            meminfo.AppendFormat(", \"{0}\":\"{1}\"", "battery", SystemInfo.batteryLevel);
            meminfo.AppendFormat(", \"{0}\":\"{1}\"", "cpuTemp", _Intent.Call<int>("getIntExtra", tempeartureModel).ToString());
            meminfo.AppendFormat(", \"{0}\":\"{1}\"", "cpuUsage", cpuString);
            meminfo.Append("}");
            NetworkServer.SendMessage(Encoding.ASCII.GetBytes(meminfo.ToString()), 1, width, height);
        }
#endif
        #endregion

        #endregion
    }
}
