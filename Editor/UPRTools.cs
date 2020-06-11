using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UPRLuaProfiler;
using System.Reflection;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;

namespace UPRProfiler
{
    public class UPRTools : EditorWindow
    {
        #region [Feilds]
        private static bool deplicated = false;
        private static bool uprLuaProfiler = false;

        private static UPRToolSetting _UPRSetting;
        private static LuaDeepProfilerSetting _Luasetting;

        private string packageVersion = string.Format("Current Version: {0}", InnerPackageS.packageVersion);
        #endregion
        
        [MenuItem("Tools/UPRTools/Setting")]
        static void showWindow()
        {
            var tempWindow = EditorWindow.GetWindow<UPRTools>();
            tempWindow.titleContent = new GUIContent("UPRTools");
        }

        private void OnEnable()
        {
            _UPRSetting = UPRToolSetting.Instance;
            _Luasetting = LuaDeepProfilerSetting.Instance;
        }
        private void OnProjectChange()
        {
            _UPRSetting = UPRToolSetting.Instance;
            _Luasetting = LuaDeepProfilerSetting.Instance;
        }

        private void OnGUI()
        {
            UPRGUIUtil.GUI_Title("UPRTools", packageVersion);

            doPackage();
        }

        void doPackage()
        {
            bool tempUPRdirty = false;
            #region [deep function profiler]
            GUILayout.Space(10);
            GUILayout.Label("Deep Function Profiler");
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var tempSceneVal = GUILayout.Toggle(_UPRSetting.loadScene, "LoadScene");
                    tempUPRdirty = _UPRSetting.loadScene != tempSceneVal;
                    _UPRSetting.loadScene = tempSceneVal;

                    var templAstsVal = GUILayout.Toggle(_UPRSetting.loadAsset, "LoadAsset");
                    tempUPRdirty = _UPRSetting.loadAsset != templAstsVal;
                    _UPRSetting.loadAsset = templAstsVal;

                    var templABVal = GUILayout.Toggle(_UPRSetting.loadAssetBundle, "LoadAssetBundle");
                    tempUPRdirty = _UPRSetting.loadAssetBundle != templABVal;
                    _UPRSetting.loadAssetBundle = templABVal;

                    var tempInsVal = GUILayout.Toggle(_UPRSetting.instantiate, "Instantiate");
                    tempUPRdirty = _UPRSetting.instantiate != tempInsVal;
                    _UPRSetting.instantiate = tempInsVal;
                }
            }
            #endregion

            #region [deep lua profiler]
            GUILayout.Space(10);
            GUILayout.Label("Deep Lua Profiler");
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var tempMonoEnableVal = GUILayout.Toggle(_UPRSetting.enableMonoProfiler, "Enable Deep Mono");
                if (tempMonoEnableVal != _UPRSetting.enableMonoProfiler)
                {
                    _UPRSetting.enableMonoProfiler = tempMonoEnableVal;
                    tempUPRdirty = true;

                    _Luasetting.isDeepMonoProfiler = _UPRSetting.enableMonoProfiler;
                    InjectMethods.Recompile(_UPRSetting.enableMonoProfiler);
                }
                if (tempMonoEnableVal)
                {
                    EditorGUILayout.HelpBox("Package will inject the necessary code to collect Mono information. This will cause some performance loss.", MessageType.Warning);
                    EditorGUILayout.HelpBox("This function doesn't support ios devices currently.", MessageType.Warning);
                }

                var tempLuaEnbaleVal = GUILayout.Toggle(_UPRSetting.enableLuaProfiler, "Enable Lua");
                if (tempLuaEnbaleVal != _UPRSetting.enableLuaProfiler)
                {
                    _UPRSetting.enableLuaProfiler = tempLuaEnbaleVal;
                    tempUPRdirty = true;

                    _Luasetting.isDeepLuaProfiler = _UPRSetting.enableLuaProfiler;
                }

                if (_UPRSetting.enableLuaProfiler)
                {
                    EditorGUILayout.HelpBox("Package will load some simple lua function to collect lua information. You can easy control whether to send data on UPR website.", MessageType.Warning);
                    if (!deplicated)
                    {
                        string[] dir =
                            Directory.GetDirectories(Application.dataPath, "*LuaProfiler*", SearchOption.TopDirectoryOnly);

                        if (dir.Length > 0)
                        {
                            deplicated = false;
                            EditorGUILayout.HelpBox("LuaProfiler is Find in the directory", MessageType.Warning);
                        }
                        else
                        {
                            deplicated = true;
                        }
                        //InjectMethods.addDeplicated(deplicated);
                    }
                }
                else
                {
                    if (deplicated)
                    {
                        deplicated = false;
                        //InjectMethods.Recompile("DEPLICATED", false);
                    }
                }

                if (!(uprLuaProfiler && (_UPRSetting.enableLuaProfiler || _UPRSetting.enableMonoProfiler)))
                {
                    uprLuaProfiler = !uprLuaProfiler;
                    //InjectMethods.Recompile("UPR_LUA_PROFILER", uprLuaProfiler);
                }

                //save dirtys;
                if (tempUPRdirty)
                {
                    _UPRSetting.Save();
                }
            }
            #endregion
        }
    }
#endif
}
