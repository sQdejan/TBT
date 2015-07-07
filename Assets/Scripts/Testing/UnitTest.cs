using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Threading;

public class UnitTest : MonoBehaviour {

//	string elapsedTime = "";
//	bool finished = false;

	Thread thread;

	void Start() {

		thread = new Thread(ThreadFunction);
		thread.Start();

//		Stopwatch sw = new Stopwatch();
//		sw.Start();
//		for(int i = 0; i < 10; i++) {
//		}
//		sw.Stop();
//		
//		TimeSpan ts = sw.Elapsed;
//		
//		// Format and display the TimeSpan value. 
//		string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
//		                                   ts.Hours, ts.Minutes, ts.Seconds,
//		                                   ts.Milliseconds);
//		UnityEngine.Debug.Log("RunTime " + elapsedTime);
	}

	void ThreadFunction() {


//		Stopwatch sw = new Stopwatch();
//		sw.Start();
//		for(int i = 0; i < int.MaxValue; i++) {
//		}
//		sw.Stop();
//		TimeSpan ts = sw.Elapsed;
		
		// Format and display the TimeSpan value. 
//		elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
//		                                   ts.Hours, ts.Minutes, ts.Seconds,
//		                                   ts.Milliseconds);

//		finished = true;
	}


//	void Update() {
//		if(!finished)
//			return;
//
//		UnityEngine.Debug.Log("Elapsed time " + elapsedTime);
//
//		finished = false;
//	}
}
