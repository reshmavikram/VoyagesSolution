package com.Utilities;

import org.testng.Assert;

import com.Reporting.ExtentTestManager;
//import com.Reporting.Extent_Reporting;

public class Assertion {
	
	
	public static void assertTrue(boolean condition, String message) {
//		Log.info("true");
		ExtentTestManager.message= message;
		Assert.assertTrue(condition);
		ExtentTestManager.pass(ExtentTestManager.testName, ExtentTestManager.stepName, ExtentTestManager.message);
	}	
	public static void assertFalse(boolean condition, String message) {
		
		ExtentTestManager.message= message;
		Assert.assertFalse(condition);
		ExtentTestManager.pass(ExtentTestManager.testName, ExtentTestManager.stepName, ExtentTestManager.message);
	}
	public static void assertEquals(String actual, String expected, String message) {
		ExtentTestManager.message= message;
		Assert.assertEquals(actual, expected);
		ExtentTestManager.pass(ExtentTestManager.testName, ExtentTestManager.stepName, ExtentTestManager.message);
		
	}
	public static void assertEquals(int actual, int expected, String message) {
		ExtentTestManager.message= message;
		Assert.assertEquals(actual, expected);
		ExtentTestManager.pass(ExtentTestManager.testName, ExtentTestManager.stepName, ExtentTestManager.message);
	}
	
	public static void assertNotEqual(String actual, String expected, String message) {
		ExtentTestManager.message= message;
		Assert.assertNotEquals(actual, expected);
		ExtentTestManager.pass(ExtentTestManager.testName, ExtentTestManager.stepName, ExtentTestManager.message);
	}
	
	public static void assertNotNull(Object actual, String message) {
		ExtentTestManager.message= message;
		Assert.assertNotNull(String.valueOf(actual));
		ExtentTestManager.pass(ExtentTestManager.testName, ExtentTestManager.stepName, ExtentTestManager.message);
	}
	

}


//
