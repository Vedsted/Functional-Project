# Functional-Project
This repository is part of a mini-project in the course Functional Programming and Property-based Testing, at University of Southern Denmark.

Contributers: @Vedsted and @csbc92


## Prerequisites
Tested with the following dotnet core versions on Windows and Linux:  
* `Dotnet core v. 3.1.103`
* `Dotnet core v. 3.1.300`

## Running the Traveling Salesman Problem (TSP)

1. Clone this github repository
1. Open and execute the project `GeneticSharpIssue` in Visual Studio or execute the project directly with cmd
```cmd
cd Functional-Project
dotnet run -p GeneticSharpIssue
```

## Running FsCheck with reproduced pseudo random sequences of the Genetic Algorithm
The seed can be configured to reproduce pseudo random sequences of the Genetic Algorithm.

`GeneticSharpIssue/Program.fs`
```fsharp
[<EntryPoint>]
let main argv =
    Arb.register<TspGenerators>()
    
    // ..
    
    // For manual testing with user defined termination and seed:
    let termination = 500
    let seed = 781433289 // This magic seed triggers an error. Can be changed to reproduce runs.
    let res = CreateTSP seed termination
    printfn "Checking GeneticAlgorithmResult: \n%s \n\nFsCheck Trace:" (res.ToString())
    (GAProperties.``P2 - Generation N+1 must have a fitness greater or equal to N`` res).QuickCheck()
    
   // ..
```

## Running FsCheck Generating new pseudo random sequences
`GeneticSharpIssue/Program.fs`
```fsharp
[<EntryPoint>]
let main argv =
   Arb.register<TspGenerators>()
   // Comment in for running 'MaxTest' times with random seeds and terminations
   Check.All<GAProperties> ({Config.Verbose with MaxTest = 100})
    
   // ..
```

## Description of other Projects in the repository
* GeneticSharpImplementations - contains implementations of TSP which is used in the FsCheck program.
* GeneticSharpIssue - contains a simplified version of the FsCheck program to replicate a specific bug.
* GeneticSharpUtils - contains modified implementations of the Random generators. The implementations supports providing an initial seed.
* TestSuite - A simple C# test playground.
* TestSuiteFSharp - The full test suite with multiple properties under test.
