package xyz.cmueller.serverless;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.PrintStream;
import java.util.HashMap;
import java.util.Map;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.amazonaws.services.lambda.runtime.events.S3Event;
import com.amazonaws.services.s3.AmazonS3Client;
import com.amazonaws.services.s3.event.S3EventNotification;
import com.amazonaws.services.s3.model.ObjectMetadata;
import com.amazonaws.services.s3.model.PutObjectResult;
import com.amazonaws.services.s3.model.S3Object;
import com.amazonaws.util.IOUtils;
import com.fasterxml.jackson.databind.ObjectMapper;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

import static xyz.cmueller.serverless.Config.THUMBNAIL_BUCKET;
import static xyz.cmueller.serverless.Config.WEBHOOK_URL;

@SuppressWarnings("unused")
public class ThumbnailGenerationHandler implements RequestHandler<S3Event, Void> {
    private static final Logger LOG = LogManager.getLogger(ThumbnailGenerationHandler.class);

    private ObjectMapper mapper = new ObjectMapper();
    private AmazonS3Client client = new AmazonS3Client();

    @Override
    public Void handleRequest(S3Event input, Context context) {
        StringBuilder bout = new StringBuilder();

        String resultMessage = null;

        for (S3EventNotification.S3EventNotificationRecord record : input.getRecords()) {
            LOG.info("Loading {}/{}", record.getS3().getBucket().getName(), record.getS3().getObject().getKey());
            InputStream in = null;
            try {
                S3Object obj = client.getObject(record.getS3().getBucket().getName(), record.getS3().getObject().getKey());
                in = obj.getObjectContent();

                byte[] convertedImage = Converter.createThumbnail(IOUtils.toByteArray(in));

                ByteArrayInputStream tin = new ByteArrayInputStream(convertedImage);

                PutObjectResult response = client.putObject(THUMBNAIL_BUCKET, record.getS3().getObject().getKey(), tin, new ObjectMetadata());

                bout.append("Processed Image: `").append(record.getS3().getObject().getKey()).append("`\n");
            } catch (Exception e) {
                e.printStackTrace();
            } finally {
                try {
                    in.close();
                } catch (IOException e) {

                }
            }
            LOG.info("Done!");
        }
        return null;
    }
}
