/**
 * @file            AssetCheckerLanucher.cs
 * @author          JunQiang(354888562@qq.com)
 * @copyright       
 * @created         2020-06-11 14:39:15
 * @updated         2020-06-11 14:39:15
 *
 * @brief           
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UPRProfiler
{
    public class AssetCheckerLanucher : EditorWindow
    {
        #region [Menu]
        [MenuItem("Tools/UPRTools/AssetChecker")]
        public static void OpenLanucher()
        {
            var tempWindow = EditorWindow.GetWindow<AssetCheckerLanucher>();
            tempWindow.titleContent = new GUIContent("AssetCheckerLanucher");
            tempWindow.minSize = new Vector2(400,500);
        }
        #endregion

        #region [Fields]
        public const string AssetCheckExcuterPrefKey = "JQ_AssetCheckerLanucher_Excuter";

        private readonly string __Version = "1.0.0";

        private string _AssetCheckExcuter;
        private string AssetCheckExcuter
        {
            get { return _AssetCheckExcuter; }
            set
            {
                _AssetCheckExcuter = value;
                if (string.IsNullOrEmpty(_AssetCheckExcuter))
                {
                    EditorPrefs.SetString(AssetCheckExcuterPrefKey, _AssetCheckExcuter);
                }
            }
        }
        private string _UnityPrjPath;
        #endregion

        #region [GUI]
        private void OnEnable() { GUI_InitCacheData(); }
        private void OnProjectChange() { GUI_InitCacheData(); }
        private void OnGUI()
        {
            GUI_Title();
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("AssetChecker:", AssetCheckExcuter);
                if (GUILayout.Button("Select"))
                {
                    var tempExeVal = EditorUtility.OpenFilePanel("Select Where the AssetChecker is", Application.dataPath, "exe");
                    AssetCheckExcuter = tempExeVal;
                }
            }
            if (string.IsNullOrEmpty(AssetCheckExcuter))
            {
                EditorGUILayout.HelpBox("AssetChecker not found.", MessageType.Error);
            }
        }
        private void GUI_Title()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                // draw the title
                GUILayout.Space(10);
                GUI.skin.label.fontSize = 24;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("AssetCheckerLanucher");

                // draw the version
                GUI.skin.label.fontSize = 12;
                GUI.skin.label.alignment = TextAnchor.LowerCenter;
                GUILayout.Label(__Version);

                //draw the text
                GUILayout.Space(10);
                GUI.skin.label.fontSize = 12;
                GUI.skin.label.alignment = TextAnchor.UpperLeft;
            }
        }
        private void GUI_InitCacheData()
        {
            AssetCheckExcuter = EditorPrefs.GetString(AssetCheckExcuterPrefKey, string.Empty);
        }
        #endregion
        

    }
}
