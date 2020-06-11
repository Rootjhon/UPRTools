/**
 * @file            
 * @author          
 * @copyright       
 * @created         2020-06-11 15:12:24
 * @updated         2020-06-11 15:12:24
 *
 * @brief           
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class UPRGUIUtil
{
    public static void GUI_Title(string varTitle,string varVersion)
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
}