using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	void Start() {
		StartCoroutine(lol());
	}

	IEnumerator lol () {
		yield return new WaitForSeconds(1);
		GetComponent<Unit>().ShowPossibleMoves();
	}
}
