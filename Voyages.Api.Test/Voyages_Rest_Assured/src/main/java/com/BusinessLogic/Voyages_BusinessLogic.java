package com.BusinessLogic;

import static io.restassured.RestAssured.given;

import java.io.IOException;
import java.lang.reflect.Method;

import org.apache.commons.configuration2.ex.ConfigurationException;
import org.openqa.selenium.WebDriver;
import org.springframework.http.StreamingHttpOutputMessage.Body;
import org.testng.annotations.Test;

import com.InitialSetup.BaseClass;
import com.PageObject.Scorpio_PageObject;
import com.Reporting.ExtentTestManager;
import com.TestData.Excel_Handling;
import com.google.gson.Gson;

import io.restassured.path.json.JsonPath;
import io.restassured.response.Response;

public class Voyages_BusinessLogic extends BaseClass {
	public WebDriver driver;
	public  String testCaseName = "";
	public JsonPath js;
	public String convertAsStr;
	Scorpio_PageObject object = new Scorpio_PageObject(driver, testCaseName);
	public Response res;

	public Voyages_BusinessLogic(WebDriver driver, String testCaseName)
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
	public Response createVoyageBlankData(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "Create voyages with  blank body");
		System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	
	public Response updateVoyage(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "update voyages");
		setBody(object.sfpM_VoyagesId, getProperty("sfpM_VoyagesId"));
		setBody(object.voyageNumber,getProperty("voyageNumber"));
		setBody(object.departurePort, getRandomName());
		setBody(object.departureTimezone, getRandomName());
		//setBody(object.arrivalPort, getRandomName());
		//setBody(object.arrivalTimezone, getRandomName());
		setBody(object.status, Excel_Handling.Get_Data(testCaseName, "status"));
//		setBody(object.actualEndOfSeaPassage, Excel_Handling.Get_Data(testCaseName, "actualEndOfSeaPassage"));
//		setBody(object.actualStartOfSeaPassage, Excel_Handling.Get_Data(testCaseName, "actualStartOfSeaPassage"));
		setBody(object.actualEndOfSeaPassage, "2020-01-14T07:41:36.154Z");
		setBody(object.actualStartOfSeaPassage, "2020-01-14T07:41:36.154Z");	
		setBody(object.imoNumber,getRandomNumber());
		//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				put(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}

	public Response getVoyageById(String testCaseID,String stepName,String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseName, stepName, "get voyage id");
		Response res=given().queryParam(object.voyagesId, getProperty("sfpM_VoyagesId")).when().
				get(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
		System.out.println("Response " + convertAsStr);
		return res;	
	}
	public Response updateVoyagesStatusTrue(String testCaseID,String stepName,String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseName, stepName, "check status of voyages");
		Response res=given().queryParam(object.voyagesId, getProperty("sfpM_VoyagesId")).queryParam(object.isActive ,Excel_Handling.Get_Data(testCaseName, "isActive")).
				when().
				put(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
		System.out.println("Response " + convertAsStr);
		return res;	
	}
	public Response updateVoyagesStatusFalse(String testCaseID,String stepName,String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseName, stepName, "check status of voyages");
		Response res=given().queryParam(object.voyagesId, getProperty("sfpM_VoyagesId")).queryParam(object.isActive ,Excel_Handling.Get_Data(testCaseName, "isActive")).
				when().
				put(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
		System.out.println("Response " + convertAsStr);
		return res;	
	}
	public Response isInitialApproved(String testCaseID,String stepName,String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseName, stepName, "check status of voyages");
		Response res=given().queryParam(object.voyagesId, getProperty("sfpM_VoyagesId")).queryParam(object.initialApprovedBy ,Excel_Handling.Get_Data(testCaseName, "initialApprovedBy")).queryParam(object.isInitialApproved, Excel_Handling.Get_Data(testCaseName, "isInitialApproved")).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
		System.out.println("Response " + convertAsStr);
		return res;	
	}
	public Response finalApprove(String testCaseID,String stepName,String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseName, stepName, "check status of voyages");
		Response res=given().queryParam(object.voyagesId, getProperty("sfpM_VoyagesId")).queryParam(object.finalApprovedBy ,Excel_Handling.Get_Data(testCaseName, "finalApprovedBy")).queryParam(object.isFinalApproved, Excel_Handling.Get_Data(testCaseName, "isFinalApproved")).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
		System.out.println("Response " + convertAsStr);
		return res;	
	}
	public Response getAllVoyagesByVessel(String testCaseID,String stepName,String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseName, stepName, "check all  voyages by vessel");
		Response res=given().queryParam(object.imoNumber, getProperty("imoNumber")).queryParam(object.showAll, Excel_Handling.Get_Data(testCaseName, "showAll")).
				when().
				get(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
		System.out.println("Response " + convertAsStr);
		return res;	
	}
	public Response getReportsForPassages(String testCaseID,String stepName,String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseName, stepName, "check all  voyages by vessel");
		Response res=given().queryParam(object.voyageNo, getProperty("sfpM_VoyagesId")).queryParam(object.actualStartOfSeaPassage, "2019-01-14 16:12:46.547").queryParam(object.actualEndOfSeaPassage, "2020-01-14 16:12:46.547").
				when().
				get(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
		System.out.println("Response " + convertAsStr);
		return res;	
	}
	public Response approvalAuditList(String testCaseID,String stepName,String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseName, stepName, "check approval audit list");
		Response res=given().queryParam(object.voyagesId, getProperty("sfpM_VoyagesId")).
				when().
				get(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
		System.out.println("Response " + convertAsStr);
		return res;	
	}
	public Response createPosition(String testCaseID,String stepName, String resourceUrl ) throws Exception {
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
	public Response createPositionBlankData(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "Create voyages wit blank data");
    	//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	public Response updatePosition(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "update position");
		setBody(object.formId, getProperty("sfpM_Form_Id"));
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
    	setBody(object.voyageNo, getProperty("voyageNo"));
    	//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				put(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
		
	public Response UpdatePositionStatusFalse(String testCaseID,String stepName,String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseName, stepName, "active to inactive");
		Response res=given().queryParam(object.formId, getProperty("sfpM_Form_Id")).queryParam(object.isActive, Excel_Handling.Get_Data(testCaseName, "isActive")).
				when().
				put(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
		System.out.println("Response " + convertAsStr);
		return res;	
	}
	public Response UpdatePositionStatusTrue(String testCaseID,String stepName,String resourceUrl) throws Exception {
		ExtentTestManager.startlog(testCaseName, stepName, "inactive to active");
		Response res=given().queryParam(object.formId, getProperty("sfpM_Form_Id")).queryParam(object.isActive, Excel_Handling.Get_Data(testCaseName, "isActive")).
				when().
				put(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		ExtentTestManager.report(testCaseName, "Response", convertAsStr);
		System.out.println("Response " + convertAsStr);
		return res;	
	}
    
	public Response copyPosition(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "copy position");
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
    	setBody(object.voyageNo, getProperty("voyageNo"));
    	//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
    
}
