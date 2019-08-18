# Event Processing - IBM Implementation

## IMPORTANT NOTE

This implmentation is untested because deployment on IBM Cloud seems to fail due to an error on the Server side.
When attempting the creation of the Triggers subscribing to `messageHubFeed` the Trigger creation fails because the IBM
provided credentials are invalid. Setting the credentials manually also fails. An attmpt of deploying a Example action from the Web UI also fails, however it only displays a very generic error message there.

An example of the produced error is found in the [trigger_creation_failure.txt](trigger_creation_failure.txt) File

This [Asciicast](https://asciinema.org/a/6RpwJ3FTCbuuRe6n3olUfr18r) shows the failure of the trigger generation during deployment.

To implement the trigger generation the this [Docuemntation Page](https://cloud.ibm.com/docs/openwhisk?topic=cloud-functions-pkg_event_streams#eventstreams_pkg) was used.

## Requirements

## Deployment Guide

First we have to ensure all dependencies i.e. The database and the Event Streams instance (including its topics) are created. This is done by running

```
make create_resources
```

Next we deploy the Actions by running:
```
make deploy_actions
```

This step also handles packaging in dependency installation of the functions using `npm`

Next the triggers can be created using the `make create_triggers` command

After the trigger has been created we need to create rules to ensure Actions get triggered from the functions by running
```
make create_rules
```

Finally run `make create_api` to dpeloy the API mappings