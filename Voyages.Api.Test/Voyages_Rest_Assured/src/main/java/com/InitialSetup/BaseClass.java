package com.InitialSetup;

import static io.restassured.RestAssured.given;
import java.util.Random; 

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.lang.reflect.Method;
import java.text.Collator;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Comparator;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Properties;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import io.restassured.http.Header;
import io.restassured.http.Headers;
import org.apache.commons.codec.binary.Base64;
import org.apache.commons.collections.BeanMap;
import org.apache.commons.configuration2.PropertiesConfiguration;
import org.apache.commons.configuration2.PropertiesConfigurationLayout;
import org.apache.commons.configuration2.ex.ConfigurationException;
import org.apache.commons.io.output.ByteArrayOutputStream;
import org.apache.commons.lang.RandomStringUtils;
import org.apache.poi.ss.usermodel.Workbook;
import org.apache.poi.xssf.usermodel.XSSFRow;
import org.apache.poi.xssf.usermodel.XSSFSheet;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import org.json.simple.JSONArray;
import org.json.simple.JSONObject;
import org.json.simple.parser.JSONParser;
import org.json.simple.parser.ParseException;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.ie.InternetExplorerDriver;
import org.openqa.selenium.remote.DesiredCapabilities;
import org.testng.Assert;
import org.testng.annotations.AfterClass;
import org.testng.annotations.BeforeClass;
import org.testng.annotations.BeforeMethod;
import org.testng.annotations.Parameters;

import com.Reporting.ExtentTestManager;
//import com.Reporting.Extent_Reporting;
import com.TestData.Excel_Handling;
import com.Utilities.Assertion;
import com.Utilities.Constants;
import com.sun.mail.handlers.image_jpeg;
//import com.github.javafaker.Faker;
import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import edu.emory.mathcs.backport.java.util.Collections;
import io.restassured.RestAssured;
import io.restassured.path.json.JsonPath;
import io.restassured.response.Response;
import net.bytebuddy.asm.Advice.This;
//import xyz.trillian.hammurabi.report.HammurabiReport;
//import xyz.trillian.hammurabi.report.HammurabiTestId;

public class BaseClass {

	public WebDriver driver;
	public WebDriver ApiTest;
	public static String testCaseName = null;
	public String driverPath = Constants.drivePath;
	public String filePath = Constants.propertiesFile + File.separator + "Config.properties";
	public static String tcID = null;
	public static Properties prop;
	public static Map<String, Object> map = new HashMap<String, Object>();
	public static String logintoken;
	public static String forgotpwdtoken;

	@BeforeMethod
	public void getTestData(Method method) {
		try {
			FileInputStream fis = new FileInputStream(new File(Constants.datasheetPath + "Datasheet.xlsx"));
			Workbook workbook = new XSSFWorkbook(fis);
			System.out.println("Class : " + method.getDeclaringClass().getSimpleName() + " --------- Method : "
					+ method.getName());
			Excel_Handling excel = new Excel_Handling(Constants.datasheetPath + "Datasheet.xlsx",
					method.getDeclaringClass().getSimpleName());
			excel.getExcelDataAll(method.getDeclaringClass().getSimpleName(), "Execute", "*", "testCaseName");
			System.out.println("Class : " + method.getDeclaringClass().getSimpleName() + " --------- Method : "
					+ method.getName());
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	/*
	 * public void getTestData() throws IOException {
	 * 
	 * prop = new Properties(); // hammurabhiProp = new Properties(); // InputStream
	 * stream = new FileInputStream(propertypath); // hammurabhiProp.load(stream); }
	 */

	public long getLongId(String name) throws IOException {
		InputStream inputStream = new FileInputStream(filePath);
		prop.load(inputStream);
		long id = Long.parseLong(prop.getProperty(name));
		inputStream.close();
		return id;

	}

	public String getProperty(String name) throws Exception {
		/*
		 * InputStream inputStream = new FileInputStream(filePath);
		 * prop.load(inputStream); String names = prop.getProperty(name);
		 * inputStream.close();
		 */

		File file = new File(filePath);
		PropertiesConfiguration config = new PropertiesConfiguration();
		PropertiesConfigurationLayout layout = new PropertiesConfigurationLayout();
		layout.load(config, new InputStreamReader(new FileInputStream(file)));
		String names = (String) config.getProperty(name);
		return names;

	}

	public void setProperty(String name, Object value) throws IOException, ConfigurationException {
		File file = new File(filePath);
		PropertiesConfiguration config = new PropertiesConfiguration();
		PropertiesConfigurationLayout layout = new PropertiesConfigurationLayout();
		layout.load(config, new InputStreamReader(new FileInputStream(file)));
		config.setProperty(name, value);
		layout.save(config, new FileWriter(filePath, false));

	}

	// Set Body content
	public static void setBody(String key, Object value) {
		String strValue = String.valueOf(value);
//		key = "\"" + key + "\"";
//		strValue = "\"" + strValue + "\"";
		map.put(key, strValue);
	}

	// Set Body content as an object
	public static void setObjectBody(String parentKey, String key, Object value) {
		if (map.containsKey(parentKey)) {
			HashMap<String, String> temp = (HashMap<String, String>) map.get(parentKey);
			temp.put(key, String.valueOf(value));
			map.remove(parentKey);
			map.put(parentKey, temp);
		} else {
			Map<String, String> parent = new HashMap<String, String>();
			if (!key.isEmpty()) {
				parent.put(key, String.valueOf(value));
			}
			map.put(parentKey, parent);
		}
	}

	// Set Body content as an object
	public static void setArrayOfObjectBody(String parentKey, String key, Object value) {
		if (map.containsKey(parentKey)) {
			System.out.println(map.get(parentKey).getClass().getSimpleName());
			ArrayList list = new ArrayList<>(Arrays.asList(map.get(parentKey)));
			List<List<String>> list1 = (List<List<String>>) list.get(0);
			HashMap<String, String> temp = (HashMap<String, String>) (list1.get(0));
			temp.put(key, String.valueOf(value));
			map.remove(parentKey);
			map.put(parentKey, Arrays.asList(temp));
		} else {
			Map<String, String> parent = new HashMap<String, String>();
			parent.put(key, String.valueOf(value));
			map.put(parentKey, Arrays.asList(parent));
		}
	}

	// Get the constructed body
	public Map<String, Object> getBody() {
		Map<String, Object> tempmap = new HashMap<String, Object>();
		tempmap.putAll(map);
		map.clear();
		return tempmap;
	}

	public String getRandomName() {
		String generatedString = RandomStringUtils.randomAlphabetic(10);
		return generatedString;
	}
	
	public int getRandomNumber() {
		Random randomnumber=new Random();
		//String generatedString=RandomStringUtils.randomNumeric(randomnumber.nextInt(2));
		int generatedint=randomnumber.nextInt(99);
		return generatedint;	
	}

	/*
	 * public String getHammurabiKeys(String keys) throws IOException { InputStream
	 * stream = new FileInputStream(propertypath); hammurabhiProp.load(stream);
	 * String hammurabikeys = hammurabhiProp.getProperty(keys); stream.close();
	 * return hammurabikeys;
	 * 
	 * }
	 * 
	 */
	

	public Object getId(Object name) throws IOException {
		InputStream inputStream = new FileInputStream(filePath);
		prop.load(inputStream);
		// int id = prop.getProperty(name);
		inputStream.close();
		return prop.getProperty(String.valueOf(name));

	}

	public void setID(Object name, int value) throws IOException, ConfigurationException {
		File file = new File(filePath);
		PropertiesConfiguration config = new PropertiesConfiguration();
		PropertiesConfigurationLayout layout = new PropertiesConfigurationLayout();
		layout.load(config, new InputStreamReader(new FileInputStream(file)));
		config.setProperty(String.valueOf(name), value);
		layout.save(config, new FileWriter(filePath, false));

	}

	public Long getAutoGeneratedID(String response) {
		JsonPath js = new JsonPath(response);
		Long getID = (long) js.getInt("id");
		ExtentTestManager.startlog("", tcID, "ID Generated as: " + getID);
		return getID;
	}

	public void setDriver(String browserType, String HostURL) throws InterruptedException {
		// switch(browserType){
		//
		// case "IE":
		// driver = initIEDriver(HostURL);
		// break;
		//
		// case "CHROME":
		// driver = initChromeDriver(HostURL);
		// break;
		//
		// case "RESTAPI":
		// driver = restApi(HostURL);
		//
		//
		// default :
		// System.out.println("you have enetered as invalid browser");
		// }
		driver = restApi(HostURL);

	}

	public WebDriver initChromeDriver(String appURL) throws InterruptedException {
		System.out.println("Launching google chrome driver!!! .");
		System.setProperty("webdriver.chrome.driver", driverPath + "chromedriver.exe");
		driver = new ChromeDriver();
		driver.manage().window().maximize();
		Thread.sleep(5000);
		driver.navigate().to(appURL);
		driver.manage().deleteAllCookies();
		return driver;
	}

	public WebDriver initIEDriver(String appURL) {
		System.out.println("Launching google IE driver!!! .");
		System.setProperty("webdriver.ie.driver", driverPath + "IEDriverServer.exe");

		DesiredCapabilities cap = DesiredCapabilities.internetExplorer();
		cap.setCapability(InternetExplorerDriver.INTRODUCE_FLAKINESS_BY_IGNORING_SECURITY_DOMAINS, true);
		cap.setCapability("nativeEvents", false);
		cap.setCapability("ignoreProctedModeSettings", true);
		cap.setCapability("disable-popup-blocking", true);

		driver = new InternetExplorerDriver(cap);
		driver.manage().window().maximize();
		driver.navigate().to(appURL);
		driver.manage().deleteAllCookies();
		return driver;

	}

	public WebDriver restApi(String HostURL) {
		return driver;
	}

	/*
	 * @Parameters({ "serviceType", "appURL", "tcID" })
	 * 
	 * @BeforeClass public void initializeTestBaseSetup(String browserType, String
	 * appURL, String tcID) { try { setDriver(browserType.toUpperCase(), appURL);
	 * testCaseName = tcID;
	 * 
	 * } catch (Exception e) { System.out.println("Error....." + e.getStackTrace());
	 * } }
	 */

	public String toString(JSONObject json) {
		return json.toString();
	}

	public void getBaseUrl(String testCaseID, String stepName) throws Exception {
		String BaseURL = Excel_Handling.Get_Data(testCaseName, "HostURL").trim();
		//if ser need to run both env
		//String BaseURL = Excel_Handling.getProperty("Target Host").trim();
		ExtentTestManager.startlog(testCaseID, stepName, "Get Host Url");
		RestAssured.baseURI = BaseURL;
		ExtentTestManager.pass(testCaseID, stepName, "Host Url Retrieve");
	}

	public String getResourceUrl(String testCaseID, String stepName) throws InterruptedException, Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "Get resourceURL Url");
		String resourceURL = Excel_Handling.Get_Data(testCaseName, "ResourceURL").trim();
		ExtentTestManager.pass(testCaseID, stepName, "resourceURL Url is: " + resourceURL);
		return resourceURL;
	}

	public void verifyResponseCode(Response response, int expectedStatusCode, String message) {
		Assertion.assertEquals(response.getStatusCode(), expectedStatusCode, message);
		ExtentTestManager.report(testCaseName, "Verifying status",
				"Actual Response code is :" + response.getStatusCode() + " expected code is: " + expectedStatusCode);
	}

	/*
	 * public boolean verify_ApplicationId_Response_Body(String testCaseID, String
	 * stepName,ApplicationIDDTO applicationIDDTO) throws Throwable { try { long
	 * AppId = js.getLong(object.appIDHeader); String AppName =
	 * js.getString(object.appNameHeader); if (applicationIDDTO == null) { boolean
	 * allNull = (AppId == 0 && AppName == null); if (allNull) {
	 * Extent_Reporting.Log_Pass(testCaseID, stepName,
	 * "Actual Application Response ID is : " + AppId +
	 * "Expected Application Response ID: " ); Extent_Reporting.Log_Pass(testCaseID,
	 * stepName, "Actual Application name is : " + AppName +
	 * "Expected Application Response ID: "); return true; } else {
	 * Extent_Reporting.Log_Fail(testCaseID, stepName,
	 * "Was expecint application id to be null but was " + AppId +
	 * " and application name to be null but was " + AppName); return false; } }
	 * else { if (AppId == applicationIDDTO.getAppId()) {
	 * 
	 * Extent_Reporting.Log_Pass(testCaseID, stepName,
	 * "Actual Application Response ID is : " + AppId +
	 * "Expected Application Response ID: " + applicationIDDTO.getAppId()); return
	 * true; } else { Extent_Reporting.Log_Fail(testCaseID, stepName,
	 * "Was expecint application id to be null but was " + AppId +
	 * "Expected Application Response ID: " + applicationIDDTO.getAppId()); return
	 * false; } } }catch(Exception e) { Extent_Reporting.Log_Fail(testCaseID,
	 * stepName,"Response body", e); return false; } }
	 */

	/*
	 * public List<Set<Map.Entry<String, JsonElement>>> createMaps(Response
	 * response) throws FileNotFoundException { List<Set<Map.Entry<String,
	 * JsonElement>>> list = new ArrayList<>(); JsonPath jsonPath = new
	 * JsonPath(response.asString()); String json = response.asString(); JsonParser
	 * parser = new JsonParser(); JsonElement element = parser.parse(json);
	 * JsonObject obj = element.getAsJsonObject(); Set<Map.Entry<String,
	 * JsonElement>> response_entries = obj.entrySet(); List list = new
	 * ArrayList<>(response_entries); Gson gson = new Gson(); Object object =
	 * gson.fromJson(new FileReader(Constants.datasheetPath +"//" +
	 * Excel_Handling.Get_Data(testCaseName, "testCaseName") + ".json"),
	 * Object.class); String str = new Gson().toJson(object); parser = new
	 * JsonParser(); element = parser.parse(str); obj = element.getAsJsonObject();
	 * Set<Map.Entry<String, JsonElement>> json_entries = obj.entrySet(); //List
	 * list = new ArrayList<>(json_entries); return List<Set<Map.Entry<String,
	 * JsonElement>>>; }
	 */

	public void verifyResponseBodyContent(Object response) throws FileNotFoundException {
		if (response instanceof String) {
			System.out.println("String value");
			JsonPath jsonPath = new JsonPath(response.toString());
			String json = response.toString();
			JsonParser parser = new JsonParser();
			JsonElement element = parser.parse(json);
			JsonObject obj = element.getAsJsonObject();
			Set<Map.Entry<String, JsonElement>> response_entries = obj.entrySet();
			List list = new ArrayList<>(response_entries);
			Gson gson = new Gson();
			Object object = gson.fromJson(new FileReader(
					Constants.JsonFilePath + "//" + Excel_Handling.Get_Data(testCaseName, "testCaseName") + ".json"),
					Object.class);
			String str = new Gson().toJson(object);
			parser = new JsonParser();
			element = parser.parse(str);
//		obj = element.getAsJsonObject();
			Set<Map.Entry<String, JsonElement>> json_entries = obj.entrySet();
			// List list = new ArrayList<>(json_entries);
			int i = 0;
			for (Map.Entry<String, JsonElement> jsonentry : json_entries) {
				System.out.println("Current JSON Key" + jsonentry.getKey());
				String jsonkey = jsonentry.getKey();
				Map.Entry<String, JsonElement> data = (Entry<String, JsonElement>) list.get(0);
				String responsekey = data.getKey();
				System.out.println("main key ----------> " + data.getKey());
				System.out.println("Current Response Key" + responsekey);
				System.out.println("key---------------> " + responsekey);
				System.out.println("value of key--------------->" + jsonPath.get(jsonkey));
//			Assertion.assertNotNull(jsonPath.getString(jsonkey), "Check \"" + jsonkey + "\" exists in the response");
				Assertion.assertNotNull(jsonPath.get(jsonkey), "Check " + responsekey + " exists in the response");
				i++;
			}
		} else if (response instanceof JSONArray) {
			System.out.println("Total records: " + ((JSONArray) response).size());
//			for(int n = 0; n < 3/*((JSONArray)response).size()*/; n++) {
			JsonPath jsonPath = new JsonPath(((JSONArray) response).get(0).toString());
//				JsonPath jsonPath = new JsonPath(((JSONArray) response).get(i));
			String json = ((JSONArray) response).get(0).toString();
			JsonParser parser = new JsonParser();
			JsonElement element = parser.parse(json);
			JsonObject obj = element.getAsJsonObject();
			Set<Map.Entry<String, JsonElement>> response_entries = obj.entrySet();
			List list = new ArrayList<>(response_entries);
			Gson gson = new Gson();
			Object object = gson.fromJson(new FileReader(
					Constants.JsonFilePath + "//" + Excel_Handling.Get_Data(testCaseName, "testCaseName") + ".json"),
					Object.class);
			String str = new Gson().toJson(object);
			parser = new JsonParser();
			element = parser.parse(str);
//				obj = element.getAsJsonObject();
			Set<Map.Entry<String, JsonElement>> json_entries = obj.entrySet();
			// List list = new ArrayList<>(json_entries);
			int i = 0;
			for (Map.Entry<String, JsonElement> jsonentry : json_entries) {
				System.out.println("Current JSON Key" + jsonentry.getKey());
				String jsonkey = jsonentry.getKey();
				Map.Entry<String, JsonElement> data = (Entry<String, JsonElement>) list.get(0);
				String responsekey = data.getKey();
				System.out.println("main key ----------> " + data.getKey());
				System.out.println("Current Response Key" + responsekey);
				System.out.println("key---------------> " + responsekey);
				System.out.println("value of key--------------->" + jsonPath.get(jsonkey));
//					Assertion.assertNotNull(jsonPath.getString(jsonkey), "Check \"" + jsonkey + "\" exists in the response");
				Assertion.assertNotNull(jsonPath.get(jsonkey), "Check " + responsekey + " exists in the response");
				i++;
			}
		}
	}

	public void verifyResponseBodyContentSequence(Object response) throws FileNotFoundException {
		if (response instanceof String) {
			JsonPath jsonPath = new JsonPath(response.toString());
			String json = response.toString();
			JsonParser parser = new JsonParser();
			JsonElement element = parser.parse(json);
			JsonObject obj = element.getAsJsonObject();
			Set<Map.Entry<String, JsonElement>> response_entries = obj.entrySet();
			List list = new ArrayList<>(response_entries);
			Gson gson = new Gson();
			Object object = gson.fromJson(new FileReader(
					Constants.JsonFilePath + "//" + Excel_Handling.Get_Data(testCaseName, "testCaseName") + ".json"),
					Object.class);
			String str = new Gson().toJson(object);
			System.out.println("JSON content: " + str + "\n Response content: " + response);
			parser = new JsonParser();
			element = parser.parse(str);
//		obj = element.getAsJsonObject();
			Set<Map.Entry<String, JsonElement>> json_entries = obj.entrySet();
			// List list = new ArrayList<>(json_entries);
			int i = 0;
			for (Map.Entry<String, JsonElement> jsonentry : json_entries) {
				System.out.println("Current JSON Key" + jsonentry.getKey());
				String jsonkey = jsonentry.getKey();
				Map.Entry<String, JsonElement> data = (Entry<String, JsonElement>) list.get(i);
				String responsekey = data.getKey();
				System.out.println("main key ----------> " + data.getKey());
				System.out.println("Current Response Key" + responsekey);
				System.out.println("key---------------> " + responsekey);
				System.out.println("value of key--------------->" + jsonPath.get(jsonkey));
				// Assertion.assertNotNull(jsonPath.getString(jsonkey), "Check \"" + jsonkey +
				// "\" exists in the response");
				Assertion.assertEquals(jsonkey, responsekey, "Check expected position of \"" + responsekey + "\"");
				// System.out.println("value ---------->>>>" + (jsonPath.get(jsonkey)));
				// System.out.println("class ---------->>>>" +
				// (jsonPath.get(jsonkey)).getClass().getSimpleName() );
				// Assertion.assertEquals(jsonPath.get(jsonkey).getClass().getSimpleName(),
				// data.getValue().getClass().getSimpleName(), "Check datatype match \"" +
				// jsonPath.get(jsonkey) + "\" - " + "Expected: " +
				// jsonPath.getJsonObject(jsonkey).getClass().getSimpleName()+ " ---- Actual: "
				// + data.getValue().getClass().getSimpleName());
				i++;
			}
		} else if (response instanceof JSONArray) {
//			System.out.println("JSONARRAY value");
//			for(int n = 0; n < 3/*((JSONArray)response).size()*/; n++) {
			JsonPath jsonPath = new JsonPath(((JSONArray) response).get(0).toString());
//				JsonPath jsonPath = new JsonPath(((JSONArray) response).get(i));
			String json = ((JSONArray) response).get(0).toString();
			JsonParser parser = new JsonParser();
			JsonElement element = parser.parse(json);
			JsonObject obj = element.getAsJsonObject();
			Set<Map.Entry<String, JsonElement>> response_entries = obj.entrySet();
			List list = new ArrayList<>(response_entries);
			Gson gson = new Gson();
			Object object = gson.fromJson(new FileReader(
					Constants.JsonFilePath + "//" + Excel_Handling.Get_Data(testCaseName, "testCaseName") + ".json"),
					Object.class);
			String str = new Gson().toJson(object);
			System.out.println("JSON content: " + str + "\n Response content: " + response);
			parser = new JsonParser();
			element = parser.parse(str);
//			obj = element.getAsJsonObject();
			Set<Map.Entry<String, JsonElement>> json_entries = obj.entrySet();
			// List list = new ArrayList<>(json_entries);
			int i = 0;
			for (Map.Entry<String, JsonElement> jsonentry : json_entries) {
				System.out.println("Current JSON Key" + jsonentry.getKey());
				String jsonkey = jsonentry.getKey();
				Map.Entry<String, JsonElement> data = (Entry<String, JsonElement>) list.get(i);
				String responsekey = data.getKey();
				System.out.println("main key ----------> " + data.getKey());
				System.out.println("Current Response Key" + responsekey);
				System.out.println("key---------------> " + responsekey);
				System.out.println("value of key--------------->" + jsonPath.get(jsonkey));
				// Assertion.assertNotNull(jsonPath.getString(jsonkey), "Check \"" + jsonkey +
				// "\" exists in the response");
				Assertion.assertEquals(jsonkey, responsekey, "Check expected position of \"" + responsekey + "\"");
				// System.out.println("value ---------->>>>" + (jsonPath.get(jsonkey)));
				// System.out.println("class ---------->>>>" +
				// (jsonPath.get(jsonkey)).getClass().getSimpleName() );
				// Assertion.assertEquals(jsonPath.get(jsonkey).getClass().getSimpleName(),
				// data.getValue().getClass().getSimpleName(), "Check datatype match \"" +
				// jsonPath.get(jsonkey) + "\" - " + "Expected: " +
				// jsonPath.getJsonObject(jsonkey).getClass().getSimpleName()+ " ---- Actual: "
				// + data.getValue().getClass().getSimpleName());
				i++;
			}
		}
	}

	public void verifyResponseBodyContentDatatype(Object response) throws FileNotFoundException, Throwable {
		if (response instanceof String) {
			JsonPath responsePath = new JsonPath(response.toString());
			String json = response.toString();
			JsonParser parser = new JsonParser();
			JsonElement element = parser.parse(json);
			JsonObject obj = element.getAsJsonObject();
			Set<Map.Entry<String, JsonElement>> response_entries = obj.entrySet();
			List list = new ArrayList<>(response_entries);
			Gson gson = new Gson();
			Object object = gson.fromJson(new FileReader(
					Constants.JsonFilePath + "//" + Excel_Handling.Get_Data(testCaseName, "testCaseName") + ".json"),
					Object.class);
			String str = new Gson().toJson(object);
			System.out.println("From response --------- " + response);
			System.out.println("From JSON --------- " + str);
			parser = new JsonParser();
			element = parser.parse(str);
			// obj = element.getAsJsonObject();
			System.out.println("String is " + obj.toString());
			JsonPath jsonPath = new JsonPath(obj.toString());

			Set<Map.Entry<String, JsonElement>> json_entries = obj.entrySet();
			// List list = new ArrayList<>(json_entries);
			int i = 0;
			for (Map.Entry<String, JsonElement> jsonentry : json_entries) {
				// System.out.println("Current JSON Key" + jsonentry.getKey());
				String jsonkey = jsonentry.getKey();
				Map.Entry<String, JsonElement> responseentry = (Entry<String, JsonElement>) list.get(i);
				String responsekey = responseentry.getKey();
				// System.out.println("main key ----------> " + data.getKey());
				// System.out.println("Current Response Key" + responsekey);
				// System.out.println("key---------------> " + responsekey);
				// System.out.println("value json--------------->" + jsonPath.get(jsonkey));
				// Assertion.assertNotNull(jsonPath.getString(jsonkey), "Check \"" + jsonkey +
				// "\" exists in the response");
				// Assertion.assertEquals(jsonkey, responsekey, "Check expected position of " +
				// responsekey);
				// System.out.println("value response ---------->>>>" +
				// responsePath.get(responsekey));
				// System.out.println("json ---------->>>>" +
				// jsonPath.get(jsonkey).getClass().getSimpleName() );
				// System.out.println("response ---------->>>>" +
				// responsePath.get(responsekey).getClass().getSimpleName());
//			System.out.println(jsonkey + " = " + jsonPath.get(jsonkey) + " &&&&&&& " + responsekey + " = " + responsePath.get(jsonkey));
				System.out.println("-----test key----- " + jsonPath.get(jsonkey));
				if (jsonPath.get(jsonkey) instanceof Long || jsonPath.get(jsonkey) instanceof Integer
						|| jsonPath.get(jsonkey) instanceof String || jsonPath.get(jsonkey) instanceof Double
						|| jsonPath.get(jsonkey) instanceof Float || jsonPath.get(jsonkey) instanceof Boolean) {
					Assertion.assertTrue(
							responsePath.get(jsonkey) instanceof Long || responsePath.get(jsonkey) instanceof Integer
									|| responsePath.get(jsonkey) instanceof String
									|| responsePath.get(jsonkey) instanceof Double
									|| responsePath.get(jsonkey) instanceof Boolean
									|| responsePath.get(jsonkey) instanceof Float,
							"Check datatype match for \"" + jsonkey + "\"   -   "
									+ "Expected: Number ----  Actual: Number");
				} else {
					if (response.toString().contains("message")) {
						ExtentTestManager.pass(testCaseName, response.toString(), "Error Message");

					} else if (jsonPath.get(jsonkey) instanceof HashMap) {
						System.out.println("-----key----- " + jsonPath.get(jsonkey));
						System.out
								.println("actual " + responsePath.get(jsonkey) + " Expected " + jsonPath.get(jsonkey));
						Assertion.assertEquals(jsonPath.get(jsonkey).getClass().getSimpleName(),
								responsePath.get(jsonkey).getClass().getSimpleName(),
								"Check datatype match for \"" + jsonkey + "\"   -   " + "Expected: "
										+ jsonPath.get(jsonkey).getClass().getSimpleName() + " ----  Actual: "
										+ responsePath.get(jsonkey).getClass().getSimpleName());
					} else if (jsonPath.get(jsonkey) == null) {
						System.out.println("-----key----- " + jsonPath.get(jsonkey));
						System.out
								.println("actual " + responsePath.get(jsonkey) + " Expected " + jsonPath.get(jsonkey));
						Assertion.assertEquals(jsonPath.get(jsonkey), responsePath.get(jsonkey),
								"Check datatype match for \"" + jsonkey + "\"   -   " + "Expected: "
										+ jsonPath.get(jsonkey) + " ----  Actual: " + responsePath.get(jsonkey));
					} else {
						String expected = jsonPath.get(jsonkey);
						long expectedLong = 0;
						Pattern pattern = Pattern.compile("\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}.\\d{3}Z");
						Matcher matcher = pattern.matcher(expected);
						Boolean b = true;
						if (matcher.matches()) {
							SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", Locale.US);
//					SimpleDateFormat new1 = new SimpleDateFormat("yyyy-MM-dd");
//						format.setTimeZone(TimeZone.getTimeZone("UTC"));
//						String str1 = "2019-08-19T07:02:47.724Z";
							Date date = format.parse(expected);
							expectedLong = date.getTime();
							b = true;
//						System.out.println(epoch); // 1055545912454
//						System.out.println(new1.format(date));
						}
						System.out.println("actual " + responsePath.get(jsonkey) + " Expected " + expected);
						Assertion.assertEquals(((b) ? expectedLong : expected).getClass().getSimpleName(),
								responsePath.get(jsonkey).getClass().getSimpleName(),
								"Check datatype match for \"" + jsonkey + "\"   -   " + "Expected: "
										+ ((b) ? expectedLong : expected).getClass().getSimpleName() + " ----  Actual: "
										+ responsePath.get(jsonkey).getClass().getSimpleName());
					}
					// Boolean numeric =
					// jsonPath.get(jsonkey).toString().matches("-?\\d+(\\.\\d+)?");
					// Assertion.assertEquals(jsonPath.get(jsonkey).getClass().getSimpleName(),
					// responsePath.get(responsekey).getClass().getSimpleName(), "Check datatype
					// match \"" + jsonPath.get(jsonkey) + "\" - " + "Expected: " +
					// jsonPath.get(jsonkey).getClass().getSimpleName()+ " ---- Actual: " +
					// responsePath.get(responsekey).getClass().getSimpleName());
					i++;
				}
			}
		} else if (response instanceof JSONArray) {
//			System.out.println("JSONARRAY value");
//			for(int n = 0; n < 3/*((JSONArray)response).size()*/; n++) {
//				JsonPath responsePath = new JsonPath(response.toString());
//				String json = response.toString();
			JsonPath responsePath = new JsonPath(((JSONArray) response).get(0).toString());
//				JsonPath jsonPath = new JsonPath(((JSONArray) response).get(i));
			String json = ((JSONArray) response).get(0).toString();
			JsonParser parser = new JsonParser();
			JsonElement element = parser.parse(json);
			JsonObject obj = element.getAsJsonObject();
			Set<Map.Entry<String, JsonElement>> response_entries = obj.entrySet();
			List list = new ArrayList<>(response_entries);
			Gson gson = new Gson();
			Object object = gson.fromJson(new FileReader(
					Constants.JsonFilePath + "//" + Excel_Handling.Get_Data(testCaseName, "testCaseName") + ".json"),
					Object.class);
			String str = new Gson().toJson(object);
			System.out.println("From response --------- " + response);
			System.out.println("From JSON --------- " + str);
			parser = new JsonParser();
			element = parser.parse(str);
			// obj = element.getAsJsonObject();
			System.out.println("String is " + obj.toString());
			JsonPath jsonPath = new JsonPath(obj.toString());

			Set<Map.Entry<String, JsonElement>> json_entries = obj.entrySet();
			// List list = new ArrayList<>(json_entries);
			int i = 0;
			for (Map.Entry<String, JsonElement> jsonentry : json_entries) {
				// System.out.println("Current JSON Key" + jsonentry.getKey());
				String jsonkey = jsonentry.getKey();
				Map.Entry<String, JsonElement> responseentry = (Entry<String, JsonElement>) list.get(i);
				String responsekey = responseentry.getKey();
				// System.out.println("main key ----------> " + data.getKey());
				// System.out.println("Current Response Key" + responsekey);
				// System.out.println("key---------------> " + responsekey);
				// System.out.println("value json--------------->" + jsonPath.get(jsonkey));
				// Assertion.assertNotNull(jsonPath.getString(jsonkey), "Check \"" + jsonkey +
				// "\" exists in the response");
				// Assertion.assertEquals(jsonkey, responsekey, "Check expected position of " +
				// responsekey);
				// System.out.println("value response ---------->>>>" +
				// responsePath.get(responsekey));
				// System.out.println("json ---------->>>>" +
				// jsonPath.get(jsonkey).getClass().getSimpleName() );
				// System.out.println("response ---------->>>>" +
				// responsePath.get(responsekey).getClass().getSimpleName());
//				System.out.println(jsonkey + " = " + jsonPath.get(jsonkey) + " &&&&&&& " + responsekey + " = " + responsePath.get(jsonkey));
				System.out.println("-----test key----- " + jsonPath.get(jsonkey));
				if (jsonPath.get(jsonkey) instanceof Long || jsonPath.get(jsonkey) instanceof Integer
						|| jsonPath.get(jsonkey) instanceof String || jsonPath.get(jsonkey) instanceof Double
						|| jsonPath.get(jsonkey) instanceof Float || jsonPath.get(jsonkey) instanceof Boolean) {
					Assertion.assertTrue(
							responsePath.get(jsonkey) instanceof Long || responsePath.get(jsonkey) instanceof Integer
									|| responsePath.get(jsonkey) instanceof String
									|| responsePath.get(jsonkey) instanceof Double
									|| responsePath.get(jsonkey) instanceof Boolean
									|| responsePath.get(jsonkey) instanceof Float,
							"Check datatype match for \"" + jsonkey + "\"   -   "
									+ "Expected: Number ----  Actual: Number");
				} else {
					if (response.toString().contains("message")) {
						ExtentTestManager.pass(testCaseName, response.toString(), "Error Message");

					} else if (jsonPath.get(jsonkey) instanceof HashMap) {
						System.out.println("-----key----- " + jsonPath.get(jsonkey));
						System.out
								.println("actual " + responsePath.get(jsonkey) + " Expected " + jsonPath.get(jsonkey));
						Assertion.assertEquals(jsonPath.get(jsonkey).getClass().getSimpleName(),
								responsePath.get(jsonkey).getClass().getSimpleName(),
								"Check datatype match for \"" + jsonkey + "\"   -   " + "Expected: "
										+ jsonPath.get(jsonkey).getClass().getSimpleName() + " ----  Actual: "
										+ responsePath.get(jsonkey).getClass().getSimpleName());
					} else if (jsonPath.get(jsonkey) == null) {
						System.out.println("-----key----- " + jsonPath.get(jsonkey));
						System.out
								.println("actual " + responsePath.get(jsonkey) + " Expected " + jsonPath.get(jsonkey));
						Assertion.assertEquals(jsonPath.get(jsonkey), responsePath.get(jsonkey),
								"Check datatype match for \"" + jsonkey + "\"   -   " + "Expected: "
										+ jsonPath.get(jsonkey) + " ----  Actual: " + responsePath.get(jsonkey));
					} else {
						String expected = jsonPath.get(jsonkey);
						long expectedLong = 0;
						Pattern pattern = Pattern.compile("\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}.\\d{3}Z");
						Matcher matcher = pattern.matcher(expected);
						Boolean b = true;
						if (matcher.matches()) {
							SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", Locale.US);
//						SimpleDateFormat new1 = new SimpleDateFormat("yyyy-MM-dd");
//							format.setTimeZone(TimeZone.getTimeZone("UTC"));
//							String str1 = "2019-08-19T07:02:47.724Z";
							Date date = format.parse(expected);
							expectedLong = date.getTime();
							b = true;
//							System.out.println(epoch); // 1055545912454
//							System.out.println(new1.format(date));
						}
						System.out.println("actual " + responsePath.get(jsonkey) + " Expected " + expected);
						Assertion.assertEquals(((b) ? expectedLong : expected).getClass().getSimpleName(),
								responsePath.get(jsonkey).getClass().getSimpleName(),
								"Check datatype match for \"" + jsonkey + "\"   -   " + "Expected: "
										+ ((b) ? expectedLong : expected).getClass().getSimpleName() + " ----  Actual: "
										+ responsePath.get(jsonkey).getClass().getSimpleName());
					}
					// Boolean numeric =
					// jsonPath.get(jsonkey).toString().matches("-?\\d+(\\.\\d+)?");
					// Assertion.assertEquals(jsonPath.get(jsonkey).getClass().getSimpleName(),
					// responsePath.get(responsekey).getClass().getSimpleName(), "Check datatype
					// match \"" + jsonPath.get(jsonkey) + "\" - " + "Expected: " +
					// jsonPath.get(jsonkey).getClass().getSimpleName()+ " ---- Actual: " +
					// responsePath.get(responsekey).getClass().getSimpleName());
					i++;
				}
			}
		}
	}

	public void verifyUpdateName(Response response, String expectedStatusCode, String message) {
		String convertAsStr = response.asString();
		JsonPath js = new JsonPath(convertAsStr);
		System.out.println("mapping--------------------->" + convertAsStr);
		Assertion.assertEquals(js.getString("name").trim(), expectedStatusCode, "Check name Updated");
		ExtentTestManager.report("Verify name", "Udpated", convertAsStr);
	}

	// ID delete operation
	public Response getID_Delete(String testCaseID, String stepName, String resourceUrl, Long id) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "trying to delete the ID");
		Response res = given().header("Content-Type", "application/json").when().delete(resourceUrl + id).then()
				.assertThat().extract().response();
		String convertAsStr = res.asString();
		JsonPath js = new JsonPath(convertAsStr);
		ExtentTestManager.report(testCaseID, "Delete operation", convertAsStr);
		return res;
	}

	public boolean verifyValidationErrorMessage(String testCaseID, String stepName, Response response,
			String expectedErrMsg) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "verying the blank validation");
		JsonPath js = new JsonPath(response.asString().trim());
		String ResPonseMsg = js.getString("message");
		System.out.println("Response@@@@@" + ResPonseMsg);
		if (expectedErrMsg.equalsIgnoreCase(ResPonseMsg)) {
			ExtentTestManager.pass(testCaseID, stepName,
					"Actual response is as: " + ResPonseMsg + " expected Error Message: " + expectedErrMsg);
			return true;

		} else {
			ExtentTestManager
					.fail("Actual response is as: " + ResPonseMsg + " expected Error Message: " + expectedErrMsg);
			return false;
		}
	}

	// Get application ID
	public Response getByID(String testCaseID, String stepName, String resourceUrl, Object id) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "trying to get the application ID");
		Response res = given().header("Content-Type", "application/json").when().get(resourceUrl + id).then().extract()
				.response();
		String convertAsStr = res.asString();
		ExtentTestManager.pass(testCaseID, stepName, "Reponse successful: \n" + convertAsStr);
		return res;
	}

	// Get actions - get by category1
	public Response getBySortCategory(String testCaseID, String stepName, String resourceUrl, String category,
			String order) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "trying to get the response");
		String str = category + "," + order;
		Response res = given().header("Content-Type", "application/json").param("sort", str).when().get(resourceUrl)
				.then().extract().response();
		String convertAsStr = res.asString();
		ExtentTestManager.pass(testCaseID, stepName, "Reponse successful: \n" + convertAsStr);
		return res;
	}

	// Get actions - get by category
	public Response getByCategory(String testCaseID, String stepName, String resourceUrl, String category,
			String categoryValue) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "trying to get the response");
		Response res = given().header("Content-Type", "application/json").param(category, categoryValue).when()
				.get(resourceUrl).then().extract().response();
		String convertAsStr = res.asString();
		ExtentTestManager.pass(testCaseID, stepName, "Reponse successful: \n" + convertAsStr);
		return res;
	}

	// Get application ID
	public Response getByGroupName(String testCaseID, String stepName, String resourceUrl, Object object)
			throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "trying to get the application ID");
		Response res = given().header("Content-Type", "application/json").when().get(resourceUrl + object).then()
				.extract().response();
		String convertAsStr = res.asString();
		ExtentTestManager.pass(testCaseID, stepName, "Reponse successful: \n" + convertAsStr);
		return res;
	}
	/*
	 * //Get actions - get by category public Response getBySampleCreatedBy(String
	 * testCaseID, String stepName, String resourceUrl, String createdBy) throws
	 * Exception { ExtentTestManager.startlog(testCaseID, stepName,
	 * "trying to get the application ID"); Response res =
	 * given().header("Content-Type", "application/json").param("createdBy",
	 * createdBy).when().get(resourceUrl).then().extract() .response(); String
	 * convertAsStr = res.asString(); JsonPath js = new JsonPath(convertAsStr);
	 * Extent_Reporting.Log_Pass(testCaseID, "getApplicationBy", convertAsStr);
	 * return res; }
	 */

	// TBU
	public List<String> getValuesInArray(Response responseString, String key) throws ParseException {
		JsonPath jsonPath = responseString.jsonPath();
		System.out.println(jsonPath.getList(key));
		List<String> list = jsonPath.getList(key);

		return list.stream() // Convert collection to Stream
				.map(String::toLowerCase) // Convert each element to upper case
				.collect(Collectors.toList()); // Collect results to a new list

	}

	// Get actions - get Array for the 3 verification steps
	public JSONObject getJSONArray(String responseString, String arrayName) throws ParseException {
		System.out.println("array name " + arrayName);
		JSONParser parse = new JSONParser();
		JSONObject jobj = (JSONObject) parse.parse(responseString);
		JSONArray jsonarr_1 = (JSONArray) jobj.get(arrayName);
		Assertion.assertTrue(jsonarr_1.size() > 0, "Check response is not empty");
		/*
		 * for (int i = 0; i < jsonarr_1.size(); i++) { JSONObject jsonobj_1 =
		 * (JSONObject) jsonarr_1.get(i); System.out.println(jsonobj_1.get(key));
		 * Assert.assertEquals(value, jsonobj_1.get(key)); }
		 */
		JSONObject jsonobj_1 = (JSONObject) jsonarr_1.get(0);
		return jsonobj_1;
	}

	public JSONObject getResponseArray(Response response) {
		JSONParser parse = new JSONParser();
		JSONArray jobj = null;
		try {
			jobj = (JSONArray) parse.parse(response.asString());
		} catch (ParseException e) {
		}
		JSONObject obj = (JSONObject) jobj.get(0);
		// System.out.println("Object " + obj);
		return obj;
	}

	// Get actions - check the key and value in all response JSON array objects
	public void checkValueInAllArray(String responseString, String arrayName, String key, String value)
			throws ParseException {
		System.out.println("array name " + arrayName);
		JSONParser parse = new JSONParser();
		JSONObject jobj = (JSONObject) parse.parse(responseString);
		JSONArray jsonarr_1 = (JSONArray) jobj.get(arrayName);
		Assertion.assertTrue(jsonarr_1.size() > 0, "Check response is not empty");
		for (int i = 0; i < jsonarr_1.size(); i++) {
			JSONObject jsonobj_1 = (JSONObject) jsonarr_1.get(i);
			System.out.println(jsonobj_1.get(key));

			Assert.assertEquals(value, String.valueOf(jsonobj_1.get(key)));
		}
	}

	// Get actions - check the key in atleast one JSON object response JSON array
	public void findValueInArray(String responseString, String arrayName, String key, String value)
			throws ParseException {
		JSONParser parse = new JSONParser();
		JSONObject jobj = (JSONObject) parse.parse(responseString);
		JSONArray jsonarr_1 = (JSONArray) jobj.get(arrayName);
		Boolean b = false;
		for (int i = 0; i < jsonarr_1.size(); i++) {
			JSONObject jsonobj_1 = (JSONObject) jsonarr_1.get(i);
			// System.out.println(jsonobj_1.get(key));
			// Assert.assertEquals(value, jsonobj_1.get(key));
			// System.out.println("Value: " + value + " ------ Key: " + jsonobj_1.get(key));
			if (value.equalsIgnoreCase(String.valueOf(jsonobj_1.get(key)))) {
				b = true;
				break;
			} else {
				b = false;
			}
		}
		Assert.assertTrue(b);
	}

	// Get actions - check the value of the key is unique throughout response JSON
	// array objects
	public void uniqueValueInArray(String responseString, String arrayName, String key, String value)
			throws ParseException {
		JSONParser parse = new JSONParser();
		JSONObject jobj = (JSONObject) parse.parse(responseString);
		JSONArray jsonarr_1 = (JSONArray) jobj.get(arrayName);
		int n = 0;
		for (int i = 0; i < jsonarr_1.size(); i++) {
			JSONObject jsonobj_1 = (JSONObject) jsonarr_1.get(i);

			if (value.equalsIgnoreCase(String.valueOf(jsonobj_1.get(key)))) {
				n++;
			}
		}
		Assertion.assertEquals(n, 1, "Expected number of records: 1 but Actual number of records: " + n);
		Assert.assertTrue(n == 1);
		// ExtentTestManager.report(testCaseName, "Count of unique id number is"+n,
		// "Unique Duplication Check iteration"+n);
		System.out.println("n ==== " + n);
	}

	// Get actions - count the occurance of the value of the key throughout response
	// JSON array objects
	public int countValueInArray(String responseString, String arrayName, String key, String value)
			throws ParseException {
		JSONParser parse = new JSONParser();
		JSONObject jobj = (JSONObject) parse.parse(responseString);
		JSONArray jsonarr_1 = (JSONArray) jobj.get(arrayName);
		int n = 0;
		System.out.println(jsonarr_1.size());
		for (int i = 0; i < jsonarr_1.size(); i++) {
			JSONObject jsonobj_1 = (JSONObject) jsonarr_1.get(i);
			if (value.equalsIgnoreCase(String.valueOf(jsonobj_1.get(key)))) {
				n++;
			}
		}
		return n;
	}

	// Get actions - count the occurance of the value of the key throughout response
	// JSON array objects
	public int countKeyInArray(String responseString, String arrayName, String key) throws ParseException {
		JSONParser parse = new JSONParser();
		JSONObject jobj = (JSONObject) parse.parse(responseString);
		JSONArray jsonarr_1 = (JSONArray) jobj.get(arrayName);
		int n = 0;
		System.out.println(jsonarr_1.size());
		for (int i = 0; i < jsonarr_1.size(); i++) {
			JSONObject jsonobj_1 = (JSONObject) jsonarr_1.get(i);
			ExtentTestManager.startlog(ExtentTestManager.testName, ExtentTestManager.stepName,
					"Check " + key + " exists in the object");
			Assert.assertNotNull(jsonobj_1.get(key));
			n++;
		}
		Assertion.assertTrue(n > 1, "Key occurance should be more than once");
		return n;
	}

	// Get actions - count the occurance of the value of the key throughout response
	// JSON array objects
	public JSONArray getObjectListArray(String responseString) throws ParseException {
		JSONParser parse = new JSONParser();
		JSONArray jobj = (JSONArray) parse.parse(responseString);
		/*
		 * int n = 0; System.out.println(jobj.size()); for (int i = 0; i < jobj.size();
		 * i++) { JSONObject jsonobj_1 = (JSONObject) jobj.get(i);
		 * System.out.println(jsonobj_1); }
		 */
		return jobj;
	}

//	
	public void isSortedByCategoryAsc(List list) {
		final Pattern p = Pattern.compile("^\\d+");
		Comparator<String> c = new Comparator<String>() {
			public int compare(String object1, String object2) {
				Matcher m = p.matcher(object1);
				Integer number1 = null;
				if (!m.find()) {
					return object1.compareTo(object2);
				} else {
					Integer number2 = null;
					number1 = Integer.parseInt(m.group());
					m = p.matcher(object2);
					if (!m.find()) {
						return object1.compareTo(object2);
					} else {
						number2 = Integer.parseInt(m.group());
						int comparison = number1.compareTo(number2);
						if (comparison != 0) {
							return comparison;
						} else {
							return object1.compareTo(object2);
						}
					}
				}
			}
		};
		List<String> compareList = new ArrayList<String>(list);
		if (list.get(0) instanceof String) {
			Collections.sort(compareList, c);
			System.out.println("------------- compareList : " + compareList);
			ExtentTestManager.report(testCaseName, "CompareList", "comapareListA" + compareList);

		} else if (list.get(0) instanceof Integer) {
			Collections.sort(compareList);
			System.out.println("------------- compareList : " + compareList);
			ExtentTestManager.report(testCaseName, "CompareList", "comapareListA" + compareList);

		}

		System.out.println("------------- List : " + list);
		ExtentTestManager.report(testCaseName, "CompareList", "listA" + list);

		for (int i = 0; i < list.size(); i++) {
			Assert.assertEquals(compareList.get(i), list.get(i));
		}
	}

	public void isSortedByCategoryDesc(List list) {
		final Pattern p = Pattern.compile("^\\d+");
		Comparator<String> c = new Comparator<String>() {
			public int compare(String object1, String object2) {
				Matcher m = p.matcher(object1);
				Integer number1 = null;
				if (!m.find()) {
					return object1.compareTo(object2);
				} else {
					Integer number2 = null;
					number1 = Integer.parseInt(m.group());
					m = p.matcher(object2);
					if (!m.find()) {
						return object1.compareTo(object2);
					} else {
						number2 = Integer.parseInt(m.group());
						int comparison = number1.compareTo(number2);
						if (comparison != 0) {
							return comparison;
						} else {
							return object1.compareTo(object2);
						}
					}
				}
			}
		};
		List<String> compareList = new ArrayList<String>(list);
		if (list.get(0) instanceof String) {
			Collections.sort(compareList, c.reversed());
			System.out.println("------------- compareList : " + compareList);
			ExtentTestManager.report(testCaseName, "CompareList", "comapareListD" + compareList);

		} else if (list.get(0) instanceof Integer) {
			Collections.sort(compareList);
			Collections.reverse(compareList);
			ExtentTestManager.report(testCaseName, "CompareList", "comapareListD" + compareList);
		}
		ExtentTestManager.report(testCaseName, "CompareList", "listD" + list);
		System.out.println("---------- list : " + list);

	}

	public int countKeyInArrayUniqueID(String responseString, String arrayName, String key) throws ParseException {
		JSONParser parse = new JSONParser();
		JSONObject jobj = (JSONObject) parse.parse(responseString);
		JSONArray jsonarr_1 = (JSONArray) jobj.get(arrayName);
		int n = 0;
		System.out.println(jsonarr_1.size());
		for (int i = 0; i < jsonarr_1.size(); i++) {
			JSONObject jsonobj_1 = (JSONObject) jsonarr_1.get(i);
			ExtentTestManager.startlog(ExtentTestManager.testName, ExtentTestManager.stepName,
					"Check " + key + " exists in the object");
			Assert.assertNotNull(jsonobj_1.get(key));
			n++;
		}
		Assertion.assertTrue(n == 1, "only one object should be present");
		return n;
	}

	public void getHeader(Response response) {
		Headers allHeaders = response.headers();
		// Iterate over all the Headers
		for (Header header : allHeaders) {
			System.out.println("Key: " + header.getName() + " Value: " + header.getValue());
		}
	}
	
}

