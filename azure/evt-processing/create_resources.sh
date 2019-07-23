#!/bin/bash

# CONFIGURATION
export RESOURCE_GROUP_NAME=bachelor-thesis
export AZURE_REGION=westeurope
export MYSQL_ADMIN_USERNAME=krnladmin
export MYSQL_ADMIN_PASSWORD=eSh4iimaeh2lioxaseukiegheNoo4phi

export STORAGE_ACCOUNT_NAME=krnlevtprocstore
export APPINSIGHTS_NAME=krnlfuncai
export FUNCTION_APP_NAME=krnlfuncapp
export MYSQL_SERVER_NAME=krnlevtprocdb
export MYSQL_DB_NAME=krnlevtprocdb
export EVT_HUB_NAME=krnlevthub

# CREATION CODE
echo Creating Storage Account...
az storage account create -g $RESOURCE_GROUP_NAME -n $STORAGE_ACCOUNT_NAME --kind StorageV2 --sku Standard_LRS --location $AZURE_REGION >> deployment.log

echo Creating MySQL instance
az mysql server create -g $RESOURCE_GROUP_NAME -n $MYSQL_SERVER_NAME -u $MYSQL_ADMIN_USERNAME -p $MYSQL_ADMIN_PASSWORD -l $AZURE_REGION --sku-name B_Gen5_2 >> deployment.log
az mysql db create -g $RESOURCE_GROUP_NAME --server-name $MYSQL_SERVER_NAME -n $MYSQL_DB_NAME >> deployment.log
az mysql server firewall-rule create -g $RESOURCE_GROUP_NAME --server-name $MYSQL_SERVER_NAME --name allow-all --start-ip-address 0.0.0.0 --end-ip-address 255.255.255.255 >> deployment.log

echo Creating EventHub Namespace
az servicebus namespace create -g $RESOURCE_GROUP_NAME -n $EVT_HUB_NAME --location $AZURE_REGION >> deployment.log

echo Creating Topics and subscriptions
echo Creating Topic 'forecast'
az servicebus topic create -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME -n forecast >> deployment.log
az servicebus topic subscription create -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME --topic-name forecast -n forecastpush >> deployment.log
az servicebus topic subscription create -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME --topic-name forecast -n forecastpull >> deployment.log
echo Creating Topic 'temperature'
az servicebus topic create -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME -n temperature >> deployment.log
az servicebus topic subscription create -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME --topic-name temperature -n temperaturepush >> deployment.log
az servicebus topic subscription create -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME --topic-name temperature -n temperaturepull >> deployment.log
echo Creating Topic 'statechange'
az servicebus topic create -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME -n statechange >> deployment.log
az servicebus topic subscription create -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME --topic-name statechange -n statechangepush >> deployment.log
az servicebus topic subscription create -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME --topic-name statechange -n statechangepull >> deployment.log

echo Creating Queue
az servicebus queue create -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME -n output >> deployment.log

STORAGE_ACCOUNT_CONNECTION_STRING=$(az storage account show-connection-string --name $STORAGE_ACCOUNT_NAME | jq -r '.connectionString')
SERVICE_BUS_CONNECTION_STRING=$(az servicebus namespace authorization-rule keys list -g $RESOURCE_GROUP_NAME --namespace-name $EVT_HUB_NAME --name RootManageSharedAccessKey | jq -r '.primaryConnectionString')
MYSQL_HOSTNAME="$MYSQL_SERVER_NAME.mysql.database.azure.com"
MYSQL_USERNAME="$MYSQL_ADMIN_USERNAME@$MYSQL_SERVER_NAME"

echo Creating Webapp and Insights
az resource create -g $RESOURCE_GROUP_NAME -n $APPINSIGHTS_NAME --resource-type "Microsoft.Insights/components" --properties '{"Application_Type":"web"}' >> deployment.log
az functionapp create -g $RESOURCE_GROUP_NAME -n $FUNCTION_APP_NAME --storage-account $STORAGE_ACCOUNT_NAME --consumption-plan-location $AZURE_REGION --app-insights $APPINSIGHTS_NAME --runtime node >> deployment.log

echo "Writing local manifest"
cp local.settings.json.template local.settings.json
jq ".Values.AzureWebJobsStorage = \"$STORAGE_ACCOUNT_CONNECTION_STRING\"" local.settings.json | sponge local.settings.json
jq ".Values.ServiceBusConnection = \"$SERVICE_BUS_CONNECTION_STRING\"" local.settings.json | sponge local.settings.json
jq ".ConnectionStrings.ServiceBusConnection = \"$SERVICE_BUS_CONNECTION_STRING\"" local.settings.json | sponge local.settings.json

jq ".Values.DBName = \"$MYSQL_DB_NAME\"" local.settings.json | sponge local.settings.json
jq ".Values.DBEndpoint = \"$MYSQL_HOSTNAME\"" local.settings.json | sponge local.settings.json
jq ".Values.DBUsername = \"$MYSQL_USERNAME\"" local.settings.json | sponge local.settings.json
jq ".Values.DBPassword = \"$MYSQL_ADMIN_PASSWORD\"" local.settings.json | sponge local.settings.json

echo "Deploying To Azure"

func azure functionapp publish $FUNCTION_APP_NAME -o

echo To update the sources in th cloud run the following command:
echo
echo func azure functionapp publish $FUNCTION_APP_NAME -o