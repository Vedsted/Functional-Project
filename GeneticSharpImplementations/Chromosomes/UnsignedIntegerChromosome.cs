using System;
using System.Collections;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;

namespace GeneticSharpImplementations.Chromosomes
{
    public class UnsignedIntegerChromosome: IntegerChromosome
    {
        private int m_minValue;
        private int m_maxValue;
        
        public UnsignedIntegerChromosome(int minValue, int maxValue) : base(minValue, maxValue)
        {
            m_minValue = minValue;
            m_maxValue = maxValue;
        }
        
        public override IChromosome CreateNew()
        {
            return new UnsignedIntegerChromosome(m_minValue, m_maxValue);
        }

        public UInt32 ToUnsignedInteger()
        {
            var array = new UInt32[1];
            var genes = GetGenes().Select(g => (bool)g.Value).ToArray();
            var bitArray = new BitArray(genes);
            bitArray.CopyTo(array, 0);

            return array[0];
        }
    }
}