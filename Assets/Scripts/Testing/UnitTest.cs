using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class UnitTest : MonoBehaviour {

	void Start() {
		
		Stopwatch sw = new Stopwatch();
		sw.Start();
		for(int i = 0; i < 10; i++) {
		}
		sw.Stop();
		
		TimeSpan ts = sw.Elapsed;
		
		// Format and display the TimeSpan value. 
		string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
		                                   ts.Hours, ts.Minutes, ts.Seconds,
		                                   ts.Milliseconds);
		UnityEngine.Debug.Log("RunTime " + elapsedTime);
	}
}
