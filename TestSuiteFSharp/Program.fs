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
open GeneticSharpImplementations.Fitnesses
open GeneticSharpUtils
open System.Collections.Generic

type GAResult (GA: GeneticAlgorithm, Termination: int, Seed: int, BestChromosomes: list<Tuple<int, IChromosome>>) =
    member this.GA = GA
    member this.Termination = Termination
    member this.Seed = Seed
    member this.BestChromosomes = BestChromosomes
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
    
    let mutable bestChromosomes = []
    let handler = EventHandler( fun sender eventArgs -> let g: GeneticAlgorithm =  sender :?> GeneticAlgorithm in  bestChromosomes <- bestChromosomes@[(g.GenerationsNumber, g.BestChromosome)])
                                                                                                                                                        
    ga.GenerationRan.AddHandler(handler) 
    ga.Start()
    let res = GAResult(ga, termination, seed, bestChromosomes)
    res
;;



let GAGen : Gen<GAResult> =
    let terminationMin = 1
    let terminationMax = 1000
    let terminationGen = Gen.choose (terminationMin,terminationMax)
    let seedMin = 0
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


type MyGenerators =
  static member GeneticAlgorithm() =
      {new Arbitrary<GAResult>() with
          override x.Generator = GAGen
          override x.Shrinker t = Seq.empty }
  static member IdenticalGeneticAlgorithmPair() =
      {new Arbitrary<Tuple<GAResult, GAResult>>() with
          override x.Generator = TwinGAGen
          override x.Shrinker t = Seq.empty }
;;

let rec CheckIncreasingFitness (l1:list<Tuple<int, IChromosome>>) (l2:list<Tuple<int, IChromosome>>)=
    match (l1, l2) with
    | ([], []) | (_,[]) | ([],_) -> (0,"",0, "")
    | (e1::l1, e2::l2) when e1 = e2 -> CheckIncreasingFitness l1 l2
    | ((i1,c1)::_, (i2,c2)::_) -> (i1, c1.ToString(), i2, c2.ToString())
;;

// Custom comparison. This compares and print the left and right sides. 
let (.=.) left right = left = right |@ sprintf "%A = %A" left right;;
let (.==.) (left:list<Tuple<int, IChromosome>>) (right:list<Tuple<int, IChromosome>>) =
    let (i1, c1, i2, c2) = CheckIncreasingFitness left right in
    left = right |@ sprintf "(Index: %i Value: %s) = (Index: %i Value: %s)" i1 c1 i2 c2 
;;

type GAProperties =
  static member ``P1 - Number of generations match the termination criteria``
    (gaRes:GAResult) =  gaRes.Termination .=. gaRes.GA.GenerationsNumber
  static member ``P2 - N+1 generation has same or better fitness than N``
    (gaRes:GAResult) = gaRes.BestChromosomes .==. (List.sortBy (fun (i,c) -> c.Fitness.GetValueOrDefault(), i) gaRes.BestChromosomes)
  static member ``P3 - Two GA's with the same inputs should result in two solutions with the same fitness``
    (g1:GAResult, g2:GAResult) =  g1.BestFitness .=. g2.BestFitness
  static member ``P4 - Two GA's with the same inputs should result in two identical best genes``
    (g1:GAResult, g2:GAResult) = g1.BestGenes .=. g2.BestGenes
;;


[<EntryPoint>]
let main argv =
    Arb.register<MyGenerators>()
    Check.All<GAProperties> ({Config.Quick with MaxTest = 10})
    //Check.One({Config.Quick with MaxTest = 1}, GAProperties.``N+1 generation has same or better fitness than N``)
    0 // return an integer exit code
