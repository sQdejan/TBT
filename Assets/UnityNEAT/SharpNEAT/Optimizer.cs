using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;
using System.Collections.Generic;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using System;
using System.Xml;
using System.IO;

public class Optimizer : MonoBehaviour {

    const int NUM_INPUTS = 64;
    const int NUM_OUTPUTS = 1;

    [HideInInspector] public int Trials;
    public float TrialDuration;
    [HideInInspector] public float StoppingFitness;
    bool EARunning;
    string popFileSavePath, champFileSavePath, generationFitness;

    SimpleExperiment experiment;
    static NeatEvolutionAlgorithm<NeatGenome> _ea;

    Dictionary<IBlackBox, UnitController> ControllerMap = new Dictionary<IBlackBox, UnitController>();
    private DateTime startTime;
    private float timeLeft;
    private float accum;
    private int frames;
    private float updateInterval = 12;

    private uint Generation;
    private double Fitness;

	public static List<float> fitnessList = new List<float>();

	// Use this for initialization
	void Awake () {
        Utility.DebugLog = true;
        experiment = new SimpleExperiment();
        XmlDocument xmlConfig = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("experiment.config");
        xmlConfig.LoadXml(textAsset.text);
        experiment.SetOptimizer(this);

        experiment.Initialize("TBT Experiment", xmlConfig.DocumentElement, NUM_INPUTS, NUM_OUTPUTS);

        champFileSavePath = Application.persistentDataPath + string.Format("/{0}.champ.xml", "tbt");
        popFileSavePath = Application.persistentDataPath + string.Format("/{0}.pop.xml", "tbt");
		generationFitness = Application.persistentDataPath + string.Format("/{0}.fit.txt", "tbt");


        print(champFileSavePath);

		StartEA();
	}

    public void StartEA()
    {        
        Utility.DebugLog = true;
        Utility.Log("Starting the experiment");
        // print("Loading: " + popFileLoadPath);
        _ea = experiment.CreateEvolutionAlgorithm(popFileSavePath);
        startTime = DateTime.Now;

        _ea.UpdateEvent += new EventHandler(ea_UpdateEvent);
        _ea.PausedEvent += new EventHandler(ea_PauseEvent);

//        var evoSpeed = 25;

     //   Time.fixedDeltaTime = 0.045f;
//        Time.timeScale = evoSpeed;       
        _ea.StartContinue();
//        EARunning = true;
    }

    void ea_UpdateEvent(object sender, EventArgs e)
    {
//        Utility.Log(string.Format("gen={0:N0} bestFitness={1:N6s}",
//            _ea.CurrentGeneration, _ea.Statistics._maxFitness));

        Fitness = _ea.Statistics._maxFitness;
        Generation = _ea.CurrentGeneration;
      

    //    Utility.Log(string.Format("Moving average: {0}, N: {1}", _ea.Statistics._bestFitnessMA.Mean, _ea.Statistics._bestFitnessMA.Length));

    
    }

	int myGen = 0;

    void ea_PauseEvent(object sender, EventArgs e)
    {
//        Time.timeScale = 1;
        Utility.Log("Saving the NN");

        XmlWriterSettings _xwSettings = new XmlWriterSettings();
        _xwSettings.Indent = true;
        // Save genomes to xml file.        
        DirectoryInfo dirInf = new DirectoryInfo(Application.persistentDataPath);
        if (!dirInf.Exists)
        {
//            Debug.Log("Creating subdirectory");
            dirInf.Create();
        }
        using (XmlWriter xw = XmlWriter.Create(popFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, _ea.GenomeList);
        }

        // Also save the best genome
        using (XmlWriter xw = XmlWriter.Create(champFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, new NeatGenome[] { _ea.CurrentChampGenome });
        }

		if(!File.Exists(generationFitness)) {
			using(StreamWriter sw = File.CreateText(generationFitness)) {

				sw.WriteLine("------ New generation " + myGen++ + " -----");

				fitnessList.Sort();

				foreach(float f in fitnessList) {
					sw.WriteLine(f.ToString());
				}

				sw.WriteLine("------ Ends here -----");
				sw.WriteLine("");
			}
		} else {
			using(StreamWriter sw = File.AppendText(generationFitness)) {
				sw.WriteLine("------ New generation " + myGen++ + " -----");
				
				fitnessList.Sort();
				
				foreach(float f in fitnessList) {
					sw.WriteLine(f.ToString());
				}
				
				sw.WriteLine("------ Ends here -----");
				sw.WriteLine("");
			}
		}


//        DateTime endTime = DateTime.Now;
//        Utility.Log("Total time elapsed: " + (endTime - startTime));

        System.IO.StreamReader stream = new System.IO.StreamReader(popFileSavePath);
       
      
//        EARunning = false;        
        
    }

    public void StopEA()
    {

        if (_ea != null && _ea.RunState == SharpNeat.Core.RunState.Running)
        {
            _ea.Stop();
        }
    }

    public void Evaluate(IBlackBox box)
    {
		NNTrainer.Instance.Activate(box);

//        GameObject obj = Instantiate(Unit, Unit.transform.position, Unit.transform.rotation) as GameObject;
//        UnitController controller = obj.GetComponent<UnitController>();
//
//        ControllerMap.Add(box, controller);
//
//        controller.Activate(box);
    }

    public void StopEvaluation(IBlackBox box)
    {
//        UnitController ct = ControllerMap[box];
//
//        Destroy(ct.gameObject);
    }

    public void RunBest()
    {
        Time.timeScale = 1;

        NeatGenome genome = null;


        // Try to load the genome from the XML document.
        try
        {
            using (XmlReader xr = XmlReader.Create(champFileSavePath))
                genome = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, (NeatGenomeFactory)experiment.CreateGenomeFactory())[0];


        }
        catch (Exception e1)
        {
            // print(champFileLoadPath + " Error loading genome from file!\nLoading aborted.\n"
            //						  + e1.Message + "\nJoe: " + champFileLoadPath);
            return;
        }

        // Get a genome decoder that can convert genomes to phenomes.
        var genomeDecoder = experiment.CreateGenomeDecoder();

        // Decode the genome into a phenome (neural network).
        var phenome = genomeDecoder.Decode(genome);

//        GameObject obj = Instantiate(Unit, Unit.transform.position, Unit.transform.rotation) as GameObject;
//        UnitController controller = obj.GetComponent<UnitController>();
//
//        ControllerMap.Add(phenome, controller);
//
//        controller.Activate(phenome);
    }

    public float GetFitness(IBlackBox box)
    {
		float fitness = NNTrainer.Instance.GetFitness();

		if(!GameFlow.playersCurrentTurn && !GameFlow.Instance.IsGameOver())
			AIGameFlow.Instance.CancelBackgroundWorker();
		else if (GameFlow.playersCurrentTurn && !GameFlow.Instance.IsGameOver())
			NNAIGameFlow.Instance.CancelBackgroundWorker();
		else 
			GameFlow.Instance.SoftRestartGame();

		return fitness;


//        if (ControllerMap.ContainsKey(box))
//        {
//            return ControllerMap[box].GetFitness();
//        }
//        return 0;
    }

    void OnGUI()
    {
//        if (GUI.Button(new Rect(10, 10, 100, 40), "Start EA"))
//        {
//            StartEA();
//        }
//        if (GUI.Button(new Rect(10, 10, 100, 40), "Stop EA"))
//        {
//            StopEA();
//        }
//        if (GUI.Button(new Rect(10, 110, 100, 40), "Run best"))
//        {
//            RunBest();
//        }

//        GUI.Button(new Rect(10, Screen.height - 70, 100, 60), string.Format("Generation: {0}\nFitness: {1:0.00}", Generation, Fitness));
    }
}
