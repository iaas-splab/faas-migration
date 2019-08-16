# ToDo API

This Directory contains the implementations of the ToDo API on Microsoft Azure. The other implementations are based on Go and can be found under
https://github.com/c-mueller/faas-migration-go

## Running the Tests

To run the tests, esure the Go SDK is installed. After that the tests can be executed by running the following command in this directory:
```
go run *.go -endpoint <API Endpoint>
```

The tests assume the implemeńtation you want to test is already deployed. To do so follow the instructions in the directories of the
implementations.
