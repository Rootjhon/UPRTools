/**
 * @file            
 * @author          
 * @copyright       
 * @created         2020-06-11 15:12:24
 * @updated         2020-06-11 15:12:24
 *
 * @brief           
 */

using UnityEngine;
using UnityEditor;

namespace UPRProfiler
{
    public static class UPRGUIUtil
    {
        public static void GUI_Title(string varTitle, string varVersion)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                // draw the title
                GUILayout.Space(10);
                GUI.skin.label.fontSize = 24;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(varTitle);

                // draw the version
                GUI.skin.label.fontSize = 12;
                GUI.skin.label.alignment = TextAnchor.LowerCenter;
                GUILayout.Label(varVersion);

                //draw the text
                GUILayout.Space(10);
                GUI.skin.label.fontSize = 12;
                GUI.skin.label.alignment = TextAnchor.UpperLeft;
            }
        }

        public static void GUI_Bottom(EditorWindow varWindow)
        {
            GUIStyle _creditsStyle = new GUIStyle();
            _creditsStyle.fontStyle = FontStyle.Italic;
            _creditsStyle.alignment = TextAnchor.MiddleCenter;
            _creditsStyle.normal.textColor = new Color(0, 0, 0, 0.5f);
            GUI.Label(new Rect(15, varWindow.position.height - 22, varWindow.position.width, 22), "Product by JunQiang", _creditsStyle);
        }
    }
}