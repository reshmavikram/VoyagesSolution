package com.Reporting;

import java.io.PrintWriter;
import java.io.StringWriter;

import com.Utilities.Log;
import org.apache.commons.lang3.exception.ExceptionUtils;
import org.testng.IClassListener;
import org.testng.IMethodInstance;
import org.testng.ITestClass;
import org.testng.ITestContext;
import org.testng.ITestListener;
import org.testng.ITestResult;

import com.aventstack.extentreports.ExtentTest;
import com.aventstack.extentreports.Status;

public class TestListener implements IClassListener, ITestListener {

	ExtentTest test;
	public void onStart(ITestContext context) {
//		Log.info("*** Test Suite " + context.getName() + " started ***");
	}

	public void onFinish(ITestContext context) {
//		Log.info(("*** Test Suite " + context.getName() + " ending ***"));
		ExtentTestManager.endTest();
		ExtentManager.getInstance().flush();
	}

	public void onTestStart(ITestResult result) {
//		Log.info(("*** Running test method " + result.getMethod().getMethodName() + "..."));
//		test = ExtentTestManager.startTest(result.getMethod().getMethodName());
	}

	public void onTestSuccess(ITestResult result) {
//		Log.info("*** Executed " + result.getMethod().getMethodName() + " test successfully...");
//		ExtentTestManager.getTest().log(Status.PASS, "Test passed");
	}

	public void onTestFailure(ITestResult result) {
//		Log.info("*** Test execution " + result.getMethod().getMethodName() + " failed...");
		ExtentTestManager.fail(ExtentTestManager.message);
		StringWriter sw = new StringWriter(); 
		result.getThrowable().printStackTrace(new PrintWriter(sw)); 
		String stacktrace = sw.toString(); // Write the stack trace to extent reports test.log(LogStatus.INFO, "<span class='label failure'>" + result.getName() + "</span>", "<pre>Stacktrace:\n" + stacktrace + "</pre>");
		boolean b = sw.toString().contains("openqa");
//		Log.info("bool " + b);
		ExtentTestManager.fail("<div class='test-heading heading' style='font-weight:bolder; font-size: 15px; color:red;'>"+((b == false)?sw.toString().substring(0,sw.toString().indexOf("\n")+1):sw.toString().substring(0,sw.toString().indexOf("}")+1))+"</div><br /><span class='label failure'>" + result.getName() + "</span>,<pre>Stacktrace:\n" + stacktrace + "</pre>");		
		ExtentTestManager.fail(result.getThrowable().getMessage());
		//test.log(Status.ERROR, /*ExceptionUtils.getStackTrace(result.getThrowable())*/ stacktrace);
	}

	public void onTestSkipped(ITestResult result) {
//		Log.info("*** Test " + result.getMethod().getMethodName() + " skipped...");
//		ExtentTestManager.getTest().log(Status.SKIP, "Test Skipped");
	}

	public void onTestFailedButWithinSuccessPercentage(ITestResult result) {
//		Log.info("*** Test failed but within percentage % " + result.getMethod().getMethodName());
	}

	public void onBeforeClass(ITestClass testClass, IMethodInstance mi) {
		// TODO Auto-generated method stub
		
	}

	public void onAfterClass(ITestClass testClass, IMethodInstance mi) {
		// TODO Auto-generated method stub
		
	}

	public void onBeforeClass(ITestClass testClass) {
		// TODO Auto-generated method stub
		
	}

	public void onAfterClass(ITestClass testClass) {
		// TODO Auto-generated method stub
		
	}
}
