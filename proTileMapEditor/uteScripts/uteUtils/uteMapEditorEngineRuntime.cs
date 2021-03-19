using UnityEngine;
using System.Collections;

public class uteMapEditorEngineRuntime : MonoBehaviour {

#if UNITY_EDITOR
#pragma warning disable 0219
#endif

	[HideInInspector]
	public GameObject MAINPLATFORM;
	[HideInInspector]
	public uteRuntimeBuilder RuntimeBuilder;
	public GameObject currentTile;
	private static float ZERO_F = .0f;
	private GameObject hitObject;
	private int isRotated;
	private float offsetZ;
	private float offsetX;
	private float offsetY;
	private float lastPosX;
	private float lastPosY;
	private float lastPosZ;
	private float rayCastDistance;
	[HideInInspector]
	public string yTypeOption;
	private bool canBuild;
	private bool isMouseDown;
	private GameObject MAP_ALLTILES;
	private GameObject MAP_STATIC;
	private GameObject MAP_DYNAMIC;
	private GameObject lastTile;
	private bool isContinuesBuild;
	private bool isNormalBuild;
	[HideInInspector]
	public bool isBuildingMass;
	private bool isCurrentStatic;
	private uteMassBuildEngine uMBE;
	private bool cantBuildForced;
	private string currentObjectID;
	private bool mouseInputBuildEnabled;
	private bool isFixedRaycast;
	private Vector2 fixedRaycastPosition;
	private Vector3 customTileOffset;

	private void Awake()
	{
		isFixedRaycast = false;
		mouseInputBuildEnabled = true;
		cantBuildForced = false;
		isCurrentStatic = true;
		isContinuesBuild = false;
		isNormalBuild = true;
		isBuildingMass = false;
		isRotated = 0;
		offsetY = .0f;
		offsetX = .0f;
		offsetZ = .0f;
		lastPosZ = .0f;
		lastPosY = .0f;
		lastPosX = .0f;
		rayCastDistance = 1000f;
		canBuild = false;
		isMouseDown = false;
		currentObjectID = "-1";
		yTypeOption = "auto";
		lastTile = null;

		MAP_ALLTILES = new GameObject("uteMAP_ALLTILES");
		MAP_STATIC = new GameObject("uteMAP_STATIC");
		MAP_STATIC.transform.parent = MAP_ALLTILES.transform;
		MAP_DYNAMIC = new GameObject("uteMAP_DYNAMIC");
		MAP_DYNAMIC.transform.parent = MAP_ALLTILES.transform;

		uMBE = (uteMassBuildEngine) this.gameObject.AddComponent<uteMassBuildEngine>();
	}

	public void SetFixedRaycastPosition(Vector2 pos)
	{
		isFixedRaycast = true;
		fixedRaycastPosition = pos;
	}

	public void DisableFixedRaycastPosition()
	{
		isFixedRaycast = false;
	}

	public Vector2 GetFixedRaycastPosition()
	{
		return fixedRaycastPosition;
	}

	public void SetSnapOption(string option)
	{
		yTypeOption = option;
	}

	public string GetSnapOption()
	{
		return yTypeOption;
	}

	public void SetCantBuildForced()
	{
		cantBuildForced = true;
	}

	public void UnsetCantBuildForced()
	{
		cantBuildForced = false;
	}

	public void SetBuildModeNormal()
	{
		isBuildingMass = false;
		isContinuesBuild = false;
		isNormalBuild = true;
	}

	public void SetBuildModeContinuous()
	{
		isBuildingMass = false;
		isContinuesBuild = true;
		isNormalBuild = false;
	}

	public void SetBuildModeMass()
	{
		isBuildingMass = true;
		isContinuesBuild = false;
		isNormalBuild = false;
	}

	public IEnumerator SetCurrentTile(GameObject go)
	{
		yield return new WaitForSeconds(0.03f);

		if(go.transform.parent.gameObject.name.Equals("uteRUNTIME_TILES_STATIC"))
		{
			isCurrentStatic = true;
		}
		else
		{
			isCurrentStatic = false;
		}

		currentTile = go;

		uteTilePivotOffset to = currentTile.GetComponentInChildren<uteTilePivotOffset>();

		if(to)
		{
			customTileOffset = to.TilePivotOffset;
		}
		else
		{
			customTileOffset = Vector3.zero;
		}

		currentObjectID = currentTile.name;

		yield return 0;
	}

	public void SetCurrentTileInstantly(GameObject go)
	{
		if(go.transform.parent.gameObject.name.Equals("uteRUNTIME_TILES_STATIC"))
		{
			isCurrentStatic = true;
		}
		else
		{
			isCurrentStatic = false;
		}

		currentTile = go;

		uteTilePivotOffset to = currentTile.GetComponentInChildren<uteTilePivotOffset>();

		if(to)
		{
			customTileOffset = to.TilePivotOffset;
		}
		else
		{
			customTileOffset = Vector3.zero;
		}

		currentObjectID = currentTile.name;
	}

	public void CancelCurrentTile()
	{
		if(currentTile)
		{
			currentTile.transform.position = new Vector3(-10000,-10000,-10000);
			currentTile = null;
		}
	}

	public GameObject GetCurrentSelectedObject()
	{
		if(hitObject)
		{
			if(hitObject!=MAINPLATFORM)
			{
				return hitObject;
			}
		}

		return null;
	}

	public bool DestroyCurrentSelectedObject()
	{
		if(hitObject)
		{
			if(hitObject!=MAINPLATFORM)
			{
				Destroy(hitObject);
				return true;
			}
		}

		return false;
	}

	private void Update()
	{
		if(cantBuildForced)
		{
			return ;
		}

		if(mouseInputBuildEnabled)
		{
			if(Input.GetMouseButtonUp(0))
			{
				isMouseDown = false;
			}
		}

		if(currentTile)
		{
			Vector2 raycastPos = Input.mousePosition;

			if(isFixedRaycast)
			{
				raycastPos = fixedRaycastPosition;
			}

			Ray buildRay = Camera.main.ScreenPointToRay(raycastPos);
    		RaycastHit buildHit;

    		if(Physics.Raycast(buildRay,out buildHit, rayCastDistance))
    		{
    			if(buildHit.collider)
    			{
    				GameObject hitObj = buildHit.collider.gameObject;
    				hitObject = hitObj;

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

					// debug
					Debug.DrawLine(castFullPos,checkXA,Color.red,ZERO_F,false);
					Debug.DrawLine(castFullPos,checkXB,Color.red,ZERO_F,false);
					Debug.DrawLine(castFullPos,checkZA,Color.green,ZERO_F,false);
					Debug.DrawLine(castFullPos,checkZB,Color.green,ZERO_F,false);
					Debug.DrawLine(castFullPos,checkYA,Color.blue,ZERO_F,false);
					Debug.DrawLine(castFullPos,checkYB,Color.blue,ZERO_F,false);
									
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
							//Debug.Log("("+buildHit.point.y+"+("+currentTile.collider.bounds.size.y+"/2.0f))-"+currentTile.GetComponent<BoxCollider>().center.y+"0.0000001f");;
							posY = (buildHit.point.y+(currentTile.GetComponent<Collider>().bounds.size.y/2.0f))-(currentTile.GetComponent<BoxCollider>().center.y*currentTile.transform.localScale.y)+0.000001f;//posY = (Mathf.Round(buildHit.point.y+normalY)+internalOffsetY);
							//Debug.Log(posY);
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
						posY = ((Mathf.Round((buildHit.point.y+normalY)))+internalOffsetY);//posY = (Mathf.Round(buildHit.point.y+normalY)+internalOffsetY);
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

    				Vector3 calculatedPosition = new Vector3(posX,finalPosY+addOnTop,posZ);

    				if(uteGLOBAL3dMapEditor.XZsnapping==false)
    				{
    					calculatedPosition = new Vector3(buildHit.point.x+Xpivot,finalPosY+addOnTop,buildHit.point.z+Zpivot);
    				}

    				calculatedPosition += customTileOffset;
    				//cameraMove.sel = calculatedPosition;

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

				if(canBuild)
	    		{
	    			//helpers_CANTBUILD.transform.position = new Vector3(-1000,0,-1000);

	    			if(mouseInputBuildEnabled)
	    			{
		    			if(Input.GetMouseButtonUp(0))
		    			{
		    				isMouseDown = false;
		    			//	isMouseDown = true;

		    				//if(!isContinuesBuild&&!isBuildingTC&&!isBuildingMass)

		    				if(isNormalBuild)
		    				{
		    					ApplyBuild();
		    				}

		    			/*	if(isBuildingTC)
		    				{
		    					uTCE.tcBuildStart(tcGoes,tcNames,tcGuids,currentTcFamilyName,tcRots,currentTCID);
		    				}

		    				if(isBuildingMass)
		    				{
		    					uMBE.massBuildStart(currentTile,currentObjectID,currentObjectGUID);
		    				}
		    				*/
			   			}

			   			if(Input.GetMouseButtonDown(0))
			   			{
			   				lastTile = null;
			   				isMouseDown = true;

			   				if(isBuildingMass)
		    				{
		    					uMBE.massBuildStart(currentTile,currentObjectID,currentObjectID);
		    				}
			   			}
			   		}
	    		}
			}
		}
		else
		{
			Vector2 raycastPos = Input.mousePosition;

			if(isFixedRaycast)
			{
				raycastPos = fixedRaycastPosition;
			}
			
			Ray buildRay2 = Camera.main.ScreenPointToRay(raycastPos);
    		RaycastHit buildHit2;

    		if(Physics.Raycast(buildRay2,out buildHit2, rayCastDistance))
    		{
    			if(buildHit2.collider)
    			{
    				GameObject hitObj = buildHit2.collider.gameObject;
    				hitObject = hitObj;
    			}
    			else
    			{
    				hitObject = null;
    			}
    		}
    		else
    		{
    			hitObject = null;
    		}
		}
	}

	public void SetRaycastDistance(float distance)
	{
		rayCastDistance = distance;
	}

	public void DisableMouseInputForBuild()
	{
		mouseInputBuildEnabled = false;
	}

	public void EnableMouseInputForBuild()
	{
		mouseInputBuildEnabled = true;	
	}

	public void MassBuildStepDown()
	{
		uMBE.StepDown();
	}

	public void MassBuildStepUp()
	{
		uMBE.StepUp();
	}

	public void MassBuildStepOne()
	{
		uMBE.StepOne();
	}

	public void MassBuildCancel()
	{
		uMBE.Cancel();
	}

	public IEnumerator SmoothTileRotate(int rot)
	{
		if(currentTile)
		{
			for(int i=0;i<9;i++)
			{
				switch(rot)
				{
					case 0:
					currentTile.transform.Rotate(new Vector3(10,0,0));
					break;
					case 1:
					currentTile.transform.Rotate(new Vector3(0,10,0));
					break;
					case 2:
					currentTile.transform.Rotate(new Vector3(-10,0,0));
					break;
					case 3:
					currentTile.transform.Rotate(new Vector3(0,-10,0));
					break;
					case 4:
					currentTile.transform.Rotate(new Vector3(0,0,20));
					break;
					default: break;
				}

				yield return 0;
			}
		}

		yield return 0;
	}

	private void FixedUpdate()
	{
		if(isMouseDown&&(isContinuesBuild||isBuildingMass)&&canBuild&&!isNormalBuild)
		{
			if(isBuildingMass)
			{
				if(currentTile)
				{
					StartCoroutine(uMBE.AddTile(currentTile));
				}
			}
			else
			{
				if(currentTile)
				{
					ApplyBuild();
				}
			}
			/*if(isBuildingMass)
			{
				if(currentTile)
				{
					StartCoroutine(uMBE.AddTile(currentTile));
				}
			}
			else if(!isBuildingTC)
			{*/
				//ApplyBuild();
			/*}
			else
			{
				if(currentTile)
				{
					uTCE.AddTile(currentTile);
				}
			}*/
		}
	}

	public void ApplyBuild(GameObject tcObj=null, Vector3 tcPos = default(Vector3), string tcName = default(string), string tcGuid = default(string), Vector3 tcRot = default(Vector3), string tcFamilyName = default(string), bool isMassBuild = false)
	{
		GameObject newObj = null;
		bool goodToGo = false;

		if(tcObj!=null)
		{
			Vector3 _pos = new Vector3(RoundTo(tcPos.x),tcPos.y,RoundTo(tcPos.z));
			
			newObj = (GameObject) Instantiate(tcObj,_pos,tcObj.transform.rotation);
			
			newObj.transform.localEulerAngles = tcRot;

			goodToGo = true;
		}
		else if(currentTile)
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
						newObj = (GameObject) Instantiate(currentTile,_pos,currentTile.transform.rotation);

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
				Vector3 _pos = new Vector3(RoundTo(currentTile.transform.position.x),currentTile.transform.position.y,RoundTo(currentTile.transform.position.z));
				newObj = (GameObject) Instantiate(currentTile,_pos,currentTile.transform.rotation);

				goodToGo = true;
			}

			if(lastTile==null)
			{
				lastTile = newObj;
			}
		}

		if(currentTile&&goodToGo)
	 	{
			newObj.layer = 0;
			Destroy(newObj.GetComponent<Rigidbody>());
			Destroy(newObj.GetComponent<uteDetectBuildCollision>());
			newObj.GetComponent<Collider>().isTrigger = false;
		}

		if(goodToGo)
		{
			RoundTo90(newObj);

			if(isCurrentStatic)
			{
				if(!MAP_STATIC)
				{
					MAP_STATIC = (GameObject) GameObject.Find("uteMAP_ALLTILES/uteMAP_STATIC");
				}

				newObj.transform.parent = MAP_STATIC.transform;
				newObj.isStatic = true;
				SetObjectsStaticInside(newObj);
			}
			else
			{
				if(!MAP_DYNAMIC)
				{
					MAP_DYNAMIC = (GameObject) GameObject.Find("uteMAP_ALLTILES/uteMAP_DYNAMIC");
				}

				newObj.transform.parent = MAP_DYNAMIC.transform;
				newObj.isStatic = false;
			}

			/*if(tcObj!=null)
			{
				newObj.name = tcName;
			}
			else
			{*/
			if(currentTile)
			{
				newObj.name = currentTile.name;
			}

			if(newObj)
			{
				newObj.AddComponent<uteTagObject>();
			}
			//}
		}

		if(goodToGo)
		{
			uteGLOBAL3dMapEditor.mapObjectCount++;
		}
	}

	private void SetObjectsStaticInside(GameObject go)
	{
		MeshRenderer[] mrs = (MeshRenderer[]) go.GetComponentsInChildren<MeshRenderer>();

		for(int i=0;i<mrs.Length;i++)
		{
			MeshRenderer mr = (MeshRenderer) mrs[i];

			if(mr)
			{
				mr.gameObject.isStatic = true;
			}
		}

		SkinnedMeshRenderer[] smrs = (SkinnedMeshRenderer[]) go.GetComponentsInChildren<SkinnedMeshRenderer>();

		for(int i=0;i<smrs.Length;i++)
		{
			SkinnedMeshRenderer smr = (SkinnedMeshRenderer) smrs[i];

			if(smr)
			{
				smr.gameObject.isStatic = true;
			}
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
}
