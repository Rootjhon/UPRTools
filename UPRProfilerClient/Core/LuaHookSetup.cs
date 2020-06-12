/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________
                我们的未来没有BUG                
* ==============================================================================
* Filename: LuaHookSetup
* Created:  2018/7/2 11:36:16
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR || UPR_LUA_PROFILER
using System;
using UnityEngine;
using System.Collections.Generic;

namespace UPRLuaProfiler
{
    public sealed class HookLuaSetup : MonoBehaviour
    {
        #region [Fields]
        public static float fps { private set; get; }
        public static int frameCount { private set; get; }
        public static int pss { private set; get; }
        public static float power { private set; get; }

        public static LuaDeepProfilerSetting setting { private set; get; }

        public float showTime = 1f;
        private int count = 0;
        private float deltaTime = 0f;

        public const float DELTA_TIME = 0.1f;
        public float currentTime = 0;
        private static bool isInite = false;
        private static Queue<Action> actionQueue = new Queue<Action>();
        public static void RegisterAction(Action a)
        {
            lock (actionQueue)
            {
                actionQueue.Enqueue(a);
            }
        }
        #endregion

#if UNITY_5 || UNITY_2017_1_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        public static void OnStartGame()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (isInite) return;
            LuaDeepProfilerSetting.MakeInstance();
            isInite = true;
            setting = LuaDeepProfilerSetting.Instance;
            LuaProfiler.mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (setting.isNeedCapture)
            {
                Screen.SetResolution(480, 270, true);
            }

#if UPR_LUA_PROFILER
#if DEPLICATED
            LuaDLL.Install();
#endif

            GameObject go = new GameObject();
            go.name = "UPRLuaProfiler";
            go.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(go);
            go.AddComponent<HookLuaSetup>();
            NetWorkClient.ConnectServer(setting.ip, setting.port);
#endif
        }

        private void Awake()
        {
            setting = LuaDeepProfilerSetting.Instance;
        }

        private void LateUpdate()
        {
            if (actionQueue.Count > 0)
            {
                lock (actionQueue)
                {
                    while (actionQueue.Count > 0)
                    {
                        actionQueue.Dequeue()();
                    }
                }
            }
            frameCount = Time.frameCount;
            count++;
            deltaTime += Time.unscaledDeltaTime;
            if (deltaTime >= showTime)
            {
                fps = count / deltaTime;
                count = 0;
                deltaTime = 0f;
            }
            if (Time.unscaledTime - currentTime > DELTA_TIME)
            {
                pss = NativeHelper.GetPass();
                power = NativeHelper.GetBatteryLevel();
                currentTime = Time.unscaledTime;
            }

        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            desotryCount = 0;
            Destroy(gameObject);
            UnityEditor.EditorApplication.update += WaitDestory;
#else
            NetWorkClient.Close();
#endif
        }

#if UNITY_EDITOR
        int desotryCount = 0;
        private void WaitDestory()
        {
            desotryCount++;
            if (desotryCount > 10)
            {
                UnityEditor.EditorApplication.update -= WaitDestory;
                if (LuaProfiler.mainL != IntPtr.Zero)
                {
                    LuaDLL.lua_close(LuaProfiler.mainL);
                }
                LuaProfiler.mainL = IntPtr.Zero;
                NetWorkClient.Close();
                desotryCount = 0;
            }
        }
#endif
    }
}
#endif