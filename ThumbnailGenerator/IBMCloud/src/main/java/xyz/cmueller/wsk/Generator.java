package xyz.cmueller.wsk;

import com.google.gson.JsonObject;

public class Generator {
    public static JsonObject main(JsonObject args) {
        System.out.println(args.toString());
        JsonObject response = new JsonObject();
        response.addProperty("greetings", "Hello! Welcome to OpenWhisk");
        return response;
    }
}
