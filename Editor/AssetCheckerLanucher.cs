/**
 * @file            AssetCheckerLanucher.cs
 * @author          JunQiang(354888562@qq.com)
 * @copyright       
 * @created         2020-06-11 14:39:15
 * @updated         2020-06-11 14:39:15
 *
 * @brief           
 */

using System.IO;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace UPRProfiler
{
    public sealed class AssetCheckerLanucher : EditorWindow
    {
        #region [Menu]
        [MenuItem("Tools/UPRTools/AssetChecker")]
        public static void OpenLanucher()
        {
            var tempWindow = EditorWindow.GetWindow<AssetCheckerLanucher>();
            tempWindow.titleContent = new GUIContent("AssetCheckerLanucher");
            tempWindow.minSize = new Vector2(400, 500);
        }
        #endregion

        #region [Fields]
        public const string AC_UPR_Lanucher_EXEKey = "JQ_AssetCheckerLanucher_Excuter";
        public const string AC_UPR_Project_IDKey = "JQ_AC_UPR_Project_IDKey";

        private readonly string __Version = "1.0.0";

        private string _UnityPrjPath;

        private string _AssetCheckExcuter;
        private string AssetCheckExcuter
        {
            get { return _AssetCheckExcuter; }
            set
            {
                _AssetCheckExcuter = Path.GetFullPath(value);
                if (!string.IsNullOrEmpty(_AssetCheckExcuter))
                {
                    EditorPrefs.SetString(AC_UPR_Lanucher_EXEKey, _AssetCheckExcuter);
                }
            }
        }
        
        private string _ProjectID;
        private string ProjectID
        {
            get { return _ProjectID; }
            set
            {
                _ProjectID = value;
                EditorPrefs.SetString(AC_UPR_Project_IDKey, _ProjectID);
            }
        }
        #endregion

        #region [GUI]
        private void OnEnable() { GUI_InitCacheData(); }
        private void OnProjectChange() { GUI_InitCacheData(); }
        private void OnGUI()
        {
            UPRGUIUtil.GUI_Title("AssetCheckerLanucher", __Version);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("AssetChecker:", AssetCheckExcuter);
                if (GUILayout.Button("Select", GUILayout.MaxWidth(100f)))
                {
                    var tempExeVal = EditorUtility.OpenFilePanel("Select Where the AssetChecker is", Application.dataPath, "exe");
                    AssetCheckExcuter = tempExeVal;
                    this.Repaint();
                    this.Focus();
                }
            }
            if (string.IsNullOrEmpty(AssetCheckExcuter))
            {
                EditorGUILayout.HelpBox("AssetChecker not found.", MessageType.Error);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("UnityProjectPath:", _UnityPrjPath);
                if (GUILayout.Button("Select", GUILayout.MaxWidth(100f)))
                {
                    var tempPrjVal = EditorUtility.OpenFolderPanel("Select Where the Unity Project is", Path.Combine(Application.dataPath, "../"), null);
                    if (!string.IsNullOrEmpty(tempPrjVal))
                    {
                        _UnityPrjPath = tempPrjVal;
                    }
                }
            }

            var tempIDVal = EditorGUILayout.TextField("ProjectID:", ProjectID);
            if (tempIDVal != ProjectID)
            {
                ProjectID = tempIDVal;
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space();

                if (GUILayout.Button("Check Project Settings", GUILayout.MaxWidth(150f), GUILayout.MaxHeight(50f)))
                {
                    var args = string.IsNullOrEmpty(ProjectID) ?
                        string.Format("--project={0} --projectId={1}", _UnityPrjPath, ProjectID) :
                        string.Format("--project={0}", _UnityPrjPath);
                    var tempIns = Process.Start(AssetCheckExcuter, args);
                }

                if (GUILayout.Button("Check Assetbundle", GUILayout.MaxWidth(150f), GUILayout.MaxHeight(50f)))
                {
                    var args = string.IsNullOrEmpty(ProjectID) ?
                        string.Format("abcheck --project={0} --projectId={1}", _UnityPrjPath, ProjectID) :
                        string.Format("abcheck --project={0}", _UnityPrjPath);
                    var tempIns = Process.Start(AssetCheckExcuter, args);
                }

                EditorGUILayout.Space();
            }

            UPRGUIUtil.GUI_Bottom(this);
        }

        private void GUI_InitCacheData()
        {
            _ProjectID = EditorPrefs.GetString(AC_UPR_Project_IDKey, string.Empty);
            AssetCheckExcuter = Path.GetFullPath(EditorPrefs.GetString(AC_UPR_Lanucher_EXEKey, string.Empty));
            _UnityPrjPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
        }
        #endregion
    }
}
