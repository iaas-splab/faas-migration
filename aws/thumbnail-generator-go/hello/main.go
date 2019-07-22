package main

import (
	"context"
	"crypto/sha256"
	"encoding/base64"
	"encoding/hex"
	"encoding/json"
	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
)

// Response is of type APIGatewayProxyResponse since we're leveraging the
// AWS Lambda Proxy Request functionality (default behavior)
//
// https://serverless.com/framework/docs/providers/aws/events/apigateway/#lambda-proxy-integration
type Response 

// Handler is our lambda handler invoked by the `lambda.Start` function call
func Handler(ctx context.Context, bb events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	data := []byte( bb.Body)

	h := sha256.New()
	h.Write(data)
	hash := h.Sum(nil)

	hashstr := hex.EncodeToString(hash)

	body := map[string]interface{}{
		"hash":           hashstr,
		"content-length": len(data),
		"content":        base64.StdEncoding.EncodeToString(data),
	}

	bodyJson, err := json.Marshal(body)
	if err != nil {
		return Response{StatusCode: 404}, err
	}

	resp := Response{
		StatusCode:      200,
		IsBase64Encoded: false,
		Body:            string(bodyJson),
		Headers: map[string]string{
			"Content-Type":           "application/json",
			"X-MyCompany-Func-Reply": "hello-handler",
		},
	}

	return resp, nil
}

func main() {
	lambda.Start(Handler)
}
