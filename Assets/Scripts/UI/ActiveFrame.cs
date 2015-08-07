using UnityEngine;
using System.Collections;

public class ActiveFrame : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<RectTransform>().position = new Vector3(Screen.width / 15, Screen.height / 14, 0);
	}

}
