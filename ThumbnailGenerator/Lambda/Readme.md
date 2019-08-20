# Thumbnail Generator

Implements the first use case on Amazon Web Services using Java and the serverless framework

## Buiilding

Assuming Java 8 (JDK) and Apache Maven are installed just run the following command to compile the source code:
```
mvn clean install
```

## Deploying

Before deploying you might want to modify the names of the buckets in the `serverless.yml` to ensure they are unique:
```yaml
custom:
  image_bucket_name: <Name of the Input Bucket>
  thumb_bucket_name: <Name of the Output Bucket>
```

After the code has been built it can be deployed by running:
```
serverless deploy -v
```

## Destroying

To destroy the application you first have to make sure both buckets are empty. This process is not automated. To do this just open
the Web Interface and delete the contents of both the input and the output bucket. Removing the buckets themselves is not needed
since the Serverless Frameworks CloudFormation Script will handle this for us.

Another option to delete the contents of the buckets is by the help of the AWS CLI. To remove contents in the bucket
just run the following command for both buckets:
```
aws s3 rm s3://<S3_BUCKET_NAME> --recursive
```

To destroy the application just run:
```
serverless remove -v
```

## Usage

Every time a image gets uploaded to a S§ bucket the `thumbnail-generator` function triggers to generate
a Thumbnail of the uploaded image (i.e. a Image with the dimensions of 160x90 Pixels).
The thumbnail gets stored under the same key but in the thumbnail bucket defined in the `serverless.yml`.

A simple upload function is also deployed to simplify the upload

Sample command:

```bash
cat img.png | base64 | curl -H "Content-Type: image/png" -d @- https://8afuw3tgc3.execute-api.us-east-1.amazonaws.com/dev/upload
```

To check if the thumbnail generation worked we also call a Discord webhook, can be disabled by setting the url to a empty string.
