S3_BUCKET := s3_bucket_name
S3_ENDPOINT := s3.eu-de.cloud-object-storage.appdomain.cloud
S3_ACCESS_KEY := c88bce1b30884479949a09567dddce77
S3_SECRET_KEY := 5d25d0c7232c4837e93376a38f140f4f64ed3ca8ae2af7cb

S3_BUCKET_NAME_KEY := s3_bucket_name
S3_ENDPOINT_NAME_KEY := s3_endpoint
S3_ACCESS_KEY_NAME_KEY := s3_access_key
S3_SECRET_KEY_NAME_KEY := s3_secret_key

init:
	ibmcloud fn namespace create matrix-mul
	ibmcloud fn property set --namespace matrix-mul

build: clean
	dotnet restore
	dotnet publish -c Release -o out
	cd out/ && zip -r -0 matrix-mul.zip *

deploy_actions: build
	cd out && ibmcloud fn action create create-matrix matrix-mul.zip --main MatrixMul::MatrixMul.CreateMatrix::Main --kind dotnet:2.2 -p $(S3_BUCKET_NAME_KEY) $(S3_BUCKET) -p $(S3_ENDPOINT_NAME_KEY) $(S3_ENDPOINT) -p $(S3_ACCESS_KEY_NAME_KEY) $(S3_ACCESS_KEY) -p $(S3_SECRET_KEY_NAME_KEY) $(S3_SECRET_KEY) -a final true   
	cd out && ibmcloud fn action create serial-multiply matrix-mul.zip --main MatrixMul::MatrixMul.SerialMultiply::Main --kind dotnet:2.2
	cd out && ibmcloud fn action create distribute-work matrix-mul.zip --main MatrixMul::MatrixMul.DistributeWork::Main --kind dotnet:2.2  
	cd out && ibmcloud fn action create parallel-worker matrix-mul.zip --main MatrixMul::MatrixMul.ParallelWorker::Main --kind dotnet:2.2
	cd out && ibmcloud fn action create build-result matrix-mul.zip --main MatrixMul::MatrixMul.BuildResult::Main --kind dotnet:2.2
	cd out && ibmcloud fn action create generate-report matrix-mul.zip --main MatrixMul::MatrixMul.GenerateReport::Main --kind dotnet:2.2

update_actions: build
	cd out && ibmcloud fn action update create-matrix matrix-mul.zip --main MatrixMul::MatrixMul.CreateMatrix::Main --kind dotnet:2.2 -p $(S3_BUCKET_NAME_KEY) $(S3_BUCKET) -p $(S3_ENDPOINT_NAME_KEY) $(S3_ENDPOINT) -p $(S3_ACCESS_KEY_NAME_KEY) $(S3_ACCESS_KEY) -p $(S3_SECRET_KEY_NAME_KEY) $(S3_SECRET_KEY) -a final true 
	cd out && ibmcloud fn action update serial-multiply matrix-mul.zip --main MatrixMul::MatrixMul.SerialMultiply::Main --kind dotnet:2.2
	cd out && ibmcloud fn action update distribute-work matrix-mul.zip --main MatrixMul::MatrixMul.DistributeWork::Main --kind dotnet:2.2  
	cd out && ibmcloud fn action update parallel-worker matrix-mul.zip --main MatrixMul::MatrixMul.ParallelWorker::Main --kind dotnet:2.2
	cd out && ibmcloud fn action update build-result matrix-mul.zip --main MatrixMul::MatrixMul.BuildResult::Main --kind dotnet:2.2
	cd out && ibmcloud fn action update generate-report matrix-mul.zip --main MatrixMul::MatrixMul.GenerateReport::Main --kind dotnet:2.2

deploy_workflow:
	compose workflow.js > out/workflow.json
	deploy workflow out/workflow.json -w

deploy: deploy_actions deploy_workflow

update: update_actions deploy_workflow

invoke:
	ibmcloud fn action invoke workflow -P redis_credentials.json

destroy_actions:
	ibmcloud fn action delete create-matrix
	ibmcloud fn action delete serial-multiply
	ibmcloud fn action delete distribute-work
	ibmcloud fn action delete build-result
	ibmcloud fn action delete generate-report

clean:
	rm -rvf bin/
	rm -rvf out/
