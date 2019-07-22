#!/bin/bash

# CONFIGURATION
export RESOURCE_GROUP_NAME=bachelor-thesis
export AZURE_REGION=westeurope
export MYSQL_ADMIN_USERNAME=krnladmin
export MYSQL_ADMIN_PASSWORD=eSh4iimaeh2lioxaseukiegheNoo4phi

# ONLY CHANGE THESE VALUES IN CASE OF CONFLICS
export STORAGE_ACCOUNT_NAME=krnlevtprocstore
export MYSQL_SERVER_NAME=krnlevtprocdb
export MYSQL_DB_NAME=krnlevtprocdb
export EVT_HUB_NAME=krnlevthub

# CREATION CODE

echo Creating Storage Account...
az storage account create -g $RESOURCE_GROUP_NAME -n $STORAGE_ACCOUNT_NAME --kind StorageV2 --location $AZURE_REGION >> deployment.log

echo Creating MySQL instance
az mysql server create -g $RESOURCE_GROUP_NAME -n $MYSQL_SERVER_NAME -u $MYSQL_ADMIN_USERNAME -p $MYSQL_ADMIN_PASSWORD -l $AZURE_REGION --sku-name B_Gen5_2 >> deployment.log
az mysql db create -g $RESOURCE_GROUP_NAME --server-name $MYSQL_SERVER_NAME -n $MYSQL_DB_NAME >> deployment.log

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