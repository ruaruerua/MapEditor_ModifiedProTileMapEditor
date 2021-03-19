using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;


public class uteTileConnectionsEditor : EditorWindow {

	private int selGridInt = 0;
	private int lastSelGridInt = 0;
    private string[] tcNamesStr;
    private string newTileConnectionName = "name";
    private string path;
    private bool firstTime = true;
    private ArrayList tcNames = new ArrayList();
    private ArrayList tcObjs = new ArrayList();
    private ArrayList tcRots = new ArrayList();
    private ArrayList currentObjs = new ArrayList();
    private ArrayList currentRots = new ArrayList();
    private uteComboBox[] rotsComboBox = new uteComboBox[5] { new uteComboBox(), new uteComboBox(), new uteComboBox(), new uteComboBox(), new uteComboBox() };
    private GUIStyle listStyle = new GUIStyle();
    private GUIContent[] rotsComboInfo = new GUIContent[5];
    private int maxTileCount = 5;
    private int[] selectedRotIndexes = new int[5] {0, 0, 0, 0, 0};
    private int[] lastSelectedRotIndexes = new int[5] {-1,-1,-1,-1,-1};

    // tiles info
    private Texture2D[] tImgs = new Texture2D[5];
    private string[] tStrs = new string[5];

	[MenuItem ("Window/proTileMapEditor/Tile Connections",false,4)]
	static void Init()
	{
		uteTileConnectionsEditor window = (uteTileConnectionsEditor)uteTileConnectionsEditor.GetWindow (typeof (uteTileConnectionsEditor));
		window.Show();
	}

	private void OnFocus()
	{
		path = AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.uteTileConnectionstxt);
		LoadInfo(true);
		LoadTileConnectionInfoByID(selGridInt);
		SetComboBoxes();
		this.Repaint();
	}

	private void OnGUI()
	{
		ReloadComboBox();
		if(firstTime)
		{
			LoadInfo();
			firstTime = false;

			if(tcNames.Count>0)
			{
				LoadTileConnectionInfoByID(selGridInt);
				SetComboBoxes();
			}
		}

		if(selGridInt!=lastSelGridInt)
		{
			lastSelGridInt = selGridInt;
			LoadTileConnectionInfoByID(selGridInt);
			SetComboBoxes();
			this.Repaint();
		}

		GUI.Label(new Rect(10,10,200,30),"Create new Tile Connection: ");
		newTileConnectionName = GUI.TextField(new Rect(170,10,120,18),newTileConnectionName);

		if(GUI.Button(new Rect(300,10,80,20),"Create"))
		{
			CreateNewTileConnection(newTileConnectionName);
		}

		int x = 0;
		int y = 0;
		int tileBoxSize = 140;
		int tileBoxSizePad = tileBoxSize+5;
		int clickedIndex = -1;

		if(tcNames.Count>0)
		{
			GUI.Box(new Rect(130,40,tileBoxSizePad*3+15,tileBoxSizePad*2+55),tcNames[selGridInt].ToString());
			GUI.Label(new Rect(130,tileBoxSizePad*2+100,400,30),"IMPORTANT: For tile-connections to work, all 5 tiles must be set up.");
			GUI.Box(new Rect(431,206,140,140),"Default Y Rotations");

			for(int i=0;i<maxTileCount;i++)
			{
				if(lastSelectedRotIndexes[i]!=rotsComboBox[i].selectedItemIndex)
				{
					lastSelectedRotIndexes[i] = rotsComboBox[i].selectedItemIndex;
					if(!firstTime)
						RewriteAllInfoToFile();
				}
			}
			
			if(currentRots.Count>0)
			{
				for(int j=0;j<maxTileCount;j++)
				{
					if(rotsComboBox[j].isClickedComboButton)
					{
						clickedIndex = j;
					}
				}

				for(int i=0;i<maxTileCount;i++)
				{
					selectedRotIndexes[i] = rotsComboBox[i].GetSelectedItemIndex();
					GUI.Label(new Rect(436,230+(i*20),110,22),tStrs[i]);

					if(clickedIndex==-1)
					{
						selectedRotIndexes[i] = rotsComboBox[i].List(new Rect(515,230+(i*20),50,15), rotsComboInfo[selectedRotIndexes[i]].text+" ^", rotsComboInfo, listStyle);
					}
					else
					{
						if(clickedIndex==i)
						{
							selectedRotIndexes[i] = rotsComboBox[i].List(new Rect(515,230+(i*20),50,15), rotsComboInfo[selectedRotIndexes[i]].text+" ^", rotsComboInfo, listStyle);
						}
					}
				}
			}
		}

		for(int i=0;i<maxTileCount;i++)
		{
			if(currentObjs.Count>0)
			{
				string guid_c = currentObjs[i].ToString();

				if(!guid_c.Equals(""))
				{
					string objpath = AssetDatabase.GUIDToAssetPath(guid_c);
					Object _obj = (Object) AssetDatabase.LoadMainAssetAtPath(objpath);
					Texture2D previewT = new Texture2D(2,2);

					if(_obj)
					{
						previewT = AssetPreview.GetAssetPreview(_obj);
							
						if(previewT)
						{
							GUI.DrawTexture(new Rect(141+(tileBoxSizePad*x),61+(tileBoxSizePad*y),tileBoxSize,tileBoxSize),previewT,ScaleMode.ScaleToFit);
						}
						else
						{
							GUI.Box(new Rect(141+(tileBoxSizePad*x),61+(tileBoxSizePad*y),tileBoxSize,tileBoxSize),"");
							GUI.Label(new Rect(165+(tileBoxSizePad*x),116+(tileBoxSizePad*y),100,70),"NO PREVIEW");
						}

						GUI.Label(new Rect(150+(tileBoxSizePad*x),63+(tileBoxSizePad*y),100,30),_obj.name+"\n("+tStrs[i]+")");

						if(GUI.Button(new Rect(141+(tileBoxSizePad*x),182+(tileBoxSizePad*y),70,18),"Remove"))
						{
							AddGUIDToCurrentTileConnection(i,"-1");
						}

						x++;
						if(x==3)
						{
							x=0;
							y++;
						}
					}
					else
					{
						GUI.Box(new Rect(141+(tileBoxSizePad*x),61+(tileBoxSizePad*y),tileBoxSize,tileBoxSize),tStrs[i]);
						GUI.DrawTexture(new Rect(178+(tileBoxSizePad*x),104+(tileBoxSizePad*y),64,64),tImgs[i],ScaleMode.ScaleToFit);
						GUI.Label(new Rect(162+(tileBoxSizePad*x),176+(tileBoxSizePad*y),110,150),"Drag Prefab Here");

						x++;
						if(x==3)
						{
							x=0;
							y++;
						}
					}
				}
			}
			else
			{
				LoadInfo();
			}
		}
		
		if(tcNames.Count>0&&clickedIndex==-1)
		{
			GUI.Box(new Rect(5,40,110,5+((tcNames.Count+1)*25)),"Tile Connections");
			selGridInt = GUI.SelectionGrid(new Rect(10, 65, 100, tcNames.Count*25), selGridInt, tcNamesStr, 1);

			if(GUI.Button(new Rect(140,355,140,25),"Delete Tile Connection"))
			{
				RemoveWholeTileConnection(selGridInt);
				this.Repaint();
			}

			if(GUI.Button(new Rect(285,355,140,25),"Clear all Tiles"))
			{
				ClearAllTilesFromCurrentTileConnection();
				this.Repaint();
			}

			if(GUI.Button(new Rect(430,355,140,25),"Reset Y Rotations"))
			{
				ResetAllDefaultYRotations();
				this.Repaint();
			}

			HandleDragContent(tileBoxSize,tileBoxSizePad);
		}
	}

	private void ResetAllDefaultYRotations()
	{
		for(int i=0;i<maxTileCount;i++)
		{
			selectedRotIndexes[i] = 0;
		}

		RewriteAllInfoToFile();
		SetComboBoxes();
	}

	private void ClearAllTilesFromCurrentTileConnection()
	{
		for(int i=0;i<maxTileCount;i++)
		{
			AddGUIDToCurrentTileConnection(i,"-");
		}
	}

	private void HandleDragContent(int tileBoxSize, int tileBoxSizePad)
	{	
		int x2 = 0;
		int y2 = 0;

	    DragAndDrop.AcceptDrag();

	    if (Event.current.type == EventType.DragUpdated)	
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Link;
		}
		
		if (Event.current.type == EventType.DragPerform)
		{
			string[] allpaths = (string[]) DragAndDrop.paths;
			string guidStrPath = allpaths[0];

			string ftype = Path.GetExtension(guidStrPath);
				
			if(ftype.Equals(".prefab"))
			{
				string _guid = AssetDatabase.AssetPathToGUID(guidStrPath);

				int selectedBox = -1;

			    for(int j=0;j<maxTileCount;j++)
				{
					Rect rc = new Rect(140+(tileBoxSizePad*x2),60+(tileBoxSizePad*y2),tileBoxSize,tileBoxSize);

					if(rc.Contains(Event.current.mousePosition))
					{
						selectedBox = j;
						break;
					}

					x2++;
					if(x2==3)
					{
						x2=0;
						y2++;
					}
				}

				if(selectedBox!=-1)
				{
					AddGUIDToCurrentTileConnection(selectedBox,_guid);
				}

				this.Repaint();
			}
		}
	}

	private void AddGUIDToCurrentTileConnection(int id, string guid)
	{
		string[] info = (string[]) (tcObjs[selGridInt].ToString()).Split(':');
		info[id] = guid;
		string newInfo = "";

		for(int i=0;i<maxTileCount;i++)
		{
			newInfo += info[i]+":";
		}

		tcObjs[selGridInt] = newInfo;

		RewriteAllInfoToFile();
	}

	private void RemoveWholeTileConnection(int id)
	{
		string allNewInfo = "";

		for(int i=0;i<tcNames.Count;i++)
		{
			if(i!=id)
			{
				allNewInfo += tcNames[i].ToString()+"$"+tcObjs[i].ToString()+"$"+tcRots[i].ToString()+"$|";
			}
		}

		StreamWriter sw = new StreamWriter(path);
		sw.Write("");
		sw.Write(allNewInfo);
		sw.Close();

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		LoadInfo();
		selGridInt = selGridInt-1;

		if(tcNames.Count>0)
		{
			if(selGridInt<0)
				selGridInt = 0;

			LoadTileConnectionInfoByID(selGridInt);
		}
	}

	private void RewriteAllInfoToFile()
	{
		string rotsAllInfo = "";

		for(int j=0;j<maxTileCount;j++)
		{
			rotsAllInfo += rotsComboInfo[selectedRotIndexes[j]].text+":";
		}

		string allInfo = "";

		for(int i=0;i<tcNames.Count;i++)
		{
			string tcName = tcNames[i].ToString();
			string tcObjsInfo = tcObjs[i].ToString();
			string tcRotsInfo = tcRots[i].ToString();

			if(i==selGridInt)
			{
				tcRotsInfo = rotsAllInfo;
			}

			allInfo += tcName+"$"+tcObjsInfo+"$"+tcRotsInfo+"$|";
		}

		StreamWriter sw = new StreamWriter(path);
		sw.Write("");
		sw.Write(allInfo);
		sw.Close();

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		LoadInfo();
		LoadTileConnectionInfoByID(selGridInt);
	}

	private void LoadTileConnectionInfoByID(int id)
	{
		if(tcNames.Count>0)
		{
			currentObjs.Clear();
			//string[] currentObjsStr = new string[6];
			string[] currentObjsInfo = tcObjs[id].ToString().Split(':');

			for(int i=0;i<maxTileCount;i++)
			{
				currentObjs.Add(currentObjsInfo[i]);
			}

			currentRots.Clear();

			string[] currentRotsInfo = tcRots[id].ToString().Split(":"[0]);

			for(int i=0;i<maxTileCount;i++)
			{
				currentRots.Add(currentRotsInfo[i]);
			}
		}
	}

	private void CreateNewTileConnection(string name)
	{	
		bool isDuplicate = false;

		if(name.Contains(" ")||name.Contains(":")||name.Contains("$")||name.Contains("|")||name.Contains(".")||name.Contains("\"")||name.Contains("/"))
		{
			Debug.Log("Warning: using whitespace, :, $, /, \", . or | chars are forbiden. They will be stripped.");
		}

		name = name.Replace(" ","");
		name = name.Replace("$","");
		name = name.Replace(":","");
		name = name.Replace("|","");
		name = name.Replace("\"","");
		name = name.Replace("/","");
		name = name.Replace(".","");

		for(int i=0;i<tcNames.Count;i++)
		{
			if(tcNames[i].ToString().Equals(name))
			{
				Debug.Log("Error: Name is already exists, try another.");
				isDuplicate = true;
				break;
			}
		}

		if(!isDuplicate)
		{
			string checkName = name.Replace(" ","");

			if(!checkName.Equals(""))
			{
				StreamReader rd = new StreamReader(path);
				string allinfo = rd.ReadToEnd();
				rd.Close();

				allinfo += name+"$-1:-1:-1:-1:-1:$0:0:0:0:0:$|";

				StreamWriter rw = new StreamWriter(path);
				rw.Write("");
				rw.Flush();
				rw.Write(allinfo);
				rw.Flush();
				rw.Close();
			}
			else
			{
				Debug.Log("Eroror: Enter a name.");
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			LoadInfo();

			selGridInt = tcNames.Count-1;
			LoadTileConnectionInfoByID(selGridInt);
			newTileConnectionName = "";
		}
	}

	private void LoadInfo(bool skipDataBase=false)
	{
		for(int i=0;i<maxTileCount;i++)
		{
			tImgs[i] = (Texture2D) Resources.Load("uteForEditor/uteImgs/t"+i.ToString());
		}

		tStrs[0] = "Turn";
		tStrs[1] = "Forward";
		tStrs[2] = "TForm";
		tStrs[3] = "Cross";
		tStrs[4] = "Forward-End";

		if(!skipDataBase)
		{
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		if(!File.Exists(path))
		{
			File.Create(path).Dispose();
		}

		StreamReader rd = new StreamReader(path);
		string info = rd.ReadToEnd();
		rd.Close();

		currentObjs.Clear();
		currentRots.Clear();
		tcNames.Clear();
		tcObjs.Clear();
		tcRots.Clear();

		string[] infobytileconnection = info.Split('|');

		for(int i=0;i<infobytileconnection.Length;i++)
		{
			if(!infobytileconnection[i].ToString().Equals(""))
			{
				string[] tileconinfo = (string[]) (infobytileconnection[i].ToString()).Split('$');

				tcNames.Add(tileconinfo[0]);
				tcObjs.Add(tileconinfo[1]);
				tcRots.Add(tileconinfo[2]);
			}
		}

		tcNamesStr = new string[tcNames.Count];

		for(int i=0;i<tcNames.Count;i++)
		{
			tcNamesStr[i] = tcNames[i].ToString();
		}
	}

	private void ReloadComboBox()
	{
		rotsComboInfo = new GUIContent[4];
		rotsComboInfo[0] = new GUIContent((string)"0");
		rotsComboInfo[1] = new GUIContent((string)"90");
		rotsComboInfo[2] = new GUIContent((string)"180");
		rotsComboInfo[3] = new GUIContent((string)"270");

		listStyle.normal.textColor = Color.white;
		listStyle.normal.background = new Texture2D(0,0);
		listStyle.onHover.background = new Texture2D(2, 2);
		listStyle.hover.background = new Texture2D(2, 2);
		listStyle.padding.bottom = 4;
	}

	private void SetComboBoxes()
	{
		if(currentRots.Count>0)
		{
			for(int i=0;i<maxTileCount;i++)
			{
				if(currentRots[i].ToString().Equals("0"))
				{
					selectedRotIndexes[i] = 0;
					rotsComboBox[i].selectedItemIndex = 0;
				}
				else if(currentRots[i].ToString().Equals("90"))
				{
					selectedRotIndexes[i] = 1;
					rotsComboBox[i].selectedItemIndex = 1;
				}
				else if(currentRots[i].ToString().Equals("180"))
				{
					selectedRotIndexes[i] = 2;
					rotsComboBox[i].selectedItemIndex = 2;
				}
				else if(currentRots[i].ToString().Equals("270"))
				{
					selectedRotIndexes[i] = 3;
					rotsComboBox[i].selectedItemIndex = 3;
				}
			}
		}
	}
}
