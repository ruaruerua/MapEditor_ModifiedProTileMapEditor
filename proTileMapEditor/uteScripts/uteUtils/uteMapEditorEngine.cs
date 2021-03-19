//#define ISDEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using static MapItemType;

#if UNITY_EDITOR
using UnityEditor;
#pragma warning disable 0219
#pragma warning disable 0162
#pragma warning disable 0414
#endif

public class uteMapEditorEngine : MonoBehaviour {
#if UNITY_EDITOR
	
	//private Texture2D debugTexture;
	// states
	[HideInInspector]
	public bool editorIsLoading;
	private bool erase;
	private int isRotated;
	private int isRotatedH;
	private bool is2D;
	private bool isOrtho;
	private bool canBuild;
	private bool isCurrentStatic;
	private bool isCamGridMoving;
	private bool isBuildingTC;
	private bool isContinuesBuild;
	private bool isSortingByNameEnabled;
	[HideInInspector]
	public bool isBuildingMass;
	private bool isMouseDown;
	private bool isInTopView;
	[HideInInspector]
	public bool isItNewPattern;
	[HideInInspector]
	public bool isItNewMap;
	[HideInInspector]
	public string newProjectName;
	[HideInInspector]
	public bool isItLoad;
	[HideInInspector]
	public GameObject mapLightGO;
	private bool isShowCameraOptions;
	private bool isTileRandomizerEnabled;
	private bool isTileRandomizerRotationEnabled;
	private bool isCommandPressed;
	private bool isControlPressed;
	[HideInInspector]
	public bool isUse360Snap;

	// info
	private string currentFilter;
	private string lastFilter;
	private string catInfoPath;
	private string currentTcFamilyName;
	private string settingsInfoPath;
	private int lastSelectedIndex;
	private int lastSelectedIndexTC;
	private float globalYSize;
	private string cameraType;
	private string sortType;
	private int globalGridSizeX;
	private int globalGridSizeZ;
	private string rightClickOption;
	public string yTypeOption;
	private Vector3 gridsize;
	private static float ZERO_F = .0f;
	private float offsetZ;
	private float offsetX;
	private float offsetY;
	private float lastPosX;
	private float lastPosY;
	private float lastPosZ;
	private float rotationOffset;
	private string currentObjectID;
	private uteTileConnectionsEngine uTCE;
	private uteMassBuildEngine uMBE;
	private uteExporter exporter;
	private uteHelpBox uteHelp;
	private uteUndoSystem UndoSystem;
	[HideInInspector]
	public uteOptionsBox uteOptions;
	private uteLM _uteLM;
	private uteLayersEngine LayersEngine;
	private int currentLayer;
	private string currentObjectGUID;
	private GUISkin ui;
	private Vector3 startingCameraTransform;
	private int currentTCID;
	private List<int> rotationList = new List<int>();
	[HideInInspector]
	public GameObject grid3d;
	private float grid3dScale;
	private float fixedTileSnap;
	private bool passSaveA;
	private bool passSaveB;

	// load camera info
	private Vector3 loadCameraPosition;
	private Vector3 loadCameraRotation;

	// map objects
	private GameObject MAP;
	private GameObject MAP_STATIC;
	private GameObject MAP_DYNAMIC;
	private GameObject MAIN;
	private GameObject cameraGO;
	private List<catInfo> allTiles = new List<catInfo>();
	private List<GameObject> catGoes = new List<GameObject>();
	private List<Texture2D> catPrevs = new List<Texture2D>();
	private List<string> catNames = new List<string>();
	private List<string> catGuids = new List<string>();
	private List<string> catLayers = new List<string>();
	private List<tcInfo> allTCTiles = new List<tcInfo>();
	private List<GameObject> tcGoes = new List<GameObject>();
	private List<Texture2D> tcPrevs = new List<Texture2D>();
	private List<string> tcNames = new List<string>();
	private List<string> tcGuids = new List<string>();
	private List<string> tcRots = new List<string>();
	private GameObject currentTile;
	private GameObject lastTile;
	[HideInInspector]
	public GameObject grid;
	private GameObject hitObject;
	private bool isAltPressed;
	private float cameraSensitivity;
	private Vector3 customTileOffset = Vector3.zero;

	// UI objects
	private Texture2D previewT;
	private Texture2D previewObjTexture;
	private Texture2D previewTTC;
	private Texture2D previewObjTextureTC;
	private uteComboBox comboBoxControl = new uteComboBox();
	private uteComboBox comboBoxForTileConnectionControl = new uteComboBox();
	private GUIContent[] comboBoxList;
	private GUIContent[] comboBoxList_TileConnections;
	private GUIStyle listStyle = new GUIStyle();
	private Rect GUIComboBoxCollider;
	private Rect GUIComboBoxColliderTC;
	private Rect GUIObjsCollider;
	private Rect GUIObjsColliderTC;
	private Rect checkGUIObjsCollider;
	private Rect checkGUIObjsColliderTC;
	private Rect cameraOptionsRect;
	private Vector2 scrollPosition = new Vector2(0,0);
	private Vector2 scrollPositionTC = new Vector3(0,0);
	private Vector2 normalMousePosition;
	private List<GameObject> undo_objs = new List<GameObject>();
	private List<Vector3> undo_poss = new List<Vector3>();
	private List<Vector3> undo_rots = new List<Vector3>();
	private List<string> undo_guids = new List<string>();
	private uteHints _hints;
	private bool _isDecorationDrop = false;
	private List<int> DecorationList = new List<int>();

	// finder	
	private bool isFindTilePressed;
	private GameObject currentFindObject;

	// helpers
	private bool eraserIsEnabled;
	private bool isShowLineHelpers;
	private LineRenderer[] helpers_LINES;
	private GameObject helpers_CANTBUILD;
	private GameObject helpers_CANBUILD;

	// other
	private uteCameraMove cameraMove;
	[HideInInspector]
	public uteSaveMap saveMap;

	public class catInfo : System.IDisposable
	{
		public string catName;
		public string catLayer;
		public string catCollision;
		public List<GameObject> catObjs = new List<GameObject>();
		public List<Texture2D> catObjsPrevs = new List<Texture2D>();
		public List<string> catObjsNames = new List<string>();
		public List<string> catGuidNames = new List<string>();
			
		public catInfo(string _catName, string _catLayer, string _catCollision)
		{
			// for clone
			catName = _catName;
			catLayer = _catLayer;
			catCollision = _catCollision;
		}

		public catInfo(string _catName, string _catLayer, List<string> _catObjsNames, string _catCollision, List<GameObject> _catObjs, List<Texture2D> _catObjsPrevs, List<string> _catGuidNames)
		{
			catName = _catName;
			catLayer = _catLayer;
			catCollision = _catCollision;
			catObjs = _catObjs;
			catObjsPrevs = _catObjsPrevs;
			catObjsNames = _catObjsNames;
			catGuidNames = _catGuidNames;
		}

		public void Dispose()
		{
		}
	}

	public class tcInfo
	{
		public string tcName;
		public List<GameObject> tcObjs = new List<GameObject>();
		public List<Texture2D> tcObjsPrevs = new List<Texture2D>();
		public List<string> tcObjsNames = new List<string>();
		public List<string> tcGuidNames = new List<string>();
		public List<string> tcRotsNames = new List<string>();

		public tcInfo(string _tcName, List<string> _tcObjsNames, List<GameObject> _tcObjs, List<Texture2D> _tcObjsPrevs, List<string> _tcGuidNames, List<string> _tcRotsNames)
		{
			tcName = _tcName;
			tcObjsNames = _tcObjsNames;
			tcObjs = _tcObjs;
			tcObjsPrevs = _tcObjsPrevs;
			tcGuidNames = _tcGuidNames;
			tcRotsNames = _tcRotsNames;
		}
	}

	private void Awake()
	{
		uteGLOBAL3dMapEditor.isEditorRunning = true;
	}
	
	public IEnumerator uteInitMapEditorEngine()
	{
		rotationList.Add(0);
		rotationList.Add(90);
		rotationList.Add(180);
		rotationList.Add(270);
		
		isFindTilePressed = false;
		currentFindObject = null;
		passSaveA = false;
		passSaveB = false;
		isTileRandomizerEnabled = false;
		isTileRandomizerRotationEnabled = false;
		eraserIsEnabled = false;
		isAltPressed = false;
		isInTopView = false;
		currentTCID = 0;
		lastTile = null;
		currentLayer = 0;
		isContinuesBuild = false;
		isMouseDown = false;
		isBuildingTC = false;
		isShowCameraOptions = false;
		isCamGridMoving = false;
		isBuildingMass = false;
		isCommandPressed = false;
		isControlPressed = false;
		uteGLOBAL3dMapEditor.canBuild = true;
		editorIsLoading = true;
		canBuild = false;
		cameraGO = (GameObject) GameObject.Find("MapEditorCamera");
		uteGLOBAL3dMapEditor.isEditorRunning = true;
		cameraOptionsRect = new Rect(Screen.width-505,Screen.height-70,410,30);
		uteHelp = (uteHelpBox) this.gameObject.AddComponent<uteHelpBox>();
		ui = (GUISkin) Resources.Load("uteForEditor/uteUI");
		catInfoPath = AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.uteCategoryInfotxt);
		settingsInfoPath = AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.uteSettingstxt);
		GUIComboBoxCollider = new Rect(10, 0, 200, 30);
		GUIComboBoxColliderTC = new Rect(10,0,200,30);

		gridsize = new Vector3(1000.0f,0.1f,1000.0f);
		previewObjTexture = new Texture2D(25,25);
		previewObjTextureTC = new Texture2D(25,25);
		erase = false;
		currentTcFamilyName = "";
		currentObjectGUID = "";
			
		currentFilter = "";
		lastFilter = "_______!@#$%*&(*^*&(^*&";

		MAP = new GameObject("MAP");
		MAP_DYNAMIC = new GameObject("MAP_DYNAMIC");
		MAP_DYNAMIC.transform.parent = MAP.transform;
		MAP_STATIC = new GameObject("MAP_STATIC");
		MAP_STATIC.transform.parent = MAP.transform;
		MAP_STATIC.isStatic = true;
		mapLightGO = (GameObject) Instantiate((GameObject) Resources.Load("uteForEditor/uteMapLight"));
		mapLightGO.name = "MapLight";
		GameObject tempGO = new GameObject();
		tempGO.name = "TEMP";

		LayersEngine = (uteLayersEngine) this.gameObject.AddComponent<uteLayersEngine>();
		LayersEngine.ReadLayersFromMap(newProjectName,MAP_STATIC,MAP_DYNAMIC,this);

		previewT = new Texture2D(2,2);
		previewTTC = new Texture2D(2,2);

		isShowLineHelpers = false;
		GameObject help = (GameObject) Instantiate((GameObject)Resources.Load("uteForEditor/uteHELPERS"),Vector3.zero,Quaternion.identity);
		help.name = "uteHELPERS";
		helpers_CANTBUILD = GameObject.Find("uteHELPERS/CANTBUILD");
		helpers_CANBUILD = GameObject.Find("uteHELPERS/CANBUILD");
		InitHelperLines();

		_hints = (uteHints) this.gameObject.AddComponent<uteHints>();
		uTCE = (uteTileConnectionsEngine) this.gameObject.AddComponent<uteTileConnectionsEngine>();
		uMBE = (uteMassBuildEngine) this.gameObject.AddComponent<uteMassBuildEngine>();

		yield return StartCoroutine(ReloadTileAssets());

		uteOptions = (uteOptionsBox) this.gameObject.AddComponent<uteOptionsBox>();
		saveMap.options = uteOptions;

		UndoSystem = (uteUndoSystem) this.gameObject.AddComponent<uteUndoSystem>();

		StartCoroutine(EraserHandler());

		if(isItLoad)
		{
			_uteLM = this.gameObject.AddComponent<uteLM>();
			_uteLM.uMEE = this;
			yield return StartCoroutine(_uteLM.LoadMap(newProjectName,MAP_STATIC,MAP_DYNAMIC,isItNewMap));
			yield return StartCoroutine(LoadCameraInfo());
			yield return StartCoroutine(uteOptions.LoadOptions());
			yield return StartCoroutine(AssignLayersToTiles());
			editorIsLoading = false;
			LayersEngine.CheckIfAllTilesHasExistingLayer();
		}
		else
		{
			editorIsLoading = false;
		}

		if(!isItLoad)
		{
			yield return StartCoroutine(saveMap.SaveMap(newProjectName,isItNewMap));
		}

		if(_hints.isShowHintsAtAll)
		{
			StartCoroutine(ShowHintSoon());
		}

		yield return 0;
	}
	
	private IEnumerator ShowHintSoon()
	{
		yield return new WaitForSeconds(1.0f);

		_hints.ShowRandomHint();

		yield return 0;
	}

	private IEnumerator AssignLayersToTiles()
	{

		yield return 0;
	}

	private IEnumerator LoadCameraInfo()
	{
		string path = uteGLOBAL3dMapEditor.getMapsDir();

		if(!System.IO.File.Exists(path+newProjectName+"_info.txt"))
		{
			yield return StartCoroutine(saveMap.SaveMap(newProjectName,isItNewMap));
		}
		
		if(System.IO.File.Exists(path+newProjectName+"_info.txt"))
		{
			StreamReader sr = new StreamReader(path+newProjectName+"_info.txt");
			string info = sr.ReadToEnd();
			sr.Close();

			GameObject _YArea = (GameObject) GameObject.Find("MAIN/YArea");

			if(info!="")
			{
				string[] allinfo = info.Split(":"[0]);
				// main pos
				float pX = System.Convert.ToSingle(allinfo[0]);
				float pY = System.Convert.ToSingle(allinfo[1]);
				float pZ = System.Convert.ToSingle(allinfo[2]);
				// main rot
				float _main_rX = System.Convert.ToSingle(allinfo[3]);
				float _main_rY = System.Convert.ToSingle(allinfo[4]);
				float _main_rZ = System.Convert.ToSingle(allinfo[5]);
				// yarea rot
				float _yarea_rX = System.Convert.ToSingle(allinfo[6]);
				float _yarea_rY = System.Convert.ToSingle(allinfo[7]);
				float _yarea_rZ = System.Convert.ToSingle(allinfo[8]);
				// mapeditorcamera rot
				float _mapeditorcamera_rX = System.Convert.ToSingle(allinfo[9]);
				float _mapeditorcamera_rY = System.Convert.ToSingle(allinfo[10]);
				float _mapeditorcamera_rZ = System.Convert.ToSingle(allinfo[11]);

				MAIN.transform.position = new Vector3(pX,pY,pZ);
				MAIN.transform.localEulerAngles = new Vector3(_main_rX,_main_rY,_main_rZ);
				_YArea.transform.localEulerAngles = new Vector3(_yarea_rX,_yarea_rY,_yarea_rZ);
				cameraGO.transform.localEulerAngles = new Vector3(_mapeditorcamera_rX,_mapeditorcamera_rY,_mapeditorcamera_rZ);
			}
		}

		yield return 0;
	}

	private IEnumerator EraserHandler()
	{
		while(true)
		{
			if(eraserIsEnabled)
			{
				Ray eraseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit eraseHit;
				
				if(Physics.Raycast(eraseRay, out eraseHit, 1000))
				{
					if(!eraseHit.collider.gameObject.name.Equals("Grid"))
					{
						UndoSystem.AddToUndoDestroy(eraseHit.collider.gameObject,eraseHit.collider.gameObject.name,eraseHit.collider.gameObject.transform.position,eraseHit.collider.gameObject.transform.localEulerAngles,true,eraseHit.collider.gameObject.transform.parent.gameObject);
						//Destroy (eraseHit.collider.gameObject);
						uteGLOBAL3dMapEditor.mapObjectCount--;
						hitObject = null;
						helpers_CANTBUILD.transform.position = new Vector3(-1000000.0f,0.0f,-1000000.0f);
					}
				}
			}

			yield return new WaitForSeconds(0.03f);
		}

		yield return 0;
	}

	private void Update()
	{
		if(editorIsLoading)
			return;

		if(isItNewPattern)
		{
			if(exporter.isShow)
			{
				return;
			}
		}

		if(LayersEngine.showLayerOptions)
		{
			return;
		}
		else if(LayersEngine.isCreatingNewLayer)
		{
			return;
		}
		else if(LayersEngine.isMergingLayers)
		{
			return;
		}

		if(uteHelp.isShow)
		{
			return;
		}

		if(uteOptions.isShow)
		{
			return;
		}
		
		if(!CheckGUIPass())
		{
			return;
		}

		if(isItLoad)
		{
			if(!_uteLM.isMapLoaded)
			{
				return;
			}
		}

		if(isCamGridMoving)
		{
			return;
		}

		if(isShowCameraOptions)
		{
			cameraOptionsRect = new Rect(Screen.width-505,Screen.height-70,410,30);
		}

		if(Input.GetKeyDown(KeyCode.LeftCommand))
		{
			isCommandPressed = true;
		}

		if(Input.GetKeyUp(KeyCode.LeftCommand))
		{
			isCommandPressed = false;
		}

		if(Input.GetKeyDown(KeyCode.LeftControl))
		{
			isControlPressed = true;
		}

		if(Input.GetKeyUp(KeyCode.LeftControl))
		{
			isControlPressed = true;
		}

		if(Input.GetKeyDown(KeyCode.LeftAlt))
		{
			isAltPressed = true;
		}

		if(Input.GetKeyUp(KeyCode.LeftAlt))
		{
			isAltPressed = false;
		}

		if(Input.GetKeyDown(KeyCode.F))
		{
			isFindTilePressed = true;
		}

		if(Input.GetKeyUp(KeyCode.F))
		{
			isFindTilePressed = false;
		}

		if(isFindTilePressed)
		{
			if(currentTile)
			{
				currentTile.transform.position = new Vector3(-10000f,-10000f,-10000f);
			}

			Ray findRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit findHit;

			if(Physics.Raycast(findRay,out findHit, 1000))
			{
				if(findHit.collider.gameObject.GetComponent<uteTagObject>())
				{
					currentFindObject = findHit.collider.gameObject;
				}
				else
				{
					currentFindObject = null;
				}
			}
			else
			{
				currentFindObject = null;
			}

			if(Input.GetMouseButtonDown(1))
			{
				if(currentFindObject)
				{
					FindAndSelectTileInAllCats(currentFindObject,true);
				}
				else
				{
					currentFilter = "";
				}
			}

			if(Input.GetMouseButtonDown(0))
			{
				if(currentFindObject)
				{
					FindAndSelectTileInAllCats(currentFindObject,false);
				}
			}
		}

		if(isFindTilePressed)
		{
			return;
		}

		if(isAltPressed)
		{
			return;
		}

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			currentFilter = "";

			if(currentTile)
			{
				Destroy(currentTile);
				helpers_CANTBUILD.transform.position = new Vector3(-1000000.0f,0.0f,-1000000.0f);
				helpers_CANBUILD.transform.position = new Vector3(-1000000.0f,0.0f,-1000000.0f);
				ShowLineHelpers(isShowLineHelpers = false);
			}
		}

		if(erase)
		{
			if(Input.GetMouseButtonDown(0))
			{
				eraserIsEnabled = true;
				
			}

			if(Input.GetMouseButtonUp(0))
			{
				eraserIsEnabled = false;
			}

			if(Input.GetMouseButtonDown(1))
			{
				Ray eraseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit eraseHit;
				
				if(Physics.Raycast(eraseRay, out eraseHit, 1000))
				{
					if(!eraseHit.collider.gameObject.name.Equals("Grid"))
					{
						UndoSystem.AddToUndoDestroy(eraseHit.collider.gameObject,eraseHit.collider.gameObject.name,eraseHit.collider.gameObject.transform.position,eraseHit.collider.gameObject.transform.localEulerAngles,true,eraseHit.collider.gameObject.transform.parent.gameObject);
						//Destroy (eraseHit.collider.gameObject);
						uteGLOBAL3dMapEditor.mapObjectCount--;
						hitObject = null;
						helpers_CANTBUILD.transform.position = new Vector3(-1000000.0f,0.0f,-1000000.0f);
					}
				}
			}
		}

		if(!isCamGridMoving)
		{
			if(Input.GetKeyUp (KeyCode.C))
			{
				StartCoroutine(gridSmoothMove(grid,true,cameraMove.gameObject));
			}
			else if(Input.GetKeyUp (KeyCode.X))
			{
				StartCoroutine(gridSmoothMove(grid,false,cameraMove.gameObject));
			}

			if(Input.GetKeyDown(KeyCode.V))
			{
				if(isShowLineHelpers)
				{
					ShowLineHelpers(isShowLineHelpers = false);
				}
				else
				{
					ShowLineHelpers(isShowLineHelpers = true);
				}
			}

			if(Input.GetKeyDown(KeyCode.Tab))
			{
				SwitchBuildMode();
			}

			if(Input.GetKeyDown(KeyCode.R))
			{
				ResetCamera();
			}

			if(Input.GetKeyDown(KeyCode.T))
			{
				if(currentTile)
				{
					Destroy(currentTile);
					helpers_CANTBUILD.transform.position = new Vector3(-1000000.0f,0.0f,-1000000.0f);
					ShowLineHelpers(isShowLineHelpers = false);
				}
				
				erase = true;
			}
		}

		if(isCamGridMoving)
			return;

		if(isShowLineHelpers&&currentTile)
		{
			CalculateLineHelpers();
		}

		if(Input.GetKeyDown(KeyCode.LeftCommand))
		{
			passSaveA = true;
		}

		if(Input.GetKeyDown(KeyCode.S))
		{
			passSaveB = true;
		}

		if(Input.GetKeyUp(KeyCode.LeftCommand))
		{
			passSaveA = false;
		}

		if(Input.GetKeyDown(KeyCode.LeftControl))
		{
			passSaveA = true;
		}

		if(Input.GetKeyUp(KeyCode.LeftControl))
		{
			passSaveA = false;
		}

		if(Input.GetKeyUp(KeyCode.S))
		{
			passSaveB = false;
		}

		if(passSaveA&&passSaveB)
		{
			passSaveB = false;

			StartCoroutine(LayersEngine.ReSaveMap());

			#if UNITY_EDITOR
			Debug.Log("[MapEditor] SAVE COMPLETED");
			#endif
		}

		if(Input.GetMouseButtonUp(1)&&!erase)
		{
			float rot3D = 10.0f;

			if(rightClickOption.Equals("rotL")&&currentTile)
			{
				StartCoroutine(smoothRotate(currentTile,new Vector3(0.0f,-rot3D,0.0f),true));
			}
			else if(rightClickOption.Equals("rotR")&&currentTile)
			{
				StartCoroutine(smoothRotate(currentTile,new Vector3(0.0f,rot3D,0.0f),true));
			}
			else if(rightClickOption.Equals("rotU")&&currentTile)
			{
				StartCoroutine(smoothRotate(currentTile,new Vector3(-rot3D,0.0f,0.0f),false));
			}
			else if(rightClickOption.Equals("rotD")&&currentTile)
			{
				StartCoroutine(smoothRotate(currentTile,new Vector3(rot3D,0.0f,0.0f),false));
			}
			else if(rightClickOption.Equals("rotI")&&currentTile)
			{
				rot3D *=2;

				StartCoroutine(smoothRotate(currentTile,new Vector3(0.0f,0.0f,rot3D),false));
			}
			else if(rightClickOption.Equals("erase"))
			{
				Ray eraseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit eraseHit;
				
				if(Physics.Raycast(eraseRay, out eraseHit, 1000))
				{
					if(!eraseHit.collider.gameObject.name.Equals("Grid"))
					{
						UndoSystem.AddToUndoDestroy(eraseHit.collider.gameObject,eraseHit.collider.gameObject.name,eraseHit.collider.gameObject.transform.position,eraseHit.collider.gameObject.transform.localEulerAngles,true,eraseHit.collider.gameObject.transform.parent.gameObject);
						//Destroy (eraseHit.collider.gameObject);
						uteGLOBAL3dMapEditor.mapObjectCount--;
					}
				}
			}
		}

		if(Input.GetMouseButtonUp(0))
		{
			lastTile = null;
			isMouseDown = false;

			if(!erase)
			{
				if(isBuildingTC)
				{
					uTCE.FinishUp();
				}
			}
		}

		if(erase&&hitObject&&!hitObject.name.Equals("Grid"))
		{
			helpers_CANTBUILD.transform.position = hitObject.transform.position+new Vector3(hitObject.GetComponent<BoxCollider>().center.x*hitObject.transform.localScale.x,hitObject.GetComponent<BoxCollider>().center.y*hitObject.transform.localScale.y,hitObject.GetComponent<BoxCollider>().center.z*hitObject.transform.localScale.z);
			helpers_CANTBUILD.transform.localScale = hitObject.GetComponent<Collider>().bounds.size + new Vector3(0.1f,0.1f,0.1f);
			helpers_CANBUILD.transform.position = new Vector3(-10000,0,-10000);
		}
		else if(hitObject&&!hitObject.name.Equals("Grid"))
		{
			if(hitObject.GetComponent<BoxCollider>())
			{
				helpers_CANBUILD.transform.position = hitObject.transform.position+new Vector3(hitObject.GetComponent<BoxCollider>().center.x*hitObject.transform.localScale.x,hitObject.GetComponent<BoxCollider>().center.y*hitObject.transform.localScale.y,hitObject.GetComponent<BoxCollider>().center.z*hitObject.transform.localScale.z);
				helpers_CANBUILD.transform.localScale = hitObject.GetComponent<Collider>().bounds.size + new Vector3(0.1f,0.1f,0.1f);
			}
		}
		else
		{
			helpers_CANBUILD.transform.position = new Vector3(-10000,0,-10000);
		}

		if(currentTile||erase)
		{
			Ray buildRay = Camera.main.ScreenPointToRay(Input.mousePosition);
    		RaycastHit buildHit;

    		if(Physics.Raycast(buildRay,out buildHit, 1000))
    		{
    			if(buildHit.collider)
    			{
    				GameObject hitObj = buildHit.collider.gameObject;
    				hitObject = hitObj;

					if ((isContinuesBuild||isBuildingMass)&&!erase)
    				{
    					if(!buildHit.collider.gameObject.name.Equals("Grid"))
    					{
    						canBuild = false;
							bool skipThisIsTc = false;

							if(isBuildingMass&&isMouseDown)
							{
								if(buildHit.collider.gameObject.name.Equals("uteTcDummy(Clone)"))
								{
									Destroy(buildHit.collider.gameObject);
								}
							}
    						else if(buildHit.collider.gameObject.GetComponent<uteTcTag>()&&isBuildingTC)
    						{
    							if(buildHit.collider.gameObject.GetComponent<uteTcTag>().tcFamilyName==currentTcFamilyName)
    							{
	    							if(Input.GetMouseButtonDown(0))
	    							{
	    								uTCE.tcBuildStart(tcGoes,tcNames,tcGuids,currentTcFamilyName,tcRots,currentTCID);
	    								isMouseDown = true;
	    								//uTCE.AddTile(buildHit.collider.gameObject);
	    								Destroy(buildHit.collider.gameObject);
	    								skipThisIsTc = true;
	    							}
	    							else if(isMouseDown)
	    							{
	    								Destroy(buildHit.collider.gameObject);
	    								skipThisIsTc = true;
	    							}
	    						}
    						}

    						if(!skipThisIsTc)
    						{
    							if(buildHit.normal!=Vector3.up)
    							{
    								return;
    							}
    						}
    						else
    						{
    							return;
    						}
    					}
    				}

    				if(erase)
    				{
    					return;
    				}

    				bool collZB = false;
					bool collZA = false;
					bool collXB = false;
					bool collXA = false;
					bool collYB = false;
					bool collYA = false;
    				float sizeX = currentTile.GetComponent<Collider>().bounds.size.x;//*currentTile.transform.localScale.x;
    				float sizeY = currentTile.GetComponent<Collider>().bounds.size.y;//*currentTile.transform.localScale.y;
	  				float sizeZ = currentTile.GetComponent<Collider>().bounds.size.z;//*currentTile.transform.localScale.z;
    				float centerX = ((float)sizeX)/2.0f;
    				float centerY = ((float)sizeY)/2.0f;
    				float centerZ = ((float)sizeZ)/2.0f;
					float centerPosX = centerX+(currentTile.transform.position.x-((float)sizeX/2.0f));
					float centerPosY = centerY+(currentTile.transform.position.y-((float)sizeY/2.0f));
					float centerPosZ = centerZ+(currentTile.transform.position.z-((float)sizeZ/2.0f));
					int castSizeX = (int) currentTile.GetComponent<Collider>().bounds.size.x;
					int castSizeZ = (int) currentTile.GetComponent<Collider>().bounds.size.z;
					int castSizeSide;

					if(castSizeX==castSizeZ)
					{
						castSizeSide = castSizeX;
					}
					else if(castSizeX>castSizeZ)
					{
						castSizeSide = castSizeX;
					}
					else
					{
						castSizeSide = castSizeZ;
					}

					float normalX = ZERO_F;
					float normalZ = ZERO_F;
					float normalY = ZERO_F;


					if(buildHit.normal.y==ZERO_F)
					{
						if((int)buildHit.normal.x>ZERO_F)
						{
							normalX = 0.5f;
						}
						else if((int)buildHit.normal.x<ZERO_F)
						{
							normalX = -0.5f;
						}

						if((int)buildHit.normal.z>ZERO_F)
						{
							normalZ = 0.5f;
						}
						else if((int)buildHit.normal.z<ZERO_F)
						{
							normalZ = -0.5f;
						}
					}


					if(buildHit.normal.y>ZERO_F)
					{
						normalY = 0.5f;
					}
					else if(buildHit.normal.y<ZERO_F)
					{
						normalY = -0.5f;
					}

					float internalOffsetX = ZERO_F;
					float internalOffsetZ = ZERO_F;
					float internalOffsetY = ZERO_F;

					if(Mathf.Round(currentTile.GetComponent<Collider>().bounds.size.z)%2==0)
					{
						internalOffsetZ = 0.5f;
					}

					if(Mathf.Round(currentTile.GetComponent<Collider>().bounds.size.x)%2==0)
					{
						internalOffsetX = 0.5f;
					}

					if(Mathf.Round(currentTile.GetComponent<Collider>().bounds.size.y)%2==0)
					{
						internalOffsetY = 0.5f;
					}

					float offsetFixX = 0.05f;//*currentTile.transform.localScale.x;
					float offsetFixZ = 0.05f;//*currentTile.transform.localScale.z;
					float offsetFixY = 0.05f;//*currentTile.transform.localScale.y;
					float castPosX = currentTile.GetComponent<Collider>().bounds.center.x;//centerPosX+(currentTile.GetComponent<BoxCollider>().center.x*currentTile.transform.localScale.x);
					float castPosZ = currentTile.GetComponent<Collider>().bounds.center.z;//centerPosZ+(currentTile.GetComponent<BoxCollider>().center.z*currentTile.transform.localScale.z);
					float castPosY = currentTile.GetComponent<Collider>().bounds.center.y;//centerPosY+(currentTile.GetComponent<BoxCollider>().center.y*currentTile.transform.localScale.y);

					Vector3 castFullPos = new Vector3(castPosX,castPosY,castPosZ);
					Vector3 checkXA = new Vector3(castPosX+centerX-offsetFixX,castPosY,castPosZ);
					Vector3 checkXB = new Vector3(castPosX-centerX+offsetFixX,castPosY,castPosZ);
					Vector3 checkZA = new Vector3(castPosX,castPosY,castPosZ-offsetFixZ+centerZ);
					Vector3 checkZB = new Vector3(castPosX,castPosY,castPosZ+offsetFixZ-centerZ);
					Vector3 checkYA = new Vector3(castPosX,castPosY+centerY-offsetFixY,castPosZ);
					Vector3 checkYB = new Vector3(castPosX,castPosY-centerY+offsetFixY,castPosZ);

					#if ISDEBUG
					// debug
					Debug.DrawLine(castFullPos,checkXA,Color.red,ZERO_F,false);
					Debug.DrawLine(castFullPos,checkXB,Color.red,ZERO_F,false);
					Debug.DrawLine(castFullPos,checkZA,Color.green,ZERO_F,false);
					Debug.DrawLine(castFullPos,checkZB,Color.green,ZERO_F,false);
					Debug.DrawLine(castFullPos,checkYA,Color.blue,ZERO_F,false);
					Debug.DrawLine(castFullPos,checkYB,Color.blue,ZERO_F,false);
					#endif
									
					collXB = false;
					collXA = false;
					collZB = false;
					collZA = false;
					collYB = false;
					collYA = false;

					float offsetToMoveXA = ZERO_F;
					float offsetToMoveXB = ZERO_F;
					float offsetToMoveZA = ZERO_F;
					float offsetToMoveZB = ZERO_F;
					float offsetToMoveYA = ZERO_F;
					float offsetToMoveYB = ZERO_F;

					if(uteGLOBAL3dMapEditor.OverlapDetection)
					{
						RaycastHit lineHit;
						if(Physics.Linecast(castFullPos+new Vector3(0,0,offsetFixZ), checkZB, out lineHit))
						{
							offsetToMoveZB = Mathf.Abs(Mathf.Round((Mathf.Abs(Mathf.Abs(lineHit.point.z)-Mathf.Abs(currentTile.transform.position.z)))-(currentTile.GetComponent<Collider>().bounds.size.z/2.0f)));
							collZB = true;
						}

						if(Physics.Linecast(castFullPos+new Vector3(0,0,-offsetFixZ), checkZA, out lineHit))
						{
							offsetToMoveZA = Mathf.Abs(Mathf.Round((Mathf.Abs(Mathf.Abs(lineHit.point.z)-Mathf.Abs(currentTile.transform.position.z)))-(currentTile.GetComponent<Collider>().bounds.size.z/2.0f)));
							collZA = true;
						}

						if(Physics.Linecast(castFullPos+new Vector3(0,offsetFixY,0), checkYB, out lineHit))
						{
							offsetToMoveYB = Mathf.Abs(Mathf.Round((Mathf.Abs(Mathf.Abs(lineHit.point.y)-Mathf.Abs(currentTile.transform.position.y)))-(currentTile.GetComponent<Collider>().bounds.size.y/2.0f)));
							collYB = true;
						}

						if(Physics.Linecast(castFullPos+new Vector3(0,-offsetFixY,0), checkYA, out lineHit))
						{
							offsetToMoveYA = Mathf.Abs(Mathf.Round((Mathf.Abs(Mathf.Abs(lineHit.point.y)-Mathf.Abs(currentTile.transform.position.y)))-(currentTile.GetComponent<Collider>().bounds.size.y/2.0f)));
							collYA = true;
						}

						if(Physics.Linecast(castFullPos+new Vector3(offsetFixX,0,0), checkXB, out lineHit))
						{
							offsetToMoveXB = Mathf.Abs(Mathf.Round((Mathf.Abs(Mathf.Abs(lineHit.point.x)-Mathf.Abs(currentTile.transform.position.x)))-(currentTile.GetComponent<Collider>().bounds.size.x/2.0f)));
							collXB = true;
						}

						if(Physics.Linecast(castFullPos+new Vector3(-offsetFixX,0,0), checkXA, out lineHit))
						{
							offsetToMoveXA = Mathf.Abs(Mathf.Round((Mathf.Abs(Mathf.Abs(lineHit.point.x)-Mathf.Abs(currentTile.transform.position.x)))-(currentTile.GetComponent<Collider>().bounds.size.x/2.0f)));
							collXA = true;
						}

						bool fixingY = false;

						if(collYA||collYB)
						{
							if(collYA&&!collYB)
							{
								offsetY-=offsetToMoveYA;
								fixingY = true;
							}
							else if(collYB&&!collYA)
							{
								offsetY+=offsetToMoveYB;
								fixingY = true;
							}
						}

						if(!fixingY)
						{
							if(collZA||collZB)
							{
								if(collZA&&!collZB)
								{
									offsetZ-=offsetToMoveZA;
								}
								else if(collZB&&!collZA)
								{
									offsetZ+=offsetToMoveZB;
								}
							}
							
							if(collXA||collXB)
							{
								if(collXA&&!collXB)
								{
									offsetX-=offsetToMoveXA;
								}
								else if(collXB&&!collXA)
								{
									offsetX+=offsetToMoveXB;
								}
							}
						}
					}

					float Xpivot = 0.0f; 
					float Zpivot = 0.0f;

					if(uteGLOBAL3dMapEditor.CalculateXZPivot)
					{
						Xpivot = -currentTile.GetComponent<BoxCollider>().center.x;
						Zpivot = -currentTile.GetComponent<BoxCollider>().center.z;

						if(isRotated==1||isRotated==3)
						{
							float _xpivot = Xpivot;
							Xpivot = Zpivot;
							Zpivot = _xpivot;
						}
					}

					float posX = (Mathf.Round(((buildHit.point.x+normalX)))+internalOffsetX+Xpivot);//-(currentTile.GetComponent<BoxCollider>().center.x*currentTile.transform.localScale.x)));
					float posZ = (Mathf.Round(((buildHit.point.z+normalZ)))+internalOffsetZ+Zpivot);//-(currentTile.GetComponent<BoxCollider>().center.z*currentTile.transform.localScale.z)));
					float posY = 0.0f;

					if(yTypeOption.Equals("auto"))
					{
						if(buildHit.normal==Vector3.up)
						{
							// Debug.Log("("+buildHit.point.y+"+("+currentTile.GetComponent<Collider>().bounds.size.y+"/2.0f))-"+currentTile.GetComponent<BoxCollider>().center.y+"0.0000001f");;
							posY = (buildHit.point.y+(currentTile.GetComponent<Collider>().bounds.size.y/2.0f))-(currentTile.GetComponent<BoxCollider>().center.y*currentTile.transform.localScale.y)+0.000001f;//posY = (Mathf.Round(buildHit.point.y+normalY)+internalOffsetY);
							// Debug.Log(posY);
						}
						else
						{
							if(currentTile.name.Equals(buildHit.collider.gameObject.name))
							{
								posY = buildHit.collider.gameObject.transform.position.y;
							}
							else
							{
								posY = 0.1f * ((Mathf.Round((buildHit.point.y+normalY)*10.0f))+internalOffsetY);
							}
						}
					}
					else if(yTypeOption.Equals("nosnap"))
					{
						posY = buildHit.point.y+(currentTile.GetComponent<Collider>().bounds.size.y/2.0f)-currentTile.GetComponent<BoxCollider>().center.y+0.000001f;
					}
					else if(yTypeOption.Equals("fixed"))
					{
						posY = 0.1f * ((Mathf.Round((buildHit.point.y+normalY)*10.0f))+internalOffsetY);//posY = (Mathf.Round(buildHit.point.y+normalY)+internalOffsetY);
					}

					if(Mathf.Abs(lastPosX-posX)>0.09f||Mathf.Abs(lastPosY-posY)>0.09f||Mathf.Abs(lastPosZ-posZ)>0.09f)
					{
						offsetZ = ZERO_F;
						offsetX = ZERO_F;
						offsetY = ZERO_F;
						offsetToMoveXA = ZERO_F;
						offsetToMoveXB = ZERO_F;
						offsetToMoveZA = ZERO_F;
						offsetToMoveZB = ZERO_F;
						offsetToMoveYA = ZERO_F;
						offsetToMoveYB = ZERO_F;
					}

					lastPosX = posX;
    				lastPosY = posY;
    				lastPosZ = posZ;
    				
    				float finalPosY = posY+offsetY;// * 100.0f);
					// finalPosY = Mathf.Clamp(finalPosY,0f,float.MaxValue);
    				posX = (posX+offsetX);// * 100.0f);
    				posZ = (posZ+offsetZ);// * 100.0f);
					float addOnTop = 0.0f;

					if(yTypeOption.Equals("auto"))
					{
						if(currentTile.GetComponent<Collider>().bounds.size.y<0.011f)
						{
							addOnTop = 0.002f;
						}
					}

					if(isUse360Snap)
					{
						currentTile.transform.rotation = Quaternion.FromToRotation(Vector3.up, buildHit.normal);
					}

    				Vector3 calculatedPosition = new Vector3(posX,finalPosY+addOnTop,posZ);

    				if(uteGLOBAL3dMapEditor.XZsnapping==false)
    				{
    					calculatedPosition = new Vector3(buildHit.point.x+Xpivot,finalPosY+addOnTop,buildHit.point.z+Zpivot);
    				}

    				calculatedPosition += customTileOffset;

    				if(isUse360Snap)
    				{
    				//	calculatedPosition += new Vector3(0,0.5f,0);
    				}

    				cameraMove.sel = calculatedPosition;

    				if(((collZA&&collZB)||(collXA&&collXB)||(collYA&&collYB))||(!collZA&&!collZB&&!collXA&&!collXB&&!collYA&&!collYB))
    				{
    					currentTile.transform.position = calculatedPosition;
    				}

    				currentTile.transform.position = calculatedPosition;

    				if(((collZA&&collZB)||(collZA||collZB))||((collXA&&collXB)||(collXA||collXB))||((collYA&&collYB)||(collYA||collYB)))
					{
						canBuild = false;
					}
					else if(!uteGLOBAL3dMapEditor.canBuild)
					{
    					canBuild = true;
    				}
    				else
    				{	
    					canBuild = true;
    				}
    			}
    			else
    			{
    				canBuild = false;
    			}
    		}
    		else
    		{
    			canBuild = false;
    		}

    		if(canBuild&&!erase)
    		{
    			helpers_CANTBUILD.transform.position = new Vector3(-1000,0,-1000);

    			if(Input.GetMouseButtonDown(0))
    			{
    				isMouseDown = true;

    				if(!isContinuesBuild&&!isBuildingTC&&!isBuildingMass)
					{
						//当前选择的是基础地格，则直接替换
						Ray checkRay = Camera.main.ScreenPointToRay(Input.mousePosition);
						RaycastHit checkHit;
						GameObject _baseGrid = null;
						bool _needOverLoadBase = false;
						if (Physics.Raycast(checkRay, out checkHit, 1000))
						{
							if (buildHit.collider)
							{
								GameObject hitObj = buildHit.collider.gameObject;

								MapItemType baseTypeComponent = hitObj.GetComponentInChildren<MapItemType>();
								MapItemType curTypeComponent = currentTile.GetComponentInChildren<MapItemType>();
								if (baseTypeComponent != null)
								{
									if (baseTypeComponent.curItemType == ItemType.BaseGrid && curTypeComponent.curItemType == ItemType.BaseGrid)
									{
										_baseGrid = hitObj;
										_needOverLoadBase = true;
									}

								}
							} 
						}
						if (_needOverLoadBase)
						{
							ApplyBuild(null,_baseGrid.transform.position);
							UndoSystem.AddToUndoDestroy(_baseGrid, _baseGrid.name, _baseGrid.transform.position, _baseGrid.transform.localEulerAngles, true, _baseGrid.transform.parent.gameObject);
							//Destroy (_baseGrid);
							uteGLOBAL3dMapEditor.mapObjectCount--;
						}
						else
						{
							ApplyBuild();
						}
					}

    				if(isBuildingTC)
    				{
    					uTCE.tcBuildStart(tcGoes,tcNames,tcGuids,currentTcFamilyName,tcRots,currentTCID);
    				}

    				if(isBuildingMass)
    				{
    					uMBE.massBuildStart(currentTile,currentObjectID,currentObjectGUID);
    				}
	   			}
    		}
    		else
    		{
    			if(!isBuildingTC&&!erase)
    			{
    				helpers_CANTBUILD.transform.position = currentTile.transform.position+new Vector3(currentTile.GetComponent<BoxCollider>().center.x*currentTile.transform.localScale.x,currentTile.GetComponent<BoxCollider>().center.y*currentTile.transform.localScale.y,currentTile.GetComponent<BoxCollider>().center.z*currentTile.transform.localScale.z);
					helpers_CANTBUILD.transform.localScale = currentTile.GetComponent<Collider>().bounds.size + new Vector3(0.1f,0.1f,0.1f);
	    		}
    		}
		}
	}

	private void FixedUpdate()
	{
		//Debug.Log(LayersEngine.GetCurrentLayersName());

		if(isMouseDown&&(isContinuesBuild||isBuildingMass)&&canBuild)
		{
			if(isBuildingMass)
			{
				if(currentTile)
				{
					StartCoroutine(uMBE.AddTile(currentTile));
				}
			}
			else if(!isBuildingTC)
			{
				ApplyBuild();
			}
			else
			{
				if(currentTile)
				{
					uTCE.AddTile(currentTile);
				}
			}
		}
	}
	
	private void FindAndSelectTileInAllCats(GameObject findObj, bool isFilter)
	{
		bool isFound = false;

		for(int i=0;i<allTiles.Count;i++)
		{
			for(int j=0;j<allTiles[i].catObjsNames.Count;j++)
			{
				if(allTiles[i].catObjsNames[j].Equals(findObj.name))
				{
					isFound = true;
					SelectTileBasedOnFindings(allTiles[i],findObj,i,j,isFilter);
					break;
				}
			}
		}

		if(!isFound)
		{
			Debug.Log("NOT FOUND");
		}
	}

	private void SelectTileBasedOnFindings(catInfo _catInfo, GameObject _findObj, int _catInfoIndex, int _catObjsIndex, bool _isFilter)
	{
		if(_isFilter)
		{
			comboBoxControl.selectedItemIndex = _catInfoIndex;
			StartCoroutine(AssignFilterNextFrame(_findObj.name));
		}
		else
		{
			comboBoxControl.selectedItemIndex = _catInfoIndex;
			StartCoroutine(AssignTileNextFrame(_catObjsIndex));
		}
	//	comboBoxForTileConnectionControl.SetSelectedItemIndex(3);
//		Debug.Log("FOUND: "+allTiles[_catInfoIndex].catName+":"+_catInfo.catObjsNames[_catObjsIndex]);
	}

	private IEnumerator AssignFilterNextFrame(string name)
	{
		yield return 0;
		yield return 0;
		yield return 0;

		currentFilter = name;

		yield return 0;
	}

	private IEnumerator AssignTileNextFrame(int _catObjsIndex)
	{
		yield return 0;
		yield return 0;
		yield return 0;

		while(_catObjsIndex>catGoes.Count)
		{
			yield return 0;
		}

		uMBE.StepOne();
									
		if(currentTile)
		{
			Destroy(currentTile);
		}
		
		if(((GameObject)catGoes[_catObjsIndex]).transform.parent.gameObject.name.Equals("static_objs"))
		{
			isCurrentStatic = true;
		}
		else
		{
			isCurrentStatic = false;
		}

		currentTile = (GameObject) Instantiate((GameObject)catGoes[_catObjsIndex],new Vector3(0.0f,0.0f,0.0f),((GameObject)catGoes[_catObjsIndex]).transform.rotation);
		uteTagObject tempTag = (uteTagObject) currentTile.AddComponent<uteTagObject>();
		tempTag.objGUID = catGuids[_catObjsIndex].ToString();
		tempTag.isStatic = isCurrentStatic;
		currentTile.AddComponent<Rigidbody>();
		currentTile.GetComponent<Rigidbody>().useGravity = false;
		currentTile.GetComponent<BoxCollider>().size -= new Vector3(0.0000001f,0.0000001f,0.0000001f);
		currentTile.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
		currentTile.GetComponent<Collider>().isTrigger = true;
		currentTile.AddComponent<uteDetectBuildCollision>();
		currentTile.layer = 2;
		currentObjectID = catNames[_catObjsIndex].ToString();
		currentTile.name = currentObjectID;
		currentObjectGUID = catGuids[_catObjsIndex].ToString();

		uteTilePivotOffset to = currentTile.GetComponentInChildren<uteTilePivotOffset>();

		if(to)
		{
			customTileOffset = to.TilePivotOffset;
		}
		else
		{
			customTileOffset = Vector3.zero;
		}

		helpers_CANTBUILD.transform.position = new Vector3(-1000,0,-1000);
		helpers_CANTBUILD.transform.localScale = currentTile.GetComponent<Collider>().bounds.size + new Vector3(0.1f,0.1f,0.1f);
		helpers_CANTBUILD.transform.localRotation = currentTile.transform.localRotation;

		isRotated = 0;
		isRotatedH = 0;
		erase = false;
	}

	private IEnumerator SmoothObjInit(GameObject obj)
	{
		Vector3 wholeScale = obj.transform.localScale;
		float objSizeY = obj.transform.localScale.y;
		float objSizeYDiv = objSizeY / 5.0f;

		obj.transform.localScale = new Vector3(obj.transform.localScale.x,0,obj.transform.localScale.z);
		obj.transform.position -= new Vector3(0,objSizeY,0);

		for(int i=0;i<5;i++)
		{
			obj.transform.localScale += new Vector3(0,objSizeYDiv,0);
			obj.transform.position += new Vector3(0,objSizeYDiv,0);
			yield return 0;
		}

		obj.transform.localScale = wholeScale;

		yield return 0;
	}

	public void ApplyBuild(GameObject tcObj=null, Vector3 tcPos = default(Vector3), string tcName = default(string), string tcGuid = default(string), Vector3 tcRot = default(Vector3), string tcFamilyName = default(string), bool isMassBuild = false)
	{
		if(!LayersEngine.isCurrentLayerVisible())
		{
			Debug.Log("[LayersEngine] Can't build on hidden Layer. Turn on visibility or choose another Layer.");
			return;
		}

		GameObject newObj = null;
		bool goodToGo = false;

		if(tcObj!=null)
		{
			Vector3 _pos = new Vector3(RoundTo(tcPos.x),tcPos.y,RoundTo(tcPos.z));

			if(!uteGLOBAL3dMapEditor.XZsnapping)
			{
				_pos = new Vector3(tcPos.x,tcPos.y,tcPos.z);
			}

			if(!isMassBuild)
			{
				newObj = (GameObject) Instantiate(tcObj,_pos,tcObj.transform.rotation);
				isCurrentStatic = true;
				uteTagObject utag = (uteTagObject) newObj.AddComponent<uteTagObject>();
				utag.objGUID = tcGuid;
				utag.isStatic = true;
				utag.isTC = true;
				
				uteTcTag uTT = (uteTcTag) newObj.AddComponent<uteTcTag>();
				newObj.transform.Rotate(tcRot);
				uTT.tcFamilyName = tcFamilyName;
			}
			else
			{
				if(isTileRandomizerEnabled)
				{
					int index = Random.Range(0,catGoes.Count);
					currentObjectID = catNames[index];
					currentObjectGUID = catGuids[index];
					
					newObj = (GameObject) Instantiate(catGoes[index],_pos,currentTile.transform.rotation);

					uteTagObject tempTag = (uteTagObject) newObj.AddComponent<uteTagObject>();
					tempTag.objGUID = catGuids[index].ToString();
					tempTag.isStatic = isCurrentStatic;
					newObj.name = currentObjectID;
				}
				else
				{
					newObj = (GameObject) Instantiate(tcObj,_pos,tcObj.transform.rotation);
				}

				if(isTileRandomizerRotationEnabled)
				{
					newObj.transform.localEulerAngles = new Vector3(tcObj.transform.rotation.x,rotationList[Random.Range(0,rotationList.Count)],tcObj.transform.rotation.z);
				}
				else
				{
					newObj.transform.localEulerAngles = tcRot;
				}

				newObj.GetComponent<uteTagObject>().isTC = false;
			}

			goodToGo = true;
		}
		else
		{
			if(currentTile)
			{
				float newTileDistance = 1000.0f;
				float newDistanceReq = 0.0f;

				if(lastTile!=null)
				{
					Vector3 lastTilePos = lastTile.transform.position;
					Vector3 currentTilePos = currentTile.transform.position;

					float tileDistanceX = Mathf.Floor(Mathf.Abs(lastTilePos.x-currentTilePos.x));
					float tileDistanceZ = Mathf.Floor(Mathf.Abs(lastTilePos.z-currentTilePos.z));
					bool skip = false;

					if(tileDistanceX!=0.0f)
					{
						newTileDistance = tileDistanceX;
						newDistanceReq = currentTile.GetComponent<Collider>().bounds.size.x;
					}
					else if(tileDistanceZ!=0.0f)
					{
						newTileDistance = tileDistanceZ;
						newDistanceReq = currentTile.GetComponent<Collider>().bounds.size.z;
					}
					else
					{
						skip = true;
						goodToGo = false;
					}

					if(!skip)
					{
						if((newTileDistance>newDistanceReq-0.01f)&&((Mathf.Abs(lastTile.transform.position.y-currentTile.transform.position.y)<0.01f)))
						{
							Vector3 _pos = new Vector3(RoundTo(currentTile.transform.position.x),currentTile.transform.position.y,RoundTo(currentTile.transform.position.z));

							if(!uteGLOBAL3dMapEditor.XZsnapping)
							{
								_pos = new Vector3(currentTile.transform.position.x,currentTile.transform.position.y,currentTile.transform.position.z);
							}

							if(isTileRandomizerEnabled)
							{
								int index = Random.Range(0,catGoes.Count);
								currentObjectID = catNames[index];
								currentObjectGUID = catGuids[index];
								
								newObj = (GameObject) Instantiate(catGoes[index],_pos,currentTile.transform.rotation);

								uteTagObject tempTag = (uteTagObject) newObj.AddComponent<uteTagObject>();
								tempTag.objGUID = catGuids[index].ToString();
								tempTag.isStatic = isCurrentStatic;
								newObj.name = currentObjectID;
							}
							else
							{
								newObj = (GameObject) Instantiate(currentTile,_pos,currentTile.transform.rotation);
							}

							if(isTileRandomizerRotationEnabled)
							{
								newObj.transform.localEulerAngles = new Vector3(newObj.transform.rotation.x,rotationList[Random.Range(0,rotationList.Count)],newObj.transform.rotation.z);
							}

							lastTile = newObj;

							goodToGo = true;
						}
						else
						{
							goodToGo = false;
						}
					}
				}
				else
				{
					Vector3 _pos = new Vector3(RoundTo(currentTile.transform.position.x), currentTile.transform.position.y, RoundTo(currentTile.transform.position.z));
					if (tcPos != default(Vector3))
					{
						_pos = tcPos;
					}
					
					if(!uteGLOBAL3dMapEditor.XZsnapping)
					{
						_pos = new Vector3(currentTile.transform.position.x,currentTile.transform.position.y,currentTile.transform.position.z);
					}

					if(isTileRandomizerEnabled)
					{
						int index = Random.Range(0,catGoes.Count);
						currentObjectID = catNames[index];
						currentObjectGUID = catGuids[index];
						
						newObj = (GameObject) Instantiate(catGoes[index],_pos,currentTile.transform.rotation);

						uteTagObject tempTag = (uteTagObject) newObj.AddComponent<uteTagObject>();
						tempTag.objGUID = catGuids[index].ToString();
						tempTag.isStatic = isCurrentStatic;
						newObj.name = currentObjectID;
					}
					else
					{
						newObj = (GameObject) Instantiate(currentTile,_pos,currentTile.transform.rotation);
					}
					
					if(isTileRandomizerRotationEnabled)
					{
						newObj.transform.localEulerAngles = new Vector3(newObj.transform.rotation.x,rotationList[Random.Range(0,rotationList.Count)],newObj.transform.rotation.z);
					}

					goodToGo = true;
				}

				if(lastTile==null)
				{
					lastTile = newObj;
				}
			}
		}
	 	
	 	if((currentTile||tcObj)&&goodToGo)
	 	{
			newObj.layer = 0;
			Destroy(newObj.GetComponent<Rigidbody>());
			Destroy(newObj.GetComponent<uteDetectBuildCollision>());
			newObj.GetComponent<Collider>().isTrigger = false;

			if(!isMassBuild)
			{
				UndoSystem.AddToUndo(newObj,currentObjectGUID,newObj.transform.position,newObj.transform.localEulerAngles);
			}
			else
			{
				if(uteGLOBAL3dMapEditor.UndoSession==2||(uteGLOBAL3dMapEditor.UndoSession==1))
				{
					undo_objs.Add(newObj);
					undo_poss.Add(newObj.transform.position);
					undo_rots.Add(newObj.transform.localEulerAngles);
					undo_guids.Add(currentObjectGUID);
				}

				if(uteGLOBAL3dMapEditor.UndoSession==2)
				{
					UndoSystem.AddToUndoMass(undo_objs,undo_guids,undo_poss,undo_rots);

					undo_objs.Clear();
					undo_poss.Clear();
					undo_guids.Clear();
					undo_rots.Clear();

					uteGLOBAL3dMapEditor.UndoSession = 0;
				}
			}
		}

		if(goodToGo)
		{
			if(!isUse360Snap)
			{
				RoundTo90(newObj);
			}

			if(isCurrentStatic||isItNewPattern)
			{
				newObj.transform.parent = MAP_STATIC.transform;
				newObj.isStatic = true;
			}
			else
			{
				newObj.transform.parent = MAP_DYNAMIC.transform;
				newObj.isStatic = false;
			}

			if(tcObj!=null)
			{
				newObj.name = tcName;
			}
			else
			{
				newObj.name = currentObjectID;
			}
		}

		if(goodToGo)
		{
			uteGLOBAL3dMapEditor.mapObjectCount++;
		}

		if(newObj)
		{
			newObj.GetComponent<uteTagObject>().layerName = LayersEngine.GetCurrentLayersName();
		}
	}

	private IEnumerator ReloadTileAssets()
	{
		StreamReader rd = new StreamReader(catInfoPath);
		string rdinfo = rd.ReadToEnd();
		rd.Close();
		StreamWriter rw = new StreamWriter(catInfoPath);
		rw.Write("");
		rw.Write(rdinfo);
		rw.Close();
		
		yield return null;
		
		LoadGlobalSettings();
		
		yield return null;
		
		UnityEditor.AssetDatabase.SaveAssets();
		UnityEditor.AssetDatabase.Refresh();
		
		yield return null;
		
		LoadTools();
		yield return StartCoroutine(LoadTiles());
		LoadTilesIntoGUI();
		FinalizeGridAndCamera();
	}

	private IEnumerator LoadTiles()
	{	
		GameObject TEMP_STATIC = new GameObject("static_objs");
		TEMP_STATIC.isStatic = true;
		GameObject TEMP_DYNAMIC = new GameObject("dynamic_objs");
		GameObject TEMP = (GameObject) GameObject.Find("TEMP");
		GameObject TEMP_TC = new GameObject("TC");
		TEMP_TC.transform.parent = TEMP.transform;
		TEMP_STATIC.transform.parent = TEMP.transform;
		TEMP_DYNAMIC.transform.parent = TEMP.transform;
		TEMP.transform.position -= new Vector3(-1000000000.0f,100000000.0f,-1000000000.0f);

		TextAsset _allTilesConnectionInfo = (TextAsset) Resources.Load("uteForEditor/uteTileConnections");
		string allTilesConnectionInfo = _allTilesConnectionInfo.text;
		string[] allTilesConnectionbycat = (string[]) allTilesConnectionInfo.Split('|');

		for(int i=0;i<allTilesConnectionbycat.Length;i++)
		{
			if(!allTilesConnectionbycat[i].ToString().Equals(""))
			{
				string[] splitedtcinfo = (string[]) allTilesConnectionbycat[i].ToString().Split('$');
				string[] splitedtcguids = (string[]) splitedtcinfo[1].Split(':');
				string[] splitedtcrots = (string[]) splitedtcinfo[2].Split(':');
				string tcName = splitedtcinfo[0];
				GameObject tcDIR = new GameObject(tcName);
				tcDIR.transform.parent = TEMP_TC.transform;
				List<GameObject> tcObjs = new List<GameObject>();
				List<Texture2D> tcObjsP = new List<Texture2D>();
				List<string> tcObjsNames = new List<string>();
				List<string> tcGuidNames = new List<string>();
				List<string> tcRotsNames = new List<string>();

				for(int k=0;k<splitedtcrots.Length;k++)
				{
					if(!splitedtcrots[k].Equals(""))
					{
						tcRotsNames.Add(splitedtcrots[k]);
					}
				}

				for(int j=0;j<splitedtcguids.Length;j++)
				{
					if(!splitedtcguids[j].ToString().Equals(""))
					{
						string opath = UnityEditor.AssetDatabase.GUIDToAssetPath(splitedtcguids[j].ToString());
						GameObject tGO = (GameObject) UnityEditor.AssetDatabase.LoadMainAssetAtPath(opath);

						if(tGO)
						{
							previewTTC = UnityEditor.AssetPreview.GetAssetPreview((Object)tGO);

							if(previewTTC)
							{
								Texture2D newTex = new Texture2D(previewTTC.width,previewTTC.height);
								newTex.LoadImage(previewTTC.EncodeToPNG());
								tcObjsP.Add(newTex);
							}
							else
							{
								tcObjsP.Add(new Texture2D(20,20));
							}

							tcGuidNames.Add(splitedtcguids[j].ToString());

							tcObjsNames.Add(tGO.name);
							GameObject tmp_tGO = (GameObject) Instantiate(tGO,Vector3.zero,new Quaternion(0,0,0,0));
							tmp_tGO.name = splitedtcguids[j].ToString();
							List<GameObject> twoGO = new List<GameObject>();
							twoGO = createColliderToObject(tmp_tGO,tGO);
							GameObject behindGO = (GameObject) twoGO[0];
							behindGO.name = tmp_tGO.name;
							GameObject objGO = (GameObject) twoGO[1];
							tGO = objGO;
							tGO.transform.parent = behindGO.transform;
							behindGO.layer = 2;
							behindGO.transform.parent = tcDIR.transform;
							tcObjs.Add(behindGO);
						}
					}
				}

				if(!tcName.Equals("")&&tcObjs.Count>0)
				{
					allTCTiles.Add(new tcInfo(tcName,tcObjsNames,tcObjs,tcObjsP,tcGuidNames,tcRotsNames));

					StartCoroutine(ReloadTCAssetPreviewInSlowMode(allTCTiles[allTCTiles.Count-1]));
				}
				else
				{
					if(tcObjs.Count<=0)
					{
						Debug.Log ("Warning: Tile-Connection ["+tcName+"] was ignored because there are no objects inside");
					}
					else
					{
						Debug.Log ("Something is Wrong (TC)");
					}
				}
			}
		}

		TextAsset _alltilesinfo = (TextAsset) Resources.Load ("uteForEditor/uteCategoryInfo");
		string alltilesinfo = _alltilesinfo.text;
		string[] allinfobycat = (string[]) alltilesinfo.Split('|');

		for(int i=0;i<allinfobycat.Length;i++)
		{
			if(!allinfobycat[i].ToString().Equals(""))
			{
				string[] splitedinfo = (string[]) allinfobycat[i].ToString().Split('$');
				string[] splitedguids = (string[]) splitedinfo[2].ToString().Split(':');
				string cName = splitedinfo[0].ToString();
				string cColl = splitedinfo[1].ToString();
				string cType = splitedinfo[3].ToString();
				string cLayer = splitedinfo[4].ToString();
				List<GameObject> cObjs = new List<GameObject>();
				List<Texture2D> cObjsP = new List<Texture2D>();
				List<string> cObjsNames = new List<string>();
				List<string> cObjsGuids = new List<string>();
				
				for(int j=0;j<splitedguids.Length;j++)
				{
					if(!splitedguids[j].ToString().Equals(""))
					{
						string opath = UnityEditor.AssetDatabase.GUIDToAssetPath(splitedguids[j].ToString());
						GameObject tGO = (GameObject) UnityEditor.AssetDatabase.LoadMainAssetAtPath(opath);

						if(tGO)
						{
							previewT = UnityEditor.AssetPreview.GetAssetPreview((Object)tGO);
							//debugTexture = previewT;

							//yield return 0;//WaitForSeconds(0.1f);

							if(previewT)
							{
								Texture2D newTex = new Texture2D(previewT.width,previewT.height);
								newTex.LoadImage(previewT.EncodeToPNG());
								cObjsP.Add(newTex);
							}
							else
							{
								cObjsP.Add(new Texture2D(20,20));
							}

							cObjsNames.Add(tGO.name);
							cObjsGuids.Add(splitedguids[j].ToString());
							GameObject tmp_tGO = (GameObject) Instantiate(tGO,Vector3.zero,new Quaternion(0,0,0,0));
							tmp_tGO.name = splitedguids[j].ToString();
							List<GameObject> twoGO = new List<GameObject>();
							twoGO = createColliderToObject(tmp_tGO,tGO);
							GameObject behindGO = (GameObject) twoGO[0];
							GameObject objGO = (GameObject) twoGO[1];
							tGO = objGO;
							tGO.transform.parent = behindGO.transform;
							behindGO.layer = 2;
							//behindGO.transform.parent = cDIR.transform;
							cObjs.Add(behindGO);
							
							if(cType.Equals("Static"))
							{
								behindGO.transform.parent = TEMP_STATIC.transform;
								tmp_tGO.isStatic = true;
							}
							else if(cType.Equals("Dynamic"))
							{
								behindGO.transform.parent = TEMP_DYNAMIC.transform;
								tmp_tGO.isStatic = false;
							}
							//GameObject.Find("TEMP").transform.position -= new Vector3(-1000000000.0f,100000000.0f,-1000000000.0f);
						}
					}
				}
				
				if(!cName.Equals("")&&!cColl.Equals("")&&cObjs.Count>0)
				{
					if(isSortingByNameEnabled)
					{
						List<string> oldNames = new List<string>();
						int foundSame = 1;
						for(int z=0;z<cObjsNames.Count;z++)
						{
							for(int zx=0;zx<cObjsNames.Count;zx++)
							{
								if(cObjsNames[z].Equals(cObjsNames[zx])&&z!=zx)
								{
									foundSame++;
									cObjsNames[zx] = cObjsNames[zx]+"("+foundSame+")";
								}
							}

							foundSame = 1;
						}

						for(int z=0;z<cObjsNames.Count;z++)
						{
							oldNames.Add(cObjsNames[z]);
						}

						cObjsNames.Sort();

						List<int> newIndexs = new List<int>();

						for(int z=0;z<cObjsNames.Count;z++)
						{
							if(!cObjsNames[z].Equals(oldNames[z]))
							{
								int foundIndex = 0;
								for(int x=0;x<oldNames.Count;x++)
								{
									if(oldNames[x].Equals(cObjsNames[z]))
									{
										foundIndex = x;
										break;
									}
								}

								newIndexs.Add(foundIndex);
							}
							else
							{
								newIndexs.Add(z);
							}
						}

						List<Texture2D> cObjsP_newlist = new List<Texture2D>();
						List<string> cObjsGuids_newlist = new List<string>();
						List<GameObject> cObjs_newlist = new List<GameObject>();

						for(int x=0;x<newIndexs.Count;x++)
						{
							cObjsP_newlist.Add(cObjsP[newIndexs[x]]);
							cObjsGuids_newlist.Add(cObjsGuids[newIndexs[x]]);
							cObjs_newlist.Add(cObjs[newIndexs[x]]);
						}

						allTiles.Add(new catInfo(cName,cLayer,cObjsNames,cColl,cObjs_newlist,cObjsP_newlist,cObjsGuids_newlist));
					}
					else
					{
						allTiles.Add(new catInfo(cName,cLayer,cObjsNames,cColl,cObjs,cObjsP,cObjsGuids));
					}

					StartCoroutine(ReloadCatAssetPreviewInSlowMode(allTiles[allTiles.Count-1]));
				}
				else
				{
					if(cObjs.Count<=0)
					{
						Debug.Log ("Warning: Category ["+cName+"] was ignored because there are no objects inside");
					}
					else
					{
						Debug.Log ("Something is Wrong (CE)");
					}
				}
			}
		}
		
		yield return 0;
	}
		
	private void SortAllTilesAlphabetically(List<catInfo> tList)
	{
		// done in Settings-Controls preprocess
	}

	private IEnumerator ReloadTCAssetPreviewInSlowMode(tcInfo _ti)
	{
		tcInfo ti = (tcInfo) _ti;

		for(int j=0;j<ti.tcObjsPrevs.Count;j++)
		{
			Texture2D _oldTex = ti.tcObjsPrevs[j];

			if(_oldTex.width==20&&_oldTex.height==20)
			{
				for(int i=0;i<25;i++)
				{
					Object _obj = (Object) ti.tcObjs[j];
					string opath = UnityEditor.AssetDatabase.GUIDToAssetPath(_obj.name);
					GameObject tcGO = (GameObject) UnityEditor.AssetDatabase.LoadMainAssetAtPath(opath);
					Texture2D _tex = (Texture2D) UnityEditor.AssetPreview.GetAssetPreview(tcGO);

					if(_tex!=null)
					{
						Texture2D newTex = new Texture2D(_tex.width,_tex.height);
						newTex.LoadImage(_tex.EncodeToPNG());
						ti.tcObjsPrevs[j] = newTex;
						break;
					}
					else
					{
						yield return 0;
					}
				}
			}
		}

		yield return 0;
	}

	private IEnumerator ReloadCatAssetPreviewInSlowMode(catInfo _ci)
	{
		catInfo ci = (catInfo) _ci;

		for(int j=0;j<ci.catObjsPrevs.Count;j++)
		{
			Texture2D _oldTex = ci.catObjsPrevs[j];

			if(_oldTex.width==20&&_oldTex.height==20)
			{
				for(int i=0;i<25;i++)
				{
					Object _obj = (Object) ci.catObjs[j];
					string opath = UnityEditor.AssetDatabase.GUIDToAssetPath(_obj.name);
					GameObject tGO = (GameObject) UnityEditor.AssetDatabase.LoadMainAssetAtPath(opath);
					Texture2D _tex = (Texture2D) UnityEditor.AssetPreview.GetAssetPreview(tGO);

					if(_tex!=null)
					{
						Texture2D newTex = new Texture2D(_tex.width,_tex.height);
						newTex.LoadImage(_tex.EncodeToPNG());
						ci.catObjsPrevs[j] = newTex;
						break;
					}
					else
					{
						yield return 0;
					}
				}
			}
		}

		yield return 0;
	}

	private void LoadTools()
	{
		MAIN = new GameObject("MAIN");
		cameraMove = MAIN.AddComponent<uteCameraMove>();
		cameraMove.cameraSensitivity = cameraSensitivity;
		saveMap = this.gameObject.AddComponent<uteSaveMap>();

	//	cameraMove = (CameraMove) ((GameObject) GameObject.Find ("MapEditorCamera")).AddComponent<CameraMove>();
		GameObject _grid = (GameObject) Resources.Load("uteForEditor/uteLayer");
		grid = (GameObject) Instantiate(_grid,new Vector3((gridsize.x/2)+0.5f,0.0f,(gridsize.z/2)+0.5f),_grid.transform.rotation);
		grid.name = "Grid";

		GameObject _grid3d = (GameObject) Resources.Load("uteForEditor/ute3dgrid");
		grid3d = (GameObject) Instantiate(_grid3d,new Vector3((gridsize.x/2)+0.5f,0.5f,(gridsize.z/2)+0.5f),_grid3d.transform.rotation);
		grid3d.name = "3dGrid";
		Set3DGrid(grid3dScale);

		if(globalGridSizeX%2.0f!=0.0f)
		{
			grid.transform.position -= new Vector3(0.5f,0,0);
		}

		if(globalGridSizeZ%2.0f!=0.0f)
		{
			grid.transform.position -= new Vector3(0,0,0.5f);
		}

		if(!sortType.Equals(""))
		{
			if(sortType.Equals("none"))
			{
				isSortingByNameEnabled = false;
			}
			else if(sortType.Equals("byname"))
			{
				isSortingByNameEnabled = true;
			}
		}
		else
		{
			sortType = "none";
			isSortingByNameEnabled = false;
		}

		cameraMove.gameObject.transform.position = Vector3.zero;
		MAP = (GameObject) GameObject.Find ("MAP");
		GameObject.Find("MapEditorCamera").transform.parent = MAIN.transform;
		GameObject.Find("MapEditorCamera").transform.position = Vector3.zero;	

		if(isItNewPattern)
		{
			exporter = (uteExporter) cameraMove.gameObject.AddComponent<uteExporter>();
			exporter.MAP_STATIC = MAP_STATIC;
			exporter.mapName = newProjectName;
		}
		
		lastSelectedIndex = -10;
		lastSelectedIndexTC = -10;

		SetGrid(globalGridSizeX,globalGridSizeZ);
		SetCamera(cameraType);
	}
	
	private void LoadTilesIntoGUI()
	{
		comboBoxList = new GUIContent[allTiles.Count];
		
		for(int i=0;i<allTiles.Count;i++)
		{
			catInfo cI = (catInfo) allTiles[i];
			
			comboBoxList[i] = new GUIContent((string)cI.catName);
		}
		
		comboBoxList_TileConnections = new GUIContent[allTCTiles.Count];

		for(int i=0;i<allTCTiles.Count;i++)
		{
			tcInfo tcI = (tcInfo) allTCTiles[i];
			comboBoxList_TileConnections[i] = new GUIContent((string)tcI.tcName);
		}

		listStyle.normal.textColor = Color.white;
		listStyle.normal.background = new Texture2D(0,0);
		listStyle.onHover.background = new Texture2D(2, 2);
		listStyle.hover.background = new Texture2D(2, 2);
		listStyle.padding.bottom = 4;
	}

	private void LoadGlobalSettings()
	{
		StreamReader rd = new StreamReader(settingsInfoPath);
		string info = rd.ReadToEnd();
		rd.Close();
		StreamWriter rw = new StreamWriter(settingsInfoPath);
		rw.Write("");
		rw.Write(info);
		rw.Close();
		
		if(!info.Equals(""))
		{
			string[] infoSplited = info.Split(':');
			
			globalYSize = System.Convert.ToSingle(infoSplited[0].ToString());

			rightClickOption = infoSplited[1].ToString();
			yTypeOption = infoSplited[2].ToString();
			globalGridSizeX = System.Convert.ToInt32(infoSplited[3]);
			globalGridSizeZ = System.Convert.ToInt32(infoSplited[4]);
			cameraType = infoSplited[5].ToString();
			sortType = infoSplited[6].ToString();
			cameraSensitivity = System.Convert.ToSingle(infoSplited[7]);
			grid3dScale = System.Convert.ToSingle(infoSplited[8]);
			fixedTileSnap = System.Convert.ToSingle(infoSplited[9]);

			DecorationList.Clear();
			if (infoSplited.Length > 10)
			{
				string decorationListString = infoSplited[10];
				string[] decorationSplited = decorationListString.Split('|');
				for (int i = 0; i < decorationSplited.Length; i++)
				{
					DecorationList.Add(System.Convert.ToInt32(decorationSplited[i]));
				}
			}

		}
		else
		{
			Debug.Log ("Error: Failed to load settings. Loading default settings.");
			globalYSize = 1.0f;

			rightClickOption = "rotL";
			globalGridSizeZ = 1000;
			globalGridSizeX = 1000;
			cameraSensitivity = 1.0f;
			grid3dScale = 2.0f;
			fixedTileSnap = 0f;
			cameraType = "isometric-perspective";

			DecorationList.Clear();
		}
	}
	
	private void SetCamera(string camType)
	{
		GameObject rotationArea = new GameObject("YArea");
		cameraGO.transform.parent = rotationArea.transform;
		rotationArea.transform.parent = MAIN.transform;
		cameraGO.transform.position = Vector3.zero;
		Camera camTemp = cameraGO.GetComponent<Camera>();

		if(camType.Equals("isometric-perspective"))
		{
			MAIN.transform.localEulerAngles = new Vector3(0,45,0);
			cameraGO.transform.localEulerAngles = new Vector3(30,0,0);
			camTemp.orthographic = false;
			camTemp.fieldOfView = 60;
			camTemp.farClipPlane = 1000.0f;
			isOrtho = false;
			is2D = false;
		}
		else if(camType.Equals("isometric-ortho"))
		{
			MAIN.transform.localEulerAngles = new Vector3(0,45,0);
			cameraGO.transform.localEulerAngles = new Vector3(30,0,0);
			camTemp.orthographic = true;
			camTemp.orthographicSize = 5;
			camTemp.nearClipPlane = -100.0f;
			camTemp.farClipPlane = 1000.0f;
			is2D = false;
			isOrtho = true;
		}
		else if(camType.Equals("2d-perspective"))
		{
			MAIN.transform.localEulerAngles = new Vector3(0,0,0);
			camTemp.orthographic = false;
			camTemp.nearClipPlane = 0.1f;
			camTemp.farClipPlane = 1000.0f;
			isOrtho = false;
			is2D = true;
		}
		else if(camType.Equals("2d-ortho"))
		{
			MAIN.transform.localEulerAngles = new Vector3(0,0,0);
			camTemp.orthographic = true;
			camTemp.orthographicSize = 5;
			camTemp.nearClipPlane = -10.0f;
			camTemp.farClipPlane = 300.0f;
			isOrtho = true;
			is2D = true;
		}
	}

	private void FinalizeGridAndCamera()
	{
		if(is2D)
		{
			cameraMove.is2D = true;
			cameraGO.transform.Rotate(new Vector3(90,0,0));
			MAIN.transform.position = new Vector3(500,14,490);
		}
		else
		{
			cameraMove.is2D = false;
			
			if(isOrtho)
			{
				MAIN.transform.position = new Vector3(492,8,492);
			}
			else
			{
				MAIN.transform.position = new Vector3(493,8,493);
			}
		}
	}

	private void SetGrid(int x, int z)
	{
		grid.transform.localScale = new Vector3((float)x,0.01f,(float)z);
		grid.GetComponent<Renderer>().material.mainTextureScale = new Vector2((float)x,(float)z);
	}

	private void Set3DGrid(float scale)
	{
		grid3d.transform.localScale = new Vector3(scale,scale,scale);
		grid3d.SetActive(false);
	}

	private void ReloadCatPrevs()
	{
		for(int i=0;i<allTiles.Count;i++)
		{
			catInfo ct = (catInfo) allTiles[i];
			
			ct.catObjsPrevs.Clear();
			
			for(int j=0;j<ct.catObjs.Count;j++)
			{
				GameObject tGO = (GameObject) ct.catObjs[j];
				
				if((Object)tGO)
				{
					previewT = UnityEditor.AssetPreview.GetAssetPreview((Object)tGO);
				}
				
				if(previewT)
				{
					ct.catObjsPrevs.Add(previewT);
				}
				else
				{
					ct.catObjsPrevs.Add(new Texture2D(20,20));
				}
			}
		}
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
		
		//BoxCollider obj_bc = obj.GetComponent<BoxCollider>();
		
		//if(!obj_bc)
		//{
		GameObject behindGO = new GameObject(obj.name);
		behindGO.AddComponent<BoxCollider>();
		obj.transform.parent = behindGO.transform;
		//}
		
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
		
		//if(objScale.x<0.99||objScale.x>1.01||objScale.y>1.01||objScale.y<0.99||objScale.z>1.01||objScale.z<0.99)
		//	Debug.Log ("Warning: "+"("+obj.name+") is not using (1,1,1) localScale. This might couse some problems with map editor. We suggest to always use object scale = 1,1,1 and change mesh size instead.");
		
		DisableAllExternalColliders(obj);

		List<GameObject> twoGO = new List<GameObject>();
		twoGO.Add(behindGO);
		twoGO.Add(obj);

		return twoGO;
		
		//Destroy(obj);
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

	private bool CheckGUIPass()
	{
		normalMousePosition = new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y);

		if(new Rect(0,0,Screen.width,40).Contains(normalMousePosition))
		{
			return false;
		}
		else if(new Rect(0,Screen.height-40,Screen.width,40).Contains(normalMousePosition))
		{
			return false;
		}
		else if(new Rect(Screen.width-100,40,100,150).Contains(normalMousePosition)&&currentTile&&!isBuildingTC)
		{
			return false;
		}
		else if(new Rect(Screen.width-100,200,100,90).Contains(normalMousePosition)&&currentTile&&!isBuildingTC)
		{
			return false;
		}
		else if(isBuildingTC&&checkGUIObjsColliderTC.Contains(normalMousePosition))
		{
			return false;
		}
		else if(!isBuildingTC&&checkGUIObjsCollider.Contains(normalMousePosition))
		{
			return false;
		}
		else if(isShowCameraOptions&&cameraOptionsRect.Contains(normalMousePosition))
		{
			return false;
		}
		else if(LayersEngine.boxRect.Contains(normalMousePosition))
		{
			return false;
		}
		else if(_hints.HintBoxRect.Contains(normalMousePosition))
		{
			return false;
		}

		return true;
	}

	private void ShowTopView()
	{
		if(!isInTopView)
		{
			MAIN.transform.position += new Vector3(0,5,0);
		}
		
		isInTopView = true;
		cameraMove.isInTopView = true;
		MAIN.transform.localEulerAngles = new Vector3(MAIN.transform.localEulerAngles.x,0,MAIN.transform.localEulerAngles.z);
		GameObject cameraYRot = (GameObject) GameObject.Find("MAIN/YArea");
		cameraYRot.transform.localEulerAngles = new Vector3(0,0,0);
		cameraGO.transform.localEulerAngles = new Vector3(90,cameraGO.transform.localEulerAngles.y,cameraGO.transform.localEulerAngles.z);
	}

	private IEnumerator TurnCamera90(int iternation, int count)
	{
		for(int i=0;i<iternation;i++)
		{
			MAIN.transform.Rotate(new Vector3(0,count,0));
			yield return 0;
		}

		yield return 0;
	}

	private void OnGUI()
	{
		/*if(debugTexture)
		{
			GUI.Button(new Rect(10,10,200,200),debugTexture);
		}*/

		GUI.skin = ui;

		GUI.Box(new Rect(0,0,Screen.width,40),"");
		GUI.Box(new Rect(0,Screen.height-40,Screen.width,40),"");

		if(editorIsLoading)
		{
			GUI.Label(new Rect(20,10,500,34),"Loading Assets... <size=12>(Might be slower when loading first time)</size>"); 
			GUI.Label(new Rect(20,Screen.height-30,500,34),"Click HELP for Camera Contorls and other Shortcuts");
			return;
		}

		if(isItLoad&&!editorIsLoading)
		{
			if(!_uteLM.isMapLoaded)
			{
				GUI.Label(new Rect(20,10,200,34),"Loading Assets..."); return;
			}
		}

		if(saveMap.isSaving) { GUI.Label(new Rect(20,10,200,34),"Saving Assets..."); return; }


		if(isFindTilePressed)
		{
			GUI.Label(new Rect(150,45,500,200),"FIND MODE\nLeft Mouse Click: Pick Tile\nRight Mouse Click: Filter Tile");

			if(currentFindObject)
			{
				GUI.Label(new Rect(Input.mousePosition.x+20,Screen.height-Input.mousePosition.y-20,300,30),"<size=20>"+currentFindObject.name+"</size>");
			}
		}

		if(erase)
		{
			GUI.Label(new Rect(150,75,500,30),"(Eraser: Hold left click: erase fast, right click: erase one/click)");
		}

		if(isInTopView)
		{
			GUI.Label(new Rect(Screen.width/2-30,65,70,30),"[Top View]");
		}

		if(isBuildingTC&&allTCTiles.Count<=0)
		{
			GUI.Label(new Rect(20,10,200,34),"[No TileConnections]");
		}
		else if(allTiles.Count<=0)
		{
			GUI.Label(new Rect(20,10,200,34),"[No Tiles found]");
		}

		if(isShowCameraOptions)
		{
			GUI.Box(cameraOptionsRect,"");
			if(GUI.Button(new Rect(Screen.width-500,Screen.height-69,100,28),"Reset"))
			{
				ResetCamera();
			}
			if(GUI.Button(new Rect(Screen.width-390,Screen.height-69,100,28),"Top View"))
			{
				ShowTopView();
			}
			if(GUI.Button(new Rect(Screen.width-280,Screen.height-69,90,28),"<- 90deg"))
			{
				StartCoroutine(TurnCamera90(9,-10));
			}
			if(GUI.Button(new Rect(Screen.width-190,Screen.height-69,90,28),"90deg ->"))
			{
				StartCoroutine(TurnCamera90(9,10));
			}
		}

		if(currentTile&&canBuild&&!isFindTilePressed)
		{
			GUI.Label(new Rect(Input.mousePosition.x+60,Screen.height-Input.mousePosition.y-60,300,30),"<size=11>"+currentTile.transform.position.x.ToString("0.0")+", "+currentTile.transform.position.y.ToString("0.0")+", "+currentTile.transform.position.z.ToString("0.0")+"</size>");
		}

		if(GUI.Button(new Rect(Screen.width-90,Screen.height-40,40,40),"+"))
		{
			if(is2D||isInTopView)
			{
				StartCoroutine(cameraMove.MoveUpDown(false,false));
			}
			else
			{
				StartCoroutine(cameraMove.MoveUpDown(false,true));
			}
		}

		if(GUI.Button(new Rect(Screen.width-50,Screen.height-40,40,40),"-"))
		{
			if(is2D||isInTopView)
			{
				StartCoroutine(cameraMove.MoveUpDown(true,false));
			}
			else
			{
				StartCoroutine(cameraMove.MoveUpDown(true,true));
			}
		}

		GUI.Label(new Rect(230,Screen.height-35,Screen.width/2,60),"Project [<color=green>"+newProjectName+"</color>] Object Count [<color=green>"+uteGLOBAL3dMapEditor.mapObjectCount+"</color>] Tile [<color=green>"+ReturnGameObjectNameIfExists(currentTile)+"</color>] Grid [<color=green>"+globalGridSizeX+"x"+globalGridSizeZ+"</color>] Layer [<color=green>"+LayersEngine.GetSelectedLayersName()+"</color>]");

		string tMode = "Tiles";

		if(isBuildingTC) tMode = "Tile-Connections";

		if(GUI.Button(new Rect(220,0,260,40),"Tile Mode: "+tMode))
		{
			if(isBuildingTC)
			{
				isBuildingTC = false;
			}
			else
			{
				isBuildingTC = true;
				isContinuesBuild = true;
				isBuildingMass = false;
			}

			if(currentTile)
			{
				Destroy(currentTile);
				ShowLineHelpers(isShowLineHelpers = false);
				currentObjectID = "-1";
				helpers_CANTBUILD.transform.position = new Vector3(-1000000.0f,0.0f,-1000000.0f);
			}
		}

		if(isBuildingTC)
		{
			if(allTCTiles.Count>0)
			{
				int selectedItemIndex = comboBoxForTileConnectionControl.GetSelectedItemIndex();
				selectedItemIndex = comboBoxForTileConnectionControl.List(GUIComboBoxColliderTC, comboBoxList_TileConnections[selectedItemIndex].text+" ^", comboBoxList_TileConnections,listStyle);

				if(lastSelectedIndexTC!=selectedItemIndex)
				{
					lastSelectedIndexTC = selectedItemIndex;
					tcGoes = ((tcInfo) allTCTiles[selectedItemIndex]).tcObjs;
					tcPrevs = ((tcInfo) allTCTiles[selectedItemIndex]).tcObjsPrevs;
					tcNames = ((tcInfo) allTCTiles[selectedItemIndex]).tcObjsNames;
					tcGuids = ((tcInfo) allTCTiles[selectedItemIndex]).tcGuidNames;
					tcRots = ((tcInfo) allTCTiles[selectedItemIndex]).tcRotsNames;
					currentTcFamilyName = comboBoxList_TileConnections[selectedItemIndex].text;

					ShowLineHelpers(isShowLineHelpers = false);
				}

				if(!comboBoxForTileConnectionControl.isClickedComboButton)
				{
					GUIObjsColliderTC = new Rect(0,0,128,tcGoes.Count*125);
					checkGUIObjsColliderTC = new Rect(0,0,148,tcGoes.Count*125);
					scrollPositionTC = GUI.BeginScrollView(new Rect(0,40,145,Screen.height-80),scrollPositionTC,GUIObjsColliderTC);
					int startDrawPoint = (int) (scrollPositionTC.y/115);
					int endDrawPoint;

					if(tcGoes.Count-startDrawPoint>=7)
					{
						endDrawPoint = startDrawPoint;
					}
					else
					{
						endDrawPoint = startDrawPoint + (tcGoes.Count-startDrawPoint);
					}

					GUI.Box(GUIObjsColliderTC,"");

					for(int i=startDrawPoint;i<endDrawPoint;i++)
					{
						if(i>=0&&i<tcGoes.Count && (GameObject) tcGoes[i])
						{
							if((Texture2D)tcPrevs[i])
							{
								previewObjTextureTC = (Texture2D) tcPrevs[i];
							}

							if(GUI.Button (new Rect(8,5+(i*122),115,115),previewObjTextureTC))
							{
								if(currentTile)
								{
									Destroy(currentTile);
								}

								currentTCID = i;
								currentTile = (GameObject) Instantiate((GameObject)tcGoes[i],new Vector3(0.0f,0.0f,0.0f),((GameObject)tcGoes[i]).transform.rotation);
								uteTagObject tempTag = (uteTagObject) currentTile.AddComponent<uteTagObject>();
								tempTag.objGUID = tcGuids[i].ToString();
								tempTag.isStatic = isCurrentStatic;

								if(isBuildingTC)
								{
									tempTag.isTC = true;
								}

								currentTile.AddComponent<Rigidbody>();
								currentTile.GetComponent<Rigidbody>().useGravity = false;
								currentTile.GetComponent<BoxCollider>().size -= new Vector3(0.01f,0.01f,0.01f);
								currentTile.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
								currentTile.GetComponent<Collider>().isTrigger = true;
								currentTile.AddComponent<uteDetectBuildCollision>();
								currentTile.layer = 2;
								currentObjectID = tcNames[i].ToString();
								isRotated = 0;
								isRotatedH = 0;
								erase = false;
							}
							
							GUI.Label(new Rect(14,7+(i*122),115,50),(string)tcNames[i]);
							
							if(!previewObjTextureTC)
							{
								GUI.Label(new Rect(18,50+(i*122),115,30),"NO PREVIEW");
							}
						}
					}

					GUI.EndScrollView();
				}
			}
		}
		else if(!isBuildingTC)
		{
			if(allTiles.Count!=0)
			{
				if(allTiles.Count>0)
				{
					int selectedItemIndex = comboBoxControl.GetSelectedItemIndex();
					
					selectedItemIndex = comboBoxControl.List(GUIComboBoxCollider, comboBoxList[selectedItemIndex].text+" ^", comboBoxList, listStyle );
					
					if(lastSelectedIndex!=selectedItemIndex)
					{
						uMBE.StepOne();
						lastSelectedIndex = selectedItemIndex;
						catGoes = ((catInfo) allTiles[selectedItemIndex]).catObjs;
						catPrevs = ((catInfo) allTiles[selectedItemIndex]).catObjsPrevs;
						catNames = ((catInfo) allTiles[selectedItemIndex]).catObjsNames;
						catGuids = ((catInfo) allTiles[selectedItemIndex]).catGuidNames;

						ShowLineHelpers(isShowLineHelpers = false);

						currentFilter = "";
						lastFilter = "__________Q#*(&(*#@&$";
					}
					
					if(!comboBoxControl.isClickedComboButton)
					{	
					//	if(currentFilter!="")

						GUIObjsCollider = new Rect(0, 0, 128, catGoes.Count*130);
						checkGUIObjsCollider = new Rect(0,0,148,catGoes.Count*125);

						if(checkGUIObjsCollider.height==0)
						{
							checkGUIObjsCollider = new Rect(0,0,148,50);
						}

						if(catGoes.Count==0)
						{
							GUI.Label(new Rect(10,60,100,30),"NO ITEMS");
						}
						
						scrollPosition = GUI.BeginScrollView(new Rect(0, 60, 145, Screen.height-100), scrollPosition, GUIObjsCollider);
						int startDrawPoint = (int) (scrollPosition.y/122);
						int endDrawPoint;
						
						if(startDrawPoint<-1)
						{
							startDrawPoint = 0;
						}

						if(catGoes.Count-startDrawPoint>=7)
						{
							endDrawPoint = startDrawPoint + 7;
						}
						else
						{
							endDrawPoint = startDrawPoint + (catGoes.Count-startDrawPoint);
						}
						
						GUI.Box(GUIObjsCollider,"");

						for(int i=startDrawPoint;i<endDrawPoint;i++)
						{	
							if(i>=0&&i<catGoes.Count && (GameObject) catGoes[i])
							{	
								if((Texture2D)catPrevs[i])
								{
									previewObjTexture = (Texture2D) catPrevs[i];
								}

								Rect buttonPosRect = new Rect(8,5+(i*122),115,115);
								Rect buttonPosRect2 = new Rect(buttonPosRect.x,buttonPosRect.y+60-scrollPosition.y,buttonPosRect.width,buttonPosRect.height);

								if(buttonPosRect2.Contains(normalMousePosition))
								{
									buttonPosRect = new Rect(buttonPosRect.x-5,buttonPosRect.y-5,buttonPosRect.width+10,buttonPosRect.height+10);
								}

								if(GUI.Button (buttonPosRect,previewObjTexture))
								{
									uMBE.StepOne();
									
									if(currentTile)
									{
										Destroy(currentTile);
									}
									
									if(((GameObject)catGoes[i]).transform.parent.gameObject.name.Equals("static_objs"))
									{
										isCurrentStatic = true;
									}
									else
									{
										isCurrentStatic = false;
									}

									currentTile = (GameObject) Instantiate((GameObject)catGoes[i],new Vector3(0.0f,0.0f,0.0f),((GameObject)catGoes[i]).transform.rotation);
									uteTagObject tempTag = (uteTagObject) currentTile.AddComponent<uteTagObject>();
									tempTag.objGUID = catGuids[i].ToString();
									tempTag.isStatic = isCurrentStatic;
									currentTile.AddComponent<Rigidbody>();
									currentTile.GetComponent<Rigidbody>().useGravity = false;
									currentTile.GetComponent<BoxCollider>().size -= new Vector3(0.0000001f,0.0000001f,0.0000001f);
									currentTile.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
									currentTile.GetComponent<Collider>().isTrigger = true;
									currentTile.AddComponent<uteDetectBuildCollision>();
									currentTile.layer = 2;
									currentObjectID = catNames[i].ToString();
									currentTile.name = currentObjectID;
									currentObjectGUID = catGuids[i].ToString();

									uteTilePivotOffset to = currentTile.GetComponentInChildren<uteTilePivotOffset>();

									if(to)
									{
										customTileOffset = to.TilePivotOffset;
									}
									else
									{
										customTileOffset = Vector3.zero;
									}

									helpers_CANTBUILD.transform.position = new Vector3(-1000,0,-1000);
									helpers_CANTBUILD.transform.localScale = currentTile.GetComponent<Collider>().bounds.size + new Vector3(0.1f,0.1f,0.1f);
									helpers_CANTBUILD.transform.localRotation = currentTile.transform.localRotation;

									isRotated = 0;
									isRotatedH = 0;
									erase = false;
								}
								
								if(!previewObjTexture)
								{
									GUI.Label(new Rect(18,50+(i*122),115,30),"NO PREVIEW");
								}
								else if(previewObjTexture.width==20)
								{
									GUI.Label(new Rect(18,30+(i*122),115,30),"LOADING...");
								}

								if(checkGUIObjsCollider.Contains(normalMousePosition))
								{
									if(buttonPosRect2.Contains(normalMousePosition))
									{
										GUI.Label(new Rect(10,4+(i*122),215,26),(string)"<size=15>"+catNames[i]+"</size>");
									}
									else
									{
										GUI.Label(new Rect(14,7+(i*122),215,26),(string)"<size=13>"+catNames[i]+"</size>");
									}
								}
								else
								{
									GUI.Label(new Rect(14,7+(i*122),105,26),(string)"<size=13>"+catNames[i]+"</size>");
								}
							}
						}
						
						GUI.EndScrollView();

						GUI.Box(new Rect(0,40,130,20),"");
						Rect checkGUIObjsCollider_2 = new Rect(checkGUIObjsCollider.x,checkGUIObjsCollider.y+40,checkGUIObjsCollider.width,checkGUIObjsCollider.height);
						if(checkGUIObjsCollider_2.Contains(normalMousePosition))
						{
							GUI.skin = ui;
							currentFilter = GUI.TextField(new Rect(4,40,122,19),currentFilter);
						}
						else
						{
							GUI.Label(new Rect(4,38,122,20),"search: "+currentFilter);
						}

						if(currentFilter!=lastFilter)
						{
							lastFilter = currentFilter;

							using(catInfo FilteredCatInfo = new catInfo(allTiles[selectedItemIndex].catName,allTiles[selectedItemIndex].catLayer,allTiles[selectedItemIndex].catCollision))
							{
								FilteredCatInfo.catObjs = new List<GameObject>();
								FilteredCatInfo.catObjsPrevs = new List<Texture2D>();
								FilteredCatInfo.catObjsNames = new List<string>();
								FilteredCatInfo.catGuidNames = new List<string>();

								for(int i=0;i<allTiles[selectedItemIndex].catObjs.Count;i++)
								{
									FilteredCatInfo.catObjs.Add(allTiles[selectedItemIndex].catObjs[i]);
								}

								for(int i=0;i<allTiles[selectedItemIndex].catObjsPrevs.Count;i++)
								{
									FilteredCatInfo.catObjsPrevs.Add(allTiles[selectedItemIndex].catObjsPrevs[i]);
								}

								for(int i=0;i<allTiles[selectedItemIndex].catObjsNames.Count;i++)
								{
									FilteredCatInfo.catObjsNames.Add(allTiles[selectedItemIndex].catObjsNames[i]);
								}

								for(int i=0;i<allTiles[selectedItemIndex].catGuidNames.Count;i++)
								{
									FilteredCatInfo.catGuidNames.Add(allTiles[selectedItemIndex].catGuidNames[i]);
								}

								for(int i=FilteredCatInfo.catObjsNames.Count-1;i>=0;i--)
								{
									if(!FilteredCatInfo.catObjsNames[i].Contains(currentFilter))
									{
										FilteredCatInfo.catObjs.RemoveAt(i);
										FilteredCatInfo.catObjsPrevs.RemoveAt(i);
										FilteredCatInfo.catObjsNames.RemoveAt(i);
										FilteredCatInfo.catGuidNames.RemoveAt(i);
									}
								}

								if(currentFilter!="")
								{
									catGoes = ((catInfo) FilteredCatInfo).catObjs;
									catPrevs = ((catInfo) FilteredCatInfo).catObjsPrevs;
									catNames = ((catInfo) FilteredCatInfo).catObjsNames;
									catGuids = ((catInfo) FilteredCatInfo).catGuidNames;
								}
								else
								{
									catGoes = ((catInfo) allTiles[selectedItemIndex]).catObjs;
									catPrevs = ((catInfo) allTiles[selectedItemIndex]).catObjsPrevs;
									catNames = ((catInfo) allTiles[selectedItemIndex]).catObjsNames;
									catGuids = ((catInfo) allTiles[selectedItemIndex]).catGuidNames;
								}
							}
						}
					}
					else
					{
						if(currentTile)
						{
							Destroy(currentTile);
						}
					}
				}
			}
		}
		
		if(is2D)
		{
			if(GUI.Button(new Rect(Screen.width-505,Screen.height-40,130,40),"Reset Camera"))
			{
				ResetCamera();
			}
		}
		else
		{
			if(GUI.Button(new Rect(Screen.width-505,Screen.height-40,130,40),"Camera box"))
			{
				if(isShowCameraOptions)
				{
					isShowCameraOptions = false;
				}
				else
				{
					isShowCameraOptions = true;
				}
			}
		}

		if(GUI.Button (new Rect(Screen.width-370,Screen.height-40,70,40),"Eraser"))
		{
			if(currentTile)
			{
				Destroy(currentTile);
				helpers_CANTBUILD.transform.position = new Vector3(-1000000.0f,0.0f,-1000000.0f);
				ShowLineHelpers(isShowLineHelpers = false);
			}
			
			erase = true;
		}
		
		if(GUI.Button(new Rect(Screen.width-295,Screen.height-40,100,40),"Grid Up"))
		{
			if(!isCamGridMoving)
			{
				StartCoroutine(gridSmoothMove(grid,true,cameraMove.gameObject));
			}
		}
		
		if(GUI.Button(new Rect(Screen.width-195,Screen.height-40,100,40),"Grid Down"))
		{
			if(!isCamGridMoving)
			{
				StartCoroutine(gridSmoothMove(grid,false,cameraMove.gameObject));
			}
		}
		
		if(currentTile&&!isBuildingTC)
		{
			GUI.Box(new Rect(Screen.width-100,200,100,90),"Randomizer");

			if(GUI.Button(new Rect(Screen.width-95,220,90,30),"Tile: "+ReturnCondition(isTileRandomizerEnabled)))
			{
				if(isTileRandomizerEnabled)
				{
					isTileRandomizerEnabled = false;
				}
				else
				{
					isTileRandomizerEnabled = true;
				}
			}

			if(GUI.Button(new Rect(Screen.width-95,250,90,30),"Rot: "+ReturnCondition(isTileRandomizerRotationEnabled)))
			{
				if(isTileRandomizerRotationEnabled)
				{
					isTileRandomizerRotationEnabled = false;
				}
				else
				{
					isTileRandomizerRotationEnabled = true;
				}
			}

			GUI.Box(new Rect(Screen.width-100,40,100,150),"Tile Settings");

			if(GUI.Button(new Rect(Screen.width-90,60,40,30),"<-"))
			{
				StartCoroutine(smoothRotate(currentTile,new Vector3(0.0f,-10.0f,0.0f),true));
			}
			
			if(GUI.Button(new Rect(Screen.width-50,60,40,30),"->"))
			{
				StartCoroutine(smoothRotate(currentTile,new Vector3(0.0f,10.0f,0.0f),true));
			}
			
			if(GUI.Button(new Rect(Screen.width-90,90,40,30),"Up"))
			{
				StartCoroutine(smoothRotate(currentTile,new Vector3(-10.0f,0.0f,0.0f),false));
			}
			
			if(GUI.Button(new Rect(Screen.width-50,90,40,30),"Dw"))
			{
				StartCoroutine(smoothRotate(currentTile,new Vector3(10.0f,0.0f,0.0f),false));
			}
			
			if(GUI.Button(new Rect(Screen.width-90,120,80,30),"INVERT"))
			{
				StartCoroutine(smoothRotate(currentTile,new Vector3(0.0f,0.0f,20.0f),false));
			}

			if(GUI.Button (new Rect(Screen.width-90,150,80,30),"Cancel"))
			{
				if(currentTile)
				{
					Destroy(currentTile);
					ShowLineHelpers(isShowLineHelpers = false);
					helpers_CANTBUILD.transform.position = new Vector3(-1000000.0f,0.0f,-1000000.0f);
				}
				
				erase = false;
			}
		}

		string buildMode = "Standard";

		if(isContinuesBuild)
		{
			buildMode = "Continuous";
		}
		else if(isBuildingMass)
		{
			buildMode = "Mass";
		}

		if(!isBuildingTC)
		{
			if(GUI.Button(new Rect(480,0,210,40),"Build Mode: "+buildMode))
			{
				SwitchBuildMode();
			}
		}

		if(GUI.Button(new Rect(0,Screen.height-40,130,40),"SAVE"))
		{
			//isShowHomeMenu = true;
			
			StartCoroutine(LayersEngine.ReSaveMap());

			#if UNITY_EDITOR
			Debug.Log("[MapEditor] SAVE COMPLETED");
			#endif
			
		}
		
		if(isItNewPattern)
		{
			if(GUI.Button (new Rect(140,Screen.height-40,80,40),"Export"))
			{
				if(currentTile)
					Destroy(currentTile);
				
				exporter.isShow = true;
			}
		}

		if(GUI.Button(new Rect(Screen.width-100,0,100,40),"OPTIONS"))
		{
			if(uteOptions.isShow)
			{
				uteOptions.isShow = false;
			}
			else
			{
				uteOptions.isShow = true;
			}
		}

		if(GUI.Button(new Rect(Screen.width-200,0,90,40),"HELP"))
		{
			if(uteHelp.isShow)
			{
				uteHelp.isShow = false;
			}
			else
			{
				uteHelp.isShow = true;
			}
		}

		if (GUI.Button(new Rect(Screen.width - 350, 0, 140, 40), "BackMenu"))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		if (GUI.Button(new Rect(Screen.width - 500, 0, 140, 40), "DelAll"))
		{
			DeleteAll();
		}

		if (GUI.Button(new Rect(Screen.width - 650, 0, 140, 40), "CheckAllItem"))
		{
			CheckAllSceneItem();
		}

		if (GUI.Button(new Rect(Screen.width - 800, 0, 140, 40), "SetDecoration"))
		{
			_isDecorationDrop = !_isDecorationDrop;
		}
		if(_isDecorationDrop == true)
		{
			for (int i = 0; i < DecorationList.Count; i++)
			{
				if (GUI.Button(new Rect(Screen.width - 800, 50*(i+1), 140, 50), DecorationList[i].ToString()))
				{
					_SetDecoration(DecorationList[i]);
				}
			}
		}

	}

	private string ReturnGameObjectNameIfExists(GameObject obj)
	{
		if(obj)
		{
			return obj.name;
		}
		else
		{
			return "none";
		}
	}

	private void GetMapBoundsInfo()
	{
		float mostLeft = 100000000.0f;
		float mostRight = -100000000.0f;
		float mostForward = -100000000.0f;
		float mostBack = 100000000.0f;
		float mostBottom = 100000000.0f;
		float mostUp = -100000000.0f;

		uteTagObject[] tS = (uteTagObject[]) MAP_STATIC.GetComponentsInChildren<uteTagObject>();

		for(int i=0;i<tS.Length;i++)
		{
			Vector3 tP = ((uteTagObject) tS[i]).gameObject.transform.position;
			Bounds bounds = ((uteTagObject) tS[i]).gameObject.GetComponent<Collider>().bounds;
			float vMLminus = (tP.x-(bounds.size.x/2.0f));
			float vMLplus = (tP.x+(bounds.size.x/2.0f));
			float vMFminus = tP.z-(bounds.size.z/2.0f);
			float vMFplus = tP.z+(bounds.size.z/2.0f);
			float vMUminus = (tP.y-(bounds.size.y/2.0f));
			float vMUplus = (tP.y+(bounds.size.y/2.0f));

			if(vMLminus<mostLeft)
			{
				mostLeft = vMLminus;
			}

			if(vMLplus>mostRight)
			{
				mostRight = vMLplus;
			}

			if(vMFminus<mostBack)
			{
				mostBack = vMFminus;
			}

			if(vMFplus>mostForward)
			{
				mostForward = vMFplus;
			}

			if(vMUminus<mostBottom)
			{
				mostBottom = vMUminus;
			}

			if(vMUplus>mostUp)
			{
				mostUp = vMUplus;
			}
		}

		saveMap.PassMapBounds(
				(float)System.Math.Round(mostLeft,2),
				(float)System.Math.Round(mostRight,2),
				(float)System.Math.Round(mostForward,2),
				(float)System.Math.Round(mostBack,2),
				(float)System.Math.Round(mostUp,2),
				(float)System.Math.Round(mostBottom,2)
		);

		/*
		test.transform.localScale = new Vector3(mostRight-mostLeft,mostUp-mostBottom,mostForward-mostBack);
		test.transform.position = new Vector3(mostLeft+(mostRight-mostLeft)/2.0f,mostBottom+(mostUp-mostBottom)/2.0f,mostBack+(mostForward-mostBack)/2.0f);
		Debug.Log("ML:"+mostLeft+" MR:"+mostRight+" MF:"+mostForward+" MBa:"+mostBack+" MU:"+mostUp+" MBo:"+mostBottom);*/
	}

	public IEnumerator SaveMapFoo()
	{
		GetMapBoundsInfo();

		yield return StartCoroutine(saveMap.SaveMap(newProjectName,isItNewMap));

		yield return 0;
	}
	
	private void SwitchBuildMode()
	{
		if(isContinuesBuild)
		{
			isContinuesBuild = false;
			isBuildingMass = false;
		}
		else
		{
			if(isBuildingMass)
			{
				isContinuesBuild = true;
				isBuildingMass = false;
			}
			else
			{
				isContinuesBuild = false;
				isBuildingMass = true;

				if(currentTile)
				{
					currentTile.transform.localEulerAngles = new Vector3(0,0,0);
				}
			}
		}

		helpers_CANTBUILD.transform.position = new Vector3(-1000000.0f,0.0f,-1000000.0f);
	}

	private IEnumerator smoothRotate(GameObject obj, Vector3 dir, bool isHorizontal)
	{
		if(isHorizontal)
		{
			isRotated++;
		}
		else
		{
			isRotatedH++;
		}

		int counter = 0;

		while(counter++!=9)
		{
			obj.transform.Rotate(dir);
			yield return null;
		}

		if(isHorizontal)
		{
			if(isRotated>=4)
			{
				isRotated = 0;
			}
		}
		else
		{
			if(isRotatedH>=4)
			{
				isRotatedH = 0;
			}
		}
	}

	private IEnumerator gridSmoothMove(GameObject gridObj, bool isUp, GameObject cam)
	{
		canBuild = false;
		isCamGridMoving = true;
		
		Vector3 endP = gridObj.transform.position;
		Vector3 camEP = cam.transform.position;
		Vector3 startP = gridObj.transform.position;
		Vector3 startEP = cam.transform.position;

		if(isUp)
		{
			currentLayer++;
			endP += new Vector3(0.0f,globalYSize,0.0f);
			camEP += new Vector3(0.0f,globalYSize,0.0f);
		}
		else
		{
			currentLayer--;
			endP -= new Vector3(0.0f,globalYSize,0.0f);
			camEP -= new Vector3(0.0f,globalYSize,0.0f);
		}
		
		while(true)
		{
			gridObj.transform.position = Vector3.Lerp(gridObj.transform.position,endP,Time.deltaTime * 20.0f);
			cam.transform.position = Vector3.Lerp(cam.transform.position,camEP,Time.deltaTime * 20.0f);
			
			float dist = Vector3.Distance(gridObj.transform.position,endP);
			
			if(Mathf.Abs(dist)<=0.1f)
			{
				gridObj.transform.position = endP;
				cam.transform.position = camEP;
				break;
			}
			
			yield return null;
		}
		
		if(isUp)
		{
			gridObj.transform.position = startP + new Vector3(0.0f,globalYSize,0.0f);
			cam.transform.position = startEP + new Vector3(0.0f,globalYSize,0.0f);
		}
		else
		{
			gridObj.transform.position = startP - new Vector3(0.0f,globalYSize,0.0f);
			cam.transform.position = startEP - new Vector3(0.0f,globalYSize,0.0f);
		}

		yield return 0;
		canBuild = true;
		isCamGridMoving = false;
	}

	private void InitHelperLines()
	{
		helpers_LINES = new LineRenderer[12];

		GameObject mainLine = GameObject.Find("uteHELPERS/LINE1");
		helpers_LINES[0] = mainLine.GetComponent<LineRenderer>();

		for(int i=0;i<11;i++)
		{
			GameObject newLine = (GameObject) Instantiate(mainLine,transform.position,transform.rotation);
			newLine.name = "LINE"+(i+2).ToString();
			newLine.transform.parent = mainLine.transform.parent;
			helpers_LINES[i+1] = newLine.GetComponent<LineRenderer>();
		}
	}

	private void CalculateLineHelpers()
	{
		int posC = 0;
		Vector3 pos = currentTile.transform.position;
		Vector3 size = currentTile.GetComponent<Collider>().bounds.size;
		Vector3 hsize = size/2.0f;
		Vector3[] newPos = new Vector3[24];
		float lineCastSize = 500.0f;

		// x
		newPos[0] = new Vector3(pos.x-lineCastSize,pos.y+hsize.y,pos.z+hsize.z);
		newPos[1] = new Vector3(pos.x+lineCastSize,pos.y+hsize.y,pos.z+hsize.z);
		newPos[2] = new Vector3(pos.x-lineCastSize,pos.y+hsize.y,pos.z-hsize.z);
		newPos[3] = new Vector3(pos.x+lineCastSize,pos.y+hsize.y,pos.z-hsize.z);
		newPos[4] = new Vector3(pos.x-lineCastSize,pos.y-hsize.y,pos.z-hsize.z);
		newPos[5] = new Vector3(pos.x+lineCastSize,pos.y-hsize.y,pos.z-hsize.z);
		newPos[6] = new Vector3(pos.x-lineCastSize,pos.y-hsize.y,pos.z+hsize.z);
		newPos[7] = new Vector3(pos.x+lineCastSize,pos.y-hsize.y,pos.z+hsize.z);

		// y
		newPos[8] = new Vector3(pos.x+hsize.x,pos.y-lineCastSize,pos.z+hsize.z);
		newPos[9] = new Vector3(pos.x+hsize.x,pos.y+lineCastSize,pos.z+hsize.z);
		newPos[10] = new Vector3(pos.x-hsize.x,pos.y-lineCastSize,pos.z+hsize.z);
		newPos[11] = new Vector3(pos.x-hsize.x,pos.y+lineCastSize,pos.z+hsize.z);
		newPos[12] = new Vector3(pos.x-hsize.x,pos.y-lineCastSize,pos.z-hsize.z);
		newPos[13] = new Vector3(pos.x-hsize.x,pos.y+lineCastSize,pos.z-hsize.z);
		newPos[14] = new Vector3(pos.x+hsize.x,pos.y-lineCastSize,pos.z-hsize.z);
		newPos[15] = new Vector3(pos.x+hsize.x,pos.y+lineCastSize,pos.z-hsize.z);

		// z
		newPos[16] = new Vector3(pos.x+hsize.x,pos.y+hsize.y,pos.z-lineCastSize);
		newPos[17] = new Vector3(pos.x+hsize.x,pos.y+hsize.y,pos.z+lineCastSize);
		newPos[18] = new Vector3(pos.x-hsize.x,pos.y+hsize.y,pos.z-lineCastSize);
		newPos[19] = new Vector3(pos.x-hsize.x,pos.y+hsize.y,pos.z+lineCastSize);
		newPos[20] = new Vector3(pos.x-hsize.x,pos.y-hsize.y,pos.z-lineCastSize);
		newPos[21] = new Vector3(pos.x-hsize.x,pos.y-hsize.y,pos.z+lineCastSize);
		newPos[22] = new Vector3(pos.x+hsize.x,pos.y-hsize.y,pos.z-lineCastSize);
		newPos[23] = new Vector3(pos.x+hsize.x,pos.y-hsize.y,pos.z+lineCastSize);

		for(int i=0;i<12;i++)
		{
			helpers_LINES[i].SetPosition(0,newPos[posC]);
			helpers_LINES[i].SetPosition(1,newPos[posC+1]);

			posC+=2;
		}
	}

	private void ShowLineHelpers(bool isTrue)
	{
		if(isTrue)
		{
			for(int i=0;i<12;i++)
			{
				helpers_LINES[i].enabled = true;
			}
		}
		else
		{
			for(int i=0;i<12;i++)
			{
				helpers_LINES[i].enabled = false;
			}
		}
	}

	private void ResetCamera()
	{
		if(isInTopView)
		{
			cameraMove.isInTopView = false;
			isInTopView = false;
			MAIN.transform.position -= new Vector3(0,5,0);
		}
		
		GameObject cameraYRot = (GameObject) GameObject.Find("MAIN/YArea");
		cameraYRot.transform.localEulerAngles = Vector3.zero;

		if(is2D)
		{
			cameraGO.transform.localEulerAngles = new Vector3(90,0,0);
			MAIN.transform.localEulerAngles = Vector3.zero;
		}
		else
		{
			MAIN.transform.localEulerAngles = new Vector3(0,45,0);
			cameraGO.transform.localEulerAngles = new Vector3(30,0,0);
		}
	}

	public void ResetCurrentTileRotation()
	{
		if(currentTile)
		{
			RoundTo90(currentTile);
		}
	}

	private void RoundTo90(GameObject go)
	{
		Vector3 vec = go.transform.eulerAngles;
		vec.x = Mathf.Round(vec.x / 90) * 90;
		vec.y = Mathf.Round(vec.y / 90) * 90;
		vec.z = Mathf.Round(vec.z / 90) * 90;
		go.transform.eulerAngles = vec;
	}

	private float RoundTo(float point, float toRound = 2.0f)
	{
		point *= toRound;
		point = Mathf.Round(point);
		point /= toRound;

		return point;
	}

	private string ReturnCondition(bool isTrue)
	{
		if(isTrue)
		{
			return "on";
		}
		else
		{
			return "off";
		}
	}

	private void CheckAllSceneItem()
	{
		GameObject main = (GameObject)GameObject.Find("MAP");
		uteTagObject[] allObjects = main.GetComponentsInChildren<uteTagObject>();
		for (int i = 0; i < allObjects.Length; i++)
		{
			GameObject curObj = allObjects[i].gameObject;

			Ray ray = new Ray(curObj.transform.position, - curObj.transform.up);
			RaycastHit rayHit;

			ItemType curItemType = _GetMapItemType(curObj);

			if (Physics.Raycast(ray, out rayHit, 1000))
			{
				if (rayHit.collider)
				{
					GameObject hitObj = rayHit.collider.gameObject;
					//print(hitObj.name);
					if (hitObj.name == "Grid")
					{
						continue;
					}
					ItemType hitType = _GetMapItemType(hitObj);
					if (hitType == curItemType)
					{
						if(curItemType == ItemType.BaseGrid)
						{
							curObj.transform.position = hitObj.transform.position;
							UndoSystem.AddToUndoDestroy(hitObj, hitObj.name, hitObj.transform.position, hitObj.transform.localEulerAngles, true, hitObj.transform.parent.gameObject);
							//Destroy (_baseGrid);
							uteGLOBAL3dMapEditor.mapObjectCount--;
						}
						if (curItemType == ItemType.EnemyRandomArea || curItemType == ItemType.MySetArea)
						{
							curObj.transform.position = hitObj.transform.position;
							UndoSystem.AddToUndoDestroy(hitObj, hitObj.name, hitObj.transform.position, hitObj.transform.localEulerAngles, true, hitObj.transform.parent.gameObject);
							//Destroy (_baseGrid);
							uteGLOBAL3dMapEditor.mapObjectCount--;
						}
					}
				}
			}

		}
	}

	private void DeleteAll()
	{
		GameObject main = (GameObject)GameObject.Find("MAP");
		uteTagObject[] allObjects = main.GetComponentsInChildren<uteTagObject>();
		for (int i = 0; i < allObjects.Length; i++)
		{
			GameObject curObj = allObjects[i].gameObject;
			UndoSystem.AddToUndoDestroy(curObj, curObj.name, curObj.transform.position, curObj.transform.localEulerAngles, true, curObj.transform.parent.gameObject);
			//Destroy (_baseGrid);
			uteGLOBAL3dMapEditor.mapObjectCount--;
		}

	}

	private ItemType _GetMapItemType(GameObject gameObject)
	{
		ItemType curType = ItemType.GridEffect;
		MapItemType curTypeComponent = gameObject.GetComponentInChildren<MapItemType>();
		if (curTypeComponent)
		{
			curType = curTypeComponent.curItemType;
		}
		return curType;
	}

	private void _SetDecoration(int decorationID)
	{
		GameObject main = (GameObject)GameObject.Find("MAP");
		uteTagObject[] allObjects = main.GetComponentsInChildren<uteTagObject>();
		for (int i = 0; i < allObjects.Length; i++)
		{
			GameObject curObj = allObjects[i].gameObject;
			ItemType curItemType = _GetMapItemType(curObj);
			if (curItemType != ItemType.BaseGrid)
			{
				continue;
			}

			string objName = curObj.name;
			string[] splitInfo = objName.Split("_"[0]);
			string BaseName = splitInfo[0];
			//if (splitInfo.Length >1 && splitInfo[1] != null)
			//{
			//	int curDecoration = System.Convert.ToInt32(splitInfo[1]);
			//	if (curDecoration == decorationID)
			//		continue;
			//}
			GameObject prefabObj;
			if (decorationID == 0)
			{
				prefabObj = AssetDatabase.LoadAssetAtPath($"Assets/LoadableResources/Grids/{decorationID}/{BaseName}.prefab", typeof(GameObject)) as GameObject;
				if(prefabObj == null)
				{
					prefabObj = AssetDatabase.LoadAssetAtPath($"Assets/LoadableResources/Grids/{decorationID}/{BaseName}_{decorationID}.prefab", typeof(GameObject)) as GameObject;
				}
			}
			else
			{
				prefabObj = AssetDatabase.LoadAssetAtPath($"Assets/LoadableResources/Grids/{decorationID}/{BaseName}_{decorationID}.prefab", typeof(GameObject)) as GameObject;
			}

			if (prefabObj != null)
			{
				GameObject child = curObj.transform.GetChild(0).gameObject;
				GameObject.Instantiate(prefabObj, child.transform.position, child.transform.rotation, curObj.transform);
				Destroy(child);
			}

		}
	}

#endif
}
