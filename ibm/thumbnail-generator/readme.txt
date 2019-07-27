
Compile:
mvn clean install

Deploy Upload Function:
ibmcloud fn action create thumb-uploader target/thumbnail-generator.jar --main xyz.cmueller.wsk.Uploader

Deploy generator function:
ibmcloud fn action create thumb-uploader target/thumbnail-generator.jar --main xyz.cmueller.wsk.Generator


Create Trigger:
ibmcloud fn trigger create tgen-trigger --feed tgen/changes --param bucket tgen-input-uss --param endpoint s3.us-south.cloud-object-storage.appdomain.cloud
