using UnityEngine;
using System.Collections;

public class ShowMove : MonoBehaviour {

	public float lineDrawSpeed = 6f;

	Vector3 a, b;
	LineRenderer lineRenderer;
	float counter;
	float dist;
	bool drawLine = false;
	IEnumerator coroutine;

	void Start() {
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.SetWidth(0.15f, 0.15f);
		lineRenderer.enabled = false;
	}

	void Update() {

		if(!drawLine)
			return;

		counter += 0.1f / lineDrawSpeed;

		float x = Mathf.Lerp(0, dist, counter);

		Vector3 p = x * Vector3.Normalize(b - a) + a + Vector3.forward * -1;

		lineRenderer.SetPosition(1, p);
	}

	public void SetupLine(Vector3 oriPos, Vector3 desPos) {
		a = oriPos;
		b = desPos;
		lineRenderer.enabled = true;
		lineRenderer.SetPosition(0, oriPos + Vector3.forward * -1);
		lineRenderer.SetPosition(1, oriPos + Vector3.forward * -1);
		dist = Vector3.Distance(oriPos, desPos);
		counter = 0;
		drawLine = true;
		StartCoroutine(DrawTheLine());
	}

	IEnumerator DrawTheLine() {
		yield return new WaitForSeconds(3f);
		drawLine = false;
		lineRenderer.enabled = false;
	}

}
