package com.Reporting;

import java.util.HashMap;
import java.util.Map;

import com.aventstack.extentreports.ExtentReports;
import com.aventstack.extentreports.ExtentTest;
import com.aventstack.extentreports.Status;

public class ExtentTestManager {
	static Map<Integer, ExtentTest> extentTestMap = new HashMap<Integer, ExtentTest>();
	static ExtentReports extent = ExtentManager.getInstance();
	public static String testName;
	public static String stepName;
	public static String message;

//	public static synchronized ExtentTest getTest(String methodName) {
//		ExtentTestManager.startTest(methodName);
//		return (ExtentTest) extentTestMap.get((int) (long) (Thread.currentThread().getId()));
//	}

	public static synchronized ExtentTest getTest() {
//		ExtentTestManager.startTest("Test");
		return (ExtentTest) extentTestMap.get((int) (long) (Thread.currentThread().getId()));
	}

	public static synchronized void startlog(String testName, String stepName, String message) {
		ExtentTestManager.testName = testName;
		ExtentTestManager.stepName = stepName;
		ExtentTestManager.message = message;
//		ExtentTestManager.getTest().log(Status.INFO, message);
	}

	public static synchronized void pass(String testName, String stepName, String message) {
		ExtentTestManager.testName = testName;
		ExtentTestManager.stepName = stepName;
		ExtentTestManager.message = message;
		ExtentTestManager.getTest().pass(message);
	}
	
	public static synchronized void report(String testName, String stepName, String message) {
		ExtentTestManager.testName = testName;
		ExtentTestManager.stepName = stepName;
		ExtentTestManager.message = message;
		ExtentTestManager.getTest().pass(message);
	}
	
	public static synchronized void fail(String message) {
//		ExtentTestManager.testName = testName;
//		ExtentTestManager.stepName = stepName;
//		ExtentTestManager.message = message;
		ExtentTestManager.getTest().fail(message);
	}

	public static synchronized void endTest() {
		extent.flush();
	}

	public static synchronized ExtentTest startTest(String testName) {
		ExtentTest test = extent.createTest(testName);
		extentTestMap.put((int) (long) (Thread.currentThread().getId()), test);
		return test;
	}
	
	public static synchronized ExtentTest createChildNode(ExtentTest test, String methodName) {
		ExtentTest test1 = test.createNode(methodName);
		extentTestMap.put((int) (long) (Thread.currentThread().getId()), test1);
		return test;
	}

}
