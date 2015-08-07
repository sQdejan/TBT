using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Core;
using System.Collections;
using UnityEngine;

namespace SharpNEAT.Core
{
    class UnityListEvaluator<TGenome, TPhenome> : IGenomeListEvaluator<TGenome>
        where TGenome : class, IGenome<TGenome>
        where TPhenome : class
	{
        
        readonly IGenomeDecoder<TGenome, TPhenome> _genomeDecoder;
        readonly IPhenomeEvaluator<TPhenome> _phenomeEvaluator;

		const int TRIALS = 3;

        #region Constructor

        /// <summary>
        /// Construct with the provided IGenomeDecoder and IPhenomeEvaluator.
        /// </summary>
        public UnityListEvaluator(IGenomeDecoder<TGenome, TPhenome> genomeDecoder,
                                         IPhenomeEvaluator<TPhenome> phenomeEvaluator)
        {
            _genomeDecoder = genomeDecoder;
            _phenomeEvaluator = phenomeEvaluator;
        }

        #endregion

        public ulong EvaluationCount
        {
            get { return _phenomeEvaluator.EvaluationCount; }
        }

        public bool StopConditionSatisfied
        {
            get { return _phenomeEvaluator.StopConditionSatisfied; }
        }

        public IEnumerator Evaluate(IList<TGenome> genomeList)
        {
            yield return Coroutiner.StartCoroutine(evaluateList(genomeList));
        }

		static int curgen = 0;

        private IEnumerator evaluateList(IList<TGenome> genomeList)
        {
			Optimizer.curOrganism = 0;
			Optimizer.curIteration = 0;

			Dictionary<TGenome, TPhenome> dict = new Dictionary<TGenome, TPhenome>();
			Dictionary<TGenome, FitnessInfo[]> fitnessDict = new Dictionary<TGenome, FitnessInfo[]>();

			for(int i = 0; i < TRIALS; i++) {

				Optimizer.curIteration = i + 1;
				_phenomeEvaluator.Reset();
				dict = new Dictionary<TGenome, TPhenome>();

				int tmpOrganism = 0;

	            foreach (TGenome genome in genomeList)
	            {
					Optimizer.curOrganism = ++tmpOrganism;

	                TPhenome phenome = _genomeDecoder.Decode(genome);
	                if (null == phenome)
	                {   // Non-viable genome.
	                    genome.EvaluationInfo.SetFitness(0.0);
	                    genome.EvaluationInfo.AuxFitnessArr = null;
	                }
	                else
	                {
						if(i == 0) {
							fitnessDict.Add(genome, new FitnessInfo[TRIALS]);
						}

						dict.Add(genome, phenome);

	                    yield return Coroutiner.StartCoroutine(_phenomeEvaluator.Evaluate(phenome));

	                    FitnessInfo fitnessInfo = _phenomeEvaluator.GetLastFitness(phenome);

						fitnessDict[genome][i] = fitnessInfo;
//	                    genome.EvaluationInfo.SetFitness(fitnessInfo._fitness);
//	                    genome.EvaluationInfo.AuxFitnessArr = fitnessInfo._auxFitnessArr;
	                }
	            }
			}

			Optimizer.fitnessList = new List<float>();

			foreach (TGenome genome in dict.Keys)
			{
				TPhenome phenome = dict[genome];
				if (phenome != null)
				{
					double fitness = 0;

					for (int i = 0; i < TRIALS; i++) {
						fitness += fitnessDict[genome][i]._fitness;
					}

					var fit = fitness;
					fitness /= TRIALS; // Averaged fitness

					Optimizer.fitnessList.Add((float)fitness);

					genome.EvaluationInfo.SetFitness(fitness);
					genome.EvaluationInfo.AuxFitnessArr = fitnessDict[genome][0]._auxFitnessArr;
				}
			}
        }

        public void Reset()
        {
            _phenomeEvaluator.Reset();
        }

    }
}
