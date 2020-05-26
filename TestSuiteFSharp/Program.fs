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
open System.Collections.Generic

type GAResult (GA: GeneticAlgorithm, Termination: int, Seed: int, HookChromosomes: list<IChromosome>) =
    member this.GA = GA
    member this.Termination = Termination
    member this.Seed = Seed
    member this.HookChromosomes = HookChromosomes
    member this.BestFitness = (this.GA.BestChromosome.Fitness.GetValueOrDefault())
    member this.BestGenes = (this.GA.BestChromosome.GetGenes())
    member this.printInitial = sprintf "{ Seed: %i, termination: %i}" this.Seed this.Termination
    member this.printResult = sprintf "{ termination: %i}" this.GA.GenerationsNumber
    override this.ToString () = sprintf "{ HashCode: %i, initial: %s, result: %s }" (this.GetHashCode()) this.printInitial this.printResult

let CreateGA seed termination : GAResult =
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
    
    let mutable hookChromosomes : list<IChromosome>= []
    let handler = EventHandler( fun sender eventArgs -> let g: GeneticAlgorithm =  sender :?> GeneticAlgorithm in  hookChromosomes <- hookChromosomes@[g.BestChromosome])
    ga.GenerationRan.AddHandler(handler)
    ga.Start()
    let res = GAResult(ga, termination, seed, hookChromosomes)
    res
;;

let CreateTSP seed termination =
    RandomizationProvider.Current <- FastRandomRandomizationWithSeed()
    FastRandomRandomizationWithSeed.setSeed(seed)
    
    let selection = EliteSelection()
    let crossover = OrderedCrossover()
    let mutation = TworsMutation()
    let fitness = TspFitness(20, 0, 1000, 0, 1000)
    let chromosome = TspChromosome(fitness.Cities.Count)
    let population = Population(50, 70, chromosome)
    let ga = GeneticAlgorithm(population, fitness, selection, crossover, mutation)
    let mutable hookChromosomes : list<IChromosome> = []
    let handler = EventHandler( fun sender eventArgs -> let g: GeneticAlgorithm =  sender :?> GeneticAlgorithm in  hookChromosomes <- hookChromosomes@[g.BestChromosome])
    ga.GenerationRan.AddHandler(handler)
    ga.Termination <- GenerationNumberTermination(termination)
    ga.Start()
    let res = GAResult(ga, termination, seed, hookChromosomes)
    res
;;


let GAGen : Gen<GAResult> =
    let terminationMin = 1
    let terminationMax = 1000
    let terminationGen = Gen.choose (terminationMin,terminationMax)
    let seedMin = Int32.MinValue
    let seedMax = Int32.MaxValue
    let seedGen = Gen.choose (seedMin,seedMax)
    Gen.map2 CreateGA seedGen terminationGen
;;


let TwinGAGen : Gen<GAResult * GAResult> =
    let CreateTwin (gaRes:GAResult) =
        let gaRes2 = CreateGA (gaRes.Seed) (gaRes.Termination) in
        (gaRes, gaRes2)
    in Gen.map CreateTwin GAGen
;;

let TspGen : Gen<GAResult> =
    let terminationMin = 1
    let terminationMax = 1000
    let terminationGen = Gen.choose (terminationMin,terminationMax)
    let seedMin = Int32.MinValue
    let seedMax = Int32.MaxValue
    let seedGen = Gen.choose (seedMin,seedMax)
    Gen.map2 CreateTSP seedGen terminationGen
;;


let TwinTspGen : Gen<GAResult * GAResult> =
    let CreateTwin (gaRes:GAResult) =
        let gaRes2 = CreateTSP (gaRes.Seed) (gaRes.Termination) in
        (gaRes, gaRes2)
    in Gen.map CreateTwin TspGen
;;

type ImpGenerators =
  static member GeneticAlgorithm() =
      {new Arbitrary<GAResult>() with
          override x.Generator = GAGen
          override x.Shrinker t = Seq.empty}
  static member IdenticalGeneticAlgorithmPair() =
      {new Arbitrary<Tuple<GAResult, GAResult>>() with
          override x.Generator = TwinGAGen
          override x.Shrinker t = Seq.empty }
;;

type TspGenerators =
  static member GeneticAlgorithm() =
      {new Arbitrary<GAResult>() with
          override x.Generator = TspGen
          override x.Shrinker t = Seq.empty}
  static member IdenticalGeneticAlgorithmPair() =
      {new Arbitrary<Tuple<GAResult, GAResult>>() with
          override x.Generator = TwinTspGen
          override x.Shrinker t = Seq.empty }
;;

// Custom comparison. This compares and print the left and right sides. 
let (.=.) left right = left = right |@ sprintf "%A = %A" left right
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

type GAProperties =
  static member ``P1 - Number of generations match the termination criteria``
    (gaRes:GAResult) =  gaRes.Termination .=. gaRes.GA.GenerationsNumber
  static member ``P2 - N+1 generation has same or better fitness than N``
    (gaRes:GAResult) = gaRes.HookChromosomes .==. (List.sortBy (fun e -> e.Fitness.GetValueOrDefault()) gaRes.HookChromosomes)
  static member ``P3 - Two GA's with the same inputs should result in two solutions with the same fitness``
    (g1:GAResult, g2:GAResult) = g1.BestFitness .=. g2.BestFitness
  static member ``P4 - Two GA's with the same inputs should result in two identical best genes``
    (g1:GAResult, g2:GAResult) = g1.BestGenes .=. g2.BestGenes
;;



[<EntryPoint>]
let main argv =
    
    printfn "####################### IMP Test #######################"
    // Run for own impl
    Arb.register<ImpGenerators>()
    Check.All<GAProperties> ({Config.Quick with MaxTest = 100})
    
    printfn "####################### TSP Test #######################"
    // Run for TSP
    Arb.register<TspGenerators>()
    Check.All<GAProperties> ({Config.Quick with MaxTest = 100})
    
    0 // return an integer exit code
