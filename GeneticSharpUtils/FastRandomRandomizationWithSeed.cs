using System.Threading;
using GeneticSharp.Domain.Randomizations;

namespace GeneticSharpUtils
{
    /// <summary>
    /// An IRandomization using FastRandom has pseudo-number generator.
    /// <see href="http://www.codeproject.com/Articles/9187/A-fast-equivalent-for-System-Random"/>
    /// </summary>
    public class FastRandomRandomizationWithSeed : RandomizationBase
    {
        private static FastRandomWithSeed _globalRandom = null;
        private static readonly object _globalLock = new object();

        /// <summary> 
        /// Random number generator 
        /// </summary> 
        private static ThreadLocal<FastRandomWithSeed> _threadRandom = new ThreadLocal<FastRandomWithSeed>(NewRandom);

        public static int _seed;


        public static void setSeed(int seed)
        {
            _globalRandom = new FastRandomWithSeed(seed);
            _threadRandom = new ThreadLocal<FastRandomWithSeed>(NewRandom);
            //Console.WriteLine("FASTRANDOM2: " + _globalRandom.GetHashCode());
            _seed = seed;
        }
        
        /// <summary> 
        /// Creates a new instance of FastRandom. The seed is derived 
        /// from a global (static) instance of Random, rather 
        /// than time. 
        /// </summary> 
        private static FastRandomWithSeed NewRandom()
        {
            lock (_globalLock)
            {
                return new FastRandomWithSeed(_seed);
            }
        }

        /// <summary> 
        /// Returns an instance of Random which can be used freely 
        /// within the current thread. 
        /// </summary> 
        private static FastRandomWithSeed Instance { get { return _threadRandom.Value; } }

        /// <summary>
        /// Gets an integer value between minimum value (inclusive) and maximum value (exclusive).
        /// </summary>
        /// <returns>The integer.</returns>
        /// <param name="min">Minimum value (inclusive).</param>
        /// <param name="max">Maximum value (exclusive).</param>
        public override int GetInt(int min, int max)
        {
            return Instance.Next(min, max);
        }

        /// <summary>
        /// Gets a float value between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// The float value.
        /// </returns>
        public override float GetFloat()
        {
            return (float)Instance.NextDouble();
        }

        /// <summary>
        /// Gets a double value between 0.0 and 1.0.
        /// </summary>
        /// <returns>The double value.</returns>
        public override double GetDouble()
        {
            return Instance.NextDouble();
        }
    }
}