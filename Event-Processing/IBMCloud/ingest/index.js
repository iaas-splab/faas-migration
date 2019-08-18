'use strict';

const { Kafka, CompressionTypes, logLevel } = require('kafkajs')

const push = function(args, topic, messages) {
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
  console.log(JSON.stringify(args));

  let message = args;

  let topic = "";

  if(message.type === undefined) {
    return{
      statuscode: 400,
      headers: {
        "Content-Type": "application/json"
      },
      body: {}
    }
  } else if (message.type === "temperature"){
    topic = "temperature";
  } else if (message.type === "forecast") {
    topic = "forecast";
  } else if (message.type === "status_change"|| message.type === "state_change") {
    topic = "state-change";
  } else {
    return{
      statuscode: 400,
      headers: {
        "Content-Type": "application/json"
      },
      body: {}
    }
  }

  push(args,topic,[message]);

  let result = {};

  return {
      statuscode: 200,
      headers: {
        "Content-Type": "application/json"
      },
      body: result
  };
}
