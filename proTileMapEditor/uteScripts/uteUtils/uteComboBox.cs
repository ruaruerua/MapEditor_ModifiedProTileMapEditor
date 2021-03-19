using UnityEngine;

public class uteComboBox
{
#if UNITY_EDITOR
    private static bool fshow = false; 
    private static int useid = -1;
    [HideInInspector]
    public bool isClickedComboButton = false;  
    [HideInInspector]
    public int selectedItemIndex = 0;  
    [HideInInspector]
    public int List(Rect rect, string buttonText, GUIContent[] listContent, GUIStyle listStyle)
    {
        return List(rect, new GUIContent( buttonText ), listContent, "button", "box", listStyle);
    }
    [HideInInspector]
    public int List(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle)
    {
        return List(rect, buttonContent, listContent, "button", "box", listStyle);
    } 
    [HideInInspector]
    public int List(Rect rect, string buttonText, GUIContent[] listContent, GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle)
    {
        return List(rect, new GUIContent( buttonText ), listContent, buttonStyle, boxStyle, listStyle);
    }
    [HideInInspector]
    public int List(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle)
    {
        if(fshow)
        {
            fshow = false;
            isClickedComboButton = false;           
        }

        bool done = false;
        int cID = GUIUtility.GetControlID(FocusType.Passive);       

        switch(Event.current.GetTypeForControl(cID))
        {
            case EventType.MouseUp:
            {
                if(isClickedComboButton)
                {
                    done = true;
                }
            }
            break;
        }       

        if(GUI.Button(rect, buttonContent, buttonStyle))
        {
            if(useid==-1)
            {
                useid = cID;
                isClickedComboButton = false;
            }

            if(useid!=cID)
            {
                fshow = true;
                useid = cID;
            }

            isClickedComboButton = true;
        }
        
        if(isClickedComboButton)
        {
            Rect listRect = new Rect(rect.x, rect.y + listStyle.CalcHeight(listContent[0], 1.0f)+11, rect.width, listStyle.CalcHeight(listContent[0], 1.0f) * listContent.Length);
            GUI.Box(listRect, "", boxStyle);
            int newSelectedItemIndex = GUI.SelectionGrid(listRect, selectedItemIndex, listContent, 1, listStyle);
            
            if( newSelectedItemIndex != selectedItemIndex)
            {
                selectedItemIndex = newSelectedItemIndex;
            }
        }

        if(done)
        {
            isClickedComboButton = false;
        }

        return GetSelectedItemIndex();
    }

    [HideInInspector]
    public int GetSelectedItemIndex()
    {
        return selectedItemIndex;
    }
    [HideInInspector]
    public void SetSelectedItemIndex(int index)
    {
        selectedItemIndex = index;
    }
#endif
}