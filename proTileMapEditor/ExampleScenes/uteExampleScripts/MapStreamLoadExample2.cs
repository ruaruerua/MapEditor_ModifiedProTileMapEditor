using UnityEngine;
using System.Collections;

public class MapStreamLoadExample2 : MonoBehaviour {

	private GameObject POINT;

	void Start () {
		POINT = GameObject.Find("POINT"); // our load point

		// Map Loader Component
		uteMapLoader mapLoader = this.gameObject.GetComponent<uteMapLoader>();

		//                                                                 Load start point,  objs/frame, initial obj load count    
		mapLoader.LoadMapAsyncFromPoint(POINT.transform.position, 15, 0);
		mapLoader.MapOffset = new Vector3(44,0,0);
		mapLoader.LoadMapAsyncFromPoint(POINT.transform.position, 15, 0);
		mapLoader.MapOffset = new Vector3(-44,0,0);
		mapLoader.LoadMapAsyncFromPoint(POINT.transform.position, 15, 0);
		mapLoader.MapOffset = new Vector3(-44,0,36);
		mapLoader.LoadMapAsyncFromPoint(POINT.transform.position, 15, 0);
		mapLoader.MapOffset = new Vector3(-44,0,-36);
		mapLoader.LoadMapAsyncFromPoint(POINT.transform.position, 15, 0);
		mapLoader.MapOffset = new Vector3(44,0,36);
		mapLoader.LoadMapAsyncFromPoint(POINT.transform.position, 15, 0);
		mapLoader.MapOffset = new Vector3(44,0,-36);
		mapLoader.LoadMapAsyncFromPoint(POINT.transform.position, 15, 0);
		mapLoader.MapOffset = new Vector3(0,0,36);
		mapLoader.LoadMapAsyncFromPoint(POINT.transform.position, 15, 0);
		mapLoader.MapOffset = new Vector3(0,0,-36);
		mapLoader.LoadMapAsyncFromPoint(POINT.transform.position, 15, 0);

	}
	
	void Update()
	{
		// Rotate camera
		Camera.main.transform.RotateAround(POINT.transform.position, Vector3.up, 2 * Time.deltaTime);
	}
}
