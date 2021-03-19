using UnityEngine;
using System.Collections;

public class MapStreamLoadExample : MonoBehaviour {

	private GameObject POINT;

	void Start () {
		POINT = GameObject.Find("POINT"); // our load point
		//                                                                 Load start point,  objs/frame, initial obj load count    
		this.gameObject.GetComponent<uteMapLoader>().LoadMapAsyncFromPoint(POINT.transform.position, 15, 0);

	}
	
	void Update()
	{
		// Rotate camera
		Camera.main.transform.RotateAround(POINT.transform.position, Vector3.up, 2 * Time.deltaTime);
	}
}
