
package com.InitialSetup;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.TypeAdapter;
import com.google.gson.internal.LinkedTreeMap;
import com.google.gson.stream.JsonReader;
import com.google.gson.stream.JsonToken;
import com.google.gson.stream.JsonWriter;
import com.Utilities.Log;

public class Util {

	public List<Map<String, Object>> keepsIntsAsIs(String json) {
		//String json = "[{\"id\":1,\"quantity\":2,\"name\":\"apple\"},{\"id\":3,\"quantity\":4,\"name\":\"orange\"}]";
		CustomizedObjectTypeAdapter adapter = new CustomizedObjectTypeAdapter();
		Gson gson = new GsonBuilder().registerTypeAdapter(Map.class, adapter).registerTypeAdapter(List.class, adapter)
				.create();
        
		List<Map<String, Object>> l = gson.fromJson(json, List.class);
		for (Map<String, Object> item : l) {
//			Log.info(item.toString());
		}
		return l;
	}

	class CustomizedObjectTypeAdapter extends TypeAdapter<Object> {

		private final TypeAdapter<Object> delegate = new Gson().getAdapter(Object.class);

		@Override
		public void write(JsonWriter out, Object value) throws IOException {
			delegate.write(out, value);
		}

		@Override
		public Object read(JsonReader in) throws IOException {
			JsonToken token = in.peek();
			switch (token) {
			case BEGIN_ARRAY:
				List<Object> list = new ArrayList<Object>();
				in.beginArray();
				while (in.hasNext()) {
					list.add(read(in));
				}
				in.endArray();
				return list;

			case BEGIN_OBJECT:
				Map<String, Object> map = new LinkedTreeMap<String, Object>();
				in.beginObject();
				while (in.hasNext()) {
					map.put(in.nextName(), read(in));
				}
				in.endObject();
				return map;

			case STRING:
				return in.nextString();

			case NUMBER:
				// return in.nextDouble();
				String n = in.nextString();
				if (n.indexOf('.') != -1) {
					return Double.parseDouble(n)
;
				}
				return Long.parseLong(n)
;

			case BOOLEAN:
				return in.nextBoolean();

			case NULL:
				in.nextNull();
				return null;

			default:
				throw new IllegalStateException();
			}
		}
	}

	/*public static void main(String[] args) {
		Util util = new Util();
	//	util.keepsIntsAsIs();
	}*/
}
