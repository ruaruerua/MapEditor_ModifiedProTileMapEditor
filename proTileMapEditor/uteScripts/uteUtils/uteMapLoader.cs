using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
#pragma warning disable 0219
using UnityEditor;

[AddComponentMenu("proTileMapEditor/uteMapLoader")]
[ExecuteInEditMode]
#endif
public class uteMapLoader : MonoBehaviour {

	[SerializeField]
	public bool LoadAuto=true;
	[SerializeField]
	public bool StaticBatching=true;
	[SerializeField]
	public bool AddMeshColliders=false;
	[SerializeField]
	public bool RemoveLeftovers=true;
	[SerializeField]
	public bool PrepareForLightmapping=false;
	[SerializeField]
	public bool UseTilePrefabRotationRef=false;
	[SerializeField]
	public Vector3 MapOffset = new Vector3(0,0,0);
	[SerializeField]
	public Vector3 MapScale = new Vector3(1,1,1);
	[HideInInspector]
	public GameObject[] refTiles;
	[HideInInspector]
	public string mapName;
	[HideInInspector]
	public bool isMapLoaded;
//	[SerializeField]
	[HideInInspector]
	public int currentOptimizationLevelIndex;

	public bool loadUseTilePrefabRotationRef
	{
		get { return UseTilePrefabRotationRef; }
		set
		{
			if(UseTilePrefabRotationRef == value) return;

			UseTilePrefabRotationRef = value;
		}
	}

	public Vector3 loadMapOffset
	{
		get { return MapOffset; }
		set
		{
			if(MapOffset == value) return;

			MapOffset = value;
		}
	}

	public Vector3 loadMapScale
	{
		get { return MapScale; }
		set
		{
			if(MapScale == value) return;

			MapScale = value;
		}
	}

	public bool loadAutoVal
    {
        get { return LoadAuto; }
        set
        {
            if (LoadAuto == value) return;
 
            LoadAuto = value;
        }
    }

    public bool loadStaticBatching
    {
    	get { return StaticBatching; }
    	set
    	{
    		if(StaticBatching==value) return;

    		StaticBatching = value;
    	}
    }

    public bool loadAddMeshColliders
    {
    	get { return AddMeshColliders; }
    	set
    	{
    		if(AddMeshColliders==value) return;

    		AddMeshColliders = value;
    	}
    }

    public bool loadRemoveLeftovers
    {
    	get { return RemoveLeftovers; }
    	set
    	{
    		if(RemoveLeftovers==value) return;

    		RemoveLeftovers = value;
    	}
    }

    public bool loadPrepareForLightmapping
    {
    	get { return PrepareForLightmapping; }
    	set
    	{
    		if(PrepareForLightmapping==value) return;

    		PrepareForLightmapping = value;
    	}
    }

    [HideInInspector]
    public string myLatestMap = "";

    public class Tile
    {
    	public Vector3 pos;
    	public Vector3 rot;
    	public int id;
    	public bool isStatic;
    	public GameObject obj;
    	public float distanceFromPoint;

    	public Tile(Vector3 _pos, Vector3 _rot, int _id, string staticInfo, GameObject _obj, float _distanceFromPoint)
    	{
    		pos = _pos;
    		rot = _rot;
    		id = _id;
    		isStatic = false;
    		obj = _obj;
    		distanceFromPoint = _distanceFromPoint;

    		if(staticInfo.Equals("1"))
    		{
    			isStatic = true;
    		}
    	}
    }
    
	#if UNITY_EDITOR
	[SerializeField]
	[HideInInspector]
	public int currentMapIndex;
	[HideInInspector]
	public string currentMapName;

	public int myMapIndexVal
    {
        get { return currentMapIndex; }
        set
        {
            if (currentMapIndex == value) return;
 
            currentMapIndex = value;
        }
    }

    public int myMapOptimizationIndexVal
    {
    	get { return currentOptimizationLevelIndex; }
    	set
    	{
    		if(currentOptimizationLevelIndex == value) return;

    		currentOptimizationLevelIndex = value;
    	}
    }

    public void LoadBounds()
    {
    	StreamReader sr = new StreamReader(uteGLOBAL3dMapEditor.getMapsDir()+mapName+"_info.txt");
    	string info = sr.ReadToEnd();
    	sr.Close();

    	string[] parts = info.Split(":"[0]);

    	if(!parts[19].Equals(""))
    	{
    		float mostLeft = System.Convert.ToSingle(parts[19]);
    		float mostRight = System.Convert.ToSingle(parts[20]);
    		float mostForward = System.Convert.ToSingle(parts[21]);
    		float mostBack = System.Convert.ToSingle(parts[22]);
    		float mostUp = System.Convert.ToSingle(parts[23]);
    		float mostBottom = System.Convert.ToSingle(parts[24]);

    		Vector3 bounderPosition = new Vector3(mostLeft+(mostRight-mostLeft)/2.0f,mostBottom+(mostUp-mostBottom)/2.0f,mostBack+(mostForward-mostBack)/2.0f);
    		Vector3 bounderScale = new Vector3(mostRight-mostLeft,mostUp-mostBottom,mostForward-mostBack);
    		GameObject mapBounder = (GameObject) Instantiate(Resources.Load("uteForEditor/uteMapBounder"),bounderPosition+MapOffset+new Vector3(-500,0,-500),new Quaternion(0,0,0,0));
    		mapBounder.transform.localScale = bounderScale;
    		mapBounder.name = mapName+"_BOUNDS";

    		//Debug.Log("ML:"+mostLeft+" MR:"+mostRight+" MF:"+mostForward+" MBa:"+mostBack+" MU:"+mostUp+" MBo:"+mostBottom);
    	}
    	else
    	{
    		Debug.Log("[Loader] This seems like an old map. Please open it in proTile Map Editor and re-save it.");
    	}
    }

    public void SetMap(string name)
    {
    	isMapLoaded = false;
    	currentMapName = name;
    	mapName = name;
    	StreamReader sr = new StreamReader(uteGLOBAL3dMapEditor.getMapsDir()+name+".txt");
    	string allmapinfo = sr.ReadToEnd();
    	sr.Close();

    	string[] allMapBaseItems = allmapinfo.Split("$"[0]);
    	List<string> allGuids = new List<string>();

    	for(int i=0;i<allMapBaseItems.Length-1;i++)
    	{
    		string[] allInfoSplit = allMapBaseItems[i].Split(":"[0]);
    		allGuids.Add(allInfoSplit[0].ToString());
    	}

    	allGuids = RemoveDuplicates(allGuids);

    	refTiles = new GameObject[allGuids.Count];

    	for(int i=0;i<allGuids.Count;i++)
    	{
    		string guid = allGuids[i].ToString();
    		string opath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
			GameObject tGO = (GameObject) UnityEditor.AssetDatabase.LoadMainAssetAtPath(opath);
			refTiles[i] = tGO;
			allmapinfo = allmapinfo.Replace(guid,i.ToString());
		}

		myLatestMap = allmapinfo;
    }

    private List<string> RemoveDuplicates(List<string> myList)
    {
        List<string> newList = new List<string>();

        for(int i=0;i<myList.Count;i++)
            if (!newList.Contains(myList[i].ToString()))
                newList.Add(myList[i].ToString());

        return newList;
    }

	#endif

	private void Awake()
	{
		#if UNITY_EDITOR
		if(EditorApplication.isPlaying)
		{
		#endif
			if(LoadAuto)
			{
				LoadMap();
			}
		#if UNITY_EDITOR
		}
		#endif
	}

	public void LoadMap()
	{
		StartCoroutine(_LoadMap());
	}

	private IEnumerator _LoadMap()
	{
		#if UNITY_EDITOR
		Debug.Log("Loading Map... (This message appears only in the Editor)");
		#endif

		GameObject MAP = new GameObject(mapName);
		GameObject MAP_S = new GameObject("STATIC");
		GameObject MAP_D = new GameObject("DYNAMIC");
		MAP_S.transform.parent = MAP.transform;
		MAP_D.transform.parent = MAP.transform;

		string[] myMapInfoAll = myLatestMap.Split("$"[0]);

		for(int i=0;i<myMapInfoAll.Length-1;i++)
		{
			#if !UNITY_EDITOR
			if(i%6000==0) yield return 0;
			#endif

			string[] myMapParts = myMapInfoAll[i].Split(":"[0]);
			int objID = System.Convert.ToInt32(myMapParts[0]);
			GameObject obj = (GameObject) refTiles[objID];
			float pX = System.Convert.ToSingle(myMapParts[1]);
			float pY = System.Convert.ToSingle(myMapParts[2]);
			float pZ = System.Convert.ToSingle(myMapParts[3]);
			int rX = System.Convert.ToInt32(myMapParts[4]);
			int rY = System.Convert.ToInt32(myMapParts[5]);
			int rZ = System.Convert.ToInt32(myMapParts[6]);
			string staticInfo = myMapParts[7];
			bool isStatic = false;

			if(staticInfo.Equals("1"))
			{
				isStatic = true;
			}

			GameObject newObj = (GameObject) Instantiate(obj,new Vector3(pX,pY,pZ)+MapOffset+new Vector3(-500,0,-500),Quaternion.identity);
			newObj.name = objID.ToString();
			newObj.transform.localEulerAngles = new Vector3(rX,rY,rZ);

			if(UseTilePrefabRotationRef)
			{
				 newObj.transform.localEulerAngles += obj.transform.localEulerAngles;
			}

			if(isStatic)
			{
				newObj.isStatic = true;
				newObj.transform.parent = MAP_S.transform;
			}
			else
			{
				newObj.isStatic = false;
				newObj.transform.parent = MAP_D.transform;
			}
		}

		if(StaticBatching)
		{
			uteCombineChildren batching = (uteCombineChildren) MAP_S.AddComponent<uteCombineChildren>();

			string optLevel = "low";

			if(currentOptimizationLevelIndex==1)
			{
				optLevel = "medium";
			}
			else if(currentOptimizationLevelIndex==2)
			{
				optLevel = "high";
			}

			batching.Batch(AddMeshColliders,RemoveLeftovers,false,PrepareForLightmapping,optLevel);
		}

		MAP_S.transform.localScale = MapScale;
		MAP_D.transform.localScale = MapScale;

		isMapLoaded = true;

		#if UNITY_EDITOR
		Debug.Log("Map LOADED! (This message appears only in the Editor)");
		#endif

		yield return 0;
	}

	public void LoadMapAsyncFromPoint(Vector3 point, int objectsPerFrame, int startAsyncAfterObjNr = 0)
	{
		StartCoroutine(_LoadMapAsyncFromPoint(point,objectsPerFrame, startAsyncAfterObjNr));
	}

	IEnumerator _LoadMapAsyncFromPoint(Vector3 point, int objectsPerFrame, int startAsyncAfterObjNr = 0)
	{
		List<Tile> allTilesForAsyncLoad = new List<Tile>();
		List<Tile> allTilesForAsyncLoadNew = new List<Tile>();
		GameObject MAP = new GameObject(mapName);
		GameObject MAP_S = new GameObject("STATIC");
		GameObject MAP_D = new GameObject("DYNAMIC");
		MAP_S.transform.parent = MAP.transform;
		MAP_D.transform.parent = MAP.transform;

		string[] myMapInfoAll = myLatestMap.Split("$"[0]);
		float distance = 10000000.0f;
		int nearestToPoint = 0;

		for(int i=0;i<myMapInfoAll.Length-1;i++)
		{
			string[] myMapParts = myMapInfoAll[i].Split(":"[0]);
			int objID = System.Convert.ToInt32(myMapParts[0]);
			GameObject obj = (GameObject) refTiles[objID];
			float pX = System.Convert.ToSingle(myMapParts[1]);
			float pY = System.Convert.ToSingle(myMapParts[2]);
			float pZ = System.Convert.ToSingle(myMapParts[3]);
			int rX = System.Convert.ToInt32(myMapParts[4]);
			int rY = System.Convert.ToInt32(myMapParts[5]);
			int rZ = System.Convert.ToInt32(myMapParts[6]);
			string staticInfo = myMapParts[7];

			Vector3 fixedPos = new Vector3(pX,pY,pZ)+new Vector3(-500,0,-500)+MapOffset;
			float dist = Vector3.Distance(point,fixedPos);
			allTilesForAsyncLoad.Add(new Tile(fixedPos, new Vector3(rX,rY,rZ), objID, staticInfo, obj, dist));
		}

		allTilesForAsyncLoad = allTilesForAsyncLoad.OrderBy(x => x.distanceFromPoint).ToList();

		/*List<int> newIndexes = new List<int>();
		int counter = 0;
		Debug.Log(nearestToPoint+"AAA");
		for(int i=0;i<newTiles.Count;i++)
		{
			if(allTilesForAsyncLoad.Count<i)
			{
				newIndexes.Add(i);
			}
			
			int res = i-(i-(++counter));

			if(res>=0)
			{
				newIndexes.Add(res);
			}
		}

		for(int i=0;i<newIndexes.Count;i++)
		{
			Debug.Log(nearestToPoint+":"+newIndexes[i]);
		}*/
		int n_ct = 0;
		for(int i=0;i<allTilesForAsyncLoad.Count;i++)
		{
			//int idx = newIndexes[i];
			
		//	if(idx>=newTiles.Count) continue;

			GameObject newObj = (GameObject) Instantiate(allTilesForAsyncLoad[i].obj,allTilesForAsyncLoad[i].pos,Quaternion.identity);
			newObj.transform.localEulerAngles = allTilesForAsyncLoad[i].rot;// = newTiles[i].obj.transform.localEulerAngles;
			newObj.name = allTilesForAsyncLoad[i].id.ToString();

			if(allTilesForAsyncLoad[i].isStatic)
			{
				newObj.isStatic = true;
				newObj.transform.parent = MAP_S.transform;
			}
			else
			{
				newObj.isStatic = false;
				newObj.transform.parent = MAP_D.transform;
			}

			if(i>startAsyncAfterObjNr)
			{
				if(n_ct++>20)// if(i%objectsPerFrame==0)
				{
					n_ct = 0;
					yield return 0;
				}
			}
		}

		if(StaticBatching)
		{
			uteCombineChildren batching = (uteCombineChildren) MAP_S.AddComponent<uteCombineChildren>();

			string optLevel = "low";

			if(currentOptimizationLevelIndex==1)
			{
				optLevel = "medium";
			}
			else if(currentOptimizationLevelIndex==2)
			{
				optLevel = "high";
			}

			batching.Batch(AddMeshColliders,RemoveLeftovers,false,PrepareForLightmapping,optLevel);
		}

		MAP_S.transform.localScale = MapScale;
		MAP_D.transform.localScale = MapScale;

		isMapLoaded = true;

		yield return 0;
	}

	public void LoadMapAsync(int frameSkip = 5)
	{
		StartCoroutine(_LoadMapAsync(frameSkip));
	}

	private IEnumerator _LoadMapAsync(int frameSkip)
	{
		#if UNITY_EDITOR
		Debug.Log("Loading Map... (This message appears only in the Editor)");
		#endif

		GameObject MAP = new GameObject(mapName);
		GameObject MAP_S = new GameObject("STATIC");
		GameObject MAP_D = new GameObject("DYNAMIC");
		MAP_S.transform.parent = MAP.transform;
		MAP_D.transform.parent = MAP.transform;

		string[] myMapInfoAll = myLatestMap.Split("$"[0]);

		for(int i=0;i<myMapInfoAll.Length-1;i++)
		{
			string[] myMapParts = myMapInfoAll[i].Split(":"[0]);
			int objID = System.Convert.ToInt32(myMapParts[0]);
			GameObject obj = (GameObject) refTiles[objID];
			float pX = System.Convert.ToSingle(myMapParts[1]);
			float pY = System.Convert.ToSingle(myMapParts[2]);
			float pZ = System.Convert.ToSingle(myMapParts[3]);
			int rX = System.Convert.ToInt32(myMapParts[4]);
			int rY = System.Convert.ToInt32(myMapParts[5]);
			int rZ = System.Convert.ToInt32(myMapParts[6]);
			string staticInfo = myMapParts[7];
			bool isStatic = false;

			if(staticInfo.Equals("1"))
			{
				isStatic = true;
			}

			GameObject newObj = (GameObject) Instantiate(obj,new Vector3(pX,pY,pZ)+MapOffset+new Vector3(-500,0,-500),Quaternion.identity);
			newObj.name = objID.ToString();
			newObj.transform.localEulerAngles = new Vector3(rX,rY,rZ);

			if(UseTilePrefabRotationRef)
			{
				newObj.transform.localEulerAngles += obj.transform.localEulerAngles;
			}

			if(i%frameSkip==0)
			{
				yield return 0;
			}

			if(isStatic)
			{
				newObj.isStatic = true;
				newObj.transform.parent = MAP_S.transform;
			}
			else
			{
				newObj.isStatic = false;
				newObj.transform.parent = MAP_D.transform;
			}
		}

		if(StaticBatching)
		{
			uteCombineChildren batching = (uteCombineChildren) MAP_S.AddComponent<uteCombineChildren>();

			string optLevel = "low";

			if(currentOptimizationLevelIndex==1)
			{
				optLevel = "medium";
			}
			else if(currentOptimizationLevelIndex==2)
			{
				optLevel = "high";
			}

			batching.Batch(AddMeshColliders,RemoveLeftovers,false,PrepareForLightmapping,optLevel);
		}

		MAP_S.transform.localScale = MapScale;
		MAP_D.transform.localScale = MapScale;

		isMapLoaded = true;

		#if UNITY_EDITOR
		Debug.Log("Map LOADED! (This message appears only in the Editor)");
		#endif

		yield return 0;
	}
}
