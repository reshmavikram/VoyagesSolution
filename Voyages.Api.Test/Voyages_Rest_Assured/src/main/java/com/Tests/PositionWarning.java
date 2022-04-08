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

public class PositionWarning extends BaseClass {
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
	
	@Test(priority =1 , description = "Create position warnning audit")
	
	public void createPositionWarningAuditVerify(Method method) throws Throwable {
		PositionWarning_BusinessLogic test=new PositionWarning_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createPositionWarningAudit(testCaseName, "Create position warnning audit", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
		verifyResponseBodyContent(response.asString());
		verifyResponseBodyContentSequence(response.asString());
	    verifyResponseBodyContentDatatype(response.asString());	
	} 
	@Test(priority =2 , description = "Create position warnning audit with dulpicate data")
	public void createPositionWarningAuditBlankDataVerify(Method method) throws Throwable {
		PositionWarning_BusinessLogic test=new PositionWarning_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createPositionWarningAuditBlankData(testCaseName, "Create position warnning audit with dulpicate data", resourceUrl);
	    verifyResponseCode(response, 400, "verify response status code");
	    getHeader(response);
	} 
	@Test(priority =3 , description = "check position warning data")
	public void getPositionWarningAuditVerify(Method method) throws Throwable {
		PositionWarning_BusinessLogic test=new PositionWarning_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.getPositionWarningAudit(testCaseName, "check position warning data", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
//	    verifyResponseBodyContent(getResponseArray(response).toJSONString());
//	    verifyResponseBodyContentDatatype(getResponseArray(response).toJSONString());
//	    verifyResponseBodyContentSequence(getResponseArray(response).toJSONString());
	} 
	@Test(priority =4 , description = "view original email")
	public void getViewOriginalEmailVerify(Method method) throws Throwable {
		PositionWarning_BusinessLogic test=new PositionWarning_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.getViewOriginalEmail(testCaseName, "view original email", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
//	    verifyResponseBodyContent(getResponseArray(response).toJSONString());
//	    verifyResponseBodyContentDatatype(getResponseArray(response).toJSONString());
//	    verifyResponseBodyContentSequence(getResponseArray(response).toJSONString());
	} 


}
