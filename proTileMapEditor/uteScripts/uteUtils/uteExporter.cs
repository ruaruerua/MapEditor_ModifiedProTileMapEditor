using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class uteExporter : MonoBehaviour
{
	[HideInInspector]
	public string mapName;
	private bool isCombineMesh;
	[HideInInspector]
	public bool isShow;
	[HideInInspector]
	public GameObject MAP_STATIC;
	private GUISkin ui;
	private bool addMeshCollider;
	
	void Start()
	{
		isShow = false;
		addMeshCollider = true;
		ui = (GUISkin) Resources.Load("uteForEditor/uteUI");
		isCombineMesh = true;
	}
	
	void OnGUI()
	{
		if(isShow)
		{
			GUI.skin = ui;
			GUI.Box (new Rect(160,110,260,300),"Export to Prefab");
			GUI.Label (new Rect(180,150,180,30),"Prefab Name");
			mapName = GUI.TextField(new Rect(180,170,180,30),mapName);
			addMeshCollider = GUI.Toggle(new Rect(180,210,180,30),isCombineMesh,"Add Mesh Colliders?");
			
			if(GUI.Button(new Rect(180,240,180,30),"Export!"))
			{
				StartCoroutine(ExportToPrefab());
			}
			if(GUI.Button (new Rect(180,270,180,30),"Close"))
			{
				isShow = false;
			}

			GUI.Label(new Rect(170,305,230,100),"* Exported prefab you will find: "+uteGLOBAL3dMapEditor.getMyPatternsDir()+"\nUse them through Tile Editor");
		}
	}
	
	IEnumerator ExportToPrefab()
	{
		GameObject _MAP = (GameObject) Instantiate(MAP_STATIC,MAP_STATIC.transform.position,MAP_STATIC.transform.rotation);
		_MAP.name = "MAPTemp";
		_MAP.transform.position = new Vector3(0.0f,0.0f,0.0f);
		
		if(isCombineMesh==false)
		{
			uteCombineChildren uteCC = (uteCombineChildren) _MAP.AddComponent<uteCombineChildren>();
			uteCC.Batch(addMeshCollider,true,true);
		}
		
		yield return 0;

		Object tempPrefab = PrefabUtility.CreateEmptyPrefab(uteGLOBAL3dMapEditor.getMyPatternsDir()+mapName+".prefab");
		
		MeshFilter[] tns = (MeshFilter[]) _MAP.GetComponentsInChildren<MeshFilter>();
		
		for(int i=0;i<tns.Length;i++)
		{
			string meshName = ((MeshFilter)tns[i]).gameObject.name;

			if(meshName.Equals("Combined mesh"))
			{
				Mesh mesh = new Mesh();
				mesh.name = "CombinedMesh1";
				mesh = ((MeshFilter)tns[i]).mesh;
				AssetDatabase.AddObjectToAsset(mesh,tempPrefab);
			}
			
		}
				
		Transform[] allObjsT = (Transform[]) _MAP.GetComponentsInChildren<Transform>();
		Transform nearestT = _MAP.transform;
		float nearestF = 10000000.0f;
		
		for(int i=0;i<allObjsT.Length;i++)
		{
			Transform objT = (Transform) allObjsT[i];
			
			if(!objT.gameObject.name.Equals("MAPTemp"))
			{
				if(objT.gameObject.transform.parent.gameObject.name.Equals("MAPTemp"))
				{
					float currentDistF = (float) Vector3.Distance(objT.transform.position, _MAP.transform.position);
					
					if(currentDistF < nearestF)
					{
						nearestF = currentDistF;
						nearestT = objT;
					}
				}
			}
		}
				
		Vector3 SavePos = nearestT.position;
		
		for(int i=0;i<allObjsT.Length;i++)
		{
			Transform objT = (Transform) allObjsT[i];
			
			if(!objT.gameObject.name.Equals("MAPTemp"))
			{
				if(objT.gameObject.transform.parent.gameObject.name.Equals("MAPTemp"))
				{
					objT.position -= SavePos+new Vector3(.5f,.5f,.5f);
				}
			}
		}
				
		PrefabUtility.ReplacePrefab(_MAP, tempPrefab, ReplacePrefabOptions.ReplaceNameBased);
		Destroy (_MAP);
		
		isShow = false;
	}
	
	string modelTemplate = "Assets/Templates/Model.prefab";
		
	void Generate() 
	{
		string	prefabPath = uteGLOBAL3dMapEditor.getMapsDir()+mapName+".prefab";
	    Object templatePrefab = AssetDatabase.LoadAssetAtPath(modelTemplate, typeof(GameObject));
	    GameObject template = (GameObject)PrefabUtility.InstantiatePrefab(templatePrefab);
	
	    Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
	    if (!prefab) {
	        prefab = PrefabUtility.CreateEmptyPrefab( prefabPath );
	    }
	
	    Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Mesh));
	    if (!mesh) {
	        mesh = new Mesh();
	        mesh.name = name;
	        AssetDatabase.AddObjectToAsset (mesh, prefabPath);
	    } else {
	        mesh.Clear();
	    }

	    MeshFilter mf = template.GetComponent<MeshFilter>();
		SkinnedMeshRenderer smr = template.GetComponent<SkinnedMeshRenderer>();
		
		if(mf)
		{
			mf.sharedMesh = mesh;
		}
		else if(smr)
		{
			Debug.Log (smr);
			smr.sharedMesh = mesh;
		}
		else
		{
			Debug.Log ("Something Wrong");
		}
	
	    PrefabUtility.ReplacePrefab(template, prefab, ReplacePrefabOptions.ReplaceNameBased);
	    Object.DestroyImmediate(template);
	}
}

#endif
