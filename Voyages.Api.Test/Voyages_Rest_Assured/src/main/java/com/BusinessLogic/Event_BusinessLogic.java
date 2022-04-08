package com.BusinessLogic;

import static io.restassured.RestAssured.given;

import org.openqa.selenium.WebDriver;

import com.InitialSetup.BaseClass;
import com.PageObject.Scorpio_PageObject;
import com.Reporting.ExtentTestManager;
import com.google.gson.Gson;

import io.restassured.path.json.JsonPath;
import io.restassured.response.Response;

public class Event_BusinessLogic extends BaseClass {
	public WebDriver driver;
	public  String testCaseName = "";
	public JsonPath js;
	public String convertAsStr;
	Scorpio_PageObject object = new Scorpio_PageObject(driver, testCaseName);
	public Response res;

	public Event_BusinessLogic(WebDriver driver, String testCaseName)
	{
		this.driver = driver; 
		this.testCaseName=testCaseName;
	}
	@SuppressWarnings("unused")
	
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
	public Response createEventDuplicateData(String testCaseID, String stepNmae, String resourceURL) throws Exception { 
		ExtentTestManager.startlog(testCaseID, stepNmae, "create event with duplicate data");
		setBody(object.formId, getProperty("formId"));
		setBody(object.eventType, getProperty("eventType"));
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
	public Response createEventBlankData(String testCaseID, String stepNmae, String resourceURL) throws Exception { 
		ExtentTestManager.startlog(testCaseID, stepNmae, "create event with balnk body");
		Response res=given().header("Content-Type","application/json").body(new Gson().toJson(getBody())).
				when().
				post(resourceURL).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response" + convertAsStr);
		return res;	
	}
	public Response updateEvent(String testCaseID, String stepNmae, String resourceURL) throws Exception { 
		ExtentTestManager.startlog(testCaseID, stepNmae, "update event");
		setBody(object.formId, getProperty("formId"));
		setBody(object.eventType, getProperty("eventType"));
		setBody(object.eventROBObsDistance, getRandomNumber());
		setBody(object.eventROBRemarks, getRandomName());
		setBody(object.eventROBStartDateTime, "2020-01-29T10:35:41.136Z");
		setBody(object.eventROBEndDateTime, "2020-01-29T10:35:41.136Z");
		setBody(object.eventROBStartLatitude, getRandomNumber());
		setBody(object.eventROBStartLongitude, getRandomNumber());
		setBody(object.eventROBEndLatitude, getRandomNumber());
		setBody(object.eventROBEndLongitude, getRandomNumber());
		setBody(object.sfpM_EventROBsRowId, getProperty("sfpM_EventROBsRowId"));	
		//System.out.println(new Gson().toJson(getBody()));
		Response res=given().header("Content-Type","application/json").body(new Gson().toJson(getBody())).
				when().
				put(resourceURL).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response" + convertAsStr);
		return res;	
	}
	public Response getEventById(String testCaseID, String stepNmae, String resourceURL) throws Exception { 
		ExtentTestManager.startlog(testCaseID, stepNmae, "create event with balnk body");
		Response res=given().queryParam(object.eventId, getProperty("sfpM_EventROBsRowId")).
				when().
				get(resourceURL).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response" + convertAsStr);
		return res;	
	}
	public Response getAllEvents(String testCaseID, String stepNmae, String resourceURL) throws Exception { 
		ExtentTestManager.startlog(testCaseID, stepNmae, "get all event ");
		Response res=given().queryParam(object.formId, getProperty("formId")).
				when().
				get(resourceURL).
				then().extract().response();
		convertAsStr=res.asString();
		System.out.println("Response" + convertAsStr);
		return res;	
	}
	
}

