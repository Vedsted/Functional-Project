// Learn more about F# at http://fsharp.org

open System
open FsCheck
open GeneticSharp.Domain
open GeneticSharp.Domain.Chromosomes
open GeneticSharp.Domain.Crossovers
open GeneticSharp.Domain.Mutations
open GeneticSharp.Domain.Populations
open GeneticSharp.Domain.Randomizations
open GeneticSharp.Domain.Selections
open GeneticSharp.Domain.Terminations
open GeneticSharpImplementations.Chromosomes
open GeneticSharpImplementations.Fitnesses
open GeneticSharpUtils

// Type for storing initial values and results of the GA
type GAResult (GA: GeneticAlgorithm, Termination: int, Seed: int, HookChromosomes: list<IChromosome>) =
    member this.GA = GA
    member this.Termination = Termination
    member this.Seed = Seed
    member this.HookChromosomes = HookChromosomes
    member this.BestFitness = (this.GA.BestChromosome.Fitness.GetValueOrDefault())
    member this.BestGenes = (this.GA.BestChromosome.GetGenes())
    member this.printInitial = sprintf "{ Seed: %i, Termination: %i}" this.Seed this.Termination
    member this.printResult = sprintf "{ Actual termination: %i, Best Chromosome: %s}" this.GA.GenerationsNumber (this.GA.BestChromosome.ToString())
    override this.ToString () = sprintf "{\n\t HashCode: %i,\n\t Initial: %s,\n\t Result: %s \n}" (this.GetHashCode()) this.printInitial this.printResult
;;


// Creates a genetic algorithm matching the TSP template from:
//     https://github.com/giacomelli/GeneticSharp/blob/master/src/Templates/content/TspConsoleApp/Program.cs
let CreateTSP seed termination =
    // Custom random with support for seed.
    // Can be commented out to use std random. If commented out the seed can't be inspected, thus the seed in the output is invalid).
    RandomizationProvider.Current <- FastRandomRandomizationWithSeed()
    FastRandomRandomizationWithSeed.setSeed(seed)
    
    let selection = EliteSelection()
    let crossover = OrderedCrossover()
    let mutation = TworsMutation()
    let fitness = TspFitness(20, 0, 1000, 0, 1000)
    let chromosome = TspChromosome(fitness.Cities.Count)
    let population = Population(50, 70, chromosome)
    let ga = GeneticAlgorithm(population, fitness, selection, crossover, mutation)
    
    // Handler stores the best chromosome for each generation ran
    let mutable bestChromosomes = []
    let handler = EventHandler( fun sender eventArgs -> let g: GeneticAlgorithm =  sender :?> GeneticAlgorithm in  bestChromosomes <- bestChromosomes@[g.BestChromosome])
    ga.GenerationRan.AddHandler(handler)
    
    ga.Termination <- FitnessStagnationTermination(termination)
    ga.Start()
    let res = GAResult(ga, termination, seed, bestChromosomes)
    res
;;

// FsCheck generator creating GAResults of TSP
let TspGen : Gen<GAResult> =
    let terminationMin = 1
    let terminationMax = 1000
    let terminationGen = Gen.choose (terminationMin,terminationMax)
    let seedMin = Int32.MinValue
    let seedMax = Int32.MaxValue
    let seedGen = Gen.choose (seedMin,seedMax)
    Gen.map2 CreateTSP seedGen terminationGen
;;


// Type for registering generators
type TspGenerators =
  static member GeneticAlgorithm() =
      {new Arbitrary<GAResult>() with
          override x.Generator = TspGen
          override x.Shrinker t = Seq.empty}
;;

// Custom comparison. This compares and print the left and right sides. 
let (.=.) left right = left = right |@ sprintf "%A = %A" left right

// Custom comparison. Compares the the left and right as well as the index of the first mismatch
let (.==.) left right =
    let CheckIncreasingFitness l =
        let rec Next (remaining:list<IChromosome>) (lastGen:IChromosome) i=
            match remaining with
            | []  -> []
            | e::l1  -> if (e.Fitness.GetValueOrDefault() - lastGen.Fitness.GetValueOrDefault()) >= 0.
                        then (Next l1 e (i+1))
                        else (List.take (i+1) l) 
        in
        match l with
        | [] -> []
        | e::l1 -> (Next l1 e 1) in
    let l = CheckIncreasingFitness left in
    let i = (List.length l) in
    left = right |@ sprintf "Fitness decreased at index: %i \nFor generations 1-%i: \n %A "  (i-1) i l 

// Test properties
type GAProperties =
  static member ``P2 - Generation N+1 must have a fitness greater or equal to N``
    (gaRes:GAResult) = gaRes.HookChromosomes .==. (List.sortBy (fun e -> e.Fitness.GetValueOrDefault()) gaRes.HookChromosomes)


[<EntryPoint>]
let main argv =
    Arb.register<TspGenerators>()
    // Comment in for running 'MaxTest' times with random seeds and terminations
    //Check.All<GAProperties> ({Config.Verbose with MaxTest = 100})
    
    // For manual testing with user defined termination and seed:
    let termination = 500
    let seed = 781433289
    let res = CreateTSP seed termination
    printfn "Checking GeneticAlgorithmResult: \n%s \n\nFsCheck Trace:" (res.ToString())
    (GAProperties.``P2 - Generation N+1 must have a fitness greater or equal to N`` res).QuickCheck()
    
    0 // return an integer exit code
