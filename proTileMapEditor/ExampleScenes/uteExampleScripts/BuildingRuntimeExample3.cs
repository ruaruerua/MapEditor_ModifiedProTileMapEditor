using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingRuntimeExample3 : MonoBehaviour {

	// Main class for runtime building
	private uteRuntimeBuilder runtimeBuilder;
	private List<uteRuntimeBuilder.Category> allCategories = new List<uteRuntimeBuilder.Category>();
	private bool isShow;
	private GUISkin skin;
	private bool middleClickDetected;
	private bool cameraActionDetected;

	private void Start()
	{
		isShow = false;

		middleClickDetected = false;
		cameraActionDetected = false;

		skin = (GUISkin) Resources.Load("uteForEditor/uteUI");

		// Get runtime builder main class
		runtimeBuilder = (uteRuntimeBuilder) this.gameObject.GetComponent<uteRuntimeBuilder>();

		// List all available category names
		List<string> allCategoryNames = runtimeBuilder.GetListOfCategoryNames();

		for(int i=0;i<allCategoryNames.Count;i++)
		{
			// Get all Category tiles and info by name
			allCategories.Add(runtimeBuilder.GetCategoryByCategoryName(allCategoryNames[i]));
		}
	}

	private void Update()
	{
		if(Input.GetMouseButtonDown(2))
		{
			middleClickDetected = true;
		}

		if(Input.GetMouseButtonUp(2))
		{
			middleClickDetected = false;
		}

		if(Input.GetKeyDown(KeyCode.LeftAlt))
		{
			cameraActionDetected = true;
		}

		if(Input.GetKeyUp(KeyCode.LeftAlt))
		{
			cameraActionDetected = false;
		}
		
		#if UNITY_IOS
		if(Input.GetMouseButtonDown(1))
		{
			cameraActionDetected = true;
		}

		if(Input.GetMouseButtonUp(1))
		{
			cameraActionDetected = false;
		}
		#endif

		// Disable RuntimeBuild when moving camera
		if(middleClickDetected||cameraActionDetected)
		{
			runtimeBuilder.DisableMouseInputForBuild();
			return;
		}
		else
		{
			runtimeBuilder.EnableMouseInputForBuild();
		}

		if(Input.mousePosition.y<70)
		{
			runtimeBuilder.DisableMouseInputForBuild();
		}
		else
		{
			runtimeBuilder.EnableMouseInputForBuild();
		}

		if(Input.GetKeyDown(KeyCode.LeftAlt))
		{
			runtimeBuilder.DisableMouseInputForBuild();
		}

		if(Input.GetKeyUp(KeyCode.LeftAlt))
		{
			runtimeBuilder.EnableMouseInputForBuild();

			return;
		}

		/*
		if(Input.mousePosition.y<100)
		{
			// Disable ability to build
			runtimeBuilder.DisableToBuild();
		}
		else
		{
			Enable ability to build
			runtimeBuilder.EnableToBuild();
		}
		*/

		// Press O to rotate current tile LEFT
		if(Input.GetKeyDown(KeyCode.O))
		{
			runtimeBuilder.RotateCurrentTileLeft();
		//	runtimeBuilder.AddTileInCategory(GameObject.Find("CubeCube"),"Castle","Test","My Test",new Texture2D(20,20),true);
		}
		// Press P to rotate current tile RIGHT
		else if(Input.GetKeyDown(KeyCode.P))
		{
		//	runtimeBuilder.RemoveTileFromCategory("Castle","Test");
			runtimeBuilder.RotateCurrentTileRight();
		}
		// Press K to rotate current tile UP
		else if(Input.GetKeyDown(KeyCode.K))
		{
			runtimeBuilder.RotateCurrentTileUp();
		}
		// Press L to rotate current tile DOWN
		else if(Input.GetKeyDown(KeyCode.L))
		{
			runtimeBuilder.RotateCurrentTileDown();
		}
		// Press J to FLIP tile
		else if(Input.GetKeyDown(KeyCode.J))
		{
			runtimeBuilder.RotateCurrentTileFlip();
		}
		// Press SPAPCE to destroy hovering Tile (if hovering)
		else if(Input.GetKeyDown(KeyCode.Space))
		{
			// Get GameObject which is mouse hovering
			if(runtimeBuilder.GetCurrentSelectedObject())
			{
				// Destroy selected tile
				bool isSuccess = runtimeBuilder.DestroyCurrentSelectedObject();

				if(isSuccess)
				{
					Debug.Log("Object Destroyed");
				}
				else
				{
					Debug.Log("No Object Selected");
				}
			}
		}
		// Press 1 to set build mode to NORMAL
		else if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			runtimeBuilder.SetBuildMode(uteRuntimeBuilder.BuildMode.Normal);
		}
		// Press 2 to set build mode to Continous
		else if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			runtimeBuilder.SetBuildMode(uteRuntimeBuilder.BuildMode.Continuous);
		}
		// Press 3 to set build mode to Mass
		else if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			runtimeBuilder.SetBuildMode(uteRuntimeBuilder.BuildMode.Mass);
		}
		// Press 4 to decrease Mass Build height
		else if(Input.GetKeyDown(KeyCode.Alpha4))
		{
			runtimeBuilder.MassBuildHeightDown();
		}
		// Press 5 to increase Mass Build height
		else if(Input.GetKeyDown(KeyCode.Alpha5))
		{
			runtimeBuilder.MassBuildHeightUp();
		}
		// Press 6 to set Mass Build height to 1
		else if(Input.GetKeyDown(KeyCode.Alpha6))
		{
			runtimeBuilder.MassBuildResetHeight();
		}
		// Press 7 to Cancel Mass Build placement
		else if(Input.GetKeyDown(KeyCode.Alpha7))
		{
			runtimeBuilder.MassBuildCancel();
		}
		// Press 9 Save Map as test1
		else if(Input.GetKeyDown(KeyCode.Alpha9))
		{
			runtimeBuilder.SaveMap("test1");
		}
		// Press 8 to Load Map with name test1
		else if(Input.GetKeyDown(KeyCode.Alpha8))
		{
			runtimeBuilder.LoadMap("test1"); // To Load Map additively use runtimeBuilder.LoadMap("test1",true);
		}
		// Press 0 to Delete Map with name test1
		else if(Input.GetKeyDown(KeyCode.Alpha0))
		{
			runtimeBuilder.DeleteMap("test1");
		}
		// Batch Static Tiles
		else if(Input.GetKeyDown(KeyCode.B))
		{
			// first argument is to add mesh collider (important to note that if you add mesh collider and you destroy selected "tile" it will destroy the whole batched mesh)
			// second to remove objects that are left after batching (important to note that if you remove all leftovers you will not be able to save your map at runtime)
			// Basically use true/true only if you load map for playable version

			runtimeBuilder.Batch(false,false);
		}
		else if(Input.GetKeyDown(KeyCode.V))
		{
			// Unbach Tiles (only if you do not remove left overs with Batch)
			runtimeBuilder.UnBatch();
		}
		else if(Input.GetKeyDown(KeyCode.H))
		{
			for(int i=0;i<10;i++)
			{
				for(int j=0;j<10;j++)
				{
					// Procedural Tile placement at Vector3 position
					runtimeBuilder.PlaceCurrentTileAtPosition(new Vector3(i,1,j));
				}
			}
		}
		else if(Input.GetKeyDown(KeyCode.F))
		{
			// Place Tile at current position (by default is now with mouse click but you can force it to place with other inputs)
			runtimeBuilder.PlaceCurrentTileAtCurrentPosition();
			// If you want to disable default mouse click tile placement use:
			// runtimeBuilder.DisableMouseInputForBuild();
		}
	}

	private void OnGUI()
	{
		GUI.skin = skin;

		//GUI.Label(new Rect(10,10,100,40),"Build Mode: "+runtimeBuilder.GetCurrentBuildMode());

		if(isShow)
		{
			int x = 0;
			int y = 0;
			int yOffset = 0;

			GUI.Box(new Rect(0,0,Screen.width,750),"Tiles");

			for(int i=0;i<allCategories.Count;i++)
			{
				if(allCategories[i].name.Equals("House")||allCategories[i].name.Equals("Other"))
				{
					for(int j=0;j<allCategories[i].allTiles.Count;j++)
					{
						// Get Tile from current cateogry tiles
						uteRuntimeBuilder.Tile tile = (uteRuntimeBuilder.Tile) allCategories[i].allTiles[j];

						GUI.Box(new Rect(10+(x*125),(y*175)+yOffset,120,175),tile.name);

						if(GUI.Button(new Rect(10+(x*125),140+(y*175)+yOffset,120,30),"BUY"))
						{
							// Set selected tile for building
							runtimeBuilder.SetCurrentTile(tile.mainObject);
							// Hide list
							isShow = false;
						}

						// Draw Tile preview texture
						GUI.DrawTexture(new Rect(15+(x*125),25+(y*175)+yOffset,110,110),tile.preview);

						// Show tile name
						//GUI.Label(new Rect(14+(x*125),100+(y*175)+yOffset,120,125),tile.name);

						x++;
						if(x==12)
						{
							x=0;
							y++;
						}
					}

					yOffset += 175;
					x=0;
				}
			}
		}

		// Show/Hide items
		if(GUI.Button(new Rect(450,Screen.height-70,200,70),"ITEMS"))
		{
			if(isShow)
			{
				isShow = false;
			}
			else
			{
				isShow = true;
				// Cancel current Tile
				runtimeBuilder.CancelCurrentTile();
			}
		}

		if(GUI.Button(new Rect(10,Screen.height-70,200,70),"Mass Build"))
		{
			runtimeBuilder.SetBuildMode(uteRuntimeBuilder.BuildMode.Mass);
		}

		if(GUI.Button(new Rect(220,Screen.height-70,200,70),"Normal Build"))
		{
			runtimeBuilder.SetBuildMode(uteRuntimeBuilder.BuildMode.Normal);
		}

		if(GUI.Button(new Rect(680,Screen.height-70,200,70),"Save Map"))
		{
			runtimeBuilder.SaveMap("HouseSave_1");
		}

		if(GUI.Button(new Rect(890,Screen.height-70,200,70),"Load Map"))
		{
			runtimeBuilder.LoadMap("HouseSave_1");
		}

		if(GUI.Button(new Rect(Screen.width-200,Screen.height-70,200,70),"Rotate"))
		{
			runtimeBuilder.RotateCurrentTileLeft();
		}
	}
}
