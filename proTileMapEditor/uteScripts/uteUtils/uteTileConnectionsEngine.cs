using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class uteTileConnectionsEngine : MonoBehaviour {
#if UNITY_EDITOR
	public class Tile
	{
		public Vector3 pos;
		public Vector3 scale;
		public BoxCollider collider;
		public int hitType;

		public Tile(Vector3 _pos, Vector3 _scale, BoxCollider _collider, int _hitType)
		{
			pos = _pos;
			scale = _scale;
			collider = _collider;
			hitType = _hitType;
		}
	}

	private List<Tile> newTiles = new List<Tile>();
	private List<GameObject> dummyTrash = new List<GameObject>();
	private List<GameObject> forApplyObj = new List<GameObject>();
	private List<Vector3> forApplyPos = new List<Vector3>();
	private List<Vector3> forApplyRot = new List<Vector3>();
	private List<string> forApplyNames = new List<string>();
	private List<string> forApplyGuids = new List<string>();
	private GameObject tcDummy;
	private List<GameObject> tileSet = new List<GameObject>();
	private List<string> tileNames = new List<string>();
	private List<string> tileGuids = new List<string>();
	private List<string> tileRots = new List<string>();
	private string currentTcFamilyName;
	private uteMapEditorEngine uteMEE;
	private Vector3 checkXA;
	private Vector3 checkXB;
	private Vector3 checkZA;
	private Vector3 checkZB;
	private Vector3 castFullPos;
	private int currentTileID;
	[HideInInspector]
	public bool isBuilding;

	private void Start()
	{
		currentTileID = 0;
		currentTcFamilyName = "";
		isBuilding = false;
		tcDummy = (GameObject) Resources.Load("uteForEditor/uteTcDummy");
		uteMEE = this.gameObject.GetComponent<uteMapEditorEngine>();
	}

	public void tcBuildStart(List<GameObject> currentTileSet, List<string> currentNames, List<string> currentGuids, string fammilyName, List<string> currentTileRotations, int _currentTileID)
	{
		currentTileID = _currentTileID;
		isBuilding = true;
		tileSet = currentTileSet;
		tileNames = currentNames;
		tileGuids = currentGuids;
		tileRots = currentTileRotations;
		currentTcFamilyName = fammilyName;

		if(newTiles.Count>0)
		{
			newTiles.Clear();
		}
	}

	public void AddTile(GameObject obj)
	{
		bool isGoodToGo = true;

		if(newTiles.Count>0)
		{
			Vector3 lastObjectPos = ((Tile) newTiles[newTiles.Count-1]).pos;
			float distance = Vector2.Distance(new Vector2(obj.transform.position.x,obj.transform.position.z),new Vector2(lastObjectPos.x,lastObjectPos.z));

			if(distance<obj.GetComponent<Collider>().bounds.size.x||Mathf.Abs(lastObjectPos.y-obj.transform.position.y)>0.05f)
			{
				isGoodToGo = false;
			}
		}

		if(isGoodToGo)
		{
			newTiles.Add(new Tile(obj.transform.position,obj.transform.localScale,obj.GetComponent<BoxCollider>(),0));

			GameObject newDummy = (GameObject) Instantiate(tcDummy,obj.transform.position+(new Vector3(0,obj.GetComponent<BoxCollider>().center.y,0)),obj.transform.rotation);
			newDummy.transform.localScale = obj.GetComponent<Collider>().bounds.size;//new Vector3(obj.transform.collider.bounds.size.x,1.0f,obj.transform.collider.bounds.size.z);// - (new Vector3(0.0f,0.0f+(obj.transform.collider.bounds.center.y*obj.transform.localScale.y),0.0f));//new Vector3(obj.transform.localScale.x,0.1f,obj.transform.localScale.z);
			BoxCollider coll = (BoxCollider) newDummy.AddComponent<BoxCollider>();
			coll.size += new Vector3(-0.01f,-0.01f,-0.01f);
			newDummy.AddComponent<uteDummyTag>();
			dummyTrash.Add(newDummy);
		}
	}

	void Update()
	{
		Debug.DrawLine(castFullPos,checkXA,Color.red,0.0f,false);
		Debug.DrawLine(castFullPos,checkXB,Color.red,0.0f,false);
		Debug.DrawLine(castFullPos,checkZA,Color.green,0.0f,false);
		Debug.DrawLine(castFullPos,checkZB,Color.green,0.0f,false);
	}

	public void FinishUp()
	{
		if(newTiles.Count>0)
		{
			if(forApplyObj.Count>0)
			{
				forApplyObj.Clear();
				forApplyPos.Clear();
				forApplyGuids.Clear();
				forApplyNames.Clear();
				forApplyRot.Clear();
			}

			for(int i=0;i<newTiles.Count;i++)
			{
				Tile tile = (Tile) newTiles[i];

			    int sizeX = (int) tile.collider.bounds.size.x;
				int sizeY = (int) tile.collider.bounds.size.y;
				int sizeZ = (int) tile.collider.bounds.size.z;

				float centerX = ((float)sizeX)/2.0f;
				float centerY = ((float)sizeY)/2.0f;
				float centerZ = ((float)sizeZ)/2.0f;
				
				float centerPosX = centerX+(tile.pos.x-((float)sizeX/2.0f));
				float centerPosY = centerY+(tile.pos.y-((float)sizeY/2.0f));
				float centerPosZ = centerZ+(tile.pos.z-((float)sizeZ/2.0f));

				float offsetFix = tile.collider.bounds.size.x;
				float castPosX = centerPosX+(tile.collider.center.x*tile.scale.x);
				float castPosZ = centerPosZ+(tile.collider.center.z*tile.scale.z);
				float castPosY = centerPosY+(tile.collider.center.y*tile.scale.y);

				castFullPos = new Vector3(castPosX,castPosY,castPosZ);
				checkXA = new Vector3(castPosX+centerX-offsetFix,castPosY,castPosZ);
				checkXB = new Vector3(castPosX-centerX+offsetFix,castPosY,castPosZ);
				checkZA = new Vector3(castPosX,castPosY,castPosZ-offsetFix+centerZ);
				checkZB = new Vector3(castPosX,castPosY,castPosZ+offsetFix-centerZ);

				bool collXB = false;
				bool collXA = false;
				bool collZB = false;
				bool collZA = false;

				RaycastHit lineHit;
				if(Physics.Linecast(castFullPos, checkZB, out lineHit))
				{
					collZB = hasValidIdentity(lineHit.collider.gameObject);
				}

				if(Physics.Linecast(castFullPos, checkZA, out lineHit))
				{
					collZA = hasValidIdentity(lineHit.collider.gameObject);
				}

				if(Physics.Linecast(castFullPos, checkXB, out lineHit))
				{
					collXB = hasValidIdentity(lineHit.collider.gameObject);
				}

				if(Physics.Linecast(castFullPos, checkXA, out lineHit))
				{
					collXA = hasValidIdentity(lineHit.collider.gameObject);
				}
				
				int tcID = 0;
				Vector3 tcRot = Vector3.zero;

				int[] rDefaults = new int[5];

				for(int z=0;z<5;z++)
				{
					rDefaults[z] = System.Convert.ToInt32(tileRots[z]);
				} 

				if(collZB&&collZA&&collXA&&collXB)
				{
					tcRot = new Vector3(.0f,.0f,.0f);
					tcID = 3;
				}
				else if((collZB&&collZA&&collXA&&!collXB)||(collZB&&collZA&&!collXA&&collXB)||(collZB&&!collZA&&collXA&&collXB)||(!collZB&&collZA&&collXA&&collXB))
				{
					if(collZB&&collZA&&collXA&&!collXB)
					{
						tcRot = new Vector3(0,270,0);
					}
					else if(collZB&&collZA&&!collXA&&collXB)
					{
						tcRot = new Vector3(0,90,0);
					}
					else if(collZB&&!collZA&&collXA&&collXB)
					{
						tcRot = new Vector3(.0f,.0f,.0f);
					}
					else if(!collZB&&collZA&&collXA&&collXB)
					{
						tcRot = new Vector3(0,180,0);
					}

					tcID = 2;
				}
				else if((collZB&&collZA)||(collXA&&collXB))
				{
					if(collZB&&collZA)
					{
						tcRot = new Vector3(.0f,.0f,.0f);
					}
					else if(collXA&&collXB)
					{
						tcRot = new Vector3(.0f,90,.0f);
					}

					tcID = 1;
				}
				else if((collZB&&collXB)||(collZA&&collXA)||(collZB&&collXA)||(collZA&&collXB))
				{
					if(collZB&&collXB)
					{
						tcRot = new Vector3(0,90,0);
					}
					else if(collZA&&collXA)
					{
						tcRot = new Vector3(0,270,0);
					}
					else if(collZB&&collXA)
					{
						tcRot = new Vector3(.0f,.0f,.0f);
					}
					else if(collZA&&collXB)
					{
						tcRot = new Vector3(0,180,0);
					}

					tcID = 0;
				}
				else if(collZB||collZA||collXA||collXB)
				{
					if(collZB)
					{
						tcRot = new Vector3(.0f,.0f,.0f);
					}
					else if(collZA)
					{
						tcRot = new Vector3(0,180,0);
					}
					else if(collXA)
					{
						tcRot = new Vector3(0,270,0);
					}
					else if(collXB)
					{
						tcRot = new Vector3(0,90,0);
					}

					tcID = 4;
				}
				else
				{
					tcID = currentTileID;
				}

				GameObject obj = (GameObject) tileSet[tcID];
				string tcGuid = tileGuids[tcID].ToString();
				string tcName = tileNames[tcID].ToString();

				forApplyObj.Add(obj);
				forApplyPos.Add(tile.pos);
				forApplyGuids.Add(tcGuid);
				forApplyNames.Add(tcName);
				forApplyRot.Add(tcRot+new Vector3(0,rDefaults[tcID],0));
			}
		}

		if(forApplyObj.Count>0)
		{
			for(int i=0;i<forApplyObj.Count;i++)
			{
				GameObject obj = (GameObject) forApplyObj[i];
				Vector3 pos = forApplyPos[i];
				string tcName = forApplyNames[i].ToString();
				string tcGuid = forApplyGuids[i].ToString();
				Vector3 rot = forApplyRot[i];
				uteMEE.ApplyBuild(obj,pos,tcName,tcGuid,rot,currentTcFamilyName);
			}
		}

		if(newTiles.Count>0)
		{
			newTiles.Clear();
		}

		if(dummyTrash.Count>0)
		{
			for(int i=0;i<dummyTrash.Count;i++)
			{
				GameObject go = (GameObject) dummyTrash[i];
				Destroy(go);
			}

			dummyTrash.Clear();
		}

		isBuilding = false;
	}

	private bool hasValidIdentity(GameObject go)
	{
		if(go.GetComponent<uteTcTag>()||go.GetComponent<uteDummyTag>())
		{
			if(go.GetComponent<uteTcTag>())
			{
				if(go.GetComponent<uteTcTag>().tcFamilyName==currentTcFamilyName)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		return false;
	}
#endif
}
