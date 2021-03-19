using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(uteRuntimeBuilder))]
public class uteRuntimePrepare : Editor {

	public override void OnInspectorGUI ()
    {
    	base.OnInspectorGUI();

 		uteRuntimeBuilder myTarget = (uteRuntimeBuilder) target;

 		if(GUILayout.Button("Prepare for Runtime"))
 		{
            if(!File.Exists(Application.streamingAssetsPath ))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            if(!File.Exists(Application.streamingAssetsPath+"/uteMaps/"))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath+"/uteMaps/");
            }

            if(!File.Exists(Application.streamingAssetsPath+"/uteMaps/uteMapList___R.txt"))
            {
                StreamWriter sw = new StreamWriter(Application.streamingAssetsPath+"/uteMaps/uteMapListForRuntime.txt");
                sw.Write("");
                sw.Close();
            }

    		myTarget.SetTiles();
    	}

        /*EditorGUILayout.BeginHorizontal();
        myTarget.currentMapIndex = EditorGUILayout.Popup("Map to Load: ",myTarget.currentMapIndex, mapListOptions, EditorStyles.popup);
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("-------------");

        GUILayout.Label("Loading in EDITOR:");
        if(GUILayout.Button("Load Map In Editor Scene Now"))
        {
        	myTarget.LoadMap();
        }*/
    }
}