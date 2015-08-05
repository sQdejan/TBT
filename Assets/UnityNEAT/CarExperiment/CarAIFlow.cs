using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarAIFlow : MonoBehaviour {

	#region Singleton

	private static CarAIFlow instance;

	public static CarAIFlow Instance {
		get {
			if(instance == null)
				instance = GameObject.FindObjectOfType<CarAIFlow>();

			return instance;
		}
	}

	#endregion

	public List<UnitController> controllers = new List<UnitController>();

	int curIndex = 0;

	public void StartProcess() {

	}

	void EndProcess() {

	}
}
