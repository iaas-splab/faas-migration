
Change Location:
ibmcloud target -r us-south
ibmcloud target -r us-south --cf

Create Namespace:
ibmcloud fn namespace create tgen-uss

Update Namespace:
ibmcloud fn property set --namespace tgen-uss

Compile:
mvn clean install

Create Package Binding:
ibmcloud fn package bind /whisk.system/cos-experimental tgen-os-binding

Deploy Upload Function:
ibmcloud fn action create thumb-uploader target/thumbnail-generator.jar --main xyz.cmueller.wsk.Uploader

Deploy generator function:
ibmcloud fn action create thumb-uploader target/thumbnail-generator.jar --main xyz.cmueller.wsk.Generator


Create Trigger:
ibmcloud fn trigger create tgen-trigger --feed tgen/changes --param bucket tgen-input-uss --param endpoint s3.us-south.cloud-object-storage.appdomain.cloud

Command to upload a Image:
echo "{}" | jq '.filename = "'$FILENAME'"' | jq '.data = "'(cat $FILEPATH | base64 -w0)'"' | curl -d @- -H "Content-Type: application/json" <Function URL>
