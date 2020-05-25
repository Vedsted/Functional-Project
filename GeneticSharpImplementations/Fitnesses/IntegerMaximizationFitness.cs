using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;

namespace GeneticSharpImplementations.Fitnesses
{
    public class IntegerMaximizationFitness: IFitness
    {
        public double Evaluate(IChromosome chromosome)
        {
            var x = ((IntegerChromosome) chromosome).ToInteger();
            
            return x<0? 0: x;
        }
    }
}