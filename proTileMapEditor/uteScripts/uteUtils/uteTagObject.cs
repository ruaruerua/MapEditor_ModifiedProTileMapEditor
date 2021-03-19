using UnityEngine;

public class uteTagObject : MonoBehaviour {
#if UNITY_EDITOR
	[HideInInspector]
	public string objGUID = "";
	[HideInInspector]
	public bool isStatic = false;
	[HideInInspector]
	public bool isTC = false;
	[HideInInspector]
	public string layerName;
#endif
}
