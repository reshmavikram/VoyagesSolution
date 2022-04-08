package com.Tests;

import java.lang.reflect.Method;

import org.testng.annotations.BeforeClass;
import org.testng.annotations.BeforeMethod;
import org.testng.annotations.Test;

import com.BusinessLogic.Event_BusinessLogic;
import com.BusinessLogic.FluidConsumption_BusinessLogic;
import com.InitialSetup.BaseClass;
import com.Reporting.ExtentTestManager;
import com.aventstack.extentreports.ExtentTest;
import io.restassured.path.json.JsonPath;
import io.restassured.response.Response;

public class Event extends BaseClass {
	public static String tcID = null;
	private ExtentTest rootTest;
	private ExtentTest test;
	private String tokenID;

	@BeforeClass
	
	public void setClass() {
//		getTestData();
		rootTest = ExtentTestManager.startTest(getClass().getSimpleName());
		
	}
	
	@BeforeMethod
	public void setUp(Method method) throws Exception {
		System.out.println(method.getName());
		testCaseName = method.getName();
		test = ExtentTestManager.createChildNode(rootTest, method.getName());
	}
	
	@Test(priority=1, description="create event")
	public void createEventVerify(Method method) throws Throwable {
		Event_BusinessLogic test=new Event_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.createEvent(testCaseName, "create event", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
	    JsonPath js=new JsonPath(response.asString());
	    setProperty("sfpM_EventROBsRowId", js.getInt("sfpM_EventROBsRowId"));
	    setProperty("eventType",js.getString("eventType"));
	    verifyResponseBodyContent(response);
	    verifyResponseBodyContentDatatype(response);
	    verifyResponseBodyContentSequence(response);
	}
	@Test(priority=2, description="create event with duplicate data")
	public void createEventDuplicateDataVerify(Method method) throws Exception {
		Event_BusinessLogic test=new Event_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.createEventDuplicateData(testCaseName, "create event with duplicate data", resourceUrl);
	    verifyResponseCode(response, 409, "verify response status code");
	    getHeader(response);
	}
	@Test(priority=3, description="create event with balnk body")
	public void createEventBlankDataVerify(Method method) throws Exception {
		Event_BusinessLogic test=new Event_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.createEventBlankData(testCaseName, "create event with balnk body", resourceUrl);
	    verifyResponseCode(response, 400, "verify response status code");
	    getHeader(response);
	}
	@Test(priority=4, description="update event")
	public void updateEventVerify(Method method) throws Throwable {
		Event_BusinessLogic test=new Event_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.updateEvent(testCaseName, "update event", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
	    verifyResponseBodyContent(response);
	    verifyResponseBodyContentDatatype(response);
	    verifyResponseBodyContentSequence(response);
	}
	
	@Test(priority=5, description=" event get by id ")
	public void getEventByIdVerify(Method method) throws Throwable {
		Event_BusinessLogic test=new Event_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.getEventById(testCaseName, "update event", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
	}
	@Test(priority=6, description="get all event ")
	public void getAllEventsVerify(Method method) throws Throwable {
		Event_BusinessLogic test=new Event_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.getAllEvents(testCaseName, "get all event ", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
	}
}
