#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

[CustomEditor(typeof(uteExportObject))]
public class ExporterEditor : Editor
{
	public string pname = "";

	public void Awake()
	{
		uteExportObject myTarget = (uteExportObject)target;
		pname = myTarget.gameObject.name;
	}

	public override void OnInspectorGUI()
    {
        uteExportObject myTarget = (uteExportObject)target;

        GUILayout.Label("Export OBJ Name:");
        pname = GUILayout.TextField(pname);

        if(GUILayout.Button("Export To OBJ"))
        {
        	Transform[] trs = myTarget.GetComponentsInChildren<Transform>();
        	Exporter.ExportGOToOBJ(trs,pname);
        }
    }
}

public class Exporter : ScriptableObject
{
  	private static int vertexOffset = 0;
  	private static int normalOffset = 0;
  	private static int uvOffset = 0;
  	private static string mainPath = "Assets/GameObjectToOBJExportFolder";
 
 
    private static string MeshToString(MeshFilter mf, Dictionary<string, OMAT> materialList) 
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;
 
        StringBuilder sb = new StringBuilder();
 
        sb.Append("g ").Append(mf.name).Append("\n");

        foreach(Vector3 lv in m.vertices) 
        {
        	Vector3 wv = mf.transform.TransformPoint(lv);
 
          sb.Append(string.Format("v {0} {1} {2}\n",-wv.x,wv.y,wv.z));
        }

        sb.Append("\n");
 
        foreach(Vector3 lv in m.normals)
        {
        	Vector3 wv = mf.transform.TransformDirection(lv);
 
          sb.Append(string.Format("vn {0} {1} {2}\n",-wv.x,wv.y,wv.z));
        }

        sb.Append("\n");
 
        foreach(Vector3 v in m.uv) 
        {
          sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
        }
 
        for(int material=0; material < m.subMeshCount; material ++)
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            sb.Append("usemap ").Append(mats[material].name).Append("\n");
 
            try
         		{
            		OMAT objMaterial = new OMAT();
   
            		objMaterial.name = mats[material].name;
                
            		if (mats[material].mainTexture)
            			objMaterial.textureName = EditorUtility.GetAssetPath(mats[material].mainTexture);
            		else 
            			objMaterial.textureName = null;
                
            		materialList.Add(objMaterial.name, objMaterial);
          	}
          	catch (ArgumentException)
          	{
          	}

            int[] triangles = m.GetTriangles(material);
            for (int i=0;i<triangles.Length;i+=3) 
            {
                sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n", 
                    triangles[i]+1 + vertexOffset, triangles[i+1]+1 + normalOffset, triangles[i+2]+1 + uvOffset));
            }
        }
 
        vertexOffset += m.vertices.Length;
        normalOffset += m.normals.Length;
        uvOffset += m.uv.Length;
 
        return sb.ToString();
    }
 
    private static void Clear()
    {
    	vertexOffset = 0;
    	normalOffset = 0;
    	uvOffset = 0;
    }
 
   	private static Dictionary<string, OMAT> PrepareFileWrite()
   	{
   		Clear();
    	return new Dictionary<string, OMAT>();
   	}
 
   	private static void MaterialsToFile(Dictionary<string, OMAT> materialList, string folder, string filename)
   	{
   		using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".mtl")) 
      {
       	foreach(KeyValuePair<string, OMAT> kvp in materialList )
       	{
      		sw.Write("\n");
      		sw.Write("newmtl {0}\n", kvp.Key);
      		sw.Write("Ka  1.0 1.0 1.0\n");
			    sw.Write("Kd  1.0 1.0 1.0\n");
			    sw.Write("Ks  1.0 1.0 1.0\n");
			    sw.Write("d  1.0\n");
			    sw.Write("Ns  0.0\n");
			    sw.Write("illum 2\n");
 
  				if (kvp.Value.textureName != null)
  				{
  					string destinationFile = kvp.Value.textureName;
   
  					int indexstrip = destinationFile.LastIndexOf('/');
   
         		if (indexstrip >= 0)
              	destinationFile = destinationFile.Substring(indexstrip + 1).Trim();
   
            string relativeFile = destinationFile;
            destinationFile = folder + "/" + destinationFile;
            
            if(System.IO.File.Exists(destinationFile)==false)
            {
  					 File.Copy(kvp.Value.textureName, destinationFile);
            }
   
  					sw.Write("map_Kd {0}", relativeFile);
  				}
 
				  sw.Write("\n\n\n");
        }
      }
   	}
 
    private static void MeshesToFile(MeshFilter[] mf, string folder, string filename) 
    {
    	Dictionary<string, OMAT> materialList = PrepareFileWrite();
 
        using (StreamWriter sw = new StreamWriter(folder +"/" + filename + ".obj")) 
        {
        	sw.Write("mtllib ./" + filename + ".mtl\n");
 
        	for (int i = 0; i < mf.Length; i++)
        	{
            	sw.Write(MeshToString(mf[i], materialList));
            }
        }
 
        MaterialsToFile(materialList, folder, filename);
    }
 
    public static void ExportGOToOBJ(Transform[] selection, string filename)
    {
      mainPath = "Assets/GameObjectToOBJExportFolder";
      mainPath += "/"+filename+"/";

    	System.IO.Directory.CreateDirectory(mainPath);
  
        int objCount = 0;
 
        ArrayList mfList = new ArrayList();
 
       	for (int i = 0; i < selection.Length; i++)
       	{
       		Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));
 
       		for (int m = 0; m < meshfilter.Length; m++)
       		{
       			objCount++;
       			mfList.Add(meshfilter[m]);
       		}
       	}
 
       	if (objCount > 0)
       	{
       		MeshFilter[] mf = new MeshFilter[mfList.Count];
 
       		for (int i = 0; i < mfList.Count; i++)
       		{
       			mf[i] = (MeshFilter)mfList[i];
       		}
 	
 			if(filename.Equals(""))
 			{
       			filename = selection[0].gameObject.name;
       		}
 
       		int indexstrip = filename.LastIndexOf('/');
 
       		if(indexstrip >= 0)
       		{
            	filename = filename.Substring(indexstrip + 1).Trim();
            }
 
       		MeshesToFile(mf, mainPath, filename);
 
       		Debug.Log("Exported: " + mainPath + "/"+ filename);

       		#if UNITY_EDITOR
       		UnityEditor.AssetDatabase.SaveAssets();
       		UnityEditor.AssetDatabase.Refresh();
       		#endif
       	}
       	else
       	{
       		Debug.Log("Error exporting. Make sure you selected the object.");
       	}
    }
}

struct OMAT
{
	public string name;
	public string textureName;
  public Color cls;
}

#endif