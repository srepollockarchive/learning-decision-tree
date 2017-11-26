# LearningDecisionTree

Learning Decision Tree from input file using testing data

This was for my COMP8901 Course at BCIT. We used the ID3 algorithm to run
training data through the program and then run test data in and check how well
the tree does in comparison to the test data.

All comments are inline in proper C# syntax.

## Build and Run

To build and run this program, it requires
[.NET Core](https://www.microsoft.com/net/learn/get-started/) to build and run.

After installing, the framework (link above) navigate the the parent directory
of the class. Once there, run the command in a shell `dotnet build`. This will
create a build in `./LearningDecisionTree/bin/Debug/netcoreapp2.0` labelled
`LearningDecisionTree.dll`. Now run
`dotnet ./LearningDecisionTree/bin/Debug/netcoreapp2.0/LearningDecisionTree.dll
<training data> <testing data>` *(formats given below)*.

## Data Formats

The program needs data in the following form strictly:

```txt
// class labels
[label 1]
[label 2]
// number of features
2
// name of features and attributes
[feature1 name] [attribute1] [attribute2]
[feature2 name] [attribute1] [attribute2]
// number of examples
4
// examples
[example name] [class label] [feature1 attribute] [feature2 attribute]
[example name] [class label] [feature1 attribute] [feature2 attribute]
[example name] [class label] [feature1 attribute] [feature2 attribute]
[example name] [class label] [feature1 attribute] [feature2 attribute]
```

The data files can then be modified as you see fit.
> Data file structure was given by my instructor Aaron, I did not make them 
> myself