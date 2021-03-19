using UnityEngine;
using UnityEditor;

public class uteScriptingReference : MonoBehaviour {

	[MenuItem ("Window/proTileMapEditor/Other/Scripting Reference",false,6)]
    static void Init ()
	{
        Application.OpenURL("http://protilemapeditor.com/?page_id=166");
    }
}
