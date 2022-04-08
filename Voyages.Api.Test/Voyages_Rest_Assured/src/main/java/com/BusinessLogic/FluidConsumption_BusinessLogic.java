package com.BusinessLogic;

import static io.restassured.RestAssured.given;

import org.openqa.selenium.WebDriver;

import com.InitialSetup.BaseClass;
import com.PageObject.Scorpio_PageObject;
import com.Reporting.ExtentTestManager;
import com.google.gson.Gson;

import io.restassured.path.json.JsonPath;
import io.restassured.response.Response;

public class FluidConsumption_BusinessLogic extends BaseClass {
	public WebDriver driver;
	public  String testCaseName = "";
	public JsonPath js;
	public String convertAsStr;
	Scorpio_PageObject object = new Scorpio_PageObject(driver, testCaseName);
	public Response res;

	public FluidConsumption_BusinessLogic(WebDriver driver, String testCaseName)
	{
		this.driver = driver; 
		this.testCaseName=testCaseName;
	}
	@SuppressWarnings("unused")
	
	
	public Response createPositionWarningAudit(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "Create position");
		setBody(object.formIdentifier, getRandomName());
		setBody(object.reportDateTime, "2020-01-21T12:09");
		setBody(object.latitude, getRandomNumber());
		setBody(object.longitude, getRandomNumber());
		setBody(object.steamingHrs, getRandomName());
		setBody(object.fwdDraft, getRandomName());
		setBody(object.distanceToGO, getRandomNumber());
		setBody(object.engineDistance,getRandomNumber());
    	setBody(object.slip, getRandomNumber());
    	setBody(object.shaftPower, getRandomNumber());
    	setBody(object.windDirection, getRandomNumber());
    	setBody(object.seaDir, getRandomNumber());
    	setBody(object.voyageNo, getRandomNumber());
    	//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	
	public Response createFluidConsumption(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "Create fluid consumption");
		//setBody(object.voyageNumber, Excel_Handling.Get_Data(testCaseName, "voyageNumber"));
		setBody(object.fluidType,getRandomName());
		setBody(object.unit, getRandomName());
		setBody(object.category, getRandomName());
		setBody(object.consumption, getRandomNumber());
		setBody(object.formId, getProperty("sfpM_Form_Id"));
		//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	public Response createFluidConsumptionDuplicateData(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "Create fluid consumption with duplicate data");
		//setBody(object.voyageNumber, Excel_Handling.Get_Data(testCaseName, "voyageNumber"));
		setBody(object.fluidType,getProperty("fluidType"));
		setBody(object.unit, getRandomName());
		setBody(object.category, getRandomName());
		setBody(object.consumption, getRandomName());
		setBody(object.formId, 371);
		//setBody(object.robId, getProperty("robId"));
		setBody(object.robsSFPM_RobsId, 2);
		System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	public Response createFluidConsumptionBlankData(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "create fluid consumption without request body");
		//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	public Response getFluidConsumptionById(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "get the fluid consumption");
		Response res=given().queryParam(object.formId, getProperty("sfpM_Form_Id")).queryParam(object.robId, getProperty("robId")).
				when().
				get(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	public Response getAllFluidConsumption(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "get all the fluid consumption");
		Response res=given().queryParam(object.formId, getProperty("sfpM_Form_Id")).
				when().
				get(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	
	public Response updateFluidConsumption(String testCaseID,String stepName, String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "update fluid condumption");				
		setBody(object.fluidType,getRandomName());
		setBody(object.unit, getRandomName());
		setBody(object.category, getRandomName());
		setBody(object.consumption, getRandomNumber());
		setBody(object.formId, getProperty("sfpM_Form_Id"));
		setBody(object.robId, getProperty("robId"));
		//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				put(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}	
	public Response updateFluidConsumptionDuplicateData(String testCaseID,String stepName, String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "update fluid condumption with duplicate data");				
		setBody(object.fluidType,getProperty("fluidType"));		
		setBody(object.unit, getRandomName());
		setBody(object.category, getRandomName());
		setBody(object.consumption, getRandomName());
		setBody(object.formId, getProperty("sfpM_Form_Id"));
		setBody(object.robId, getProperty("robId"));
		setBody(object.robsSFPM_RobsId, 2);
		System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				put(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	public Response deleteFluidConsumption(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "delete the fluid consumption");
		Response res=given().queryParam(object.robId, getProperty("robId")).
				when().
				delete(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	public Response createEvent(String testCaseID, String stepNmae, String resourceURL) throws Exception { 
		ExtentTestManager.startlog(testCaseID, stepNmae, "create evnet");
		setBody(object.formId, getProperty("formId"));
		setBody(object.eventType, getRandomName());
		setBody(object.eventROBObsDistance, getRandomNumber());
		setBody(object.eventROBRemarks, getRandomName());
		setBody(object.eventROBStartDateTime, "2020-01-29T10:35:41.136Z");
		setBody(object.eventROBEndDateTime, "2020-01-29T10:35:41.136Z");
		setBody(object.eventROBStartLatitude, getRandomNumber());
		setBody(object.eventROBStartLongitude, getRandomNumber());
		setBody(object.eventROBEndLatitude, getRandomNumber());
		setBody(object.eventROBEndLongitude, getRandomNumber());
		//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type","application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceURL).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response" + convertAsStr);
		return res;	
	}
	public Response createFluidEvent(String testCaseID, String stepNmae, String resourceURL) throws Exception { 
		ExtentTestManager.startlog(testCaseID, stepNmae, "create event fluid consumption");
		setBody(object.fluidType,getRandomName());
		setBody(object.unit, getRandomName());
		setBody(object.category, getRandomName());
		setBody(object.consumption, getRandomNumber());
		setBody(object.formId, getProperty("formId"));
		setBody(object.eventRobsRowId, getProperty("sfpM_EventROBsRowId"));
		//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type","application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceURL).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response" + convertAsStr);
		return res;	
	}
	public Response createFluidEventDuplicateData(String testCaseID, String stepNmae, String resourceURL) throws Exception { 
		ExtentTestManager.startlog(testCaseID, stepNmae, "create event fluid consumption with duplicate data");
		setBody(object.fluidType,getProperty("fluidType"));
		setBody(object.unit, getProperty("unit"));
		setBody(object.category, getProperty("category"));
		setBody(object.consumption, getRandomNumber());
		setBody(object.formId, getProperty("formId"));
		setBody(object.eventRobsRowId, getProperty("sfpM_EventROBsRowId"));
		//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type","application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceURL).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response" + convertAsStr);
		return res;	
	}
	public Response createFluidEventBlankData(String testCaseID, String stepNmae, String resourceURL) throws Exception { 
		ExtentTestManager.startlog(testCaseID, stepNmae, "create event fluid consumption with blank request body");
		//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type","application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceURL).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response" + convertAsStr);
		return res;	
	}
	public Response updateEventFluidConsumption(String testCaseID,String stepName, String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "update fluid condumption");				
		setBody(object.fluidType,getProperty("fluidType"));
		setBody(object.unit, getProperty("unit"));
		setBody(object.category, getProperty("category"));
		setBody(object.consumption, getRandomNumber());
		setBody(object.formId, getProperty("formId"));
		setBody(object.robId, getProperty("robId"));
		setBody(object.eventRobsRowId, getProperty("sfpM_EventROBsRowId"));
		System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				put(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	public Response getAllEventFluidConsumption(String testCaseID,String stepName, String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "check all  fluid consumption");				
		Response res=given().queryParam(object.formId, getProperty("formId")).queryParam(object.eventId, getProperty("sfpM_EventROBsRowId")).
				when().
				get(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}	
	
}
