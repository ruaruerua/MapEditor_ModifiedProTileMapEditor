using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;
using static MapItemType;
using System.Linq;

public class uteSaveMap : MonoBehaviour {
#if UNITY_EDITOR
	[HideInInspector]
	public bool isSaving;
	public uteOptionsBox options;
	private string boundsInfo;

	private class ItemGridXY
	{
		public int gridX;
		public int gridY;
	}

	private class GridData
	{
		public string prefabName;
		public int MainID;
		public int gridX;
		public int gridY;
		public Vector3 postition;
		public Vector3 eulerAngles;
		public ItemType itemType = ItemType.BaseGrid;
		public List<string> effectPrefabName = new List<string>();
		public List<int> effectID = new List<int>();
		public string scenesItemPrefabName = "";
		public int scenesItemID = 0;
		public int IsMyArea = 0;
		public int IsEnemyArea = 0; 
	}

	private void Start()
	{
		boundsInfo = "";
		isSaving = false;
	}

	public void PassMapBounds(float mostLeft, float mostRight, float mostForward, float mostBack, float mostUp, float mostBottom)
	{
		boundsInfo += mostLeft.ToString()+":"+mostRight.ToString()+":"+mostForward.ToString()+":"+mostBack.ToString()+":"+mostUp.ToString()+":"+mostBottom.ToString();
	}

	public IEnumerator SaveMap(string mapName, bool isItMap)
	{
		isSaving = true;
		yield return 0;

		GameObject main = (GameObject) GameObject.Find("MAP");
		uteTagObject[] allObjects = main.GetComponentsInChildren<uteTagObject>();
		string info = "";

		for (int i=0;i< allObjects.Length;i++)
		{
			if(i%2000==0) yield return 0;
			
			GameObject obj = (GameObject) ((uteTagObject)allObjects[i]).gameObject;
			string objGUID = ((uteTagObject)allObjects[i]).objGUID;
			string objName = ((uteTagObject)allObjects[i]).name;
			bool objIsStatic = ((uteTagObject)allObjects[i]).isStatic;
			string layerName = ((uteTagObject)allObjects[i]).layerName;
			bool objTC = ((uteTagObject)allObjects[i]).isTC;
			string tcFamilyName = "-";

			if(obj.GetComponent<uteTcTag>())
			{
				tcFamilyName = ((uteTcTag) obj.GetComponent<uteTcTag>()).tcFamilyName;
			}

			string staticInfo = "0";
			string tcInfo = "0";

			if(objIsStatic)
				staticInfo = "1";

			if(objTC)
				tcInfo = "1";
			info += objGUID + ":" + obj.transform.position.x + ":" + obj.transform.position.y + ":" + obj.transform.position.z + ":" + ((int)obj.transform.localEulerAngles.x) + ":" + ((int)obj.transform.localEulerAngles.y) + ":" + ((int)obj.transform.localEulerAngles.z) + ":" + staticInfo + ":" + tcInfo + ":" + tcFamilyName + ":" + layerName + ":$";
			//info += objName + ":" + obj.transform.position.x + ":" + obj.transform.position.y + ":" + obj.transform.position.z + ":" + ((int)obj.transform.localEulerAngles.x) + ":" + ((int)obj.transform.localEulerAngles.y) + ":" + ((int)obj.transform.localEulerAngles.z) + ":" + staticInfo + ":" + tcInfo + ":" + tcFamilyName + ":" + layerName + ":$";
		}

		string path;

		if(isItMap)
		{
			path = uteGLOBAL3dMapEditor.getMapsDir();
			SaveToLua(mapName);
		}
		else
		{
			path = uteGLOBAL3dMapEditor.getPatternsDir();
		}

		StreamWriter sw = new StreamWriter(path+mapName+".txt");
		sw.Write("");
		sw.Write(info);
		sw.Flush();
		sw.Close();

		SaveMapSettings(mapName, isItMap);

		isSaving = false;
		yield return 0;
	}

	private void SaveToLua(string mapName)
	{
		GameObject main = (GameObject)GameObject.Find("MAP");
		uteTagObject[] allObjects = main.GetComponentsInChildren<uteTagObject>();

		string lua_string = "local list = { \n";
		List<string> luaTable_strings = new List<string>();
		string left = "{";
		string right = "}";

		List<uteTagObject> basegridList = new List<uteTagObject>();
		List<uteTagObject> elseItemList = new List<uteTagObject>();
		Dictionary<string, int> GridToIndex = new Dictionary<string, int>();
		Dictionary<int, GridData> GridDataList = new Dictionary<int, GridData>();

		for (int i = 0; i < allObjects.Length; i++)
		{
			if(_GetMapItemType(allObjects[i].gameObject) == ItemType.BaseGrid)
			{
				basegridList.Add(allObjects[i]);
			}
			else
			{
				elseItemList.Add(allObjects[i]);
			}
		}
		basegridList.Sort(delegate (uteTagObject x, uteTagObject y) {
			Transform transx = x.gameObject.transform;
			Transform transy = y.gameObject.transform;
			if (transx.position.x == transy.position.x)
			{
				return transx.position.z.CompareTo(transy.position.z);
			}
			return transx.position.x.CompareTo(transy.position.x);
		});

		int globalIndex = 1;
		//遍历基础地块计算出棋盘信息
		for (int i = 0; i < basegridList.Count; i++)
		{
			GameObject obj = (GameObject)((uteTagObject)basegridList[i]).gameObject;
			string objName = ((uteTagObject)basegridList[i]).name;
			ItemType itemType = _GetMapItemType(obj);
			int mainID = _GetMapItemMainID(obj);
			//默认8*8地块，通过i计算当前地块的x，y
			int curIndex = i + 1;
			int gridX = curIndex % 8;
			gridX = gridX == 0 ? 8 : gridX;
			int gridY = (int)Math.Ceiling((double)curIndex / 8);

			GridData _gridData = new GridData();
			_gridData.prefabName = objName;
			_gridData.MainID = mainID;
			_gridData.postition = obj.transform.position;
			_gridData.eulerAngles = obj.transform.localEulerAngles;
			_gridData.itemType = itemType;
			_gridData.gridX = gridX;
			_gridData.gridY = gridY;
			GridDataList.Add(globalIndex, _gridData);

			//string SimpleString = $"[{globalIndex}] = {left}\"{objName}\",{gridX},{gridY},{left}{Vector3ToTableType(obj.transform.position)}{right},{left}{Vector3ToTableType(obj.transform.localEulerAngles)}{right},\"{itemType}\",{right},";
			//luaTable_strings.Add(SimpleString);
			string posxz = ((int)obj.transform.position.x).ToString() + ((int)obj.transform.position.z).ToString();
			ItemGridXY girdxy = new ItemGridXY();
			girdxy.gridX = gridX;
			girdxy.gridY = gridY;
			GridToIndex.Add(posxz, globalIndex);

			globalIndex++;
		}
		//通过棋盘信息计算出其他物体的棋盘位置
		for (int i = 0; i < elseItemList.Count; i++)
		{
			GameObject obj = (GameObject)((uteTagObject)elseItemList[i]).gameObject;
			string objName = ((uteTagObject)elseItemList[i]).name;
			int curID = _GetMapItemMainID(obj);
			ItemType itemType = _GetMapItemType(obj);

			string posxz = ((int)obj.transform.position.x).ToString() + ((int)obj.transform.position.z).ToString();
			int _index = GridToIndex[posxz];
			GridData curData = GridDataList[_index];

			if(itemType == ItemType.GridEffect)
			{
				//curData.effectPrefabName.Add(objName);
				curData.effectID.Add(curID);
			}
			else if(itemType == ItemType.ScenesItem)
			{
				//curData.scenesItemPrefabName = objName;
				curData.scenesItemID = curID;
			}
			else if (itemType == ItemType.MySetArea)
			{
				curData.IsMyArea = 1;
			}
			else if (itemType == ItemType.EnemyRandomArea)
			{
				curData.IsEnemyArea = 1;
			}
		}

		var Sortedresult = from pair in GridDataList orderby pair.Key ascending select pair;
		foreach (KeyValuePair<int, GridData> SimpleInfo in Sortedresult)
		{
			var Value = SimpleInfo.Value;
			string SimpleString = $"[{SimpleInfo.Key}] = {left}{Value.MainID},{Value.gridX},{Value.gridY},{left}{Vector3ToTableType(Value.postition)}{right},{left}{Vector3ToTableType(Value.eulerAngles)}{right}," +
				$"{left}{intToList(Value.effectID)}{right},{Value.scenesItemID},{Value.IsMyArea},{Value.IsEnemyArea},\"{Value.prefabName}\",{right},";
			luaTable_strings.Add(SimpleString);
		}

		//Set Lua
		foreach (string SimpleInfo in luaTable_strings)
		{
			lua_string += SimpleInfo + "\n";
		}
		lua_string += "}\n";
		string ExString = $@"
Phoenix_G.map_{mapName} = Phoenix_G.map_{mapName} or {left}{right}
Phoenix_G.map_{mapName} = list
local mapValues = {left}objID=1,gridX=2,gridY=3,position=4,rotation=5,effectList=6,scenesItem=7,isMyArea=8,isEnemyArea=9,objName=10{right}
local meta = {left}{right}
meta.__index = function ( table, key )
return rawget(table,mapValues[key])
end
for k, v in pairs(list) do
   setmetatable(v, meta)
end
return list
";
		lua_string += ExString;
		string luaPath = uteGLOBAL3dMapEditor.LuaMapConfigPath + $"map_{mapName}.lua";
		if (!File.Exists(luaPath))
		{
			File.Create(luaPath).Close();
		}
		File.WriteAllText(luaPath, lua_string, Encoding.Default);

		//SaveMapLuaInclude();
	}

	private ItemType _GetMapItemType(GameObject gameObject)
	{
		ItemType curType = ItemType.BaseGrid;
		MapItemType curTypeComponent = gameObject.GetComponentInChildren<MapItemType>();
		if (curTypeComponent)
		{
			curType = curTypeComponent.curItemType;
		}
		return curType;
	}
	private int _GetMapItemMainID(GameObject gameObject)
	{
		int ID = 0;
		MapItemType curTypeComponent = gameObject.GetComponentInChildren<MapItemType>();
		if (curTypeComponent)
		{
			ID = curTypeComponent.MainID;
		}
		return ID;
	}

	private void SaveMapLuaInclude()
	{
		string luaMapPath = uteGLOBAL3dMapEditor.LuaMapConfigPath;
		string lua_string = "";
		List<string> luaTable_strings = new List<string>();
		DirectoryInfo direction = new DirectoryInfo(luaMapPath);
		FileInfo[] fileArray = direction.GetFiles();
		foreach (FileInfo file in fileArray)
		{
			if (file.Name == "Include.lua")
			{
				continue;
			}
			if(file.Extension == ".lua")
			{
				string[] txts = file.Name.Split("."[0]);
				luaTable_strings.Add(txts[0]);
			}

		}
		foreach (string SimpleInfo in luaTable_strings)
		{
			lua_string += "require(\"MapConfigs/" + SimpleInfo + "\")\n";
		}
		string includePath = uteGLOBAL3dMapEditor.LuaMapConfigPath + "Include.lua";
		if (!File.Exists(includePath))
		{
			File.Create(includePath).Close();
		}
		File.WriteAllText(includePath, lua_string, Encoding.Default);
	}

	private void SaveMapSettings(string mapName, bool isItMap)
	{
		if(isItMap)
		{
			string path = uteGLOBAL3dMapEditor.getMapsDir();
			GameObject MAIN = (GameObject) GameObject.Find("MAIN");
			GameObject YArea = (GameObject) GameObject.Find("MAIN/YArea");
			GameObject MapEditorCamera = (GameObject) GameObject.Find("MAIN/YArea/MapEditorCamera");

			string info = MAIN.transform.position.x+":"+MAIN.transform.position.y+":"+MAIN.transform.position.z+":"+MAIN.transform.localEulerAngles.x+":"+MAIN.transform.localEulerAngles.y+":"+MAIN.transform.localEulerAngles.z+":"+YArea.transform.localEulerAngles.x+":"+YArea.transform.localEulerAngles.y+":"+YArea.transform.localEulerAngles.z+":"+MapEditorCamera.transform.localEulerAngles.x+":"+MapEditorCamera.transform.localEulerAngles.y+":"+MapEditorCamera.transform.localEulerAngles.z+":";

			info += options.isEditorLightOn+":"+options.isCastShadows+":"+uteGLOBAL3dMapEditor.XZsnapping+":"+uteGLOBAL3dMapEditor.OverlapDetection+":"+options.isShowGrid+":"+uteGLOBAL3dMapEditor.CalculateXZPivot+":"+options.snapOnTop+":"+boundsInfo+":";
			info += options.isUse360Snap+":";

			StreamWriter sw = new StreamWriter(path+mapName+"_info.txt");
			sw.Write("");
			sw.Write(info);
			sw.Flush();
			sw.Close();
		}
	}

	private float RoundToHalf(float point)
	{
		point *= 2.0f;
		point = Mathf.Round(point);
		point /= 2.0f;

		return point;
	}

	private string Vector3ToTableType(Vector3 v)
	{
		string TableString = string.Format("{0},{1},{2}", (float)Math.Round((double)v.x, 2), (float)Math.Round((double)v.y, 2), (float)Math.Round((double)v.z, 2));
		return TableString;
	}

	private string ListToString(List<string> effectPrefabName)
	{
		String _string = "";
		for (int i = 0; i < effectPrefabName.Count; i++)
		{
			_string += $"\"{effectPrefabName[i]}\"";
			if (i != effectPrefabName.Count -1)
			{
				_string += "|";
			}
		}
		return _string;
	}

	private string intToList(List<int> effectID)
	{
		String _string = "";
		for (int i = 0; i < effectID.Count; i++)
		{
			_string += $"{effectID[i]}";
			if (i != effectID.Count - 1)
			{
				_string += "|";
			}
		}
		return _string;
	}
#endif
}
