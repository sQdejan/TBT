using UnityEngine;
using System.Collections;

public class RangedAttackEffect : MonoBehaviour {

	Vector3 targetPos;
	Vector3 oriPos;

	public void StartProcess(Vector3 targetPos) {
		this.targetPos = targetPos;
		oriPos = transform.position;
		StartCoroutine(Slide());
	}

	IEnumerator Slide() {
		float time = 0;
		float runtime = 0.3f;

		while(time < runtime) {
			transform.position = Vector3.Lerp(oriPos, this.targetPos, time/runtime);
			time += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		Destroy(gameObject);
	}
}
