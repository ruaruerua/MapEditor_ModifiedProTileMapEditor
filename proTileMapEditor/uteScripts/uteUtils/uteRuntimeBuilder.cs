using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class uteRuntimeBuilder : MonoBehaviour {

	public GameObject MAINPLATFORM;
	[HideInInspector]
	public List<string> tilesRefDIC_STR = new List<string>();
	[HideInInspector]
	public List<GameObject> tilesRefDIC_OBJ = new List<GameObject>();
	[HideInInspector]
	public List<Texture2D> tilesRefDIC_TEX = new List<Texture2D>();
	private List<string> allCategories = new List<string>();
	private List<string> allGUIDIDS = new List<string>();
	[HideInInspector]
	public List<GameObject> allTilesRef = new List<GameObject>();
	[HideInInspector]
	public List<Category> allCategoriesRuntime = new List<Category>();
	[HideInInspector]
	public List<string> allCategoriesRuntimeStr = new List<string>();
	private GameObject runtimeTilesParent;
	private GameObject runtimeTilesStatic;
	private GameObject runtimeTilesDynamic;
	private uteMapEditorEngineRuntime RuntimeEngine;
	private List<string> customPreviews = new List<string>();
	private BuildMode buildMode;
	private List<string> uniqueTileNames = new List<string>();
	private float fixedTileSnap = 0f;

	public enum BuildMode
	{
		Normal = 0,
		Continuous = 1,
		Mass = 2
	}

	public class Tile
	{
		public string guidid;
		public GameObject ref_obj;
		public GameObject mainObject;
		public Texture2D preview;
		public string name;
		public string title;

		public Tile(string _guidid, GameObject _obj, Texture2D _preview, string _name, string _title)
		{
			guidid = _guidid;
			ref_obj = _obj;
			preview = _preview;
			name = _name;
			title = _title;
		}

		public void SetMainObject(GameObject obj)
		{
			mainObject = obj;
		}
	}

	public class Category
	{
		public List<Tile> allTiles = new List<Tile>();
		public string name;
		public string type;

		public Category(List<Tile> _allTiles, string _name, string _type)
		{
			allTiles = _allTiles;
			name = _name;
			type = _type;
		}
	}

	public void SetFixedRaycastPosition(Vector2 position)
	{
		RuntimeEngine.SetFixedRaycastPosition(position);
	}

	public void DisableFixedRaycastPosition()
	{
		RuntimeEngine.DisableFixedRaycastPosition();
	}

	public Vector2 GetFixedRaycastPosition()
	{
		return RuntimeEngine.GetFixedRaycastPosition();
	}

	public void SetRaycastDistance(float distance)
	{
		RuntimeEngine.SetRaycastDistance(distance);
	}

	public void SetSnapOption(string option)
	{
		RuntimeEngine.SetSnapOption(option);
	}

	public string GetSnapOption()
	{
		return RuntimeEngine.GetSnapOption();
	}

	public void EnableToBuild()
	{
		RuntimeEngine.UnsetCantBuildForced();
	}

	public void DisableToBuild()
	{
		RuntimeEngine.SetCantBuildForced();
	}

	public void DisableMouseInputForBuild()
	{
		RuntimeEngine.DisableMouseInputForBuild();
	}

	public void EnableMouseInputForBuild()
	{
		RuntimeEngine.EnableMouseInputForBuild();
	}

	public void SetBuildMode(BuildMode mode)
	{
		switch(mode)
		{
			case BuildMode.Normal:
				RuntimeEngine.SetBuildModeNormal();
				break;
			case BuildMode.Continuous:
				RuntimeEngine.SetBuildModeContinuous();
				break;
			case BuildMode.Mass:
				RuntimeEngine.SetBuildModeMass();
				break;
			default: break;
		}

		buildMode = mode;
	}

	public BuildMode GetCurrentBuildMode()
	{
		return buildMode;
	}

	private void Awake()
	{
		buildMode = BuildMode.Normal;

		InitAllTilesRuntime();
		InitBuildEngine();
	}

	private GameObject GetTileMainObjectByGUIDID(string guidid)
	{
		for(int i=0;i<allCategoriesRuntime.Count;i++)
		{
			Category cat = (Category) allCategoriesRuntime[i];

			for(int j=0;j<cat.allTiles.Count;j++)
			{
				Tile tile = (Tile) cat.allTiles[j];

				if(tile.guidid.Equals(guidid))
				{
					return tile.mainObject;
				}
			}
		}

		for(int i=0;i<allCategoriesRuntime.Count;i++)
		{
			Category cat = (Category) allCategoriesRuntime[i];

			for(int j=0;j<cat.allTiles.Count;j++)
			{
				Tile tile = (Tile) cat.allTiles[j];

				if(tile.name.Equals(guidid))
				{
					return tile.mainObject;
				}
			}
		}

		Debug.LogError("Error: Tile ["+guidid+"] is missing. Make sure if you are loading map with runtime-only Tiles, the Tile is loaded.");

		return null;
	}

	public void UnBatch()
	{
		GameObject mapTilesStatic = (GameObject) GameObject.Find("uteMAP_ALLTILES/uteMAP_STATIC");

		uteCombineChildren combiner = (uteCombineChildren) mapTilesStatic.GetComponent<uteCombineChildren>();

		if(combiner)
		{
			combiner.UnBatch();
		}
		else
		{
			Debug.Log("[RuntimeCombiner] No Batch information was found.");
		}
	}

	public void Batch(bool AddMeshColliders, bool RemoveLeftOvers)
	{
		GameObject mapTilesStatic = (GameObject) GameObject.Find("uteMAP_ALLTILES/uteMAP_STATIC");
		uteCombineChildren combiner = (uteCombineChildren) mapTilesStatic.GetComponent<uteCombineChildren>();

		if(!combiner)
		{
			combiner = (uteCombineChildren) mapTilesStatic.AddComponent<uteCombineChildren>();
		}
		
		combiner.Batch(AddMeshColliders,RemoveLeftOvers);
	}

	public List<GameObject> GetBatchedObjects()
	{
		uteCombineChildren combiner = GameObject.Find("uteMAP_ALLTILES/uteMAP_STATIC").GetComponent<uteCombineChildren>();

		if(combiner)
		{
			return combiner.GetBatchedObjects();
		}

		Debug.Log("[RuntimeCombiner] No Batch information was found.");

		return null;
	}

	public void RemoveTileFromCategory(string categoryName, string tileUniqueName)
	{
		int foundCatIndex = -1;
		int foundTileIndex = -1;

		for(int i=0;i<allCategoriesRuntime.Count;i++)
		{
			if(allCategoriesRuntime[i].name.Equals(categoryName))
			{
				for(int j=0;j<allCategoriesRuntime[i].allTiles.Count;j++)
				{
					if(allCategoriesRuntime[i].allTiles[j].name.Equals(tileUniqueName))
					{
						foundTileIndex = j;
						foundCatIndex = i;
						break;
					}
				}
			}
		}

		if(foundTileIndex!=-1)
		{
			allCategoriesRuntime[foundCatIndex].allTiles.RemoveAt(foundTileIndex);
			int inx = -1;

			for(int i=0;i<uniqueTileNames.Count;i++)
			{
				if(uniqueTileNames[i].Equals(tileUniqueName))
				{
					inx = i;
					break;
				}
			}

			uniqueTileNames.RemoveAt(inx);
		}
		else
		{
			Debug.Log("[RuntimeEngine] Tile was not found.");
		}
	}

	public void AddTileInCategory(GameObject obj, string categoryName, string tileUniqueName, string tileTitle, Texture2D tilePreview, bool isStatic)
	{
		bool catFound = false;
		bool notUnique = false;

		for(int i=0;i<allCategoriesRuntime.Count;i++)
		{
			if(allCategoriesRuntime[i].name.Equals(categoryName))
			{
				tileUniqueName = tileUniqueName.Replace(":","");
				tileUniqueName = tileUniqueName.Replace(" ","");
				tileUniqueName = tileUniqueName.Replace("$","");
				tileUniqueName = tileUniqueName.Replace("\n","");
				tileUniqueName = tileUniqueName.Replace("\r","");
				
				for(int k=0;k<uniqueTileNames.Count;k++)
				{
					if(uniqueTileNames[k].Equals(tileUniqueName))
					{
						notUnique = true;
						break;
					}
				}

				if(notUnique)
				{
					break;
				}
				else
				{
					uniqueTileNames.Add(tileUniqueName);
				}

				catFound = true;
				GameObject newObj = (GameObject) Instantiate(obj,Vector3.zero,obj.transform.rotation);
				newObj.name = tileUniqueName;
				allCategoriesRuntime[i].allTiles.Add(new Tile("",newObj,tilePreview,tileUniqueName,tileTitle));
				List<GameObject> twoGO = new List<GameObject>();
				twoGO = createColliderToObject(newObj,obj);

				GameObject behindGO = (GameObject) twoGO[0];
				behindGO.name = tileUniqueName;
				GameObject objGO = (GameObject) twoGO[1];
				allCategoriesRuntime[i].allTiles[allCategoriesRuntime[i].allTiles.Count-1].ref_obj = objGO;
				allCategoriesRuntime[i].allTiles[allCategoriesRuntime[i].allTiles.Count-1].ref_obj.transform.parent = behindGO.transform;
				behindGO.layer = 2;

				if(!isStatic)
				{
					behindGO.transform.parent = runtimeTilesDynamic.transform;
				}
				else if(isStatic)
				{
					behindGO.transform.parent = runtimeTilesStatic.transform;
				}

				allCategoriesRuntime[i].allTiles[allCategoriesRuntime[i].allTiles.Count-1].SetMainObject(behindGO);

				behindGO.transform.localPosition = new Vector3(0,0,0);
			}
		}

		if(notUnique)
		{
			Debug.Log("[RuntimeEngine] Can't add. Tile with this Unique name already exists.");
		}
		else if(!catFound)
		{
			Debug.Log("[RuntimeEngine] Category you want to include Tile in, does not exists.");
		}
	}

	private void InitBuildEngine()
	{
		RuntimeEngine = (uteMapEditorEngineRuntime) this.gameObject.AddComponent<uteMapEditorEngineRuntime>();

		if(MAINPLATFORM)
		{
			RuntimeEngine.MAINPLATFORM = MAINPLATFORM;
		}
		else
		{
			Debug.Log("[RuntimeEngine] Main Platform is not set in the uteRuntimeBuilder Component. Make sure that you have something with colliders to build on.");
			RuntimeEngine.MAINPLATFORM = new GameObject("MAINPLATFORM_DUMMY");
		}

		RuntimeEngine.RuntimeBuilder = this;
	}

	private void DisableAllExternalColliders(GameObject obj)
	{
		BoxCollider[] boxColls = obj.GetComponentsInChildren<BoxCollider>();

		for(int i=0;i<boxColls.Length;i++)
		{
			BoxCollider coll = (BoxCollider) boxColls[i];
			if(coll) coll.enabled = false;
		}

		MeshCollider[] mrColls = obj.GetComponentsInChildren<MeshCollider>();

		for(int i=0;i<mrColls.Length;i++)
		{
			MeshCollider coll = (MeshCollider) mrColls[i];
			if(coll) coll.enabled = false;
		}

		SphereCollider[] spColls = obj.GetComponentsInChildren<SphereCollider>();

		for(int i=0;i<spColls.Length;i++)
		{
			SphereCollider coll = (SphereCollider) spColls[i];
			if(coll) coll.enabled = false;
		}

		CapsuleCollider[] cpColls = obj.GetComponentsInChildren<CapsuleCollider>();

		for(int i=0;i<cpColls.Length;i++)
		{
			CapsuleCollider coll = (CapsuleCollider) cpColls[i];
			if(coll) coll.enabled = false;
		}
	}

	public void SetCurrentTile(GameObject go)
	{
		StartCoroutine(RuntimeEngine.SetCurrentTile(go));
	}

	public void SetCurrentTileInstantly(GameObject go)
	{
		RuntimeEngine.SetCurrentTileInstantly(go);
	}

	public List<string> GetListOfCategoryNames()
	{
		List<string> names = new List<string>();

		for(int i=0;i<allCategoriesRuntime.Count;i++)
		{
			names.Add(allCategoriesRuntime[i].name);
		}

		if(names.Count==0)
		{
			Debug.LogError("There are no categories!");
		}

		return names;
	}

	public List<GameObject> createColliderToObject(GameObject obj, GameObject obj_rs)
	{
		float lowestPointY = 10000.0f;
		float highestPointY = -10000.0f;
		float lowestPointZ = 10000.0f;
		float highestPointZ = -10000.0f;
		float lowestPointX = 10000.0f;
		float highestPointX = -10000.0f;
		float finalYSize = 1.0f;
		float finalZSize = 1.0f;
		float finalXSize = 1.0f;
		float divX = 2.0f;
		float divY = 2.0f;
		float divZ = 2.0f;
		float globalYSize = 0.1f;
		
		Vector3 objScale = obj.transform.localScale;
		obj.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
		
		MeshFilter mfs = (MeshFilter) obj.GetComponent<MeshFilter>();
		MeshFilter[] mfs_arr = (MeshFilter[]) obj.GetComponentsInChildren<MeshFilter>();
		SkinnedMeshRenderer smfs = (SkinnedMeshRenderer) obj.GetComponent(typeof(SkinnedMeshRenderer));
		SkinnedMeshRenderer[] smfs_arr = (SkinnedMeshRenderer[]) obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		Transform[] trms = (Transform[]) obj.GetComponentsInChildren<Transform>();
		
		if(mfs&&mfs.GetComponent<Renderer>())
		{
			lowestPointY = mfs.GetComponent<Renderer>().bounds.min.y;
			highestPointY = mfs.GetComponent<Renderer>().bounds.max.y;
		}
		
		if(mfs_arr.Length>0)
		{
			for(int i=0;i<mfs_arr.Length;i++)
			{
				MeshFilter mf_c = (MeshFilter) mfs_arr[i];
				
				if(mf_c&&mf_c.GetComponent<Renderer>())
				{
					if(mf_c.GetComponent<Renderer>().bounds.min.y<lowestPointY)
					{
						lowestPointY = mf_c.GetComponent<Renderer>().bounds.min.y;
					}
					
					if(mf_c.GetComponent<Renderer>().bounds.max.y>highestPointY)
					{
						highestPointY = mf_c.GetComponent<Renderer>().bounds.max.y;
					}
					
					if(mf_c.GetComponent<Renderer>().bounds.min.x<lowestPointX)
					{
						lowestPointX = mf_c.GetComponent<Renderer>().bounds.min.x;
					}
					
					if(mf_c.GetComponent<Renderer>().bounds.max.x>highestPointX)
					{
						highestPointX = mf_c.GetComponent<Renderer>().bounds.max.x;
					}
		
					if(mf_c.GetComponent<Renderer>().bounds.min.z<lowestPointZ)
					{
						lowestPointZ = mf_c.GetComponent<Renderer>().bounds.min.z;
					}
					
					if(mf_c.GetComponent<Renderer>().bounds.max.z>highestPointZ)
					{
						highestPointZ = mf_c.GetComponent<Renderer>().bounds.max.z;
					}
				}
			}
		}
		
		if(smfs)
		{
			lowestPointY = smfs.GetComponent<Renderer>().bounds.min.y;
			highestPointY = smfs.GetComponent<Renderer>().bounds.max.y;
		}
		
		if(smfs_arr.Length>0)
		{
			for(int i=0;i<smfs_arr.Length;i++)
			{
				SkinnedMeshRenderer smfs_c = (SkinnedMeshRenderer) smfs_arr[i];
				
				if(smfs_c)
				{
					if(smfs_c.GetComponent<Renderer>().bounds.min.y<lowestPointY)
					{
						lowestPointY = smfs_c.GetComponent<Renderer>().bounds.min.y;
					}
					
					if(smfs_c.GetComponent<Renderer>().bounds.max.y>highestPointY)
					{
						highestPointY = smfs_c.GetComponent<Renderer>().bounds.max.y;
					}
					
					if(smfs_c.GetComponent<Renderer>().bounds.min.x<lowestPointX)
					{
						lowestPointX = smfs_c.GetComponent<Renderer>().bounds.min.x;
					}
					
					if(smfs_c.GetComponent<Renderer>().bounds.max.x>highestPointX)
					{
						highestPointX = smfs_c.GetComponent<Renderer>().bounds.max.x;
					}
					
					if(smfs_c.GetComponent<Renderer>().bounds.min.z<lowestPointZ)
					{
						lowestPointZ = smfs_c.GetComponent<Renderer>().bounds.min.z;
					}
					
					if(smfs_c.GetComponent<Renderer>().bounds.max.z>highestPointZ)
					{
						highestPointZ = smfs_c.GetComponent<Renderer>().bounds.max.z;
					}
				}
			}
		}

		if(highestPointX - lowestPointX != -20000)
		{
			finalXSize = highestPointX - lowestPointX;
		} else { finalXSize = 1.0f; divX = 1.0f; lowestPointX = 0; Debug.Log ("X Something wrong with "+obj_rs.name); }
		
		if(highestPointY - lowestPointY != -20000)
		{
			finalYSize = highestPointY - lowestPointY;
		} else { finalYSize = globalYSize; divY = 1.0f; lowestPointY = 0; Debug.Log ("Y Something wrong with "+obj_rs.name); }
		
		if(highestPointZ - lowestPointZ != -20000)
		{
			finalZSize = highestPointZ - lowestPointZ;
		} else { finalZSize = 1.0f; divZ = 1.0f; lowestPointZ = 0; Debug.Log ("Z Something wrong with "+obj_rs.name); }
		
		for(int i=0;i<trms.Length;i++)
		{
			GameObject trm_go = (GameObject) ((Transform) trms[i]).gameObject;
			trm_go.layer = 2;
		}
		
		GameObject behindGO = new GameObject(obj.name);
		behindGO.AddComponent<BoxCollider>();
		obj.transform.parent = behindGO.transform;
		
		if(Mathf.Approximately(finalXSize,1.0f) || finalXSize<1.0f)
		{
			if(finalXSize<1.0f)
			{
				divX=1.0f;
				lowestPointX=-1.0f;
			}

			finalXSize=1.0f;
		}
		
		if(Mathf.Approximately(finalYSize,1.0f) || finalYSize<0.1f)
		{
		//	finalYSize=1.0f;
		//	divY=1.0f;
		//	lowestPointY=-1.0f;
		}
		
		if(Mathf.Approximately(finalYSize,0.0f)) { finalYSize = 0.01f; divY = 0.1f; lowestPointY = 0.0f; }
		
		if(Mathf.Approximately(finalZSize,1.0f) || finalZSize<1.0f)
		{
			if(finalZSize<1.0f)
			{
				divZ=1.0f;
				lowestPointZ=-1.0f;
			}

			finalZSize=1.0f;
		}
		behindGO.transform.localScale = objScale;
		((BoxCollider)behindGO.GetComponent(typeof(BoxCollider))).size = new Vector3(finalXSize,finalYSize,finalZSize);
		((BoxCollider)behindGO.GetComponent(typeof(BoxCollider))).center = new Vector3(finalXSize/divX+lowestPointX,finalYSize/divY+lowestPointY,finalZSize/divZ+lowestPointZ);
		
		if(fixedTileSnap!=0f)
		{
			((BoxCollider)behindGO.GetComponent(typeof(BoxCollider))).size = new Vector3(fixedTileSnap,fixedTileSnap,fixedTileSnap);
			((BoxCollider)behindGO.GetComponent(typeof(BoxCollider))).center = new Vector3(0,0,0);
		}
		
		uteCustomSnapSize customSizeSnap = obj.GetComponent<uteCustomSnapSize>();

		if(customSizeSnap)
		{
			if(customSizeSnap.ignoreObjectScale)
			{
				((BoxCollider)behindGO.GetComponent(typeof(BoxCollider))).size = customSizeSnap.ColliderSize;
			}
			else
			{
				((BoxCollider)behindGO.GetComponent(typeof(BoxCollider))).size = new Vector3(customSizeSnap.ColliderSize.x/behindGO.transform.localScale.x,customSizeSnap.ColliderSize.y/behindGO.transform.localScale.y,customSizeSnap.ColliderSize.z/behindGO.transform.localScale.z);
			}

			if(customSizeSnap.ignoreObjectScale)
			{
				((BoxCollider)behindGO.GetComponent(typeof(BoxCollider))).center = customSizeSnap.ColliderCenter;
			}
			else
			{
				((BoxCollider)behindGO.GetComponent(typeof(BoxCollider))).center = new Vector3(customSizeSnap.ColliderCenter.x/behindGO.transform.localScale.x,customSizeSnap.ColliderCenter.y/behindGO.transform.localScale.y,customSizeSnap.ColliderCenter.z/behindGO.transform.localScale.z);
			}
		}

		DisableAllExternalColliders(obj);

		List<GameObject> twoGO = new List<GameObject>();
		twoGO.Add(behindGO);
		twoGO.Add(obj);

		return twoGO;
	}

	public void CancelCurrentTile()
	{
		RuntimeEngine.CancelCurrentTile();
	}

	public void RotateCurrentTileRight()
	{
		StartCoroutine(RuntimeEngine.SmoothTileRotate(1));
	}

	public void RotateCurrentTileLeft()
	{
		StartCoroutine(RuntimeEngine.SmoothTileRotate(3));
	}

	public void RotateCurrentTileUp()
	{
		StartCoroutine(RuntimeEngine.SmoothTileRotate(0));
	}

	public void RotateCurrentTileDown()
	{
		StartCoroutine(RuntimeEngine.SmoothTileRotate(2));
	}

	public void RotateCurrentTileFlip()
	{
		StartCoroutine(RuntimeEngine.SmoothTileRotate(4));
	}

	public GameObject GetCurrentSelectedObject()
	{
		return RuntimeEngine.GetCurrentSelectedObject();
	}

	public GameObject GetCurrentTile()
	{
		return RuntimeEngine.currentTile;
	}

	public bool DestroyCurrentSelectedObject()
	{
		return RuntimeEngine.DestroyCurrentSelectedObject();
	}

	private void InitAllTilesRuntime()
	{		
		for(int i=0;i<allCategoriesRuntimeStr.Count;i++)
		{
			string catPart = allCategoriesRuntimeStr[i];

			if(!catPart.Equals(""))
			{
				string[] catParts = catPart.Split("$"[0]);

				string cName = catParts[0];
				string[] cTiles = catParts[1].Split(":"[0]);
				string cType = catParts[2];

				List<Tile> listTiles = new List<Tile>();

				for(int j=0;j<cTiles.Length;j++)
				{
					string cTile = cTiles[j];

					if(!cTile.Equals(""))
					{
						GameObject _gogo = null;
						int index = 0;

						for(int k=0;k<tilesRefDIC_STR.Count;k++)
						{
							if(tilesRefDIC_STR[k].Equals(cTile))
							{
								_gogo = tilesRefDIC_OBJ[k];
								index = k;
								break;
							}
						}

						if(_gogo)
						{
							listTiles.Add(new Tile(cTile,_gogo,tilesRefDIC_TEX[index],_gogo.name,"title"));
						}
						else
						{
							Debug.Log("[RuntimeEditor] Warning: Tile not found. Make sure you hit 'Prepare for Runtime' button on uteRuntimeBuilder Component.");
						}
					}
				}

				allCategoriesRuntime.Add(new Category(listTiles,cName,cType));
			}
		}

		runtimeTilesParent = new GameObject("uteRUNTIME_TILES");
		runtimeTilesStatic = new GameObject("uteRUNTIME_TILES_STATIC");
		runtimeTilesDynamic = new GameObject("uteRUNTIME_TILES_DYNAMIC");

		runtimeTilesStatic.transform.parent = runtimeTilesParent.transform;
		runtimeTilesDynamic.transform.parent = runtimeTilesParent.transform;

		for(int i=0;i<allCategoriesRuntime.Count;i++)
		{
			Category cat = allCategoriesRuntime[i];

			for(int j=0;j<cat.allTiles.Count;j++)
			{
				GameObject newTile = (GameObject) Instantiate(cat.allTiles[j].ref_obj,Vector3.zero,cat.allTiles[j].ref_obj.transform.rotation);
				newTile.name = cat.allTiles[j].guidid;
				//newTile.transform.parent = runtimeTilesParent.transform;

				List<GameObject> twoGO = new List<GameObject>();
				twoGO = createColliderToObject(newTile,cat.allTiles[j].ref_obj);

				GameObject behindGO = (GameObject) twoGO[0];
				behindGO.name = newTile.name;
				GameObject objGO = (GameObject) twoGO[1];
				cat.allTiles[j].ref_obj = objGO;
				cat.allTiles[j].ref_obj.transform.parent = behindGO.transform;
				behindGO.layer = 2;

				if(cat.type.Equals("Dynamic"))
				{
					behindGO.transform.parent = runtimeTilesDynamic.transform;
				}
				else if(cat.type.Equals("Static"))
				{
					behindGO.transform.parent = runtimeTilesStatic.transform;
				}
				else
				{
					Debug.LogError("Can't Happen!");
				}

				cat.allTiles[j].SetMainObject(behindGO);
			}
		}

		runtimeTilesParent.transform.position = new Vector3(-10000,-10000,-10000);
	}

	public void PlaceCurrentTileAtPosition(Vector3 position, Vector3 rotation)
	{
		if(GetCurrentTile())
		{
			RuntimeEngine.ApplyBuild(GetCurrentTile(),position,"","",rotation);
		}
	}

	public void PlaceCurrentTileAtPosition(Vector3 position)
	{
		if(GetCurrentTile())
		{
			RuntimeEngine.ApplyBuild(GetCurrentTile(),position,"","",GetCurrentTile().transform.localEulerAngles);
		}
	}

	public void PlaceCurrentTileAtCurrentPosition()
	{
		if(GetCurrentTile())
		{
			RuntimeEngine.ApplyBuild(GetCurrentTile(),GetCurrentTile().transform.position,"","",GetCurrentTile().transform.localEulerAngles);
		}
	}

	public void MassBuildHeightUp()
	{
		RuntimeEngine.MassBuildStepUp();
	}

	public void MassBuildHeightDown()
	{
		RuntimeEngine.MassBuildStepDown();
	}

	public void MassBuildResetHeight()
	{
		RuntimeEngine.MassBuildStepOne();
	}

	public void MassBuildCancel()
	{
		RuntimeEngine.MassBuildCancel();
	}

	public Category GetCategoryByCategoryName(string catName)
	{
		for(int i=0;i<allCategoriesRuntime.Count;i++)
		{
			if(allCategoriesRuntime[i].name.Equals(catName))
			{
				return allCategoriesRuntime[i];
			}
		}

		Debug.LogError("Category with name '"+catName+"' was not found!");

		return null;
	}

	private string ValidateMapName(string mapName)
	{
#if UNITY_EDITOR
		if(mapName.Contains(":")||mapName.Contains("/")||mapName.Contains("\\")||mapName.Contains("'")||mapName.Contains("\"")||mapName.Contains(".")||mapName.Contains("*"))
		{
			Debug.Log("Some illegal characters where used in the map name, they were stripped (illegal characters: : / \\ ' \" . *)");
		}
#endif

		mapName = mapName.Replace(":","");
		mapName = mapName.Replace("/","");
		mapName = mapName.Replace("\\","");
		mapName = mapName.Replace("'","");
		mapName = mapName.Replace("\"","");
		mapName = mapName.Replace(".","");
		mapName = mapName.Replace("*","");

		return mapName;
	}

	public string SaveMap(string mapName)
	{
		mapName = ValidateMapName(mapName);
		AddMapToList(mapName);
		string allInfo = "";
		GameObject ALL_TILES = (GameObject) GameObject.Find("uteMAP_ALLTILES");

		uteTagObject[] allObjsD = (uteTagObject[]) ALL_TILES.GetComponentsInChildren<uteTagObject>();

		for(int i=0;i<allObjsD.Length;i++)
		{
			uteTagObject objDTag = (uteTagObject) allObjsD[i];

			if(objDTag)
			{
				GameObject goD = objDTag.gameObject;
				string goDID = goD.name;

				goDID = goDID.Replace("(Clone)","");
				goDID = goDID.Replace(" ","");

				Vector3 goDPos = goD.transform.position;
				Vector3 goDRot = goD.transform.localEulerAngles;
				string goDisStatic = GetStringFromBool(goD.isStatic);

				if(goDID.Equals(""))
				{
					goDID = goD.name;
					Debug.Log(goDID);
				}

				allInfo += goDID+":"+goDPos.x+":"+goDPos.y+":"+goDPos.z+":"+goDRot.x+":"+goDRot.y+":"+goDRot.z+":"+goDisStatic+":$";
			}
		}

		string fpth = uteGLOBAL3dMapEditor.getRuntimeMapDir()+mapName+".txt";
		string fpthC = uteGLOBAL3dMapEditor.getRuntimeMapDir();

		if(Directory.Exists(fpthC))
		{
			StreamWriter sw = new StreamWriter(fpth);
			sw.Write("");
			sw.Write(allInfo);
			sw.Flush();
			sw.Close();

			return allInfo;
		}
		else
		{
			Debug.LogError("[Runtime Builder] Error saving map. Make sure you click on 'Prepare for Runtime' on uteRuntimeBuilder script before going runtime.");
		}

		return "";
	}

	public bool LoadMapWithData(string mapData, bool loadAdditive = false)
	{
		GameObject ALLT;
		GameObject ALLD;
		GameObject ALLS;
		
		ALLD = (GameObject) GameObject.Find("uteMAP_ALLTILES/uteMAP_DYNAMIC");
		ALLS = (GameObject) GameObject.Find("uteMAP_ALLTILES/uteMAP_STATIC");

		if(!loadAdditive)
		{
			ALLT = (GameObject) GameObject.Find("uteMAP_ALLTILES");

			Destroy(ALLD);
			Destroy(ALLS);

			ALLS = new GameObject("uteMAP_STATIC");
			ALLD = new GameObject("uteMAP_DYNAMIC");

			ALLS.transform.parent = ALLT.transform;
			ALLD.transform.parent = ALLT.transform; 
		}

		if(mapData!="")
		{
			string allInfo = mapData;

			string[] mapParts = allInfo.Split("$"[0]);

			for(int i=0;i<mapParts.Length;i++)
			{
				string mapPart = mapParts[i];

				if(!mapPart.Equals(""))
				{
					string[] allParts = mapPart.Split(":"[0]);
					string id = allParts[0];
					float posX = (float) System.Convert.ToSingle(allParts[1]);
					float posY = (float) System.Convert.ToSingle(allParts[2]);
					float posZ = (float) System.Convert.ToSingle(allParts[3]);
					float rotX = (float) System.Convert.ToSingle(allParts[4]);
					float rotY = (float) System.Convert.ToSingle(allParts[5]);
					float rotZ = (float) System.Convert.ToSingle(allParts[6]);
					string isS = allParts[7];


					GameObject tileRef = GetTileMainObjectByGUIDID(id);

					GameObject newTile = null;

					if(tileRef!=null)
					{
						newTile = (GameObject) Instantiate(tileRef,new Vector3(posX,posY,posZ),Quaternion.identity);
						newTile.name = id;
						newTile.transform.localEulerAngles = new Vector3(rotX,rotY,rotZ);
						newTile.layer = 0;
						newTile.AddComponent<uteTagObject>();
						
						if(isS.Equals("1"))
						{
							newTile.transform.parent = ALLS.transform;
						}
						else
						{
							newTile.transform.parent = ALLD.transform;
						}
					}
				}
			}

			return true;
		}
		else
		{
			Debug.Log("[Runtime Builder] Failed to load mapData.");

			return false;
		}
	}

	public bool LoadMap(string mapName, bool loadAdditive = false)
	{
		GameObject ALLT;
		GameObject ALLD;
		GameObject ALLS;
		
		ALLD = (GameObject) GameObject.Find("uteMAP_ALLTILES/uteMAP_DYNAMIC");
		ALLS = (GameObject) GameObject.Find("uteMAP_ALLTILES/uteMAP_STATIC");

		if(!loadAdditive)
		{
			ALLT = (GameObject) GameObject.Find("uteMAP_ALLTILES");

			Destroy(ALLD);
			Destroy(ALLS);

			ALLS = new GameObject("uteMAP_STATIC");
			ALLD = new GameObject("uteMAP_DYNAMIC");

			ALLS.transform.parent = ALLT.transform;
			ALLD.transform.parent = ALLT.transform; 
		}

		string fpth = uteGLOBAL3dMapEditor.getRuntimeMapDir()+mapName+".txt";

		if(File.Exists(fpth))
		{
			StreamReader sr = new StreamReader(uteGLOBAL3dMapEditor.getRuntimeMapDir()+mapName+".txt");
			string allInfo = sr.ReadToEnd();
			sr.Close();

			string[] mapParts = allInfo.Split("$"[0]);

			for(int i=0;i<mapParts.Length;i++)
			{
				string mapPart = mapParts[i];

				if(!mapPart.Equals(""))
				{
					string[] allParts = mapPart.Split(":"[0]);
					string id = allParts[0];
					float posX = (float) System.Convert.ToSingle(allParts[1]);
					float posY = (float) System.Convert.ToSingle(allParts[2]);
					float posZ = (float) System.Convert.ToSingle(allParts[3]);
					float rotX = (float) System.Convert.ToSingle(allParts[4]);
					float rotY = (float) System.Convert.ToSingle(allParts[5]);
					float rotZ = (float) System.Convert.ToSingle(allParts[6]);
					string isS = allParts[7];

					GameObject tileRef = GetTileMainObjectByGUIDID(id);

					GameObject newTile = null;

					if(tileRef!=null)
					{
						newTile = (GameObject) Instantiate(tileRef,new Vector3(posX,posY,posZ),Quaternion.identity);
						newTile.name = id;
						newTile.transform.localEulerAngles = new Vector3(rotX,rotY,rotZ);
						newTile.layer = 0;
						newTile.AddComponent<uteTagObject>();
						
						if(isS.Equals("1"))
						{
							newTile.transform.parent = ALLS.transform;
						}
						else
						{
							newTile.transform.parent = ALLD.transform;
						}
					}
				}
			}

			return true;
		}
		else
		{
			Debug.Log("[Runtime Builder] Map with this name does not exist.");

			return false;
		}
	}

	public Tile GetTileByID(int id)
	{
		for(int i=0;i<allCategoriesRuntime.Count;i++)
		{
			for(int j=0;j<allCategoriesRuntime[i].allTiles.Count;j++)
			{
				if(allCategoriesRuntime[i].allTiles[j].guidid.Equals(id.ToString()))
				{
					return allCategoriesRuntime[i].allTiles[j];
				}
			}
		}

		Debug.LogError("[RuntimeEngine] Tile not found.");
		
		return null;
	}

	public string[] GetMapNamesList()
	{
		StreamReader sr = new StreamReader(uteGLOBAL3dMapEditor.getRuntimeMapListPath());
		string info = sr.ReadToEnd();
		sr.Close();
		string[] parts = info.Split(":"[0]);
		int counter = 0;

		for(int i=0;i<parts.Length;i++)
		{
			string str = parts[i];

			if(!str.Equals(""))
			{
				counter++;
			}
		}

		string[] goodParts = new string[counter];

		for(int i=0;i<counter;i++)
		{
			goodParts[i] = parts[i];
		}

		return goodParts;
	}

	public void DeleteMap(string mapName)
	{
		StreamReader sr = new StreamReader(uteGLOBAL3dMapEditor.getRuntimeMapListPath());
		string maps = sr.ReadToEnd();
		sr.Close();

		string newMapList = "";
		string[] mapParts = maps.Split(":"[0]);

		for(int i=0;i<mapParts.Length;i++)
		{
			string mapPart = mapParts[i];

			if(!mapPart.Equals("")&&!mapPart.Equals(mapName))
			{
				newMapList += mapPart+":";
			}
		}

		StreamWriter sw = new StreamWriter(uteGLOBAL3dMapEditor.getRuntimeMapListPath());
		sw.Write("");
		sw.Write(newMapList);
		sw.Flush();
		sw.Close();

		string path = uteGLOBAL3dMapEditor.getRuntimeMapDir()+mapName+".txt";

		if(File.Exists(path))
		{
			File.Delete(path);
		}

		if(File.Exists(path+".meta"))
		{
			File.Delete(path+".meta");
		}
	}

	private string GetStringFromBool(bool b)
	{
		if(b)
		{
			return "1";
		}
		else
		{
			return "0";
		}
	}

	private void AddMapToList(string mapName)
	{
		if(Directory.Exists(uteGLOBAL3dMapEditor.getRuntimeMapDir()))
		{
			if(!CheckIfMapExists(mapName))
			{
				StreamReader sr = new StreamReader(uteGLOBAL3dMapEditor.getRuntimeMapListPath());
				string info = sr.ReadToEnd();
				sr.Close();

				info += mapName+":";

				StreamWriter sw = new StreamWriter(uteGLOBAL3dMapEditor.getRuntimeMapListPath());
				sw.Write("");
				sw.Write(info);
				sw.Flush();
				sw.Close();
			}
			else
			{
				Debug.Log("[Runtime Builder] Map with this name already exists. Please use DeleteMap before saving to same file.");
			}
		}
		else
		{
			Debug.LogError("[Runtime Builder] Error saving map. Make sure you click on 'Prepare for Runtime' on uteRuntimeBuilder script before going runtime.");
		}
	}

	public Tile GetTileFromCategoryByName(string categoryName, string tileName)
	{
		Category cat = GetCategoryByCategoryName(categoryName);

		for(int i=0;i<cat.allTiles.Count;i++)
		{
			if(cat.allTiles[i].name.Equals(tileName))
			{
				return cat.allTiles[i];
			}
		}

		Debug.LogError("Tile name ["+tileName+"] in category ["+categoryName+"] was not found.");

		return null;
	}

	public bool CheckIfMapExists(string mapName)
	{
		StreamReader sr = new StreamReader(uteGLOBAL3dMapEditor.getRuntimeMapListPath());
		string info = sr.ReadToEnd();
		sr.Close();

		string[] parts = info.Split(":"[0]);

		for(int i=0;i<parts.Length;i++)
		{
			string str = parts[i];

			if(!str.Equals(""))
			{
				if(str.Equals(mapName))
				{
					return true;
				}
			}
		}

		return false;
	}

	public List<Tile> GetTileListByCategoryName(string catName)
	{
		for(int i=0;i<allCategoriesRuntime.Count;i++)
		{
			if(allCategoriesRuntime[i].name.Equals(catName))
			{
				return allCategoriesRuntime[i].allTiles;
			}
		}

		Debug.LogError("Category with name '"+catName+"' was not found!");

		return null;
	}

	#if UNITY_EDITOR
	public void SetTiles()
	{
		ReadTiles();

		string allNewStuff = "";
		tilesRefDIC_STR.Clear();
		tilesRefDIC_OBJ.Clear();
		tilesRefDIC_TEX.Clear();

		for(int i=0;i<allCategories.Count;i++)
		{
			string catInfo = allCategories[i];
			string[] catDetails = catInfo.Split("$"[0]);

			string catName = catDetails[0];
			string[] catTiles = catDetails[2].Split(":"[0]);
			string catType = catDetails[3];

			allNewStuff+= catName+"$";

			for(int j=0;j<catTiles.Length-1;j++)
			{
				string catTile = catTiles[j];

				if(!catTile.Equals(""))
				{
					string opath = UnityEditor.AssetDatabase.GUIDToAssetPath(catTile);
					GameObject tGO = (GameObject) UnityEditor.AssetDatabase.LoadMainAssetAtPath(opath);

					if(tGO)
					{
						bool isCustomTexture = true;

						Texture2D texture = getTextureByObjectGUID(catTile);

						if(texture==null)
						{
							isCustomTexture = false;
							texture = AssetPreview.GetAssetPreview((Object)tGO);
						}

						if(texture)
						{
							if(!isCustomTexture)
							{
								Texture2D newTex = new Texture2D(texture.width,texture.height);
								newTex.LoadImage(texture.EncodeToPNG());
								tilesRefDIC_TEX.Add(newTex);
							}
							else
							{
								tilesRefDIC_TEX.Add(texture);
							}
						}
						else
						{
							AssetDatabase.ImportAsset(opath);
							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
							j--;
							continue;
						}
					
						catTiles[j] = getIDByGUID(catTile);
						tilesRefDIC_STR.Add(catTiles[j]);
						tilesRefDIC_OBJ.Add(tGO);
						allNewStuff += catTiles[j]+":";
						allTilesRef.Add(tGO);
					}
					else
					{
						Debug.Log(tGO+":"+catTile+":"+opath);
					}
				}
			}

			allNewStuff+="$"+catType+"$|";
		}

		if(allNewStuff.Equals(""))
		{
			Debug.Log("No Tiles to prepare.");
		}
		else
		{
			allCategoriesRuntimeStr.Clear();

			string[] allNewParts = allNewStuff.Split("|"[0]);

			for(int i=0;i<allNewParts.Length;i++)
			{
				string part = allNewParts[i];

				if(!part.Equals(""))
				{
					allCategoriesRuntimeStr.Add(part);
				}
			}
		}
	}

	private void ReadTiles()
	{
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		allCategories.Clear();
		allGUIDIDS.Clear();
		allTilesRef.Clear();

		string catGUID = uteGLOBAL3dMapEditor.uteCategoryInfotxt;
		string filePath = AssetDatabase.GUIDToAssetPath(catGUID);
		string catInfo = "";
		string guididspath = AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.uteGUIDIDtxt);
		string custompreviewspath = AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.utePrievstxt);

		StreamReader sr = new StreamReader(filePath);
		catInfo = sr.ReadToEnd();
		sr.Close();

		string[] allInfo = catInfo.Split("|"[0]);

		for(int i=0;i<allInfo.Length;i++)
		{
			string part = allInfo[i];

			if(!part.Equals(""))
			{
				allCategories.Add(allInfo[i]);
			}
		}

		StreamReader sr2 = new StreamReader(guididspath);
		string _allguidids = sr2.ReadToEnd();
		sr2.Close();
		string[] _allguidparts = _allguidids.Split("$"[0]);

		for(int i=0;i<_allguidparts.Length;i++)
		{
			string part = _allguidparts[i];

			if(!part.Equals(""))
			{
				allGUIDIDS.Add(part);
			}
		}

		StreamReader sr3 = new StreamReader(custompreviewspath);
		string _allcustompriews = sr3.ReadToEnd();
		sr3.Close();
		string[] _allcustompriewsparts = _allcustompriews.Split("$"[0]);

		for(int i=0;i<_allcustompriewsparts.Length;i++)
		{
			string part = _allcustompriewsparts[i];

			if(!part.Equals(""))
			{
				customPreviews.Add(part);
			}
		}

		Debug.Log("[RuntimeEngine] Prepare Done.");
	}

	private string getIDByGUID(string guid)
	{
		for(int i=0;i<allGUIDIDS.Count;i++)
		{
			string[] parts = allGUIDIDS[i].Split(":"[0]);

			if(parts[0].Equals(guid))
			{
				return parts[1];
			}
		}

		Debug.LogError("Can't happen!");

		return "";
	}

	private Texture2D getTextureByObjectGUID(string guid)
	{
		for(int i=0;i<customPreviews.Count;i++)
		{
			string[] parts = customPreviews[i].Split(":"[0]);

			if(parts[0].Equals(guid))
			{
				string path = AssetDatabase.GUIDToAssetPath(parts[1]);
				Texture2D _tex = (Texture2D) AssetDatabase.LoadMainAssetAtPath(path);
				return _tex;
			}
		}

		return null;
	}
	#endif
}