package com.BusinessLogic;

import static io.restassured.RestAssured.given;

import org.openqa.selenium.WebDriver;

import com.InitialSetup.BaseClass;
import com.PageObject.Scorpio_PageObject;
import com.Reporting.ExtentTestManager;
import com.google.gson.Gson;

import io.restassured.path.json.JsonPath;
import io.restassured.response.Response;

public class PassageWarning_BusinessLogic extends BaseClass {
	public WebDriver driver;
	public  String testCaseName = "";
	public JsonPath js;
	public String convertAsStr;
	Scorpio_PageObject object = new Scorpio_PageObject(driver, testCaseName);
	public Response res;

	public PassageWarning_BusinessLogic(WebDriver driver, String testCaseName)
	{
		this.driver = driver; 
		this.testCaseName=testCaseName;
	}
	@SuppressWarnings("unused")
	
	
	public Response createPassageWarningAudit(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "Create passge warnning audit ");
		setBody(object.passageWarningId, 1);
		setBody(object.isApproved, false);
		setBody(object.reviewedBy, 51);
    	//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	
	public Response createPassageWarningAuditBlankData(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "Create passage warnning audit with dulpicate data");
    	//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type", "application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	
	public Response getPassageWarningAudit(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "check position warning data");
    	//System.out.println(new Gson().toJson(getBody()));
		Response res=given().queryParam(object.voyagesId, 4).
				when().
				get(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}
	
	public Response getPassageWarning(String testCaseID,String stepName, String resourceUrl ) throws Exception {
		ExtentTestManager.startlog(testCaseID, stepName, "view original email");
    	//System.out.println(new Gson().toJson(getBody()));
		Response res=given().queryParam(object.voyagesId, 4).
				when().
				get(resourceUrl).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response " + convertAsStr );	
		return res;	
	}

}
