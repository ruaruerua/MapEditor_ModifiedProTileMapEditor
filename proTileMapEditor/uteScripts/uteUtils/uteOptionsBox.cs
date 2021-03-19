using UnityEngine;
using System.Collections;
using System.IO;

public class uteOptionsBox : MonoBehaviour {
#if UNITY_EDITOR
	[HideInInspector]
	public bool isShow;
	private GUISkin ui;
	private uteMapEditorEngine MapEngine;

	//options
	[HideInInspector]
	public bool isEditorLightOn;
	[HideInInspector]
	public bool isShowGrid;
	[HideInInspector]
	public bool isCastShadows;
	[HideInInspector]
	public bool snapOnTop;
	[HideInInspector]
	public bool isShow3DGrid;
	[HideInInspector]
	public bool isUse360Snap;

	private void Start()
	{
		isUse360Snap = false;
		isShow3DGrid = false;
		isCastShadows = false;
		isEditorLightOn = true;
		isShowGrid = true;
		ui = (GUISkin) Resources.Load("uteForEditor/uteUI");
		isShow = false;
		MapEngine = (uteMapEditorEngine) this.gameObject.GetComponent<uteMapEditorEngine>();

		if(MapEngine.yTypeOption.Equals("auto"))
		{
			snapOnTop = true;
		}
		else
		{
			snapOnTop = false;
		}
	}

	private void OnGUI()
	{
		if(isShow)
		{
			GUI.skin = ui;
			GUI.Box(new Rect(Screen.width-300,40,200,350),"OPTIONS");
			
			GUI.Label(new Rect(Screen.width-294,70,120,30),"Editor Light ");
			if(GUI.Button(new Rect(Screen.width-170,70,60,25),ReturnCondition(isEditorLightOn)))
			{
				if(isEditorLightOn)
				{
					SetEditorLight(false);
				}
				else
				{
					SetEditorLight(true);
				}
			}

			GUI.Label(new Rect(Screen.width-294,100,120,30),"Shadows ");
			if(GUI.Button(new Rect(Screen.width-170,100,60,25),ReturnCondition(isCastShadows)))
			{
				if(isEditorLightOn)
				{
					if(isCastShadows)
					{
						SetCastShadows(false);
					}
					else
					{
						SetCastShadows(true);
					}
				}
				else
				{
					Debug.Log("Enable lights first!");
				}
			}

			GUI.Label(new Rect(Screen.width-294,130,120,30),"XZ Snapping ");
			if(GUI.Button(new Rect(Screen.width-170,130,60,25),ReturnCondition(uteGLOBAL3dMapEditor.XZsnapping)))
			{
				if(uteGLOBAL3dMapEditor.XZsnapping)
				{
					SetXZSnapping(false);
				}
				else
				{
					SetXZSnapping(true);
				}
			}

			GUI.Label(new Rect(Screen.width-294,160,130,30),"Overlap Detection ");
			if(GUI.Button(new Rect(Screen.width-170,160,60,25),ReturnCondition(uteGLOBAL3dMapEditor.OverlapDetection)))
			{
				if(uteGLOBAL3dMapEditor.OverlapDetection)
				{
					SetOverlapDetection(false);
				}
				else
				{
					SetOverlapDetection(true);
				}
			}

			GUI.Label(new Rect(Screen.width-294,190,130,30),"Show Grid");
			if(GUI.Button(new Rect(Screen.width-170,190,60,25),ReturnCondition(isShowGrid)))
			{
				if(isShowGrid)
				{
					SetShowGrid(false);
				}
				else
				{
					SetShowGrid(true);
				}
			}

			GUI.Label(new Rect(Screen.width-294,220,130,30),"Calculate XZ Pivot");
			if(GUI.Button(new Rect(Screen.width-170,220,60,25),ReturnCondition(uteGLOBAL3dMapEditor.CalculateXZPivot)))
			{
				if(uteGLOBAL3dMapEditor.CalculateXZPivot)
				{
					SetCalculateXZPivot(false);
				}
				else
				{
					SetCalculateXZPivot(true);
				}
			}

			GUI.Label(new Rect(Screen.width-294,250,130,30),"Snap on TOP");
			if(GUI.Button(new Rect(Screen.width-170,250,60,25),ReturnCondition(snapOnTop)))
			{
				if(snapOnTop)
				{
					SetSnapOnTop(false);
				}
				else
				{
					SetSnapOnTop(true);
				}
			}

			GUI.Label(new Rect(Screen.width-294,280,130,30),"Show 3D Grid");
			if(GUI.Button(new Rect(Screen.width-170,280,60,25),ReturnCondition(isShow3DGrid)))
			{
				if(isShow3DGrid)
				{
					Set3DGrid(false);
				}
				else
				{
					Set3DGrid(true);
				}
			}

			GUI.Label(new Rect(Screen.width-294,310,130,30),"360 Snap (Alpha)");
			if(GUI.Button(new Rect(Screen.width-170,310,60,25),ReturnCondition(isUse360Snap)))
			{
				if(isUse360Snap)
				{
					SetUse360Snap(false);
					SetOverlapDetection(true);
				}
				else
				{
					SetOverlapDetection(false);
					SetUse360Snap(true);
				}
			}

			if(GUI.Button(new Rect(Screen.width-280,350,160,30),"CLOSE"))
			{
				isShow = false;
			}
		}
	}

	private void SetUse360Snap(bool isTrue)
	{
		MapEngine.ResetCurrentTileRotation();

		if(isTrue)
		{
			MapEngine.isUse360Snap = true;
			isUse360Snap = true;
		}
		else
		{
			MapEngine.isUse360Snap = false;
			isUse360Snap = false;
		}
	}

	private void Set3DGrid(bool isTrue)
	{
		if(isTrue)
		{
			MapEngine.grid3d.SetActive(true);
			isShow3DGrid = true;
		}
		else
		{
			MapEngine.grid3d.SetActive(false);
			isShow3DGrid = false;
		}
	}

	private void SetEditorLight(bool isTrue)
	{
		if(isTrue)
		{
			isEditorLightOn = true;
			MapEngine.mapLightGO.SetActive(true);
		}
		else
		{
			isEditorLightOn = false;
			MapEngine.mapLightGO.SetActive(false);
		}
	}

	private void SetCastShadows(bool isTrue)
	{
		RenderSettings.ambientLight = new Color(0.5f,0.5f,0.5f,1f);
		
		if(isTrue)
		{
			MapEngine.mapLightGO.GetComponent<Light>().shadows = LightShadows.Hard;
			MapEngine.mapLightGO.GetComponent<Light>().shadowStrength = 0.8f;
			MapEngine.mapLightGO.GetComponent<Light>().shadowBias = 0.01f;

			QualitySettings.shadowDistance = 100;
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowProjection = ShadowProjection.CloseFit;
			isCastShadows = true;
		}
		else
		{
			isCastShadows = false;
			MapEngine.mapLightGO.GetComponent<Light>().shadows = LightShadows.None;
		}
	}

	private void SetXZSnapping(bool isTrue)
	{
		if(isTrue)
		{
			uteGLOBAL3dMapEditor.XZsnapping = true;
		}
		else
		{
			uteGLOBAL3dMapEditor.XZsnapping = false;
		}
	}

	private void SetOverlapDetection(bool isTrue)
	{
		if(isTrue)
		{
			uteGLOBAL3dMapEditor.OverlapDetection = true;
		}
		else
		{
			uteGLOBAL3dMapEditor.OverlapDetection = false;
		}
	}

	private void SetShowGrid(bool isTrue)
	{
		if(isTrue)
		{
			isShowGrid = true;
			MapEngine.grid.SetActive(true);
		}
		else
		{
			isShowGrid = false;
			MapEngine.grid.SetActive(false);
		}
	}

	private void SetCalculateXZPivot(bool isTrue)
	{
		if(isTrue)
		{
			uteGLOBAL3dMapEditor.CalculateXZPivot = true;
		}
		else
		{
			uteGLOBAL3dMapEditor.CalculateXZPivot = false;
		}
	}

	private void SetSnapOnTop(bool isTrue)
	{
		if(isTrue)
		{
			snapOnTop = true;
			MapEngine.yTypeOption = "auto";
		}
		else
		{
			snapOnTop = false;
			MapEngine.yTypeOption = "fixed";
		}
	}

	public IEnumerator LoadOptions()
	{
		string path = uteGLOBAL3dMapEditor.getMapsDir();
		
		if(System.IO.File.Exists(path+MapEngine.newProjectName+"_info.txt"))
		{
			StreamReader sr = new StreamReader(path+MapEngine.newProjectName+"_info.txt");
			string info = sr.ReadToEnd();
			sr.Close();
			string[] allinfo = info.Split(":"[0]);

			if(allinfo.Length>10)
			{
				SetEditorLight(StrToBool(allinfo[12]));
				SetCastShadows(StrToBool(allinfo[13]));
				SetXZSnapping(StrToBool(allinfo[14]));
				SetOverlapDetection(StrToBool(allinfo[15]));
				SetShowGrid(StrToBool(allinfo[16]));
				SetCalculateXZPivot(StrToBool(allinfo[17]));
				SetSnapOnTop(StrToBool(allinfo[18]));

//				Debug.Log("---");
				if(allinfo.Length>26)
				{
				//	Debug.Log(allinfo[25]);
					SetUse360Snap(StrToBool(allinfo[25]));
				}
			}
		}

		yield return 0;
	}

	private bool StrToBool(string str)
	{
//		Debug.Log(str);
		if(str.Equals("True"))
		{
			return true;
		}
		else if(str.Equals("False"))
		{
			return false;
		}

		//Debug.LogError("Error!");

		return false;
	}

	private string ReturnCondition(bool isTrue)
	{
		if(isTrue)
		{
			return "ON";
		}
		else
		{
			return "OFF";
		}
	}

#endif
}
