using UnityEngine;
using System.Collections;

public class uteDetectBuildCollision : MonoBehaviour {
#if UNITY_EDITOR
	void OnTriggerEnter()
	{
		if(uteGLOBAL3dMapEditor.OverlapDetection)
			uteGLOBAL3dMapEditor.canBuild = false;
		else
			uteGLOBAL3dMapEditor.canBuild = true;
	}

	void OnTriggerStay()
	{
		if(uteGLOBAL3dMapEditor.OverlapDetection)
			uteGLOBAL3dMapEditor.canBuild = false;
		else
			uteGLOBAL3dMapEditor.canBuild = true;
	}
	
	void OnTriggerExit()
	{
		uteGLOBAL3dMapEditor.canBuild = true;
	}
#endif
}
