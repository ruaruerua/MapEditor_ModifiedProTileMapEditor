using UnityEngine;
using System.Collections;

public class uteHelpBox : MonoBehaviour {
#if UNITY_EDITOR
	[HideInInspector]
	public bool isShow;
	private string[] keys;
	private string[] keysInfo;
	private int keysCount;
	private GUISkin ui;

	private void Start()
	{
		ui = (GUISkin) Resources.Load("uteForEditor/uteUI");
		keysCount = 15;
		keys = new string[keysCount];
		keysInfo = new string[keysCount];
		isShow = false;

		SetInfo();
	}
	
	private void OnGUI()
	{
		if(isShow)
		{
			GUI.skin = ui;
			GUI.Box(new Rect((Screen.width/2)-250,50,550,530),"HELP");

			for(int i=0;i<keysCount;i++)
			{
				GUI.Label(new Rect((Screen.width/2)-220,80+(i*26),250,26),keys[i]);
				GUI.Label(new Rect((Screen.width/2)-10,80+(i*26),400,26),keysInfo[i]);
			}

			if(GUI.Button(new Rect((Screen.width/2)-220,470,490,40),"For more information visit www.protilemapeditor.com"))
			{
				Application.OpenURL("http://www.protilemapeditor.com");
			}

			if(GUI.Button(new Rect((Screen.width/2)+130,530,150,30),"Close"))
			{
				isShow = false;
			}
		}
	}

	private void SetInfo()
	{
		keys[0] = "Hold ALT + Left Mouse Drag";
		keysInfo[0] = "- Camera orbit rotation.";
		keys[1] = "Hold ALT + Right Mouse Drag";
		keysInfo[1] = "- Camera pan movement.";
		keys[2] = "Middle Mouse Drag";
		keysInfo[2] = "- Camera pan movement.";
		keys[3] = "Mouse Scroll";
		keysInfo[3] = "- Scroll down/up will zoom in/out camera.";
		keys[4] = "Key TAB";
		keysInfo[4] = "- Switch Build Mode.";
		keys[5] = "Key X, C";
		keysInfo[5] = "- Move grid up / down (defined in settings).";
		keys[6] = "Keys W, S, D, A";
		keysInfo[6] = "- Move camera with Keyboard.";
		keys[7] = "Keys Q, E";
		keysInfo[7] = "- Rotate camera left/right.";
		keys[8] = "Key V";
		keysInfo[8] = "- Show/Hide object line helpers.";
		keys[9] = "Key R";
		keysInfo[9] = "- Reset camera rotation to default.";
		keys[10] = "Mouse Left Click";
		keysInfo[10] = "- Place object (drag for mass build).";
		keys[11] = "Mouse Right Click";
		keysInfo[11] = "- Rotate object left (or as defined in settings).";
		keys[12] = "Key T";
		keysInfo[12] = "- Select Eraser.";
		keys[13] = "Ctrl/Cmd + Z";
		keysInfo[13] = "- Undo last build action.";
		keys[14] = "Ctrl/Cmd + S";
		keysInfo[14] = "- Save Map.";
	}
#endif
}
