# Thumbnail Generator

Implements the first use case on Amazon Web Services using Java and the serverless framework

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