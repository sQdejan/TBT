using UnityEngine;
using System.Collections;

public class ShowMoveController : MonoBehaviour {

	#region Singleton
	
	private static ShowMoveController instance;
	
	public static ShowMoveController Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<ShowMoveController>();
			}
			
			return instance;
		}
	}
	
	#endregion

	public ShowMove[] lineRenderers;

	int curIndex = 0;

	public void ShowMove(Vector3 a, Vector3 b) {
		if(++curIndex >= lineRenderers.Length)
			curIndex = 0;

		lineRenderers[curIndex].SetupLine(a,b);
	}
}
