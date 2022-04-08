package com.Tests;

import java.lang.reflect.Method;

import org.testng.annotations.BeforeClass;
import org.testng.annotations.BeforeMethod;
import org.testng.annotations.Test;

import com.BusinessLogic.PassageWarning_BusinessLogic;
import com.BusinessLogic.PositionWarning_BusinessLogic;
import com.InitialSetup.BaseClass;
import com.Reporting.ExtentTestManager;
import com.aventstack.extentreports.ExtentTest;

import io.restassured.response.Response;

public class PassageWarning extends BaseClass {
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
	
	public void createPassageWarningAuditVerify(Method method) throws Throwable {
		PassageWarning_BusinessLogic test=new PassageWarning_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createPassageWarningAudit(testCaseName, "Create position warnning audit", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
		verifyResponseBodyContent(response.asString());
		verifyResponseBodyContentSequence(response.asString());
	    verifyResponseBodyContentDatatype(response.asString());	
	} 
	@Test(priority =2 , description = "Create position warnning audit with dulpicate data")
	public void createPassageWarningAuditBlankDataVerify(Method method) throws Throwable {
		PassageWarning_BusinessLogic test=new PassageWarning_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createPassageWarningAuditBlankData(testCaseName, "Create position warnning audit with dulpicate data", resourceUrl);
	    verifyResponseCode(response, 400, "verify response status code");
	    getHeader(response);
	} 
	@Test(priority =3 , description = "check position warning data")
	public void getPassageWarningAuditVerify(Method method) throws Throwable {
		PassageWarning_BusinessLogic test=new PassageWarning_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.getPassageWarningAudit(testCaseName, "check position warning data", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
//	    verifyResponseBodyContent(getResponseArray(response).toJSONString());
//	    verifyResponseBodyContentDatatype(getResponseArray(response).toJSONString());
//	    verifyResponseBodyContentSequence(getResponseArray(response).toJSONString());
	} 
	@Test(priority =4 , description = "view original email")
	public void getPassageWarningVerify(Method method) throws Throwable {
		PassageWarning_BusinessLogic test=new PassageWarning_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.getPassageWarning(testCaseName, "view original email", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
//	    verifyResponseBodyContent(getResponseArray(response).toJSONString());
//	    verifyResponseBodyContentDatatype(getResponseArray(response).toJSONString());
//	    verifyResponseBodyContentSequence(getResponseArray(response).toJSONString());
	} 


}
