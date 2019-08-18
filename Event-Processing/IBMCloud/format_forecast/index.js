'use strict';

const { Kafka, CompressionTypes, logLevel } = require('kafkajs')

const push = function(args,topic, messages) {
  const kafka = new Kafka({
    logLevel: logLevel.DEBUG,
    brokers: args.kafka_brokers_sasl,
    clientId: 'ingest-func',
    ssl: {
      ssl:true,
      rejectUnauthorized: false
    },
    sasl: {
      mechanism: 'plain',
      username: args.username,
      password: args.password,
    },
  });
  let prod = kafka.producer();
  await prod.connect();
  await prod.send({
    topic,
    messages: messages
  });
  prod.disconnect();
}

exports.main =  async function(args) {
  let params = args;
  console.log(JSON.stringify(args));

  var msgs = params.messages; 
  var results = [];
  for (var i = 0; i < msgs.length; i++) {
    var tempEvent = msgs[i];

    context.log(JSON.stringify(tempEvent));

    let message = tempEvent.source + " has Forecasted " + tempEvent.forecast + " at " + tempEvent.place + " for " + tempEvent.forecast_for;

    let evt = {
      type: tempEvent.type,
      source: tempEvent.source,
      timestamp: tempEvent.timestamp,
      formatting_timestamp: getUnixTime(),
      message: message
    }

    let messageString = JSON.stringify(evt);
    context.log(messageString);
    results.push(evt);
  }
  push(args,"dbingest",results);
  return {
  };
}

const getUnixTime = () => {
  return new Date().getTime()/1000|0
}