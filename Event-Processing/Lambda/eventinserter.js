const createDBQuery = "CREATE TABLE IF NOT EXISTS events (ID int unsigned NOT NULL auto_increment PRIMARY KEY, source VARCHAR(255) NOT NULL, timestamp int unsigned NOT NULL, message VARCHAR(1000) NOT NULL);"

const mysql = require('serverless-mysql')({
  config: {
    database: process.env.RDS_DB_NAME,
    user: process.env.RDS_USERNAME,
    password: process.env.RDS_PASSWORD,
    host: process.env.RDS_HOST,
    port: process.env.RDS_PORT
  }
});

module.exports.insertEvent = async (event) => {
  let creationResult = await mysql.query(createDBQuery);
  const evt= JSON.parse(event.Records[0].body);
  console.log(creationResult);
  console.log(JSON.stringify(evt));
  let insertResult = await mysql.query({
    sql: 'INSERT INTO events(source, timestamp, message) VALUES (?, ?, ?);',
    timeout: 10000,
    values: [evt.source, evt.timestamp, evt.message]
  });
  console.log(insertResult);
  await mysql.end();
  return {};
};