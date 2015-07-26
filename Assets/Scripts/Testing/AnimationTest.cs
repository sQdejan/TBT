using UnityEngine;
using System.Collections;

public class AnimationTest : MonoBehaviour {

	Animator thisAnimator;

	void Start() {
		thisAnimator = GetComponent<Animator>();
	}

	void Update() {
		if(Input.GetKeyUp(KeyCode.G)) {
			thisAnimator.SetInteger("active", 1);
		}

		if(Input.GetKeyUp(KeyCode.H)) {
			thisAnimator.SetInteger("active", 0);
		}
	}
}
