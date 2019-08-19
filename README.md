# Practical Aspects of FaaS Applications' Migration

This Repository contains the implementation of the Use Cases used in my Bachelor Thesis.

## General Documentation

Some general notes on configuring the cloud providers command line interfaces or the general deployment of a .NET based Function
Application on Azure can be found in the [Docs Folder](/docs).

## Directory Structure

The Use-Cases are grouped together in a Directory in the Root Directory. In the Directory for every use-case the provider specific implementation can be found. With two exceptions: The ToDo API implementations for IBMCloud and AWS Lambda are located in a different repository since these are implemented in Go. Their implementation can be found [here](https://github.com/c-mueller/faas-migration-go/).

## License

The Source Code is Licensed under Apache License (Version 2.0). See [License](LICENSE) for more informations.
