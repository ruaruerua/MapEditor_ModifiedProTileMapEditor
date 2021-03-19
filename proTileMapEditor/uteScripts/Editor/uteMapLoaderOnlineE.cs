using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(uteMapLoader))]
public class uteMapLoaderOnlineE : Editor {

	private string[] mapListOptions;
	private string[] mapOptimizationLevel;
	
	public int selectedItemIndex;
	private int lastSelectedIndex;
	public int optIndex;
	private int lastOptIndex;

	private void Awake()
	{
		lastSelectedIndex = -1;
		lastOptIndex = -1;

		mapOptimizationLevel = new string[3];
		mapOptimizationLevel[0] = "low";
		mapOptimizationLevel[1] = "medium";
		mapOptimizationLevel[2] = "high";
		ReadMaps();
	}

	private void ReadMaps()
	{
		StreamReader sr = new StreamReader(AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.uteMyMapstxt));
		string allMaps = sr.ReadToEnd();
		sr.Close();
		string[] mapItems = allMaps.Split(":"[0]);
		mapListOptions = new string[mapItems.Length-1];

		for(int i=0;i<mapItems.Length-1;i++)
		{
			mapListOptions[i] = mapItems[i];
		}
	}

	public override void OnInspectorGUI ()
    {
    	GUILayout.Label("Loading in RUNTIME:");
    	base.OnInspectorGUI();

 		uteMapLoader myTarget = (uteMapLoader) target;

 		if(myTarget.currentMapIndex!=lastSelectedIndex)
 		{
 			lastSelectedIndex = myTarget.currentMapIndex;
    		myTarget.SetMap(mapListOptions[myTarget.currentMapIndex]);
    	}

        EditorGUILayout.BeginHorizontal();
        myTarget.currentMapIndex = EditorGUILayout.Popup("Map to Load: ",myTarget.currentMapIndex, mapListOptions, EditorStyles.popup);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        myTarget.currentOptimizationLevelIndex = EditorGUILayout.Popup("Optimization Level: ",myTarget.currentOptimizationLevelIndex, mapOptimizationLevel, EditorStyles.popup);
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("-------------");

        GUILayout.Label("Loading in EDITOR:");
        if(GUILayout.Button("Load Map In Editor Scene Now"))
        {
        	myTarget.LoadMap();
        }

        if(GUILayout.Button("Load Map Bounds"))
        {
        	myTarget.LoadBounds();
        }
    }
}