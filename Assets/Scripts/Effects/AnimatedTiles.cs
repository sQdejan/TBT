using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimatedTiles : MonoBehaviour {

	public float min, max;

	List<Transform> tilesList = new List<Transform>();
	List<int> indexes = new List<int>();

	void Start () {
		foreach(Transform c in transform) {
			foreach(Transform t in c) {
				tilesList.Add(t);
			}
		}

		StartCoroutine(Animate());
	}

	IEnumerator Animate() {
		yield return new WaitForSeconds(Random.Range(5,15));

		int amountToAnimate = Random.Range(1,5);

		for(int i = 0; i < amountToAnimate; i++) {
			int j = Random.Range(0, tilesList.Count);
			if(!indexes.Contains(j)) {
				indexes.Add(j);
				StartCoroutine(AnimateUp(j));
			}
		}

		StartCoroutine(Animate());
	}

	IEnumerator AnimateUp(int index) {
		Vector3 oriPos = tilesList[index].position;
		Vector3 newPos = new Vector3(oriPos.x, oriPos.y + max, oriPos.z);
		float animateTime = Random.Range(2.1f, 6.5f);
		float time = 0;

		while(time < animateTime) {
			tilesList[index].position = Vector3.Lerp(oriPos, newPos, time/(animateTime));
			time += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		time = 0;

		while(time < animateTime) {
			tilesList[index].position = Vector3.Lerp(newPos, oriPos, time/(animateTime));
			time += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		indexes.Remove(index);
	}

}
