using UnityEngine;
using System.Collections;

public class ModeControl : MonoBehaviour {
	
	public enum Mode{
		AFFIN,
		SIMILARY,
		RIGID,
	}
	
	public Mode mode{
		get;
		set;
	}
	
	string[] mode_string;
	GUIText guitext;
	// Use this for initialization
	void Start () {
		mode = Mode.AFFIN;
		mode_string = new string[3];
		
		mode_string[0] = "AFFIN";
		mode_string[1] = "SIMILARY";
		mode_string[2] = "RIGID";
		
		guitext = GetComponent<GUIText>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void ChangeMode(){
		if(Input.GetKeyDown(KeyCode.UpArrow)){
			if(mode == Mode.AFFIN){
				mode = Mode.RIGID;
			}
			else{
				mode--;
			}
			
		}
		
		if(Input.GetKeyDown(KeyCode.DownArrow)){
			if(mode == Mode.RIGID){
				mode = Mode.AFFIN;
			}
			else{
				mode++;
			}
		}

		guitext.text = mode_string[(int)mode];
	}
}
