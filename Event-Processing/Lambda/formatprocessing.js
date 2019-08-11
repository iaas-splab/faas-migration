'use strict';

const aws = require('aws-sdk')
const sqs = new aws.SQS({ region: process.env.CLOUD_REGION })

const getUnixTime = () => {
  return new Date().getTime()/1000|0
}
const getQueueURL = () => {
  return 'https://sqs.'+process.env.CLOUD_REGION+'.amazonaws.com/' +
       process.env.CLOUD_ACCOUNT_ID + '/'+ process.env.QUEUE_NAME;
};

module.exports.formatTemperatureEvent = async (event) => {
  let tempEvent = JSON.parse(event.Records[0].Sns.Message);

  console.log(JSON.stringify(tempEvent));

  let message = "Measured Temperature "+ tempEvent.value + " on device "+  tempEvent.source;

  let evt = {
    type: tempEvent.type,
    source: tempEvent.source,
    timestamp: tempEvent.timestamp,
    formatting_timestamp: getUnixTime(),
    message: message
  }

  let messageString = JSON.stringify(evt);
  console.log(messageString);
  console.log(getQueueURL());

  var params = {
    MessageBody: messageString,
    QueueUrl: getQueueURL()
  };

  sqs.sendMessage(params, function(err, data) {
    if(err != null) {
      console.log(err);
    }
    console.log(JSON.stringify(data));
  });
  return {};
};

module.exports.formatForecastEvent = async (event) => {
  let tempEvent = JSON.parse(event.Records[0].Sns.Message);

  console.log(JSON.stringify(tempEvent));

  let message = tempEvent.source + " has Forecasted " + tempEvent.forecast +
     " at " + tempEvent.place + " for " + tempEvent.forecast_for;

  let evt = {
    type: tempEvent.type,
    source: tempEvent.source,
    timestamp: tempEvent.timestamp,
    formatting_timestamp: getUnixTime(),
    message: message
  }

  let messageString = JSON.stringify(evt);
  console.log(messageString);
  console.log(getQueueURL());

  var params = {
    MessageBody: messageString,
    QueueUrl: getQueueURL()
  };

  sqs.sendMessage(params, function(err, data) {
    if(err != null) {
      console.log(err);
    }
    console.log(JSON.stringify(data));
  });
  return {};
};

module.exports.formatStateChangeEvent = async (event) => {
  let tempEvent = JSON.parse(event.Records[0].Sns.Message);

  console.log(JSON.stringify(tempEvent));

  let message = tempEvent.source + " has Submitted a status change with the message "+ tempEvent.message;

  let evt = {
    type: tempEvent.type,
    source: tempEvent.source,
    timestamp: tempEvent.timestamp,
    formatting_timestamp: getUnixTime(),
    message: message
  }

  let messageString = JSON.stringify(evt);
  console.log(messageString);
  console.log(getQueueURL());

  var params = {
    MessageBody: messageString,
    QueueUrl: getQueueURL()
  };

  sqs.sendMessage(params, function(err, data) {
    if(err != null) {
      console.log(err);
    }
    console.log(JSON.stringify(data));
  });
  return {};
};

module.exports.handleErr = async (event) => {
  let tempEvent = event.Records[0].Sns.Message;
  console.log("Event with Payload has failed! "+ tempEvent);
  return {};
};