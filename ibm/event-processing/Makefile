CF_ORG_NAME := kaffemuehle@posteo.de
CF_ORG_SPACE := dev

EVENT_STREAMS_SERVICE_NAME := evt-hub
EVENT_STREAMS_ALIAS_NAME := my-evt-hub
EVENT_STREAMS_CREDENTIALS_NAME := evt-hub-creds

EVENT_STREAMS_TEMPERATURE_EVENTS_TOPIC_NAME := temperature
EVENT_STREAMS_STATE_CHANGE_EVENTS_TOPIC_NAME := state-change
EVENT_STREAMS_FORECAST_EVENTS_TOPIC_NAME := forecast
EVENT_STREAMS_DB_INGEST_TOPIC_NAME := db-ingest

TEMPERATURE_TRIGGER_NAME := temp-trigger
FORECAST_TRIGGER_NAME := forecast-trigger
STATE_CHANGE_TRIGGER_NAME := statechange-trigger
DB_INGEST_TRIGGER := dbingest-trigger

KAFKA_BINDING_NAME := evt-hub-messages
KAFKA_BINDING_PATH := /$(CF_ORG_NAME)_$(CF_ORG_SPACE)/$(KAFKA_BINDING_NAME)/messageHubFeed

create_kafka:
	ibmcloud resource service-instance-create $(EVENT_STREAMS_SERVICE_NAME) messagehub standard eu-de
	ibmcloud resource service-alias-create $(EVENT_STREAMS_ALIAS_NAME) --instance-name $(EVENT_STREAMS_SERVICE_NAME)
	ibmcloud resource service-key-create $(EVENT_STREAMS_CREDENTIALS_NAME) Manager --instance-name evt-hub
	ibmcloud resource service-key $(EVENT_STREAMS_CREDENTIALS_NAME) --output json | jq '.[] | .credentials'> kafka_creds.json

sleep_30:
	sleep 30

create_topics: 
	ibmcloud es init
	ibmcloud es topic-create $(EVENT_STREAMS_DB_INGEST_TOPIC_NAME)
	ibmcloud es topic-create $(EVENT_STREAMS_FORECAST_EVENTS_TOPIC_NAME)
	ibmcloud es topic-create $(EVENT_STREAMS_STATE_CHANGE_EVENTS_TOPIC_NAME)
	ibmcloud es topic-create $(EVENT_STREAMS_TEMPERATURE_EVENTS_TOPIC_NAME)
	ibmcloud fn package bind /whisk.system/messaging evt-hub-messages -P kafka_creds.json

create_resources: create_kafka sleep_30 create_topics

create_triggers:
	ibmcloud fn trigger create $(TEMPERATURE_TRIGGER_NAME) -f $(KAFKA_BINDING_PATH) -p topic $(EVENT_STREAMS_TEMPERATURE_EVENTS_TOPIC_NAME)
	ibmcloud fn trigger create $(FORECAST_TRIGGER_NAME) -f $(KAFKA_BINDING_PATH) -p topic $(EVENT_STREAMS_FORECAST_EVENTS_TOPIC_NAME)
	ibmcloud fn trigger create $(STATE_CHANGE_TRIGGER_NAME) -f $(KAFKA_BINDING_PATH)-p topic $(EVENT_STREAMS_STATE_CHANGE_EVENTS_TOPIC_NAME)
	ibmcloud fn trigger create $(DB_INGEST_TRIGGER_NAME) -f $(KAFKA_BINDING_PATH) -p topic $(EVENT_STREAMS_DB_INGEST_TOPIC_NAME)
