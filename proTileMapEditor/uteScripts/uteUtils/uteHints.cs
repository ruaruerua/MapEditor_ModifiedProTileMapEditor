using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class uteHints : MonoBehaviour {

	public List<Hint> allHints = new List<Hint>();
	public bool isShowHintsAtAll;
	public bool isShowHint;
	public bool isEnough;
	public int currentHintIndex;
	public Rect HintBoxRect;
	private GUISkin guiskin;

	public class Hint
	{
		public string text;
		public bool seen;
		public int offset;

		public Hint(string text, bool seen, int offset)
		{
			this.text = text;
			this.seen = seen;
			this.offset = offset;
		}
	}

	private void Awake()
	{
		guiskin = (GUISkin) Resources.Load("uteForEditor/uteUI");
		HintBoxRect = new Rect(0,0,0,0);
		isEnough = false;
		isShowHint = false;
		currentHintIndex = 0;
		isShowHintsAtAll = CheckIfShowHints();

		if(isShowHintsAtAll)
		{
			ReadHints();
		}
		else
		{
			isEnough = true;
			isShowHint = false;
		}
	}

	private void ReadHints()
	{
		StreamReader sr = new StreamReader(uteGLOBAL3dMapEditor.getHintsPath());
		string data = sr.ReadToEnd();
		sr.Close();

		data = data.Replace("\n","");
		data = data.Replace("\r","");

		string[] parts = data.Split("$"[0]);

		for(int i=0;i<parts.Length;i++)
		{
			string part = parts[i].ToString();

			if(part!="")
			{
				string[] _p = part.Split("^"[0]);
				string _txt = _p[0].ToString();
				bool _seen = StrToBool(_p[1].ToString());
				int _offset = System.Convert.ToInt32(_p[2]);

				allHints.Add(new Hint(_txt,_seen,_offset));
			}
		}
	}

	private bool StrToBool(string str)
	{
		if(str.Equals("True"))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	private bool CheckIfShowHints()
	{
		StreamReader sr = new StreamReader(uteGLOBAL3dMapEditor.getHintsOptPath());
		string data = sr.ReadToEnd();
		sr.Close();

		if(data.Equals("yes"))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public void ShowRandomHint()
	{
		if(AreThereUnSeenHints())
		{
			int rand = Random.Range(0,allHints.Count);

			while(allHints[rand].seen)
			{
				rand = Random.Range(0,allHints.Count);
			}

			currentHintIndex = rand;
			allHints[currentHintIndex].seen = true;

			RewriteHints();

			isShowHint = true;
		}
		else
		{
			DisableHints();
		}
	}

	private void RewriteHints()
	{
		string new_data = "";

		for(int i=0;i<allHints.Count;i++)
		{
			new_data += allHints[i].text+"^"+allHints[i].seen.ToString()+"^"+allHints[i].offset.ToString()+"^$\n\r";
		}

		StreamWriter sw = new StreamWriter(uteGLOBAL3dMapEditor.getHintsPath());
		sw.Write("");
		sw.Write(new_data);
		sw.Flush();
		sw.Close();
	}

	public void HideHint()
	{
		isShowHint = false;

		if(!isEnough)
		{
			StartCoroutine(ShowHintLater());
		}
	}

	private IEnumerator ShowHintLater()
	{
		yield return new WaitForSeconds(60.0f);
		ShowRandomHint();
		yield return 0;
	}

	public void HideHintAndDontShowAnymore()
	{
		isShowHint = false;
		isShowHintsAtAll = false;
		DisableHints();
	}

	private bool AreThereUnSeenHints()
	{
		for(int i=0;i<allHints.Count;i++)
		{
			if(!allHints[i].seen)
			{
				return true;
			}
		}

		return false;
	}

	private void DisableHints()
	{
		isShowHint = false;
		isShowHintsAtAll = false;
		isEnough = true;

		StreamWriter sw = new StreamWriter(uteGLOBAL3dMapEditor.getHintsOptPath());
		sw.Write("");
		sw.Write("no");
		sw.Flush();
		sw.Close();
	}

	private void OnGUI()
	{
		if(!isShowHintsAtAll)
		{
			return;
		}

		if(isEnough)
		{
			return;
		}

		if(isShowHint)
		{
			GUI.skin = guiskin;
			HintBoxRect = new Rect(Screen.width/2-300,Screen.height-150,600+allHints[currentHintIndex].offset,80);
			GUI.Box(HintBoxRect,"HINT!");
			GUI.Label(new Rect(Screen.width/2-295,Screen.height-130,700,80),allHints[currentHintIndex].text);

			if(GUI.Button(new Rect(Screen.width/2-300,Screen.height-100,90,25),"Thanks!"))
			{
				HideHint();
			}

			if(GUI.Button(new Rect(Screen.width/2-210,Screen.height-100,140,25),"Enough for now"))
			{
				isEnough = true;
				HideHint();
			}

			if(GUI.Button(new Rect(Screen.width/2-70,Screen.height-100,140,25),"Disable Hints"))
			{
				DisableHints();
			}
		}
	}
}
