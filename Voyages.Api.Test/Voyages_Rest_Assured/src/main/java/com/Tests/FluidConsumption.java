package com.Tests;

import java.lang.reflect.Method;

import org.testng.annotations.BeforeClass;
import org.testng.annotations.BeforeMethod;
import org.testng.annotations.Test;

import com.BusinessLogic.FluidConsumption_BusinessLogic;
import com.BusinessLogic.Voyages_BusinessLogic;
import com.InitialSetup.BaseClass;
import com.Reporting.ExtentTestManager;
import com.aventstack.extentreports.ExtentTest;

import cucumber.runtime.io.Resource;
import io.restassured.path.json.JsonPath;
import io.restassured.response.Response;

public class FluidConsumption extends BaseClass {
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
	
	@Test(priority =1 , description = "create position")
	public void createPositionVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createPosition(testCaseName, "create position", resourceUrl);
		verifyResponseCode(response, 200, "verify response status code");
		getHeader(response);	
		JsonPath js = new  JsonPath(response.asString());
	    setProperty("sfpM_Form_Id", js.getInt("sfpM_Form_Id"));	
	}
	
	@Test(priority =2 , description = "create fluid consumption")
	
	public void createFluidConsumptionVerify(Method method) throws Throwable {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createFluidConsumption(testCaseName, "create fluid consumption", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
	    JsonPath js = new  JsonPath(response.asString());
		setProperty("robId", js.getInt("robId"));	
		setProperty("fluidType", js.getString("fluidType"));
		verifyResponseBodyContent(response.asString());
		verifyResponseBodyContentSequence(response.asString());
	    verifyResponseBodyContentDatatype(response.asString());	
	} 
	@Test(priority =2 , description = "create fluid consumption with duplicate data")
	
	public void createFluidConsumptionDuplicateDataVerify(Method method) throws Throwable {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createFluidConsumptionDuplicateData(testCaseName, "create fluid consumption with duplicate data", resourceUrl);
	    verifyResponseCode(response, 409, "verify response status code");
	    getHeader(response);	  
	} 
	@Test(priority =3 , description = "create fluid consumption without request body")
	public void createFluidConsumptionBlankDataVerify(Method method) throws Throwable {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createFluidConsumptionBlankData(testCaseName, "create fluid consumption without request body", resourceUrl);
	    verifyResponseCode(response, 400, "verify response status code");
	    getHeader(response);	  
	} 
	
	@Test(priority =4 , description = "get the fluid consumption")
	public void getFluidConsumptionByIdVerify(Method method) throws Throwable {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.getFluidConsumptionById(testCaseName, "get the fluid consumptio", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
		verifyResponseBodyContent(response.asString());
		verifyResponseBodyContentSequence(response.asString());
	   // verifyResponseBodyContentDatatype(response.asString());	
	} 
	@Test(priority =5 , description = "get all the fluid consumption")
	public void getAllFluidConsumptionVerify(Method method) throws Throwable {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.getAllFluidConsumption(testCaseName, "get all the fluid consumption", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);	 
//	    verifyResponseBodyContent(getResponseArray(response).toJSONString());
//		verifyResponseBodyContentSequence(getResponseArray(response).toJSONString());
//		verifyResponseBodyContentDatatype(getResponseArray(response).toJSONString());
	} 
		
	@Test(priority =6 , description = "update fluid consumption")
	public void updateFluidConsumptionVerify(Method method) throws Throwable {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.updateFluidConsumption(testCaseName, "update fluid consumption", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);	  
	} 
	
	@Test(priority = 7, description = "update fluid consumption with duplicate data")
	public void updateFluidConsumptionDuplicateDataVerify(Method method) throws Throwable {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.updateFluidConsumptionDuplicateData(testCaseName, "uupdate fluid consumption with duplicate data", resourceUrl);
	    verifyResponseCode(response, 409, "verify response status code");
	    getHeader(response);	  
	} 

	@Test(priority = 8, description = "update fluid consumption with duplicate data")
	public void deleteFluidConsumptionVerify(Method method) throws Throwable {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.deleteFluidConsumption(testCaseName, "uupdate fluid consumption with duplicate data", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);	  
	} 
	@Test(priority=8 , description="create event")
	public void createEventVerify(Method method) throws Exception {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.createEvent(testCaseName, "create event", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
	    JsonPath js=new JsonPath(response.asString());
	    setProperty("sfpM_EventROBsRowId", js.getInt("sfpM_EventROBsRowId"));
	}
	@Test(priority=9 , description="create event fluid consumption ")
	public void createFluidEventVerify(Method method) throws Throwable {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.createFluidEvent(testCaseName, "create event fluid consumption", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);	
	    JsonPath js=new JsonPath(response.asString());
	    setProperty("fluidType", js.getString("fluidType"));
	    setProperty("unit", js.getString("unit"));
	    setProperty("category", js.getString("category"));
	    verifyResponseBodyContent(response);
	    verifyResponseBodyContentDatatype(response);
	    verifyResponseBodyContentSequence(response);
	}
	@Test(priority=10 , description="create event fluid consumption with duplicate data")
	public void createFluidEventDuplicateDataVerify(Method method) throws Exception {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.createFluidEventDuplicateData(testCaseName, "create event fluid consumption with duplicate data", resourceUrl);
	    verifyResponseCode(response, 409, "verify response status code");
	    getHeader(response);	
	}
	@Test(priority=10 , description="create event fluid consumption with blank request body")
	public void createFluidEventBlankDataVerify(Method method) throws Exception {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.createFluidEventBlankData(testCaseName, "create event fluid consumption with blank request body", resourceUrl);
	    verifyResponseCode(response, 400, "verify response status code");
	    getHeader(response);	
	}
	@Test(priority=11 , description="Update fluid consumption")
	public void updateEventFluidConsumptionVerify(Method method) throws Exception {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.updateEventFluidConsumption(testCaseName, "Update fluid consumption", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);	
	}
	@Test(priority=12 , description="check all  fluid consumption")
	public void getAllEventFluidConsumptionVerify(Method method) throws Exception {
		FluidConsumption_BusinessLogic test=new FluidConsumption_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response =test.getAllEventFluidConsumption(testCaseName, "check all  fluid consumption", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);	
	}
	
}
