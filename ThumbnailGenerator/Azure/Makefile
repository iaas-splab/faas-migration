# Configuration
RESOURCE_GROUP := bachelor-thesis
FUNCTION_APP_NAME := cmueller-bt-thumbnail-generator
REGION := westeurope

# Do not change from here
STORAGE_ACCOUNT_CONNECTION_STRING := `az functionapp config appsettings list -g $(RESOURCE_GROUP) -n $(FUNCTION_APP_NAME) | jq -r '.[] | select(.name == "AzureWebJobsStorage") | .value'`
build:
	APP_NAME=$(FUNCTION_APP_NAME) RG=$(RESOURCE_GROUP) AZ_REGION=$(REGION) mvn clean install

publish: build
	APP_NAME=$(FUNCTION_APP_NAME) RG=$(RESOURCE_GROUP) AZ_REGION=$(REGION) mvn azure-functions:deploy

deploy: publish
	az storage container create -n output --connection-string $(STORAGE_ACCOUNT_CONNECTION_STRING)
	az storage container create -n input --connection-string $(STORAGE_ACCOUNT_CONNECTION_STRING)

run_local: build
	echo "{}" > local.settings.json
	func azure functionapp fetch-app-settings $(FUNCTION_APP_NAME)
	APP_NAME=$(FUNCTION_APP_NAME) RG=$(RESOURCE_GROUP) AZ_REGION=$(REGION) mvn azure-functions:run

ls_input:
	az storage blob list --container-name input --connection-string $(STORAGE_ACCOUNT_CONNECTION_STRING) | jq '.[] | {file_name: .name, file_size: .properties.contentLength,creation_timestamp: .properties.creationTime}'

ls_output:
	az storage blob list --container-name output --connection-string $(STORAGE_ACCOUNT_CONNECTION_STRING) | jq '.[] | {file_name: .name, file_size: .properties.contentLength,creation_timestamp: .properties.creationTime}'
