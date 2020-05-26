# Functional-Project
This repository is part of a mini-project in the course Functional Programming and Property-based Testing, at University of Southern Denmark.

Contributers: @Vedsted and @csbc92


## Prerequisites
Tested with the following dotnet core versions on Windows and Linux:  
* `Dotnet core v. 3.1.103`
* `Dotnet core v. 3.1.300`

## Running the Test Suite

1. Clone this github repository
1. Open and execute the project `TestSuiteFSharp` in Visual Studio or execute the project directly with cmd
```cmd
cd Functional-Project
dotnet run -p TestSuiteFSharp
```

## Description of other Projects in the repository
* GeneticSharpImplementations - contains implementations of TSP which is used in the FsCheck program.
* GeneticSharpIssue - contains a simplified version of the FsCheck program to replicate a specific bug.
* GeneticSharpUtils - contains modified implementations of the Random generators. The implementations supports providing an initial seed.
* TestSuite - A simple C# test playground.
* TestSuiteFSharp - The full test suite with multiple properties under test.
