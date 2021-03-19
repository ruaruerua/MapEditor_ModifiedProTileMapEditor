using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
#pragma warning disable 0219
using UnityEditor;
#endif

public class uteCombineChildren : MonoBehaviour {
	
	private bool generateTriangleStrips = true;
	private List<GameObject> BatchedObjects = new List<GameObject>();
	private int optLevel;

	public List<GameObject> GetBatchedObjects()
	{
		return BatchedObjects;
	}

	public void UnBatch()
	{
		for(int i=0;i<BatchedObjects.Count;i++)
		{
			Destroy(BatchedObjects[i]);
		}

		BatchedObjects.Clear();

		GameObject mStaticTiles = (GameObject) GameObject.Find("uteMAP_ALLTILES/uteMAP_STATIC");
		uteTagObject[] objs = (uteTagObject[]) mStaticTiles.GetComponentsInChildren<uteTagObject>();

		if(objs.Length>0)
		{
			for(int i=0;i<objs.Length;i++)
			{
				MeshRenderer[] mrs = (MeshRenderer[]) objs[i].gameObject.GetComponentsInChildren<MeshRenderer>();

				for(int j=0;j<mrs.Length;j++)
				{
					mrs[j].enabled = true;
				}
			}
		}
		else
		{
			Debug.Log("[RuntimeCombiner] No Batch information was found.");
		}
	}

	public void Batch(bool AddMeshColliders=false, bool RemoveLeftOvers=false, bool isItPatternExport=false, bool isPrepareForLightmapping=false, string optimizationLevel = "low")
	{
		generateTriangleStrips = true;

		#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
		generateTriangleStrips = false;
		#endif

		if(optimizationLevel.Equals("low"))
		{
			optLevel = 32000;
		}
		else if(optimizationLevel.Equals("medium"))
		{
			optLevel = 63000;
		}
		else if(optimizationLevel.Equals("high"))
		{
			optLevel = 81000;
		}

		StartCoroutine(_Batch(AddMeshColliders,RemoveLeftOvers,isItPatternExport,isPrepareForLightmapping));
	}

	public IEnumerator _Batch (bool AddMeshColliders=false, bool RemoveLeftOvers=false, bool isItPatternExport=false, bool isPrepareForLightmapping=false)
	{
		for(int i=0;i<BatchedObjects.Count;i++)
		{
			Destroy(BatchedObjects[i]);
		}

		BatchedObjects.Clear();
		
		Component[] filters  = GetComponentsInChildren(typeof(MeshFilter));
		Matrix4x4 myTransform = transform.worldToLocalMatrix;
		List<Hashtable> materialToMesh = new List<Hashtable>();
		int vertexCalc = 0;
		int hasIterations = 0;
		materialToMesh.Add(new Hashtable());

		for (int i=0;i<filters.Length;i++)
		{
			MeshFilter filter = (MeshFilter)filters[i];
			Renderer curRenderer  = filters[i].GetComponent<Renderer>();
			MeshCombineUtility.MeshInstance instance = new MeshCombineUtility.MeshInstance ();
			instance.mesh = filter.sharedMesh;

			if(!instance.mesh)
			{
				continue;
			}
			
			vertexCalc+=instance.mesh.vertexCount;
			
			if(curRenderer != null && curRenderer.enabled && instance.mesh != null)
			{
				instance.transform = myTransform * filter.transform.localToWorldMatrix;
				
				Material[] materials = curRenderer.sharedMaterials;
				for(int m=0;m<materials.Length;m++)
				{
					instance.subMeshIndex = System.Math.Min(m, instance.mesh.subMeshCount - 1);
	
					ArrayList objects = (ArrayList)materialToMesh[hasIterations][materials[m]];
					if(objects != null)
					{
						objects.Add(instance);
					}
					else
					{
						objects = new ArrayList ();
						objects.Add(instance);
						materialToMesh[hasIterations].Add(materials[m], objects);
					}

					if(vertexCalc>optLevel)
					{
						vertexCalc = 0;
						hasIterations++;
						materialToMesh.Add(new Hashtable());
					}
				}
				
				if(!RemoveLeftOvers)
				{
					curRenderer.enabled = false;
				}
			}
		}

		int counter = 0;

		for(int i=0;i<hasIterations+1;i++)
		{
			foreach (DictionaryEntry de  in materialToMesh[i])
			{
				#if UNITY_EDITOR
				if(EditorApplication.isPlaying)
				{
				#endif
				yield return 0;
				#if UNITY_EDITOR
				}
				#endif
				
				ArrayList elements = (ArrayList)de.Value;
				MeshCombineUtility.MeshInstance[] instances = (MeshCombineUtility.MeshInstance[])elements.ToArray(typeof(MeshCombineUtility.MeshInstance));

				GameObject go = new GameObject("uteTagID_1555");
				BatchedObjects.Add(go);
				go.transform.parent = transform;
				go.transform.localScale = Vector3.one;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localPosition = Vector3.zero;
				go.AddComponent(typeof(MeshFilter));
				go.AddComponent<MeshRenderer>();
				go.isStatic = true;
				go.GetComponent<Renderer>().material = (Material)de.Key;
				MeshFilter filter = (MeshFilter)go.GetComponent(typeof(MeshFilter));
				filter.mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);

				if(isPrepareForLightmapping)
				{
					#if UNITY_EDITOR
						#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
						Unwrapping.GeneratePerTriangleUV(filter.mesh);
						#endif
					Unwrapping.GenerateSecondaryUVSet(filter.mesh);
					#endif
				}

				if(AddMeshColliders)
				{
					#if UNITY_EDITOR
					if(EditorApplication.isPlaying)
					{
					#endif
					yield return 0;
					#if UNITY_EDITOR
					}
					#endif
					
					go.AddComponent<MeshCollider>();
				}
			}
		}

		if(RemoveLeftOvers)
		{
			List<GameObject> children = new List<GameObject>();
			int counterpp = 0;
			foreach (Transform child in transform) 
			{
				children.Add(child.gameObject);
			}

			#if UNITY_EDITOR
			if(EditorApplication.isPlaying)
			{
			#endif
				for(int s=0;s<children.Count;s++)
				{
					if(children[s].name!="uteTagID_1555")
					{
						#if UNITY_EDITOR
						if(EditorApplication.isPlaying)
						{
							if(s%1000==0)
							{
								yield return 0;
							}
						#endif
						Destroy(children[s]);
						#if UNITY_EDITOR
						}
						#endif
					}
					else
					{
						children[s].name = "Batch_"+(counterpp++).ToString();
					}
				}							
			#if UNITY_EDITOR
			}
			else
			{
				for(int s=0;s<children.Count;s++)
				{
					if(children[s].name!=("uteTagID_1555"))
					{
						DestroyImmediate(children[s],true);
					}
					else
					{
						children[s].name = "Batch_"+(counterpp++).ToString();
					}
				}
			}
			#endif
		}

		yield return 0;
	}
}