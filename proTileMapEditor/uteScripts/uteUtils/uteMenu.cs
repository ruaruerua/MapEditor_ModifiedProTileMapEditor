using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
using System.Reflection;
#endif

#if UNITY_EDITOR
#pragma warning disable 0219
using UnityEditor;
#endif

public class uteMenu : MonoBehaviour {
#if UNITY_EDITOR
	public bool RunInBackground = true;
	private bool isShowMyMaps;
	private bool isShowMyPatterns;
	private bool isShowMenu;
	private string newMapName;
	private string newPatternName;
	private string myMapsPath;
	private string myPatternsPath;
	private ArrayList myMaps = new ArrayList();
	private ArrayList myPatterns = new ArrayList();
	private Vector2 scrollPosMaps = Vector2.zero;
	private Vector2 scrollPosPatterns = Vector2.zero;
	private GUISkin ui;
	private bool isConfirmingDeleteMap;
	private bool isConfirmingDeletePattern;
	private bool isConfirmingRenameMap;
	private bool isConfirmingSaveAs;
	private string currentMapToDelete;
	private string currentPatternToDelete;
	private string currentMapToRename;
	private string currentMapToSaveAs;
	private string newMapNameForRename;
	private string newMapNameForSaveAs;

	#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
	public bool UseSkybox = true;
	private MethodInfo getBuiltinExtraResourcesMethod;
	#endif

	private void Awake()
	{
		uteGLOBAL3dMapEditor.isEditorRunning = true;
	}
	
	private void Start()
	{
		#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
		if(UseSkybox)
		{
			BindingFlags bfs = BindingFlags.NonPublic | BindingFlags.Static;
			getBuiltinExtraResourcesMethod = typeof( EditorGUIUtility ).GetMethod( "GetBuiltinExtraResource", bfs );
			UnityEngine.RenderSettings.skybox = (Material)getBuiltinExtraResourcesMethod.Invoke( null, new object[] { typeof( Material ), "Default-Skybox.mat" } );
		}
		#endif
		
		GameObject welcomeText = GameObject.Find("_____ute_____");

		if(welcomeText)
		{
			Destroy(welcomeText);
		}
		
		Application.runInBackground = RunInBackground;

		isConfirmingDeleteMap = false;
		isConfirmingDeletePattern = false;
		isConfirmingRenameMap = false;
		isConfirmingSaveAs = false;
		currentMapToDelete = "";
		currentPatternToDelete = "";
		newMapNameForRename = "";
		currentMapToRename = "";
		newMapNameForSaveAs = "";
		currentMapToSaveAs = "";
		isShowMenu = true;
		isShowMyPatterns = false;
		isShowMyMaps = true;
		newMapName = "myMap01";
		newPatternName = "myPattern01";
		ui = (GUISkin) Resources.Load("uteForEditor/uteUI");
		myMapsPath = AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.uteMyMapstxt);
		myPatternsPath = AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.uteMyPatternstxt);

		ReadAllMaps();
		ReadAllPatterns();
	}

	private void OnGUI()
	{
		if(!isShowMenu)
			return;

		GUI.skin = ui;

		GUI.Box(new Rect(40,50,200,200),"Menu");

		if(isConfirmingDeleteMap)
		{
			GUI.Box(new Rect(240,50,420,500),"My MAPS");
			GUI.Label(new Rect(250,80,310,44),"Do you really want to delete "+currentMapToDelete+" map?");

			if(GUI.Button(new Rect(260,120,80,40),"Yes"))
			{
				DeleteMap(currentMapToDelete,true);
				isConfirmingDeleteMap = false;
			}

			if(GUI.Button(new Rect(350,120,80,40),"No"))
			{
				isConfirmingDeleteMap = false;
			}

			return;
		}
		else if(isConfirmingDeletePattern)
		{
			GUI.Box(new Rect(240,50,320,500),"My PATTERNS");
			GUI.Label(new Rect(250,80,310,44),"Do you really want to delete "+currentMapToDelete+" pattern?");

			if(GUI.Button(new Rect(260,120,80,40),"Yes"))
			{
				DeleteMap(currentPatternToDelete,false);
				isConfirmingDeletePattern = false;
			}

			if(GUI.Button(new Rect(350,120,80,40),"No"))
			{
				isConfirmingDeletePattern = false;
			}

			return;
		}
		else if(isConfirmingRenameMap)
		{
			GUI.Box(new Rect(240,50,420,500),"My MAPS");
			GUI.Label(new Rect(260,80,310,44),"Your Map Name");

			newMapNameForRename = GUI.TextField(new Rect(300,115,150,30),newMapNameForRename);

			if(GUI.Button(new Rect(260,160,130,40),"Apply"))
			{
				RenameMap(currentMapToRename,newMapNameForRename);
				isConfirmingRenameMap = false;
			}

			if(GUI.Button(new Rect(400,160,130,40),"Cancel"))
			{
				isConfirmingRenameMap = false;
			}

			return;
		}
		else if(isConfirmingSaveAs)
		{
			GUI.Box(new Rect(240,50,420,500),"My MAPS");
			GUI.Label(new Rect(260,80,310,44),"Your Map Name of a Copy: "+currentMapToSaveAs);

			newMapNameForSaveAs = GUI.TextField(new Rect(300,115,150,30),newMapNameForSaveAs);

			if(GUI.Button(new Rect(260,160,130,40),"OK"))
			{
				SaveAsMap(currentMapToSaveAs,newMapNameForSaveAs);
				isConfirmingSaveAs = false;
			}

			if(GUI.Button(new Rect(400,160,130,40),"Cancel"))
			{
				isConfirmingSaveAs = false;
			}

			return;
		}

		if(GUI.Button(new Rect(50,90,180,40),"My MAPS"))
		{
			isShowMyMaps = true;
			isShowMyPatterns = false;
		}

		if(GUI.Button(new Rect(50,140,180,40),"My PATTERNS"))
		{
			isShowMyPatterns = true;
			isShowMyMaps = false;
		}

		if(isShowMyMaps)
		{
			GUI.Label(new Rect(30,Screen.height-90,Screen.width,90),"NOTE: If your Tile Previews are not loaded and it keeps saying 'LOADING', do the following:\n1. Stop map editor and open Tile-Editor (window->proTileMapEditor->Tile-Editor)\n2. Go through each tile category and scroll through all Tiles for Unity to cache Previews.\nThis only needs to be done once.");
		
			GUI.Box(new Rect(240,50,520,500),"My MAPS");
			newMapName = GUI.TextField(new Rect(250,75,150,30),newMapName);

			if(GUI.Button(new Rect(410,75,160,30),"Create new Map"))
			{
				if(CreateNewMap(newMapName))
				{
					InitMapEditorEngine(false,true,newMapName,false);
				}
			}

			if(myMaps.Count>0)
			{
				scrollPosMaps = GUI.BeginScrollView(new Rect(240, 120, 510, 420), scrollPosMaps, new Rect(240, 120, 390, myMaps.Count*35));

				for(int i=0;i<myMaps.Count;i++)
				{
					if(GUI.Button(new Rect(250,120+(i*35),180,30),myMaps[i].ToString()))
					{
						InitMapEditorEngine(false,true,myMaps[i].ToString(),true);
					}

					if(GUI.Button(new Rect(440,120+(i*35),90,30),"Delete"))
					{
						currentMapToDelete = myMaps[i].ToString();
						isConfirmingDeleteMap = true;
					}

					if(GUI.Button(new Rect(540,120+(i*35),90,30),"Rename"))
					{
						currentMapToRename = myMaps[i].ToString();
						newMapNameForRename = currentMapToRename;
						isConfirmingRenameMap = true;
					}

					if(GUI.Button(new Rect(640,120+(i*35),90,30),"Duplicate"))
					{
						currentMapToSaveAs = myMaps[i].ToString();
						newMapNameForSaveAs = currentMapToSaveAs+"_copy";
						isConfirmingSaveAs = true;
					}
				}

				GUI.EndScrollView();
			}
		}
		else if(isShowMyPatterns)
		{
			GUI.Box(new Rect(240,50,320,500),"My PATTERNS");
			newPatternName = GUI.TextField(new Rect(250,75,150,30),newPatternName);

			if(GUI.Button(new Rect(410,75,80,30),"Create"))
			{
				if(CreateNewPattern(newPatternName))
				{
					InitMapEditorEngine(true,false,newPatternName,false);
				}
			}

			if(myPatterns.Count>0)
			{
				scrollPosPatterns = GUI.BeginScrollView(new Rect(240, 120, 310, 420), scrollPosPatterns, new Rect(240, 120, 290, myPatterns.Count*35));
				
				for(int i=0;i<myPatterns.Count;i++)
				{
					if(GUI.Button(new Rect(250,120+(i*35),180,30),myPatterns[i].ToString()))
					{
						InitMapEditorEngine(true,false,myPatterns[i].ToString(),true);
					}

					if(GUI.Button(new Rect(440,120+(i*35),90,30),"Delete"))
					{
						currentPatternToDelete = myPatterns[i].ToString();
						isConfirmingDeletePattern = true;
					}
				}

				GUI.EndScrollView();
			}
		}
	}

	private void SaveAsMap(string old_name, string new_name)
	{
		new_name = FilterName(new_name);

		if(!CheckIfMapExists(new_name)&&!new_name.Equals(""))
		{
			string path = "";
			string mapPath;
			string mapInfoPath = "";
			string mapLayerPath = "";
			string mapPathMetaPath = "";
			string mapInfoMetaPath = "";
			string mapLayerMetaPath = "";
			string new_map_array_str = "";

			string mapInfoPath_2 = uteGLOBAL3dMapEditor.getMapsDir()+new_name+"_info.txt";
			string mapInfoMetaPath_2 = uteGLOBAL3dMapEditor.getMapsDir()+new_name+"_info.txt.meta";
			string mapPath_2 = uteGLOBAL3dMapEditor.getMapsDir()+new_name+".txt";
			string mapPathMetaPath_2 = uteGLOBAL3dMapEditor.getMapsDir()+new_name+".txt.meta";
			string mapLayerPath_2 = uteGLOBAL3dMapEditor.getMapsDir()+new_name+"_layers.txt";
			string mapLayerMetaPath_2 = uteGLOBAL3dMapEditor.getMapsDir()+new_name+"_layers.txt.meta";

			mapInfoPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+"_info.txt";
			mapInfoMetaPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+"_info.txt.meta";
			mapPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+".txt";
			mapPathMetaPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+".txt.meta";
			mapLayerPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+"_layers.txt";
			mapLayerMetaPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+"_layers.txt.meta";

			if(File.Exists(mapInfoPath))
			{
				File.Copy(mapInfoPath,mapInfoPath_2);
			}

			if(File.Exists(mapInfoMetaPath))
			{
				File.Copy(mapInfoMetaPath,mapInfoMetaPath_2);
			}

			if(File.Exists(mapPath))
			{
				File.Copy(mapPath,mapPath_2);
			}

			if(File.Exists(mapPathMetaPath))
			{
				File.Copy(mapPathMetaPath,mapPathMetaPath_2);
			}

			if(File.Exists(mapLayerPath))
			{
				File.Copy(mapLayerPath,mapLayerPath_2);
			}

			if(File.Exists(mapLayerMetaPath))
			{
				File.Copy(mapLayerMetaPath,mapLayerMetaPath_2);
			}

			if(File.Exists(myMapsPath))
			{
				StreamReader sr = new StreamReader(myMapsPath);
				string all_map_info = sr.ReadToEnd();
				sr.Close();

				all_map_info += new_name+":";

				StreamWriter sw = new StreamWriter(myMapsPath);
				sw.Write("");
				sw.Write(all_map_info);
				sw.Flush();
				sw.Close();
			}

			ReadAllMaps();
		}
	}

	private void RenameMap(string old_name, string new_name)
	{
		new_name = FilterName(new_name);

		if(!CheckIfMapExists(new_name)&&!new_name.Equals(""))
		{
			string path = "";
			string mapPath;
			string mapInfoPath = "";
			string mapLayerPath = "";
			string mapPathMetaPath = "";
			string mapInfoMetaPath = "";
			string mapLayerMetaPath = "";
			string new_map_array_str = "";

			mapInfoPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+"_info.txt";
			mapInfoMetaPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+"_info.txt.meta";
			mapPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+".txt";
			mapPathMetaPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+".txt.meta";
			mapLayerPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+"_layers.txt";
			mapLayerMetaPath = uteGLOBAL3dMapEditor.getMapsDir()+old_name+"_layers.txt.meta";

			for(int i=0;i<myMaps.Count;i++)
			{
				if(myMaps[i].Equals(old_name))
				{
					myMaps[i] = new_name;
					new_map_array_str += new_name+":";
				}
				else
				{
					new_map_array_str += myMaps[i]+":";
				}
			}

			if(File.Exists(myMapsPath))
			{
				StreamWriter sw = new StreamWriter(myMapsPath);
				sw.Write("");
				sw.Write(new_map_array_str);
				sw.Flush();
				sw.Close();
			}

			string new_map_settings_info = "";
			string new_map_info = "";
			string new_map_layer_info = "";

			bool mapInfoPath_exists = false;
			bool mapPath_exists = false;
			bool mapLayerPath_exists = false;

			if(File.Exists(mapInfoPath))
			{
				mapInfoPath_exists = true;
				StreamReader sr1 = new StreamReader(mapInfoPath);
				new_map_settings_info = sr1.ReadToEnd();
				sr1.Close();
			}

			if(File.Exists(mapPath))
			{
				mapPath_exists = true;
				StreamReader sr2 = new StreamReader(mapPath);
				new_map_info = sr2.ReadToEnd();
				sr2.Close();
			}

			if(File.Exists(mapLayerPath))
			{
				mapLayerPath_exists = true;
				StreamReader sr3 = new StreamReader(mapLayerPath);
				new_map_layer_info = sr3.ReadToEnd();
				sr3.Close();
			}

			if(mapInfoPath_exists)
			{
				StreamWriter sw1 = new StreamWriter(uteGLOBAL3dMapEditor.getMapsDir()+new_name+"_info.txt");
				sw1.Write(new_map_settings_info);
				sw1.Flush();
				sw1.Close();
			}

			if(mapPath_exists)
			{
				StreamWriter sw2 = new StreamWriter(uteGLOBAL3dMapEditor.getMapsDir()+new_name+".txt");
				sw2.Write(new_map_info);
				sw2.Flush();
				sw2.Close();
			}

			if(mapLayerPath_exists)
			{
				StreamWriter sw3 = new StreamWriter(uteGLOBAL3dMapEditor.getMapsDir()+new_name+"_layers.txt");
				sw3.Write(new_map_layer_info);
				sw3.Flush();
				sw3.Close();
			}

			DeleteMap(old_name,true);
		}
		else
		{
			Debug.Log("not ok");
		}
	}

	private void InitMapEditorEngine(bool _isItNewPattern, bool _isItNewMap, string name, bool isLoad)
	{
		uteMapEditorEngine uteMEE = this.gameObject.AddComponent<uteMapEditorEngine>();
		uteMEE.isItNewPattern = _isItNewPattern;
		uteMEE.isItNewMap = _isItNewMap;
		uteMEE.isItLoad = isLoad;
		uteMEE.newProjectName = name;
		StartCoroutine(uteMEE.uteInitMapEditorEngine());
		HideMenu();
	}

	public void HideMenu()
	{
		isShowMenu = false;
	}

	public void ShowMenu()
	{
		isShowMenu = true;
	}

	private string FilterName(string name)
	{
		if(name.Contains(" ")||name.Contains("/")||name.Contains("\"")||name.Contains(":")||name.Contains("$")||name.Contains("."))
		{
			Debug.Log("Warning: empty spaces or symbols: \" : / $ : . are forbiden. They will be stripped.");
		}

		name = name.Replace(" ","");
		name = name.Replace("/","");
		name = name.Replace("\"","");
		name = name.Replace(":","");
		name = name.Replace("$","");
		name = name.Replace(".","");

		return name;
	}

	private bool CreateNewMap(string name)
	{
		name = FilterName(name);

		if(!CheckIfMapExists(name)&&!name.Equals(""))
		{
			StreamReader sr = new StreamReader(myMapsPath);
			string info = sr.ReadToEnd();
			sr.Close();
			info += name+":";

			StreamWriter sw = new StreamWriter(myMapsPath);
			sw.Write("");
			sw.Write(info);
			sw.Flush();
			sw.Close();

			ReadAllMaps();

			return true;
		}
		else
		{
			Debug.Log("Error: Map with this name already exists or map name is empty.");

			return false;
		}
	}

	private bool CreateNewPattern(string name)
	{
		name = FilterName(name);

		if(!CheckIfPatternExists(name))
		{
			StreamReader sr = new StreamReader(myPatternsPath);
			string info = sr.ReadToEnd();
			sr.Close();
			info += name+":";

			StreamWriter sw = new StreamWriter(myPatternsPath);
			sw.Write("");
			sw.Write(info);
			sw.Flush();
			sw.Close();

			ReadAllPatterns();

			return true;
		}
		else
		{
			Debug.Log("Error: Pattern with this name already exists.");

			return false;
		}
	}

	private bool CheckIfMapExists(string name)
	{
		ReadAllMaps();

		for(int i=0;i<myMaps.Count;i++)
		{
			if(myMaps[i].ToString().Equals(name))
			{
				return true;
			}
		}

		return false;
	}

	private bool CheckIfPatternExists(string name)
	{
		ReadAllPatterns();

		for(int i=0;i<myPatterns.Count;i++)
		{
			if(myPatterns[i].ToString().Equals(name))
			{
				return true;
			}
		}

		return false;
	}

	private void ReadAllMaps()
	{
		if(myMaps.Count>0)
		{
			myMaps.Clear();
		}

		StreamReader sr = new StreamReader(myMapsPath);
		string info = sr.ReadToEnd();
		sr.Close();

		string[] allinfo = info.Split(":"[0]);

		for(int i=0;i<allinfo.Length;i++)
		{
			string str = allinfo[i];

			if(!str.Equals(""))
			{
				myMaps.Add(str);
			}
		}
	}

	private void ReadAllPatterns()
	{
		if(myPatterns.Count>0)
		{
			myPatterns.Clear();
		}

		StreamReader sr = new StreamReader(myPatternsPath);
		string info = sr.ReadToEnd();
		sr.Close();

		string[] allinfo = info.Split(":"[0]);

		for(int i=0;i<allinfo.Length;i++)
		{
			string str = allinfo[i];

			if(!str.Equals(""))
			{
				myPatterns.Add(str);
			}
		}
	}

	private void DeleteMap(string name, bool isItMap)
	{
		string path = "";
		string mapPath;
		string mapInfoPath = "";
		string mapLayerPath = "";
		string mapPathMetaPath = "";
		string mapInfoMetaPath = "";
		string mapLayerMetaPath = "";

		if(isItMap)
		{
			mapInfoPath = uteGLOBAL3dMapEditor.getMapsDir()+name+"_info.txt";
			mapInfoMetaPath = uteGLOBAL3dMapEditor.getMapsDir()+name+"_info.txt.meta";
			mapPath = uteGLOBAL3dMapEditor.getMapsDir()+name+".txt";
			mapPathMetaPath = uteGLOBAL3dMapEditor.getMapsDir()+name+".txt.meta";
			mapLayerPath = uteGLOBAL3dMapEditor.getMapsDir()+name+"_layers.txt";
			mapLayerMetaPath = uteGLOBAL3dMapEditor.getMapsDir()+name+"_layers.txt.meta";
		}
		else
		{
			mapPath = uteGLOBAL3dMapEditor.getPatternsDir()+name+".txt";
			mapPathMetaPath = uteGLOBAL3dMapEditor.getMapsDir()+name+".txt.meta";
		}
		string luaPath = uteGLOBAL3dMapEditor.LuaMapConfigPath + $"map_{name}.lua";
		if (File.Exists(luaPath))
		{
			File.Delete(luaPath);
		}

		ArrayList arr = new ArrayList();

		if(File.Exists(mapPathMetaPath))
		{
			File.Delete(mapPathMetaPath);
		}

		if(File.Exists(mapPath))
		{
			File.Delete(mapPath);
		}

		if(isItMap)
		{
			if(File.Exists(mapLayerPath))
			{
				File.Delete(mapLayerPath);
			}

			if(File.Exists(mapLayerMetaPath))
			{
				File.Delete(mapLayerMetaPath);
			}

			if(File.Exists(mapInfoPath))
			{
				File.Delete(mapInfoPath);
			}

			if(File.Exists(mapInfoMetaPath))
			{
				File.Delete(mapInfoMetaPath);
			}

			path = myMapsPath;
			ReadAllMaps();
			arr = myMaps;
		}
		else
		{
			path = myPatternsPath;
			ReadAllPatterns();
			arr = myPatterns;
		}

		string allnewpr = "";

		for(int i=0;i<arr.Count;i++)
		{
			if(!arr[i].ToString().Equals("")&&!arr[i].ToString().Equals(name))
			{
				allnewpr += arr[i].ToString()+":";
			}
		}

		StreamWriter sw = new StreamWriter(path);
		sw.Write("");
		sw.Write(allnewpr);
		sw.Flush();
		sw.Close();

		if(isItMap)
		{
			ReadAllMaps();
		}
		else
		{
			ReadAllPatterns();
		}
	}
#endif
}
