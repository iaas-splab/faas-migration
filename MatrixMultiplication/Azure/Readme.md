# MatrixMultiplication - Azure

## Prerequisites

The following commands are requried to deploy and run this use-case

- Azure CLI
- Azure Functions Core Tools
- .NET Core SDK Version 2.1

We assumed that the login using `az login` was successful.

Only when working with make:
- Linux based environment (Others are untested!)
- `jq`
- `make`
- `sponge` (moreutils)
- A Previously created Resource Group

## Building and Deploying

### Variant 1: Using `make`

#### Step 1: Customizing `Makefile`

The configuration section in the beginning of the `Makefile` defines the names of the services that should be created.
You also must define the name of a previously created resource group there.

```makefile
# Define The name of the Resource Group to deploy in (must exist)
RESOURCE_GROUP_NAME := bachelor-thesis
# Define the name of the storage account (gets created if not present)
STORAGE_ACCOUNT_NAME := cmuellermtrxmulstore
# Define the Name of the Function App (gets created if not present)
FUNCTION_APP_NAME := cmuellermatrixmul
# Define the name of the Application insights instance used for tracing/logging (gets created if not present)
APPINSIGHTS_NAME := cmuellermatrixmulai
# Set the name of your preferred region e.g. westeurope, westus...
AZURE_REGION := westeurope
```

A important side note: The name of the Storage Account and the Function App must be unique globally. because the script
might crash if the resources already exist outside of your namespace. We recommed a name that is most likely unique.
In case you have no ideas: generate some random passwords built from lower case letters and numbers using a password generator
like `pwgen`(https://linux.die.net/man/1/pwgen):
```
pwgen -1 -A 16
```

#### Step 2a: Running Locally

To run the function app locally just run
```
make run_local
```

This command creates the mandatory resources, updates the connection string in the `local.settings.json` and launches the function host.

if no further modifications occur in the cloud environment you can call 
```
func host start
```
After you called the make command once, ensuring the resources and the configuration is written.

On the function host is up and running the Application is accessible on http://localhost:7071/
To trigger a Calculation you can call http://localhost:7071/api/TriggerMatrixMultiplication
For more information how parameters are passed over, please take a look at the "Starting a Multiplication"section.


### Variant 2: Using Rider

To deploy the function just run
```
make deploy
```

This will create all resources and will deploy the function after bulding it.
This also writes the local configuration allowing local execution using `func host start`

## Starting a Multiplication