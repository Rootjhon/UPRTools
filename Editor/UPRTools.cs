using System.IO;
using UnityEngine;
using UnityEditor;
using UPRLuaProfiler;

namespace UPRProfiler
{
    public sealed class UPRTools : EditorWindow
    {
        #region [Feilds]
        private static bool deplicated = false;
        private static bool uprLuaProfiler = false;

        private static UPRToolSetting _UPRSetting;

        private string packageVersion = string.Format("Current Version: {0}", InnerPackageS.packageVersion);
        #endregion

        [MenuItem("Tools/UPRTools/Setting")]
        static void showWindow()
        {
            var tempWindow = EditorWindow.GetWindow<UPRTools>();
            tempWindow.titleContent = new GUIContent("UPRTools");
            tempWindow.minSize = new Vector2(400, 500);
        }

        private void OnEnable()
        {
            _UPRSetting = UPRToolSetting.Instance;
        }
        private void OnProjectChange()
        {
            _UPRSetting = UPRToolSetting.Instance;
        }

        private void OnGUI()
        {
            UPRGUIUtil.GUI_Title("UPRTools", packageVersion);

            using (new EditorGUI.DisabledScope(EditorApplication.isCompiling))
            {
                bool tempUPRdirty = false;
                GUI_AssetProfiler(ref tempUPRdirty);

                GUILayout.Space(10);
                GUI_LuaProfiler(ref tempUPRdirty);

                //save dirtys;
                if (tempUPRdirty)
                {
                    _UPRSetting.Save();
                }
            }

            UPRGUIUtil.GUI_Bottom(this);
        }

        private void GUI_AssetProfiler(ref bool varUprSetDirty)
        {
            GUILayout.Label("Deep Function Profiler");
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
            {
                var tempSceneVal = GUILayout.Toggle(_UPRSetting.loadScene, "LoadScene");
                varUprSetDirty = _UPRSetting.loadScene != tempSceneVal;
                _UPRSetting.loadScene = tempSceneVal;

                var templAstsVal = GUILayout.Toggle(_UPRSetting.loadAsset, "LoadAsset");
                varUprSetDirty = _UPRSetting.loadAsset != templAstsVal;
                _UPRSetting.loadAsset = templAstsVal;

                var templABVal = GUILayout.Toggle(_UPRSetting.loadAssetBundle, "LoadAssetBundle");
                varUprSetDirty = _UPRSetting.loadAssetBundle != templABVal;
                _UPRSetting.loadAssetBundle = templABVal;

                var tempInsVal = GUILayout.Toggle(_UPRSetting.instantiate, "Instantiate");
                varUprSetDirty = _UPRSetting.instantiate != tempInsVal;
                _UPRSetting.instantiate = tempInsVal;
            }
        }

        private void GUI_LuaProfiler(ref bool varUprSetDirty)
        {
            GUILayout.Label("Deep Lua Profiler");
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var tempMonoEnableVal = GUILayout.Toggle(_UPRSetting.enableMonoProfiler, "Enable Deep Mono");
                if (tempMonoEnableVal != _UPRSetting.enableMonoProfiler)
                {
                    var tempTip = string.Format("This action will {0} \"MIKU_RECOMPILE\" Macro compiler project code.",
                        tempMonoEnableVal ? "append" : "remove");

                    if (EditorUtility.DisplayDialog("Confirm", tempTip, "Just Do it!"))
                    {
                        _UPRSetting.enableMonoProfiler = tempMonoEnableVal;

                        varUprSetDirty = true;
                        _UPRSetting.enableMonoProfiler = tempMonoEnableVal;
                        _UPRSetting.Save();

                        InjectMethods.Recompile(_UPRSetting.enableMonoProfiler);
                    }
                }
                if (tempMonoEnableVal)
                {
                    var tempTip = "Package will inject the necessary code to collect Mono information.\nThis will cause some performance loss.\nThis function doesn't support IOS devices currently.";
                    EditorGUILayout.HelpBox(tempTip, MessageType.Warning);
                }

                var tempLuaEnbaleVal = GUILayout.Toggle(_UPRSetting.enableLuaProfiler, "Enable Lua");
                if (tempLuaEnbaleVal != _UPRSetting.enableLuaProfiler)
                {
                    varUprSetDirty = true;
                    _UPRSetting.enableLuaProfiler = tempLuaEnbaleVal;
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
            }
        }
    }
}
