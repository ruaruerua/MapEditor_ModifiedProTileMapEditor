using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class uteLayersEngine : MonoBehaviour {
#if UNITY_EDITOR
#pragma warning disable 0618
	private string mapLPath;
	[HideInInspector]
	public List<Layer> AllLayers = new List<Layer>();
	[HideInInspector]
	public bool isShow;
	[HideInInspector]
	public int selectedLayer;
	private GUISkin skin;
	[HideInInspector]
	public bool showLayerOptions;
	private int showLayerOptionsID;
	private string m_mapName;
	private string newLayerName;
	private uteMapEditorEngine MEE;
	private GameObject MAP_STATIC;
	private GameObject MAP_DYNAMIC;
	[HideInInspector]
	public bool isMergingLayers;
	[HideInInspector]
	public Rect boxRect;
	private int boxFull;
	[HideInInspector]
	public bool isCreatingNewLayer;
	private string brandNewLayerName;
	private List<int> collectInvisibleLayerIDs = new List<int>();
	private Texture2D icon_eyeIcon_y;
	private Texture2D icon_eyeIcon_n;
	private Texture2D icon_cubeIcon_y;
	private Texture2D icon_cubeIcon_n;
	private Texture2D icon_editIMG;

	public class Layer
	{
		public string layerName;
		public bool isVisible;
		public bool isIgnoreCollision;
		public List<GameObject> hiddenObjects = new List<GameObject>();
		public List<GameObject> ignoredObjects = new List<GameObject>();

		public Layer(string _layerName)
		{
			isVisible = true;
			isIgnoreCollision = false;
			layerName = _layerName;
		}
	}

	private void Awake()
	{
		icon_eyeIcon_y = (Texture2D) Resources.Load("uteForEditor/uteUI/uteEyeIcon_y");
		icon_eyeIcon_n = (Texture2D) Resources.Load("uteForEditor/uteUI/uteEyeIcon_n");
		icon_cubeIcon_y = (Texture2D) Resources.Load("uteForEditor/uteUI/uteCubeIcon_y");
		icon_cubeIcon_n = (Texture2D) Resources.Load("uteForEditor/uteUI/uteCubeIcon_n");
		icon_editIMG = (Texture2D) Resources.Load("uteForEditor/uteUI/editimg");
	}

	public string GetCurrentLayersName()
	{
		return AllLayers[selectedLayer].layerName;
	}

	public bool isCurrentLayerVisible()
	{
		return AllLayers[selectedLayer].isVisible;
	}

	public void ReadLayersFromMap(string mapName, GameObject _MAP_STATIC, GameObject _MAP_DYNAMIC, uteMapEditorEngine _MEE)
	{
		MEE = _MEE;
		MAP_STATIC = _MAP_STATIC;
		MAP_DYNAMIC = _MAP_DYNAMIC;
		newLayerName = "";
		brandNewLayerName = "";
		m_mapName = mapName;
		showLayerOptions = false;
		isMergingLayers = false;
		isCreatingNewLayer = false;
		showLayerOptionsID = -1;
		isShow = true;
		skin = (GUISkin) Resources.Load("uteForEditor/uteUI");

		mapLPath = uteGLOBAL3dMapEditor.getMapsDir()+mapName+"_layers.txt";

		if(!File.Exists(mapLPath))
		{
			StreamWriter sw = new StreamWriter(mapLPath);
			sw.Write("DEFAULT:");
			sw.Flush();
			sw.Close();
		}

		StreamReader sr = new StreamReader(mapLPath);
		string allInfo = sr.ReadToEnd();
		sr.Close();

		string[] infoParts = allInfo.Split(":"[0]);

		for(int i=0;i<infoParts.Length;i++)
		{	
			string info = infoParts[i];

			if(!info.Equals(""))
			{
				AllLayers.Add(new Layer(info));
			}
		}

		if(AllLayers.Count<=0)
		{
			RestoreLayersToDefault();
			return;
		}

		if(!AllLayers[0].layerName.Equals("DEFAULT"))
		{
			AllLayers[0].layerName = "DEFAULT";
			RewriteLayersFromArrayToFile();
		}
	}

	public void CheckIfAllTilesHasExistingLayer()
	{
		bool needsRewrite = false;
		uteTagObject[] uteS = (uteTagObject[]) MAP_STATIC.GetComponentsInChildren<uteTagObject>();
		uteTagObject[] uteD = (uteTagObject[]) MAP_DYNAMIC.GetComponentsInChildren<uteTagObject>();

		for(int i=0;i<uteS.Length;i++)
		{
			uteTagObject m_uteS = (uteTagObject) uteS[i];
			if(!isLayerExists(m_uteS.layerName))
			{
				needsRewrite = true;
				m_uteS.layerName = AllLayers[0].layerName;
			}

		}

		for(int i=0;i<uteD.Length;i++)
		{
			uteTagObject m_uteD = (uteTagObject) uteD[i];
			if(!isLayerExists(m_uteD.layerName))
			{
				needsRewrite = true;
				m_uteD.layerName = AllLayers[0].layerName;
			}
		}

		if(needsRewrite)
		{
			Debug.Log("[LayersEngine] Some of the Tiles has non-existing layer, these tiles will be merged to DEFAULT layer.");
			AllLayers.Clear();
			ReadLayersFromMap(m_mapName,MAP_STATIC,MAP_DYNAMIC,MEE);
			StartCoroutine(ReSaveMap());
		}
	}

	public IEnumerator ReSaveMap()
	{
		if(collectInvisibleLayerIDs.Count>0)
		{
			collectInvisibleLayerIDs.Clear();
		}

		for(int i=0;i<AllLayers.Count;i++)
		{
			if(!AllLayers[i].isVisible)
			{
				collectInvisibleLayerIDs.Add(i);
				ShowObjectsWithLayer(i);
			}
		}

		yield return StartCoroutine(MEE.SaveMapFoo());

		for(int i=0;i<collectInvisibleLayerIDs.Count;i++)
		{
			HideObjectsWithLayer(collectInvisibleLayerIDs[i]);
		}

		yield return 0;
	}

	private bool isLayerExists(string layerName)
	{
		for(int i=0;i<AllLayers.Count;i++)
		{
			if(AllLayers[i].layerName.Equals(layerName))
			{
				return true;
			}
		}

		return false;
	}

	private void HideObjectsWithLayer(int id)
	{
		uteTagObject[] uteS = (uteTagObject[]) MAP_STATIC.GetComponentsInChildren<uteTagObject>();
		uteTagObject[] uteD = (uteTagObject[]) MAP_DYNAMIC.GetComponentsInChildren<uteTagObject>();

		for(int i=0;i<uteS.Length;i++)
		{
			uteTagObject m_uteS = (uteTagObject) uteS[i];

			if(m_uteS.layerName.Equals(AllLayers[id].layerName))
			{
				AllLayers[id].hiddenObjects.Add(m_uteS.gameObject);
				m_uteS.gameObject.SetActive(false);
			}
		}

		for(int i=0;i<uteD.Length;i++)
		{
			uteTagObject m_uteD = (uteTagObject) uteD[i];

			if(m_uteD.layerName.Equals(AllLayers[id].layerName))
			{
				AllLayers[id].hiddenObjects.Add(m_uteD.gameObject);
				m_uteD.gameObject.SetActive(false);
			}
		}
	}

	private void ShowObjectsWithLayer(int id)
	{
		for(int i=0;i<AllLayers[id].hiddenObjects.Count;i++)
		{
			AllLayers[id].hiddenObjects[i].active = true;
		}

		AllLayers[id].hiddenObjects.Clear();
	}

	private void IgnoreCollisionWithLayer(int id)
	{
		uteTagObject[] uteS = (uteTagObject[]) MAP_STATIC.GetComponentsInChildren<uteTagObject>();
		uteTagObject[] uteD = (uteTagObject[]) MAP_DYNAMIC.GetComponentsInChildren<uteTagObject>();

		for(int i=0;i<uteS.Length;i++)
		{
			uteTagObject m_uteS = (uteTagObject) uteS[i];

			if(m_uteS.layerName.Equals(AllLayers[id].layerName))
			{
				AllLayers[id].ignoredObjects.Add(m_uteS.gameObject);
				m_uteS.gameObject.layer = 2;
			}
		}

		for(int i=0;i<uteD.Length;i++)
		{
			uteTagObject m_uteD = (uteTagObject) uteD[i];

			if(m_uteD.layerName.Equals(AllLayers[id].layerName))
			{
				AllLayers[id].ignoredObjects.Add(m_uteD.gameObject);
				m_uteD.gameObject.layer = 2;
			}
		}
	}

	private void UnIgnoreCollisionWithLayer(int id)
	{
		for(int i=0;i<AllLayers[id].ignoredObjects.Count;i++)
		{
			AllLayers[id].ignoredObjects[i].layer = 0;
		}

		AllLayers[id].ignoredObjects.Clear();
	}

	private void RestoreLayersToDefault()
	{
		StreamWriter sw = new StreamWriter(mapLPath);
		sw.Write("DEFAULT:");
		sw.Flush();
		sw.Close();

		ReadLayersFromMap(m_mapName,MAP_STATIC,MAP_DYNAMIC,MEE);
	}

	private void DeleteLayer(int id)
	{
		uteTagObject[] uteS = (uteTagObject[]) MAP_STATIC.GetComponentsInChildren<uteTagObject>();
		uteTagObject[] uteD = (uteTagObject[]) MAP_DYNAMIC.GetComponentsInChildren<uteTagObject>();
		
		for(int i=0;i<uteS.Length;i++)
		{
			uteTagObject m_uteS = (uteTagObject) uteS[i];

			if(m_uteS.layerName.Equals(AllLayers[id].layerName))
			{
				Destroy(m_uteS.gameObject);
			}
		}

		for(int i=0;i<uteD.Length;i++)
		{
			uteTagObject m_uteD = (uteTagObject) uteD[i];

			if(m_uteD.layerName.Equals(AllLayers[id].layerName))
			{
				Destroy(m_uteD.gameObject);
			}
		}

		AllLayers.RemoveAt(id);

		if(selectedLayer==id)
		{
			if(selectedLayer>0)
			{
				selectedLayer--;
			}
		}

		RewriteLayersFromArrayToFile();
	}

	private void MergeLayerTo(int idA, int idB)
	{
		if(!AllLayers[idA].isVisible||!AllLayers[idB].isVisible)
		{
			Debug.Log("[LayersEngine] In order to merge layers, both layers must be visible.");
			return;
		}

		if(idA==idB)
		{
			Debug.Log("[LayersEngine] Can't merge to same Layer.");
			return;
		}

		uteTagObject[] uteS = (uteTagObject[]) MAP_STATIC.GetComponentsInChildren<uteTagObject>();
		uteTagObject[] uteD = (uteTagObject[]) MAP_DYNAMIC.GetComponentsInChildren<uteTagObject>();

		for(int i=0;i<uteS.Length;i++)
		{
			uteTagObject m_uteS = (uteTagObject) uteS[i];

			if(m_uteS.layerName.Equals(AllLayers[idA].layerName))
			{
				m_uteS.layerName = AllLayers[idB].layerName;
			}
		}

		for(int i=0;i<uteD.Length;i++)
		{
			uteTagObject m_uteD = (uteTagObject) uteD[i];

			if(m_uteD.layerName.Equals(AllLayers[idA].layerName))
			{
				m_uteD.layerName = AllLayers[idB].layerName;
			}
		}

		DeleteLayer(idA);
	}

	private void RenameLayer(int id, string newName)
	{
		uteTagObject[] uteS = (uteTagObject[]) MAP_STATIC.GetComponentsInChildren<uteTagObject>();
		uteTagObject[] uteD = (uteTagObject[]) MAP_DYNAMIC.GetComponentsInChildren<uteTagObject>();

		for(int i=0;i<uteS.Length;i++)
		{
			uteTagObject m_uteS = (uteTagObject) uteS[i];

			if(m_uteS.layerName.Equals(AllLayers[id].layerName))
			{
				m_uteS.layerName = newName;
			}
		}

		for(int i=0;i<uteD.Length;i++)
		{
			uteTagObject m_uteD = (uteTagObject) uteD[i];

			if(m_uteD.layerName.Equals(AllLayers[id].layerName))
			{
				m_uteD.layerName = newName;
			}
		}

		for(int i=0;i<AllLayers.Count;i++)
		{
			if(AllLayers[i].layerName.Equals(AllLayers[id].layerName))
			{
				AllLayers[i].layerName = newName;
				break;
			}
		}

		RewriteLayersFromArrayToFile();
	}

	private void RewriteLayersFromArrayToFile()
	{
		string allNames = "";

		for(int i=0;i<AllLayers.Count;i++)
		{
			allNames += AllLayers[i].layerName+":";
		}

		StreamWriter sw = new StreamWriter(mapLPath);
		sw.Write("");
		sw.Write(allNames);
		sw.Flush();
		sw.Close();
	}

	private bool isLayerAlreadyExists(string layerName)
	{
		for(int i=0;i<AllLayers.Count;i++)
		{
			if(AllLayers[i].layerName.Equals(layerName))
			{
				return true;
			}
		}

		return false;
	}

	private void CreateNewLayer(string newLayerName)
	{
		AllLayers.Add(new Layer(newLayerName));

		RewriteLayersFromArrayToFile();

		selectedLayer = AllLayers.Count-1;
	}

	public string GetSelectedLayersName()
	{
		if(AllLayers[selectedLayer]!=null)
		{
			return AllLayers[selectedLayer].layerName;
		}
		else
		{
			return "none";
		}
	}

	private void OnGUI()
	{
		if(MEE.editorIsLoading)
		{
			return;
		}
		
		if(MEE.uteOptions.isShow)
		{
			return;
		}

		GUI.skin = skin;

		if(isCreatingNewLayer)
		{
			GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-100,300,150),"Create new Layer");
			GUI.Label(new Rect(Screen.width/2-140,Screen.height/2-70,135,30),"Layer Name: ");
			brandNewLayerName = GUI.TextField(new Rect(Screen.width/2-30,Screen.height/2-70,170,25),brandNewLayerName);

			if(GUI.Button(new Rect(Screen.width/2-140,Screen.height/2-30,280,30),"Create"))
			{
				brandNewLayerName = brandNewLayerName.Replace(":","");
				brandNewLayerName = brandNewLayerName.Replace(" ","");
				brandNewLayerName = brandNewLayerName.Replace("\n","");
				brandNewLayerName = brandNewLayerName.Replace("\r","");
				brandNewLayerName = brandNewLayerName.Replace("$","");

				if(!isLayerAlreadyExists(brandNewLayerName)&&!brandNewLayerName.Equals(""))
				{
					CreateNewLayer(brandNewLayerName);
					brandNewLayerName = "";
					isCreatingNewLayer = false;
				}
				else
				{
					if(brandNewLayerName.Equals(""))
					{
						Debug.Log("[LayersEngine] Layer name can't be empty.");
					}
					else
					{
						Debug.Log("[LayersEngine] Layer with this name already exists.");
					}
				}
			}

			if(GUI.Button(new Rect(Screen.width/2-140,Screen.height/2+5,280,30),"Close"))
			{
				brandNewLayerName = "";
				isCreatingNewLayer = false;
			}
		}
		else if(isMergingLayers)
		{
			GUI.Box(new Rect(Screen.width/2-200,Screen.height/2-300,400,300),"["+AllLayers[showLayerOptionsID].layerName+"] layer options");

			int x = 0;
			for(int i=0;i<AllLayers.Count;i++)
			{
				if(i!=showLayerOptionsID)
				{
					x++;
					if(GUI.Button(new Rect(Screen.width/2-190,Screen.height/2-270+(x*35),380,30),"Merge TO: "+AllLayers[i].layerName))
					{
						MergeLayerTo(showLayerOptionsID,i);
						showLayerOptions = false;
						showLayerOptionsID = -1;
						StartCoroutine(ReSaveMap());
						isMergingLayers = false;
					}
				}
			}
		}
		else if(showLayerOptions)
		{
			GUI.Box(new Rect(Screen.width/2-200,Screen.height/2-300,400,300),"["+AllLayers[showLayerOptionsID].layerName+"] layer options");

			if(GUI.Button(new Rect(Screen.width/2-190,Screen.height/2-40,380,30),"CLOSE"))
			{
				showLayerOptions = false;
				showLayerOptionsID = -1;
			}

			if(GUI.Button(new Rect(Screen.width/2-190,Screen.height/2-270,380,30),"Delete Layer & Containing Objects"))
			{
				DeleteLayer(showLayerOptionsID);
				showLayerOptions = false;
				showLayerOptionsID = -1;
				StartCoroutine(ReSaveMap());
			}

			if(GUI.Button(new Rect(Screen.width/2-190,Screen.height/2-220,380,30),"Delete Layer & Merge Objects to DEFAULT"))
			{
				MergeLayerTo(showLayerOptionsID,0);
				showLayerOptions = false;
				showLayerOptionsID = -1;
				StartCoroutine(ReSaveMap());
			}

			if(GUI.Button(new Rect(Screen.width/2-190,Screen.height/2-170,380,30),"Merge Layer TO..."))
			{
				isMergingLayers = true;
			}

			newLayerName = GUI.TextField(new Rect(Screen.width/2-190,Screen.height/2-120,190,25),newLayerName);

			if(GUI.Button(new Rect(Screen.width/2+10,Screen.height/2-120,180,30),"Rename"))
			{
				if(!newLayerName.Equals(AllLayers[showLayerOptionsID].layerName))
				{
					newLayerName = newLayerName.Replace(" ","");
					newLayerName = newLayerName.Replace(":","");
					newLayerName = newLayerName.Replace("\n","");
					newLayerName = newLayerName.Replace("\r","");
					newLayerName = newLayerName.Replace("$","");

					if(!isLayerAlreadyExists(newLayerName)&&!newLayerName.Equals(""))
					{
						Debug.Log("[LayersEngine] Changed Layer name from "+AllLayers[showLayerOptionsID].layerName+" to "+newLayerName+".");
						RenameLayer(showLayerOptionsID,newLayerName);
						showLayerOptions = false;
						showLayerOptionsID = -1;
						newLayerName = "";
						StartCoroutine(ReSaveMap());
					}
					else
					{
						if(!newLayerName.Equals(""))
						{
							Debug.Log("[LayersEngine] Layer with name already exists.");
						}
						else
						{
							Debug.Log("[LayersEngine] Layer name can't be empty.");
						}
					}
				}
				else
				{
					Debug.Log("[LayersEngine] Nothing to change.");
				}
			}

			return;
		}

		if(isShow&&selectedLayer>-1&&selectedLayer<AllLayers.Count)
		{
			if(AllLayers.Count>=10)
			{
				boxFull = 30;
			}
			else
			{
				boxFull = 55;
			}

			boxRect = new Rect(Screen.width-190,300,190,(AllLayers.Count*25)+boxFull);

			GUI.Box(boxRect,"Layers ("+AllLayers[selectedLayer].layerName+") ");

			if(AllLayers.Count<10)
			{
				if(GUI.Button(new Rect(Screen.width-160,325+(AllLayers.Count*25),155,23),"+ New Layer"))
				{
					isCreatingNewLayer = true;
				}
			}
			
			for(int i=0;i<AllLayers.Count;i++)
			{
				Layer layer = (Layer) AllLayers[i];

				if(selectedLayer==i)
				{
					GUI.Button(new Rect(Screen.width-110,320+(i*25),105,25),"["+layer.layerName+"]");
				}
				else
				{
					if(GUI.Button(new Rect(Screen.width-110,320+(i*25),105,25),layer.layerName))
					{
						selectedLayer = i;
					}
				}
				
				if(layer.isVisible)
				{
					if(GUI.Button(new Rect(Screen.width-185,320+(i*25),25,25),icon_eyeIcon_y))
					{
						HideObjectsWithLayer(i);
						layer.isVisible = false;
					}
				}
				else
				{
					if(GUI.Button(new Rect(Screen.width-185,320+(i*25),25,25),icon_eyeIcon_n))
					{
						ShowObjectsWithLayer(i);
						layer.isVisible = true;
					}
				}

				if(layer.isVisible)
				{
					if(!layer.isIgnoreCollision)
					{
						if(GUI.Button(new Rect(Screen.width-160,320+(i*25),25,25),icon_cubeIcon_y))
						{
							IgnoreCollisionWithLayer(i);
							layer.isIgnoreCollision = true;
						}
					}
					else
					{
						if(GUI.Button(new Rect(Screen.width-160,320+(i*25),25,25),icon_cubeIcon_n))
						{
							UnIgnoreCollisionWithLayer(i);
							layer.isIgnoreCollision = false;
						}
					}
				}

				if(!layer.layerName.Equals("DEFAULT")&&layer.isVisible)
				{
					if(GUI.Button(new Rect(Screen.width-135,320+(i*25),25,25),icon_editIMG))
					{
						newLayerName = layer.layerName;
						showLayerOptions = true;
						showLayerOptionsID = i;
					}
				}
			}
		}
		else if(selectedLayer>-1&&selectedLayer<AllLayers.Count)
		{
			boxRect = new Rect(Screen.width-125,300,125,24);
			GUI.Box(boxRect,AllLayers[selectedLayer].layerName);
		}

		if(GUI.Button(new Rect(Screen.width-20,300,18,18),"-"))
		{
			if(isShow)
			{
				isShow = false;
			}
			else
			{
				isShow = true;
			}
		}
	}
#endif
}