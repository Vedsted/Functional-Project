// Learn more about F# at http://fsharp.org

open System
open System
open FsCheck
open GeneticSharp.Domain
open GeneticSharp.Domain.Chromosomes
open GeneticSharp.Domain.Crossovers
open GeneticSharp.Domain.Mutations
open GeneticSharp.Domain.Populations
open GeneticSharp.Domain.Populations
open GeneticSharp.Domain.Randomizations
open GeneticSharp.Domain.Selections
open GeneticSharp.Domain.Terminations
open GeneticSharpImplementations.Fitnesses
open GeneticSharpUtils
open System.Collections.Generic

type GAResult (GA: GeneticAlgorithm, Termination: int, Seed: int, Generations: List<Generation>) =
    member this.GA = GA
    member this.Termination = Termination
    member this.Seed = Seed
    member this.Generations = Generations 
    member this.printInitial () = sprintf "{ Seed: %i, termination: %i}" this.Seed this.Termination
    member this.printResult () = sprintf "{ termination: %i}" this.GA.GenerationsNumber
    override this.ToString () = sprintf "{ HashCode: %i, initial: %s, result: %s }" (this.GetHashCode()) (this.printInitial()) (this.printResult())

let getGA seed termination =
    RandomizationProvider.Current <- FastRandomRandomizationWithSeed()
    FastRandomRandomizationWithSeed.setSeed(seed)
    let selection = EliteSelection()
    let crossover = OnePointCrossover()
    let mutation = FlipBitMutation()
    let fitness = IntegerMaximizationFitness()
    let chromosome = IntegerChromosome(0, 9)
    let population = Population (2, 10, chromosome)
    let ga = GeneticAlgorithm(population, fitness, selection, crossover, mutation)
    ga.Termination <- GenerationNumberTermination(termination)
    
    let generations = new List<Generation>()
    let handler = EventHandler( fun sender eventArgs -> let g: GeneticAlgorithm =  sender :?> GeneticAlgorithm in  generations.Add(g.Population.CurrentGeneration))
    ga.GenerationRan.AddHandler(handler)
    ga.Start()
    let res = GAResult(ga, termination, seed, generations)
    res
;;

let IdenticalGARes (gaRes:GAResult) =
    let gaRes2 = getGA (gaRes.Seed) (gaRes.Termination)
    (gaRes, gaRes2)

let GAGen =
    let seedIntervalGen = Gen.choose (0,Int32.MaxValue) in
    let terminationIntervalGen = Gen.choose (1,100) in 
    Gen.map2 getGA seedIntervalGen terminationIntervalGen;;

let GAClones = Gen.map IdenticalGARes GAGen

let CheckIncreasingFitness l =
    let rec Next (l:list<Generation>) (lastGen:Generation) =
        match l with
        | []  -> true
        | e::l  -> if (e.BestChromosome.Fitness.GetValueOrDefault() - lastGen.BestChromosome.Fitness.GetValueOrDefault()) >= 0.
                    then (Next l e)
                    else let _ = printf "New should be grater than old. Actual: \n Old best = %f  -  New best = %f \n"
                                        (lastGen.BestChromosome.Fitness.GetValueOrDefault())
                                        (e.BestChromosome.Fitness.GetValueOrDefault()) in false
    in
    match l with
    | [] -> true
    | e::[] -> true
    | e::l -> (Next l e)
;;

let rec listtolist (l:List<Generation>) (acc:list<Generation>)=
    match l.Count with
    | 0 -> acc
    | i -> let x = l.[0] in let _ =  l.RemoveAt(0) in listtolist l (x::acc)
;;

type MyGenerators =
  static member GeneticAlgorithm() =
      {new Arbitrary<GAResult>() with
          override x.Generator = GAGen
          override x.Shrinker t = Seq.empty }
  static member IdenticalGeneticAlgorithmPair() =
      {new Arbitrary<Tuple<GAResult, GAResult>>() with
          override x.Generator = GAClones
          override x.Shrinker t = Seq.empty }

// Custom comparison. This compares and print the left and right sides. 
let (.=.) left right = left = right |@ sprintf "%A = %A" left right
let (.>=.) left right = left >= right |@ sprintf "%A >= %A" left right
let (.<=.) left right = left <= right |@ sprintf "%A <= %A" left right



type GAProperties =
  static member ``Number of generations match the termination criteria`` (gaRes:GAResult) =  gaRes.Termination .=. gaRes.GA.GenerationsNumber
  static member ``Two GA's with the same inputs should result in two solutions with the same fitness`` (g1:GAResult, g2:GAResult) =  g1.GA.BestChromosome.Fitness.GetValueOrDefault() .=. g2.GA.BestChromosome.Fitness.GetValueOrDefault()
  static member ``Two GA's with the same inputs should result in two identical best genes`` (g1:GAResult, g2:GAResult) =  g1.GA.BestChromosome.GetGenes() .=. g2.GA.BestChromosome.GetGenes()
  static member ``N+1 generation has same or better fitness than N`` (gaRes:GAResult) = let l = listtolist (gaRes.Generations) [] in CheckIncreasingFitness l
;;


[<EntryPoint>]
let main argv =
    Arb.register<MyGenerators>()
    //Check.All<GAProperties> ({Config.Quick with MaxTest = 500})
    Check.One({Config.Quick with MaxTest = 1}, GAProperties.``N+1 generation has same or better fitness than N``)
    0 // return an integer exit code
