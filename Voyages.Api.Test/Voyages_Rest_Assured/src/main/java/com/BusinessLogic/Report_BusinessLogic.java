package com.BusinessLogic;

import static io.restassured.RestAssured.given;

import org.openqa.selenium.WebDriver;

import com.InitialSetup.BaseClass;
import com.PageObject.Scorpio_PageObject;
import com.Reporting.ExtentTestManager;
import com.google.gson.Gson;

import io.restassured.path.json.JsonPath;
import io.restassured.response.Response;

public class Report_BusinessLogic extends BaseClass {
	public WebDriver driver;
	public  String testCaseName = "";
	public JsonPath js;
	public String convertAsStr;
	Scorpio_PageObject object = new Scorpio_PageObject(driver, testCaseName);
	public Response res;

	public Report_BusinessLogic(WebDriver driver, String testCaseName)
	{
		this.driver = driver; 
		this.testCaseName=testCaseName;
	}
	@SuppressWarnings("unused")
	
	

	public Response createVoyage(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "Create voyages");
		//setBody(object.voyageNumber, Excel_Handling.Get_Data(testCaseName, "voyageNumber"));
		setBody(object.voyageNumber,getRandomNumber());
		setBody(object.departurePort, getRandomName());
		setBody(object.departureTimezone, getRandomName());
		setBody(object.arrivalPort, getRandomName());
		setBody(object.arrivalTimezone, getRandomName());
//		setBody(object.actualEndOfSeaPassage, Excel_Handling.Get_Data(testCaseName, "actualEndOfSeaPassage"));
//		setBody(object.actualStartOfSeaPassage, Excel_Handling.Get_Data(testCaseName, "actualStartOfSeaPassage"));
		setBody(object.actualEndOfSeaPassage, "2020-01-14T07:41:36.154Z");
		setBody(object.actualStartOfSeaPassage, "2020-01-14T07:41:36.154Z");
		setBody(object.imoNumber,getRandomNumber());
		//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
//	public Response createPassageReportExclusion(String testCaseID,String stepName, String resourceUrl ) throws Exception {
//		ExtentTestManager.startlog(testCaseID, stepName, "update pool term");
//		setBody(object.voyagesId, getProperty("unitOfMeasureThresholdId"));
//		Gson gson = new Gson();
//		//com.google.gson.JsonObject object =  (com.google.gson.JsonObject) gson.fromJson(new FileReader(Constants.JsonFilePath + "//" + Excel_Handling.Get_Data(testCaseName, "testCaseName") +".json"),com.google.gson.JsonObject.class);	
//		//object.addProperty("termsTitle", getRandomName());
//		System.out.println("tostring:::::---->"+object.toString());		
//		Response res=given().header("Content-Type", "application/json").
//				body(object.toString()).
//				when().
//				put(resourceUrl).
//				then().extract().response();
//		convertAsStr=res.asString();
//		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
//		System.out.println("Response " + convertAsStr);	
//		return res;	
//	}

}
