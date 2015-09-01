using UnityEngine;
using System.Collections;

public class DestroyObject : MonoBehaviour {

	void Start() {
		StartCoroutine(Boom());
	}

	IEnumerator Boom() {
		yield return new WaitForSeconds(0.7f);
		Destroy(gameObject);
	}
}
