# Defines the name of the input bucket used to upload Images to
OBJECT_STORAGE_INPUT_BUCKET := krnl-tgen-uss
# Defines the name of the output bucket used to thumbnails to
OBJECT_STORAGE_OUTPUT_BUCKET := krnl-tgen-uss-thumbnails
# Access Key to Object Storage with Manager Permissions
OBJECT_STORAGE_ACCESS_KEY := 83233f26ea46473189aca35dc634ca63
# Secret Key to Object Storage with Manager Permissions
OBJECT_STORAGE_SECRET_KEY := d06b121718b21dbe694fb2be6963d91137b47c884040adf1

# Defines the Endpoint used to Access the data. Usualy this should not be changed
OBJECT_STORAGE_ENDPOINT := s3.us-south.cloud-object-storage.appdomain.cloud

# NAME DEFINITION
NAMESPACE_NAME := tgen-uss
PACKAGE_BINDING_NAME := tgen-os-binding
TRIGGER_NAME := tgen-os-trigger
RULE_NAME := tgen-os-rule
UPLOADER_ACTION_NAME := uploader
GENERATOR_ACTION_NAME := generator

# MAKEFILE OPERATIONS

init:
	# Change Region to Dallas
	ibmcloud target -r us-south
	ibmcloud target -r us-south --cf
	# Create New Namespace
	ibmcloud fn namespace create $(NAMESPACE_NAME)
	ibmcloud fn property set --namespace $(NAMESPACE_NAME)
	# Create Package Binding for experimental object storage trigger
	ibmcloud fn package bind /whisk.system/cos-experimental $(PACKAGE_BINDING_NAME)
	# Bind service to function namespace
	ibmcloud fn service bind cloud-object-storage $(PACKAGE_BINDING_NAME)

build:
	# Build JAR
	mvn clean package

configure_trigger:
	ibmcloud fn trigger create $(TRIGGER_NAME) --feed $(PACKAGE_BINDING_NAME)/changes -p bucket $(OBJECT_STORAGE_INPUT_BUCKET) -p endpoint $(OBJECT_STORAGE_ENDPOINT)

deploy_actions: build
	ibmcloud fn action create $(UPLOADER_ACTION_NAME) target/thumbnail-generator.jar --web true --main xyz.cmueller.wsk.Uploader -p endpoint $(OBJECT_STORAGE_ENDPOINT) -p bucket $(OBJECT_STORAGE_INPUT_BUCKET) -p access_key $(OBJECT_STORAGE_ACCESS_KEY) -p secret_key $(OBJECT_STORAGE_SECRET_KEY) -a final true
	ibmcloud fn action create $(GENERATOR_ACTION_NAME) target/thumbnail-generator.jar --main xyz.cmueller.wsk.Generator -p endpoint $(OBJECT_STORAGE_ENDPOINT) -p output_bucket $(OBJECT_STORAGE_OUTPUT_BUCKET) -p access_key $(OBJECT_STORAGE_ACCESS_KEY) -p secret_key $(OBJECT_STORAGE_SECRET_KEY)

update_actions: build
	ibmcloud fn action update $(UPLOADER_ACTION_NAME) target/thumbnail-generator.jar --web true --main xyz.cmueller.wsk.Uploader -p endpoint $(OBJECT_STORAGE_ENDPOINT) -p bucket $(OBJECT_STORAGE_INPUT_BUCKET) -p access_key $(OBJECT_STORAGE_ACCESS_KEY) -p secret_key $(OBJECT_STORAGE_SECRET_KEY) -a final true
	ibmcloud fn action update $(GENERATOR_ACTION_NAME) target/thumbnail-generator.jar --main xyz.cmueller.wsk.Generator -p endpoint $(OBJECT_STORAGE_ENDPOINT) -p output_bucket $(OBJECT_STORAGE_OUTPUT_BUCKET) -p access_key $(OBJECT_STORAGE_ACCESS_KEY) -p secret_key $(OBJECT_STORAGE_SECRET_KEY)

register_rule:
	ibmcloud fn rule create $(RULE_NAME) $(TRIGGER_NAME) $(GENERATOR_ACTION_NAME)