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

  let result = await mysql.query({
    sql: 'SELECT * FROM events ORDER BY ID DESC LIMIT 1;',
    timeout: 10000
  });
  context.log(result);
  await mysql.end();

  context.res = {
      statuscode: 200,
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(result)
  };
}