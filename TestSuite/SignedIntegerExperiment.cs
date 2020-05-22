using System;
using System.Collections;
using GeneticSharp.Domain.Chromosomes;

namespace TestSuite
{
    public class SignedIntegerExperiment
    {
        void Example()
        {
            var i = new IntegerChromosome(3, 3);
            // 31 is the least significant bit
            // 0 is sign bit
            i.FlipGene(31);
            Console.WriteLine("I3: " + i.ToInteger());
            
            
            var array = new int[1];
            // most significant bit is the right most bit.
            // Last bit is sign bit
            var genes = new bool[]{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false};
            var bitArray = new BitArray(genes);
            bitArray.CopyTo(array, 0);

            Console.WriteLine(array[0]);    
        }
    }
}