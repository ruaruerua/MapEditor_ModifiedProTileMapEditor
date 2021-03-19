using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class uteGLOBAL3dMapEditor : MonoBehaviour {

	public static bool isEditorRunning = false;
	public static bool canBuild = false;
	public static int mapObjectCount = 0;
	public static bool XZsnapping = true;
	public static bool OverlapDetection = true;
	public static bool CalculateXZPivot = false;
	public static int UndoSession = 0;
	public static string runtimeMapListPath = "/uteMaps/uteMapListForRuntime.txt";
	public static string LoadPrefabResourcePath = "Assets/LoadableResources/Grids/";
	public static string LuaMapConfigPath = Application.dataPath + "/Lua/GameCore/Config/MapConfigs/";
#if UNITY_EDITOR
	public static string uteCategoryInfotxt = "1f97d4f7dd2d64acf8e5c832a15ba53a";
	public static string uteMyMapstxt = "9e9936f47d5da4b88ad35169fb0d9982";
	public static string uteMyPatternstxt = "14541d53b60ba4c7cbf8e194d319d46a";
	public static string uteSettingstxt = "bf29964db71db4b81b9d25b4dc99d63a";
	public static string uteTileConnectionstxt = "c3b36fa978c5f48029aa1c4811f7ffa4";
	public static string uteGUIDIDtxt = "5336387937cc749ae901d4b0701b8aee";
	public static string utePrievstxt = "20cb4a38eaa4f44979afae4e9ab23e91";
	public static string uteHintsOpttxt = "4249c19e6737d488b95d4893615240e3";
	public static string uteHintstxt = "c4a7f364a43994c7d9fde91a1fa77f31";

	public static string getHintsPath()
	{
		string dir = AssetDatabase.GUIDToAssetPath(uteHintstxt);
		return dir;
	}

	public static string getHintsOptPath()
	{
		string dir = AssetDatabase.GUIDToAssetPath(uteHintsOpttxt);
		return dir;
	}

	public static string getMapsDir()
	{
		//string dir = AssetDatabase.GUIDToAssetPath("8af9fb7782c66401d9e3c5ebcc151cee");
		string dir = Application.dataPath + "/proTileMapEditor/uteMaps/";
		dir = dir.Replace("utemapsdirtagdonotdelete.txt", "");
		return dir;
	}

	public static string getPatternsDir()
	{
		string dir = AssetDatabase.GUIDToAssetPath("e594b2557db7549f8aedcf80e01c62ed");
		dir = dir.Replace("utepatternsdirtagdonotdelete.txt","");
		return dir;
	}

	public static string getMyPatternsDir()
	{
		string dir = Application.dataPath + "/LoadableResources/Grids/";
		//string dir = AssetDatabase.GUIDToAssetPath("14c6fcc5cd6c34faeb9053aba565446d");
		dir = dir.Replace("utemypatternsdirtagdonotdelete.txt","");
		return dir;
	}
#endif

	public static string getRuntimeMapListPath()
	{
		return Application.streamingAssetsPath+runtimeMapListPath;
	}

	public static string getRuntimeMapDir()
	{
		return Application.streamingAssetsPath+"/uteMaps/";
	}
}
