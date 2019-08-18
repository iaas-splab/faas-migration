'use strict';

exports.main =  async function(args) {
  let params = args;
  console.log(JSON.stringify(args));

  let dburl = args.uri.toString().replace("mysql://","");
  let spliturl = dburl.split("@");
  let creds = spliturl[0].split(":");
  let username = creds[0];
  let password = creds[1];
  let hostDb = spliturl[1].split("/");
  let hostnamePort = hostDb[0].split(":");
  let hostname = hostnamePort[0];
  let port = hostnamePort[1];
  let db = hostDb[1];

  console.log(JSON.stringify({
    user: username,
    pass: password,
    db: db,
    host: hostname,
    port: port
  }));

  const createDBQuery = "CREATE TABLE IF NOT EXISTS events (ID int unsigned NOT NULL auto_increment PRIMARY KEY, source VARCHAR(255) NOT NULL, timestamp int unsigned NOT NULL, message VARCHAR(1000) NOT NULL);"

  const mysql = require('serverless-mysql')({
    config: {
      database: db,
      user: username,
      password: password,
      host: hostname,
      port: parseInt(port,10),
      ssl: {
        rejectUnauthorized: false
      }
    }
  });

  let creationResult = await mysql.query(createDBQuery);

  var msgs = params.messages; 
  var results = [];
  for (var i = 0; i < msgs.length; i++) {
    var item = msgs[i];
    
    let evt = item;
    let insertResult = await mysql.query({
      sql: 'INSERT INTO events(source, timestamp, message) VALUES (?, ?, ?);',
      timeout: 10000,
      values: [evt.source, evt.timestamp, evt.message]
    });
    console.log(insertResult);
    results.push(insertResult);
  }
  await mysql.end();
  return {
    results: results
  };
}