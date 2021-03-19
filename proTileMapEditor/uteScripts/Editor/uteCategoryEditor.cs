using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;

public class uteCategoryEditor : EditorWindow
{
    private string catName = "newCategoryName";
    private GUIContent[] comboBoxList;
    private GUIContent[] comboBoxList_TilesType;
	private uteComboBox comboBoxControl = new uteComboBox();
	private uteComboBox comboBoxTilesType = new uteComboBox();
	private GUIStyle listStyle = new GUIStyle();
	private string filepathstring;
	private string catprievsfilepath;
	private string guididfilepath;
	private ArrayList catNames = new ArrayList();
	private ArrayList catColls = new ArrayList();
	private ArrayList catObjs = new ArrayList();
	private ArrayList catState = new ArrayList();
	private ArrayList catIDs = new ArrayList();
	private ArrayList catLayers = new ArrayList();
	private ArrayList realCatObjs = new ArrayList();
	private ArrayList oldCatPrievs = new ArrayList();
	private ArrayList realCatPrievs = new ArrayList();
	private Vector2 scrollPosition;
	private string selItemText = "";
	[SerializeField]
	private int lastTileTypeIndex = -1;
	private int lastCategoryIndex = -1;
	private int boxSize = 125;
	private Texture2D uiBG;
	private List<string> catPreviews = new List<string>();
	
	public int lastTileTypeIndexP
    {
        get { return lastTileTypeIndex; }
        set
        {
            if (lastTileTypeIndex == value) return;
 
            lastTileTypeIndex = value;
        }
    }

    [MenuItem ("Window/proTileMapEditor/Tiles Editor",false,3)]
    static void Init () {
        uteCategoryEditor window = (uteCategoryEditor)EditorWindow.GetWindow (typeof (uteCategoryEditor));
		window.Show();
    }
	
	private void LoadMain()
	{
		ReadCategoriesFromFile();
		uiBG = (Texture2D) Resources.Load("uteForEditor/uteUI/uteUI1");
	}
	
	private void ReloadComboBox()
	{
		comboBoxList = new GUIContent[catNames.Count];
		for(int i=0;i<catNames.Count;i++)
		{
			comboBoxList[i] = new GUIContent((string)catNames[i].ToString());
		}

		comboBoxList_TilesType = new GUIContent[2];
		comboBoxList_TilesType[0] = new GUIContent((string)"Static");
		comboBoxList_TilesType[1] = new GUIContent((string)"Dynamic");

		listStyle.normal.textColor = Color.white;
		listStyle.normal.background = new Texture2D(0,0);
		listStyle.onHover.background = new Texture2D(2, 2);
		listStyle.hover.background = new Texture2D(2, 2);
		listStyle.padding.bottom = 4;
	}

	private void OnFocus()
	{
		uiBG = (Texture2D) Resources.Load("uteForEditor/uteUI/uteUI1");
		filepathstring = AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.uteCategoryInfotxt);
		guididfilepath = AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.uteGUIDIDtxt);
		catprievsfilepath = AssetDatabase.GUIDToAssetPath(uteGLOBAL3dMapEditor.utePrievstxt);
		LoadMain();
		ReloadComboBox();
		SetComboBoxes();
		this.Repaint();
	}
	
    private void OnGUI()
	{
		if(lastCategoryIndex!=comboBoxControl.selectedItemIndex)
		{
			lastCategoryIndex = comboBoxControl.selectedItemIndex;
			LoadMain();
			SetComboBoxes();

			this.Repaint();
		}
		
		if(lastTileTypeIndex!=comboBoxTilesType.selectedItemIndex)
		{
			lastTileTypeIndex = comboBoxTilesType.selectedItemIndex;
			ChangeTileTypeInCategory();
		}

        GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
        catName = EditorGUILayout.TextField ("Category Name", catName);
		
		if(GUILayout.Button("Create New Category"))
		{
			AddNewCategory(catName);
			this.Repaint();
		}
		
		HandleAndDrawAllObjects();
		
		if(catNames.Count>0)
		{
			GUI.Box(new Rect(10,70,540,50), "Category: " + selItemText);
			GUI.Box(new Rect(10,120,540,20),"Drag Prefabs to add them to ["+selItemText+"] category!");
			GUI.Box(new Rect(560,70,300,70),"Category Settings");
			GUI.Label(new Rect(570,90,100,22),"Tiles Type: ");
			//GUI.Label(new Rect(570,115,100,22),"Layer: ");

			int selectedItemIndex = comboBoxControl.GetSelectedItemIndex();
			int selectedItemIndex_tilestype = comboBoxTilesType.GetSelectedItemIndex();

			if(selectedItemIndex>=0&&!comboBoxTilesType.isClickedComboButton)
			{
			/*	catLayers[selectedItemIndex] = GUI.TextField(new Rect(630,115,100,18),catLayers[selectedItemIndex].ToString());

				if(GUI.Button(new Rect(740,115,100,18),"Apply Layer"))
				{
					ApplyLayer(catNames[comboBoxControl.selectedItemIndex].ToString(),catLayers[comboBoxControl.selectedItemIndex].ToString());	
				}*/
			}

			selItemText = comboBoxList[selectedItemIndex].text;

			if(selectedItemIndex<catNames.Count)
			{
				if(catNames[selectedItemIndex].ToString().Equals(""))
				{
					comboBoxControl.selectedItemIndex = 0;
					selectedItemIndex = 0;
				}
			}
			else
			{
				comboBoxControl.selectedItemIndex = 0;
				selectedItemIndex = 0;
			}
			
			selectedItemIndex = comboBoxControl.List(new Rect(20,90,200,20), comboBoxList[selectedItemIndex].text+" ^", comboBoxList, listStyle );
			selectedItemIndex_tilestype = comboBoxTilesType.List(new Rect(640,90,150,20),comboBoxList_TilesType[selectedItemIndex_tilestype].text+" ^",comboBoxList_TilesType,listStyle);
			
			if(GUI.Button(new Rect(230,90,150,20),"Remove this Category"))
			{
				if(selectedItemIndex>=0&&catNames.Count>0)
				{
					RemoveCategory(catNames[selectedItemIndex].ToString());
					this.Repaint();
				}
			}
			
			if(GUI.Button(new Rect(390,90,150,20),"Clear all objects"))
			{
				ClearAllObjectsInCat(catNames[selectedItemIndex].ToString());
				this.Repaint();
			}
		}
		else
		{
			GUI.Label(new Rect(20,70,400,21), "No Categories, Please Create.");
		}
		
		HandleDragContent();
	}
		
	private void ApplyLayer(string cname, string lname)
	{
		StreamReader rd = new StreamReader(filepathstring);
		string allinfo = rd.ReadToEnd();
		rd.Close();

		string allNewInfo = "";
		string[] splitedcats = allinfo.Split("|"[0]);

		for(int i=0;i<splitedcats.Length;i++)
		{
			string splitedinfo = splitedcats[i].ToString();

			if(!splitedinfo.Equals(""))
			{
				string[] infoparts = splitedinfo.Split("$"[0]);

				if(infoparts[0].Equals(cname))
				{
					allNewInfo += infoparts[0]+"$"+infoparts[1]+"$"+infoparts[2]+"$"+infoparts[3]+"$"+lname+"$|";
				}
				else
				{
					allNewInfo += splitedinfo+"|";
				}
			}
		}

		StreamWriter sw = new StreamWriter(filepathstring);
		sw.Write("");
		sw.Write(allNewInfo);
		sw.Flush();
		sw.Close();

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log("Layer Saved.");
	}

	private int FindTheLastGUIIDID()
	{
		StreamReader sr = new StreamReader(guididfilepath);
		string allguidids = sr.ReadToEnd();
		sr.Close();

		if(!allguidids.Equals(""))
		{
			string[] allparts = allguidids.Split("$"[0]);
			string[] pPart = allparts[allparts.Length-2].Split(":"[0]);
			int lastuidid = System.Convert.ToInt32(pPart[1]);

			return lastuidid;
		}

		return 0;
	}

	private void AddNewGUIDID(string guid)
	{
		string allguidids = "";

		StreamReader sr = new StreamReader(guididfilepath);
		allguidids = sr.ReadToEnd();
		sr.Close();

		string[] guididsparts = allguidids.Split("$"[0]);
		bool found = false;

		for(int i=0;i<guididsparts.Length;i++)
		{
			string part = guididsparts[i];

			if(!part.Equals(""))
			{
				string[] pSplit = part.Split(":"[0]);
				string pGuid = pSplit[0];

				if(pGuid.Equals(guid))
				{
					found = true;
					break;
				}
			}
		}

		if(!found)
		{
			allguidids += guid+":"+(FindTheLastGUIIDID()+1).ToString()+":$";
		}

		StreamWriter sw = new StreamWriter(guididfilepath);
		sw.Write("");
		sw.Write(allguidids);
		sw.Flush();
		sw.Close();

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private void ClearAllObjectsInCat(string cname)
	{
		StreamReader rd = new StreamReader(filepathstring);
		string allinfo = rd.ReadToEnd();
		rd.Close();
		string allnewinfo = "";
		string[] allinfobycat = (string[]) allinfo.Split('|');
		
		for(int i=0;i<allinfobycat.Length;i++)
		{
			string[] splitedinfo = (string[]) allinfobycat[i].ToString().Split('$');
			
			if(!allinfobycat[i].Equals(""))
			{				
				if(splitedinfo[0].ToString().Equals(cname))
				{
					allnewinfo += cname+"$"+splitedinfo[1].ToString()+"$:$"+splitedinfo[3].ToString()+"$"+splitedinfo[4].ToString()+"$|";
				}
				else
				{
					allnewinfo += allinfobycat[i].ToString() + "|";
				}
			}
		}
		
		StreamWriter rw = new StreamWriter(filepathstring);
		rw.Write("");
		rw.Write(allnewinfo);
		rw.Close();
			
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		LoadMain();
		ReloadComboBox();
	}
	
	private void HandleAndDrawAllObjects()
	{
		if(comboBoxControl.selectedItemIndex>=0&&comboBoxControl.selectedItemIndex<catNames.Count)
		{
			string[] _objs = (string[]) catObjs[comboBoxControl.selectedItemIndex].ToString().Split(':');

			int x = 0;
			int y = 0;
			
			boxSize = ((realCatObjs.Count/4)*135)+135;
			scrollPosition = GUI.BeginScrollView(new Rect(20, 140, 870, 250), scrollPosition, new Rect(0, 0, 650, boxSize));
			
			int startDrawPoint = (int) (scrollPosition.y/135);

			int endDrawPoint;
			
			if(startDrawPoint==-1)
			{
				startDrawPoint = 0;
			}

			startDrawPoint *= 4;

			if(realCatObjs.Count-startDrawPoint>=13)
			{
				endDrawPoint = startDrawPoint + 13;
			}
			else
			{
				endDrawPoint = startDrawPoint + (realCatObjs.Count-startDrawPoint);
			}

			if(endDrawPoint>realCatObjs.Count)
			{
				endDrawPoint = realCatObjs.Count;
			}

			y = startDrawPoint/4;

			for(int j=startDrawPoint;j<endDrawPoint;j++)
			{
				if(j<realCatObjs.Count)
				if((Object)realCatObjs[j]&&_objs.Length>j)
				{

					//string objpath = AssetDatabase.GUIDToAssetPath(_objs[j].ToString());
					//Object _obj = (Object) realCatObjs[j];//AssetDatabase.LoadMainAssetAtPath(objpath);
					Texture2D previewT = new Texture2D(2,2);
					//Texture icontexture = AssetDatabase.GetCachedIcon(objpath);
					string guidid = getIDByGUID(_objs[j].ToString());

					//if((Texture2D)realCatPrievs[j]==null)
					//{
					if((Object)realCatObjs[j])
					{
						previewT = AssetPreview.GetAssetPreview((Object)realCatObjs[j]);
					}

					if(!previewT)
					{
						string m_path = AssetDatabase.GetAssetPath((Object)realCatObjs[j]);
						AssetDatabase.ImportAsset(m_path);
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						previewT = AssetPreview.GetAssetPreview((Object)realCatObjs[j]);
					}
					//}
					/*else
					{
						previewT = (Texture2D) realCatPrievs[j];
					}*/
					
					if((Object)realCatObjs[j])
					{
						GUI.Box(new Rect(10+(210*x),10+(131*y),100,114),"");
						//GUI.Label(new Rect(14+(210*x),9+(131*y),200,20),_obj.name);
						
						if(!comboBoxControl.isClickedComboButton&&!comboBoxTilesType.isClickedComboButton)
						{
							if(previewT)
							{
								GUI.DrawTexture(new Rect(11+(210*x),25+(131*y),99,99),previewT,ScaleMode.ScaleToFit);
							}
							else
							{
							//	GUI.DrawTexture(new Rect(56+(125*x),166+(120*y),50,50),icontexture,ScaleMode.ScaleToFit);
								GUI.Label(new Rect(25+(210*x),56+(131*y),80,50),"LOADING...");
							}

							GUI.DrawTexture(new Rect(10+(210*x),10+(131*y),200,14),uiBG);

							realCatObjs[j] = EditorGUI.ObjectField(new Rect(14+(210*x),9+(131*y),115,17),(Object)realCatObjs[j],typeof(GameObject),false);

							GUI.DrawTexture(new Rect(10+(210*x),124+(131*y),200,14),uiBG);
							GUI.Label(new Rect(16+(210*x),123+(131*y),70,15),"ID: "+guidid);

							realCatPrievs[j] = EditorGUI.ObjectField(new Rect(111+(210*x),24+(131*y),99,100),(Object)realCatPrievs[j],typeof(Texture2D),false);

							if(GUI.Button(new Rect(184+(210*x),9+(131*y),26,16),"X"))
							{
								RemoveObjectFromCat(catNames[comboBoxControl.selectedItemIndex].ToString(),_objs[j].ToString());
							}

							if(realCatPrievs.Count>0&&realCatPrievs.Count>j)
							{
								if((Texture2D)realCatPrievs[j])
								{
									GUI.Label(new Rect(120+(210*x),123+(131*y),85,15),((Texture2D)realCatPrievs[j]).name);
								}
							}
						}
						
						x++;
						if(x==4)
						{
							x=0;
							y++;
							//boxSize = (y*135)+135;
						}
					}
				}
			}
			
			GUI.EndScrollView();
		}
	}

	private void RemoveObjectFromCat(string cname, string objguid)
	{
		StreamReader rd = new StreamReader(filepathstring);
		string allinfo = rd.ReadToEnd();
		rd.Close();
		
		string allnewinfo = "";
		string[] allinfobycat = (string[]) allinfo.Split('|');
		
		for(int i=0;i<allinfobycat.Length;i++)
		{
			string[] splitedinfo = (string[]) allinfobycat[i].ToString().Split('$');
			
			if(splitedinfo[0].ToString().Equals(cname))
			{
				string[] objs = (string[]) splitedinfo[2].ToString().Split(':');
				string newobjs = "";
				
				for(int j=0;j<objs.Length;j++)
				{
					if(!objs[j].ToString().Equals(objguid) && !objs[j].ToString().Equals(""))
					{
						newobjs += objs[j].ToString() + ":";
					}
				}
				
				allnewinfo += cname+"$"+splitedinfo[1].ToString()+"$"+newobjs+"$"+splitedinfo[3].ToString()+"$"+splitedinfo[4].ToString()+"$|";
			}
			else
			{
				allnewinfo += allinfobycat[i].ToString() + "|";
			}
		}
		
		StreamWriter rw = new StreamWriter(filepathstring);
		rw.Write("");
		rw.Write(allnewinfo);
		rw.Close();
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		LoadMain();
		ReloadComboBox();
	}

	private void HandleDragContent()
	{	
	    DragAndDrop.AcceptDrag();

		if (Event.current.type == EventType.DragUpdated)	
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Link;
		}
		
		if (Event.current.type == EventType.DragPerform && catNames.Count>0)
		{
			string[] allpaths = (string[]) DragAndDrop.paths;
			//object[] allobjs = (object[]) DragAndDrop.objectReferences;
			ArrayList guids = new ArrayList();
			
			for(int i=0;i<allpaths.Length;i++)
			{
				string ftype = Path.GetExtension(allpaths[i].ToString());
				
				if(ftype.Equals(".prefab"))
				{
					string _guid = AssetDatabase.AssetPathToGUID(allpaths[i].ToString());

					if(!_guid.Equals(""))
					{
						AddNewGUIDID(_guid);
						guids.Add (_guid);
					}
				}
				else
				{
					string _guid = AssetDatabase.AssetPathToGUID(allpaths[i].ToString());
					Debug.Log(_guid);
					Debug.Log ("Warning: File ("+allpaths[i].ToString()+") was ignored because it's not a prefab");
				}
			}
			
			if(guids.Count>0)
			{
				AddGUIDStoCategory(guids);
			}

			this.Repaint();
		}
	}
	
	private void AddGUIDStoCategory(ArrayList guids)
	{
		string cname = catNames[comboBoxControl.selectedItemIndex].ToString();
		string _guids = "";
		//string collidertype = "";
		string allinfo = "";
		string newcatinfo = "";
		
		StreamReader rd = new StreamReader(filepathstring);
		allinfo = rd.ReadToEnd();
		rd.Close();
		
		string[] allinfobycat = (string[]) allinfo.Split('|');
		
		for(int i=0;i<allinfobycat.Length;i++)
		{
			string[] splitedinfo = (string[]) allinfobycat[i].ToString().Split('$');
			
			if(!allinfobycat[i].ToString().Equals(""))
			{
				if(splitedinfo[0].ToString().Equals(cname))
				{
					string[] oldguids = (string[]) splitedinfo[2].ToString().Split(':');
					
					for(int k=0;k<guids.Count;k++)
					{
						bool exist = false;
						
						for(int l=0;l<oldguids.Length;l++)
						{
							if(guids[k].ToString().Equals(oldguids[l].ToString()))
							{
								exist = true;
								break;
							}
						}
						
						if(!exist && !guids[k].ToString().Equals(""))
						{
							_guids += guids[k].ToString() + ":";
						}
					}	
					
					newcatinfo += cname+"$"+splitedinfo[1].ToString()+"$"+splitedinfo[2].ToString()+""+_guids+"$"+splitedinfo[3].ToString()+"$"+splitedinfo[4].ToString()+"$|";
				}
				else
				{
					newcatinfo += allinfobycat[i].ToString() + "|";
				}
			}
		}
		
		StreamWriter rw = new StreamWriter(filepathstring);
		rw.Write("");
		rw.Write(newcatinfo);
		rw.Close();
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		LoadMain();
		ReloadComboBox();
	}
	
	private void RemoveCategory(string catN)
	{
		StreamReader rd = new StreamReader(filepathstring);
		string allinfo = rd.ReadToEnd ();
		rd.Close();
		string[] infobycat = (string[]) allinfo.Split('|');
		string allnewinfo = "";
		
		for(int i=0;i<infobycat.Length;i++)
		{
			if(!infobycat[i].ToString().Equals(""))
			{
				string[] splitedinfo = (string[]) infobycat[i].ToString().Split('$');
				
				if(!splitedinfo[0].ToString().Equals(catN))
				{
					allnewinfo += infobycat[i].ToString() + "|";
				}
			}
		}
		
		StreamWriter rw = new StreamWriter(filepathstring);
		rw.Write("");
		rw.Write(allnewinfo);
		rw.Close();
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		LoadMain();
		ReloadComboBox();
		
		comboBoxControl.selectedItemIndex = catNames.Count-1;
	}
	
	private void AddNewCategory(string catN)
	{
		if(catN.Contains("$")||catN.Contains("|")||catN.Contains(":")||catN.Contains("/")||catN.Contains("\"")||catN.Contains(".")||catN.Contains(" "))
		{
			Debug.Log ("Warning: Can't containt symbols: /,\",$,:,|,.. They will be stripped.");
		}

		catN = catN.Replace(" ","");
		catN = catN.Replace(".","");
		catN = catN.Replace("\"","");
		catN = catN.Replace("/","");
		catN = catN.Replace(":","");
		catN = catN.Replace("$","");
		catN = catN.Replace("|","");
		
		if(CheckIfCategoryExists(catN))
		{
			return;
		}

		catName = "";
		
		StreamReader rd = new StreamReader(filepathstring);
		string info = rd.ReadToEnd();
		rd.Close();
		string addinfo = catN+"$boxcollider$:$Static$default$|";
		info += addinfo;
		StreamWriter rw = new StreamWriter(filepathstring);
		rw.Write("");
		rw.Write(info);
		rw.Close();
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		LoadMain();
		ReloadComboBox();
		
		comboBoxControl.selectedItemIndex = catNames.Count-1;
	}
	
	private bool CheckIfCategoryExists(string catN)
	{
		if(catN.Equals("") || catN.Length<3)
		{
			Debug.Log ("Error: Category name should contain at least 3 letters and can't be empty.");
			return true;
		}
		
		for(int i=0;i<catNames.Count;i++)	
		{
			if(catNames[i].ToString().Equals(catN))
			{
				Debug.Log ("Error: Category name already exists.");
				return true;
			}
		}

		return false;
	}
		
	private void AddPreviewInList(Texture2D texture, string objrefguid)
	{
		string path = AssetDatabase.GetAssetOrScenePath((Object)texture);
		string texguid = AssetDatabase.AssetPathToGUID(path);

		StreamReader sr = new StreamReader(catprievsfilepath);
		string allinfo = sr.ReadToEnd();
		sr.Close();

		string[] alltexrefs = allinfo.Split("$"[0]);
		bool replaced = false;
		string allNewInfo = "";

		for(int i=0;i<alltexrefs.Length;i++)
		{
			string part = alltexrefs[i];

			if(!part.Equals(""))
			{
				string[] parts = part.Split(":"[0]);
				string obj_p1 = parts[0];
				string tex_p2 = parts[1];

				if(obj_p1.Equals(objrefguid))
				{
					replaced = true;
					tex_p2 = texguid;
				}

				allNewInfo += obj_p1+":"+tex_p2+":$";
			}
		}

		if(!replaced)
		{
			allNewInfo += objrefguid+":"+texguid+":$";
		}

		StreamWriter sw = new StreamWriter(catprievsfilepath);
		sw.Write("");
		sw.Write(allNewInfo);
		sw.Flush();
		sw.Close();

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private void ReadCategoriesFromFile()
	{
		if(!File.Exists(filepathstring))
		{
			File.Create(filepathstring).Dispose();
		}

		StreamReader rd = new StreamReader(filepathstring);
		string info = rd.ReadToEnd();
		rd.Close();
		string[] infobycat = (string[]) info.Split('|');
		
		if(realCatPrievs.Count>0)
		{
			string[] __objs = (string[]) catObjs[comboBoxControl.selectedItemIndex].ToString().Split(':');
			for(int i=0;i<realCatPrievs.Count;i++)
			{
				if(realCatPrievs[i]!=oldCatPrievs[i])
				{
					AddPreviewInList((Texture2D)realCatPrievs[i],__objs[i]);
				}
			}
		}

		catNames.Clear();
		catColls.Clear();
		catObjs.Clear();
		catState.Clear();
		catIDs.Clear();
		catPreviews.Clear();
		realCatObjs.Clear();
		realCatPrievs.Clear();
		oldCatPrievs.Clear();
		catLayers.Clear();

		for(int i=0;i<infobycat.Length;i++)
		{
			if(!infobycat[i].ToString().Equals(""))
			{
				string[] infoin = (string[]) (infobycat[i].ToString()).Split('$');
				catNames.Add(infoin[0].ToString());
				catColls.Add(infoin[1].ToString());
				catObjs.Add(infoin[2].ToString());
				catState.Add(infoin[3].ToString());
				catLayers.Add(infoin[4].ToString());
			}
		}

		StreamReader sr3 = new StreamReader(catprievsfilepath);
		string allprievs = sr3.ReadToEnd();
		sr3.Close();
		string[] allpriewsparts = allprievs.Split("$"[0]);

		for(int i=0;i<allpriewsparts.Length;i++)
		{
			string priev = allpriewsparts[i];

			if(!priev.Equals(""))
			{
				catPreviews.Add(priev);
			}
		}

		if(catObjs.Count>0&&comboBoxControl.selectedItemIndex>=0&&(catObjs.Count>comboBoxControl.selectedItemIndex))
		{
			string[] _objs = (string[]) catObjs[comboBoxControl.selectedItemIndex].ToString().Split(':');
			for(int i=0;i<_objs.Length-1;i++)
			{
				Texture2D tex = getTextureByGUID(_objs[i].ToString());

				realCatPrievs.Add(tex);
				oldCatPrievs.Add(tex);
				string objpath = AssetDatabase.GUIDToAssetPath(_objs[i].ToString());
				Object _obj = (Object) AssetDatabase.LoadMainAssetAtPath(objpath);
				realCatObjs.Add(_obj);
			}
		}

		StreamReader sr2 = new StreamReader(guididfilepath);
		string allguidids = sr2.ReadToEnd();
		sr2.Close();
		string[] allguididsparts = allguidids.Split("$"[0]);

		for(int i=0;i<allguididsparts.Length;i++)
		{
			string guidid = allguididsparts[i].ToString();

			if(!guidid.Equals(""))
			{
				catIDs.Add(guidid);
			}
		}
	}

	private void SetComboBoxes()
	{
		if(catState.Count>0)
		{
			if(catState[comboBoxControl.GetSelectedItemIndex()].ToString().Equals("Static"))
			{
				comboBoxTilesType.selectedItemIndex = 0;
			}
			else
			{
				comboBoxTilesType.selectedItemIndex = 1;
			}
		}
	}

	private void ChangeTileTypeInCategory()
	{
		string allinfo = "";

		for(int i=0;i<catNames.Count;i++)
		{
			if(!catNames[i].ToString().Equals(catNames[comboBoxControl.selectedItemIndex].ToString()))
			{
				allinfo += catNames[i].ToString()+"$boxcollider$"+catObjs[i].ToString()+"$"+catState[i].ToString()+"$"+catLayers[i].ToString()+"$|";
			}
			else
			{
				allinfo += catNames[i].ToString()+"$boxcollider$"+catObjs[i].ToString()+"$"+comboBoxList_TilesType[comboBoxTilesType.selectedItemIndex].text+"$"+catLayers[i].ToString()+"$|";
			}
		}

		StreamWriter sw = new StreamWriter(filepathstring);
		sw.Write("");
		sw.Write(allinfo);
		sw.Close();

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private Texture2D getTextureByGUID(string guid)
	{	
		for(int i=0;i<catPreviews.Count;i++)
		{
			string[] parts = catPreviews[i].ToString().Split(":"[0]);

			if(parts[0].Equals(guid))
			{
				string path = AssetDatabase.GUIDToAssetPath(parts[1]);
				Texture2D _tex = (Texture2D) AssetDatabase.LoadMainAssetAtPath(path);
				return _tex;
			}
		}

		return null;
	}

	private string getIDByGUID(string guid)
	{
		for(int i=0;i<catIDs.Count;i++)
		{
			string[] parts = catIDs[i].ToString().Split(":"[0]);

			if(parts[0].Equals(guid))
			{
				return parts[1];
			}
		}

		return "";
	}
}