using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIController : MonoBehaviour {

#region Singleton
	
	private static AIController instance;
	
	public static AIController Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<AIController>();
			}
			
			return instance;
		}
	}
	
#endregion

	public Transform AIEnvironment;

	public Transform unitParent;
	public Transform enemyParent;

	public GameObject warriorPrefab;
	public GameObject enemyWarriorPrefab;

	public List<GameObject> units = new List<GameObject>();
	public List<GameObject> enemies = new List<GameObject>();

	void Start() {
		InitialiseSetup();
	}

	//Copy the current setup into what the AI will use
	void InitialiseSetup() {
		foreach(Transform t in unitParent) {
			if(t.tag == "Warrior") {
				units.Add((GameObject)Instantiate(warriorPrefab, t.position + AIEnvironment.position, Quaternion.identity));
			}
		}

		foreach(Transform t in enemyParent) {
			if(t.tag == "Warrior") {
				enemies.Add((GameObject)Instantiate(enemyWarriorPrefab, t.position + AIEnvironment.position, Quaternion.identity));
			}
		}
	}

}
