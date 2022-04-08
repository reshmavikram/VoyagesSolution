package com.Tests;

import java.lang.reflect.Method;

import org.testng.annotations.BeforeClass;
import org.testng.annotations.BeforeMethod;
import org.testng.annotations.Test;
import com.BusinessLogic.Voyages_BusinessLogic;
import com.InitialSetup.BaseClass;
import com.Reporting.ExtentTestManager;
import com.aventstack.extentreports.ExtentTest;

import io.restassured.path.json.JsonPath;
import io.restassured.response.Response;

public class Voyages extends BaseClass {
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
//		response=test.createfuel(testCaseName, "check fuel type by id", resourceUrl);
//		setProperty("FuelTypeId", js.getInt("FuelTypeId"));	
		verifyResponseBodyContent(response.asString());
		verifyResponseBodyContentSequence(response.asString());
	    verifyResponseBodyContentDatatype(response.asString());	
	} 

	@Test(priority =2 , description = "check blank body")
	
	public void createVoyageBlankDataVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createVoyageBlankData(testCaseName, "check blank body", resourceUrl);
	    verifyResponseCode(response, 400, "verify response status code");
	    getHeader(response);
	}
	
	@Test(priority =3 , description = "update voyages")
	public void updateVoyageVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.updateVoyage(testCaseName, "update voyages", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);	
	} 
	
	@Test(priority =4 , description = "get voyages by id")
	public void getVoyageByIdVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.getVoyageById(testCaseName, "get voyages by id", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
	    verifyResponseBodyContent(response.asString());
		verifyResponseBodyContentSequence(response.asString());
	    verifyResponseBodyContentDatatype(response.asString());	
	   
	} 
	@Test(priority =5 , description = "update staust active to inactive")
	public void updateVoyagesStatusFalseVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.updateVoyagesStatusFalse(testCaseName, "update staust inactive to active", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
}
	@Test(priority =6 , description = "update staust active to inactive")
	public void updateVoyagesStatusTrueVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.updateVoyagesStatusTrue(testCaseName, "update staust active to inactive", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
}
	
	@Test(priority =7 , description = "approve inital")
	public void isInitialApprovedVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.isInitialApproved(testCaseName, "approve inital", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code"); 
	    getHeader(response);
}
	@Test(priority =8 , description = "approve final")
	public void finalApproveVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.finalApprove(testCaseName, "approve final", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
}
	@Test(priority =9 , description = "check all  voyages by vessel")
	public void getAllVoyagesByVesselVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.getAllVoyagesByVessel(testCaseName, "check all  voyages by vessel", resourceUrl);
	    verifyResponseCode(response, 200, "verify response status code");
	    getHeader(response);
}
	@Test(priority =10 , description = "check all  voyages by vessel")
	public void getReportsForPassagesVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.getReportsForPassages(testCaseName, "check report", resourceUrl);
		verifyResponseCode(response, 200, "verify response status code");
		getHeader(response);	
	}
	@Test(priority =11 , description = "check approval audit list")
	public void approvalAuditListVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.approvalAuditList(testCaseName, "check approval audit list", resourceUrl);
		verifyResponseCode(response, 200, "verify response status code");
		getHeader(response);	
	}
	@Test(priority =12 , description = "create position")
	public void createPositionVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createPosition(testCaseName, "create position", resourceUrl);
		verifyResponseCode(response, 200, "verify response status code");
		getHeader(response);	
		JsonPath js = new  JsonPath(response.asString());
	    setProperty("sfpM_Form_Id", js.getInt("sfpM_Form_Id"));	
	    setProperty("voyageNo", js.getString("voyageNo"));
	}
	@Test(priority =13 , description = "create position")
	public void createPositionBlankDataVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.createPositionBlankData(testCaseName, "create position", resourceUrl);
		verifyResponseCode(response, 400, "verify response status code");
		getHeader(response);
	
	}
	@Test(priority =14 , description = "update position")
	public void UpdatePositionVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.updatePosition(testCaseName, "create position", resourceUrl);
		verifyResponseCode(response, 200, "verify response status code");
		getHeader(response);
	
	}
	@Test(priority =15 , description = "Active to inactive ")
	public void UpdatePositionStatusFalseVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.UpdatePositionStatusFalse(testCaseName, "Active to inactive", resourceUrl);
		verifyResponseCode(response, 200, "verify response status code");
		getHeader(response);
	}
	@Test(priority =16 , description = "InActive to active")
	public void UpdatePositionStatusTrueVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.UpdatePositionStatusTrue(testCaseName, "InActive to active", resourceUrl);
		verifyResponseCode(response, 200, "verify response status code");
		getHeader(response);
	}
	
	@Test(priority =17 , description = "copy position")
	public void copyPositionVerify(Method method) throws Throwable {
		Voyages_BusinessLogic test=new Voyages_BusinessLogic(driver, testCaseName);
		getBaseUrl(testCaseName, "reterive host url");
		String resourceUrl=getResourceUrl(testCaseName, "reterive base url");
		Response response=test.copyPosition(testCaseName, "copy position", resourceUrl);
		verifyResponseCode(response, 200, "verify response status code");
		getHeader(response);
	}
	
	

}
