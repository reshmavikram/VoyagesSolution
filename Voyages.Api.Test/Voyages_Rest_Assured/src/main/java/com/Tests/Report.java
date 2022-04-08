package com.Tests;

import java.lang.reflect.Method;

import org.testng.annotations.BeforeClass;
import org.testng.annotations.BeforeMethod;
import org.testng.annotations.Test;

import com.BusinessLogic.PositionWarning_BusinessLogic;
import com.BusinessLogic.Voyages_BusinessLogic;
import com.InitialSetup.BaseClass;
import com.Reporting.ExtentTestManager;
import com.aventstack.extentreports.ExtentTest;

import io.restassured.path.json.JsonPath;
import io.restassured.response.Response;

public class Report extends BaseClass {
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
	
@Test(priority =1 , description = "create voyages")
	
	public void createVoyageVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createVoyage(testCaseName, "create voyages", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
	    JsonPath js = new  JsonPath(response.asString());
		setProperty("sfpM_VoyagesId", js.getInt("sfpM_VoyagesId"));	
		setProperty("imoNumber", js.getInt("imoNumber"));	
		setProperty("voyageNumber", js.getString("voyageNumber"));	
		verifyResponseBodyContent(response.asString());
		verifyResponseBodyContentSequence(response.asString());
	    verifyResponseBodyContentDatatype(response.asString());	
	} 
	

}
