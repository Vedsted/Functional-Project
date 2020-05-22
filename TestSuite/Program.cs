using System;
using System.Collections;
using System.Collections.Generic;
using FsCheck;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharpImplementations.Fitnesses;
using GeneticSharpUtils;

namespace TestSuite
{
    class Program
    {
        static void Main(string[] args)
        {
        
            
            var tmp = new Tmp();

            var termination = 0;
            var seed = 226927338;
            var g1 = tmp.GetGA(seed, termination);

            Console.WriteLine("Termination set: " + termination + " Actual: " + g1.GenerationsNumber );
        }
    }


    class Tmp
    {
        public GeneticAlgorithm GetGA(int seed, int termination)
        {
            RandomizationProvider.Current = new FastRandomRandomizationWithSeed();
            FastRandomRandomizationWithSeed.setSeed(seed);
            
            var selection = new EliteSelection();
            var crossover = new OnePointCrossover();
            var mutation = new FlipBitMutation();
            var fitness = new IntegerMaximizationFitness();
            var chromosome = new IntegerChromosome(0, 9);
            var population = new Population (2, 10, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new GenerationNumberTermination(termination)
            };

            ga.Start();
            
            return ga;
        }
    }
}