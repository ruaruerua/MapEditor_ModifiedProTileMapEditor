 
 using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class uteLM : MonoBehaviour {
	#if UNITY_EDITOR
	[HideInInspector]
	public uteMapEditorEngine uMEE;
	[HideInInspector]
	public bool isMapLoaded;

	public IEnumerator LoadMap(string name, GameObject map_static, GameObject map_dynamic, bool isItMap)
	{
		isMapLoaded = false;
		string path;

		if(isItMap)
		{
			path = uteGLOBAL3dMapEditor.getMapsDir()+name+".txt";
		}
		else
		{
			path = uteGLOBAL3dMapEditor.getPatternsDir()+name+".txt";
		}

		StreamReader sr = new StreamReader(path);
		string info = sr.ReadToEnd();
		sr.Close();

		string[] allparts = info.Split("$"[0]);

		for(int i=0;i<allparts.Length;i++)
		{
			if(!allparts[i].Equals(""))
			{
				if(i%2000==0) yield return 0;
				
				string[] allinfo = allparts[i].Split(":"[0]);

				string guid = allinfo[0].ToString();
				float pX = System.Convert.ToSingle(allinfo[1].ToString());
				float pY = System.Convert.ToSingle(allinfo[2].ToString());
				float pZ = System.Convert.ToSingle(allinfo[3].ToString());
				int rX = System.Convert.ToInt32(allinfo[4].ToString());
				int rY = System.Convert.ToInt32(allinfo[5].ToString());
				int rZ = System.Convert.ToInt32(allinfo[6].ToString());
				string staticInfo = allinfo[7].ToString();
				string tcInfo = allinfo[8].ToString();
				string familyName = allinfo[9].ToString();
				string layerName = allinfo[10].ToString();

				if(layerName.Equals(""))
				{
					layerName = "DEFAULT";
				}

				string opath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				GameObject tGO = (GameObject) UnityEditor.AssetDatabase.LoadMainAssetAtPath(opath);

				if(tGO)
				{
					GameObject newGO = (GameObject) Instantiate(tGO,Vector3.zero,new Quaternion(0,0,0,0));
					List<GameObject> twoGO = new List<GameObject>();
					twoGO = uMEE.createColliderToObject(newGO,newGO);
					GameObject behindGO = (GameObject) twoGO[0];
					GameObject objGO = (GameObject) twoGO[1];
					newGO = objGO;
					behindGO.name = tGO.name;
					newGO.name = tGO.name;
					behindGO.layer = 0;
					behindGO.transform.position = new Vector3(pX,pY,pZ);
					behindGO.transform.localEulerAngles = new Vector3(rX,rY,rZ);// + tGO.transform.localEulerAngles;
					behindGO.GetComponent<Collider>().isTrigger = false;
					uteTagObject uTO = behindGO.AddComponent<uteTagObject>();
					uTO.objGUID = guid;
					uTO.layerName = layerName;
					
					if(staticInfo.Equals("1"))
					{
						newGO.isStatic = true;
						uTO.isStatic = true;
						behindGO.transform.parent = map_static.transform;
					}
					else if(staticInfo.Equals("0"))
					{
						newGO.isStatic = false;
						uTO.isStatic = false;
						behindGO.transform.parent = map_dynamic.transform;
					}

					if(tcInfo.Equals("1"))
					{
						uTO.isTC = true;
						uteTcTag uTT = (uteTcTag) behindGO.AddComponent<uteTcTag>();
						uTT.tcFamilyName = familyName;
					}

					uteGLOBAL3dMapEditor.mapObjectCount++;
				}
			}
		}

		isMapLoaded = true;
		
		yield return 0;
	}
	#endif
}
