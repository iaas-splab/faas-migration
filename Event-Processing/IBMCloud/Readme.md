# Event Processing - IBM Implementation

## IMPORTANT NOTE

This implmentation is untested because deployment on IBM Cloud seems to fail due to an error on the Server side.
When attempting the creation of the Triggers subscribing to `messageHubFeed` the Trigger creation fails because the IBM
provided credentials are invalid. Setting the credentials manually also fails. An attmpt of deploying a Example action from the Web UI also fails, however it only displays a very generic error message there.

An example of the produced error is found in the [trigger_creation_failure.txt](trigger_creation_failure.txt) File

This [Asciicast](https://asciinema.org/a/6RpwJ3FTCbuuRe6n3olUfr18r) shows the failure of the trigger generation during deployment.

To implement the trigger generation the this [Docuemntation Page](https://cloud.ibm.com/docs/openwhisk?topic=cloud-functions-pkg_event_streams#eventstreams_pkg) was used.

Due to these isses we have also not verified the suitability of KafkaJS to produce/publish messages to the broker.

## Requirements

Deploying this application requires the following dependencies and prerequisites:

- A CloudFoundry Organzation and space to deploy in
- IBM Cloud CLI with Cloud Functions extension
- GNU Make
- `jq`

## Deployment Guide

To deploy the use-case first modify the following lines in the `Makefile` accordingly to suit your environment.

```make
# Define the Name and Space of your Cloud Foundry organzation accordingly
# The Makefile assumes this namespace is selected using "ibmcloud fn property --namespace"
# and "ibmcloud targed" appropriately
CF_ORG_NAME := kaffemuehle@posteo.de
CF_ORG_SPACE := dev
```

After that make sure the `<Org>_<Space>` function namespace is the current namespace and also ensure the
same space is defined as Target (can be checked by running `ibmcloud target`)

Next we have to ensure all dependencies i.e. The database and the Event Streams instance (including its topics) are created. This is done by running

```bash
make create_resources
```

Next we deploy the Actions by running:

```bash
make deploy_actions
```

This step also handles packaging in dependency installation of the functions using `npm`

Next the triggers can be created using the following command

```bash
make create_triggers
```

After the trigger has been created we need to create rules to ensure Actions get triggered from the functions by running:

```
make create_rules
```

Finally run `make create_api` to dpeloy the API mappings.
