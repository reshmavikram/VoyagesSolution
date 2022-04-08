package com.Utilities;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;

import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import org.testng.IAlterSuiteListener;
import org.testng.TestNG;
import org.testng.xml.XmlClass;
import org.testng.xml.XmlInclude;
import org.testng.xml.XmlSuite;
import org.testng.xml.XmlTest;

import com.InitialSetup.*;
import com.TestData.Excel_Handling;


public class IAltersuite implements IAlterSuiteListener {
	public String className;
	public String methodName;
	private static XSSFWorkbook workbook;
	private static FileInputStream fis;
	public static int Sheetcount;
	public static String SheetnameTest = "";
	public File f;
	
	@Override
	public void alter(List<XmlSuite> suites) {
//		Log.info(suites.get(0).getListeners());
//		className = System.getProperty("class");
//		methodName = System.getProperty("method");
//		className = "Account";
//		methodName = "loginVerify";
//		className = null;
//		methodName = null;
		Properties prop = new Properties();
		FileInputStream stream;

		//		Log.info((suites.get(0).getListeners().toString()).contains("TestListener"));
		if (suites.get(0).getTests().size() <= 0) {
//			Log.info("className " + className);
//			Log.info("methodName " + methodName);
//		Log.info(str);
			if (!(className == null)) {
				if (methodName == null) {
//					Log.info("Method null");
					methodName = "";
				}
//				Log.info("In alter listener");
				// XmlSuite xmlSuite = new XmlSuite();
				suites.get(0).setName("Suite");

				XmlTest xmlTest = new XmlTest(suites.get(0));
				xmlTest.setName("Test");
				xmlTest.setVerbose(2);
				XmlClass xmlClass = new XmlClass("com.Tests." + className);
				ArrayList<XmlInclude> methodsToRun = new ArrayList<XmlInclude>();
				XmlInclude xmlInclude = new XmlInclude(methodName);
				methodsToRun.add(xmlInclude);
				if (!methodName.equals(""))
					xmlClass.setIncludedMethods(methodsToRun);
				xmlTest.getXmlClasses().add(xmlClass);
				TestNG result = new TestNG();
//		      com.haud.qa.svalinn.api.report.TestListener listener = new com.haud.qa.svalinn.api.report.TestListener();
				result.setUseDefaultListeners(true);
				// result.addListener((ITestNGListener) listener);
				suites.get(0).setVerbose(0);
//				(suites.get(0)).addListener("com.haud.qa.svalinn.api.report.TestListener");
				result.setXmlSuites(Arrays.asList(suites.get(0)));
//				Log.info(suites.get(0).toXml());
//				result.run();

			} else {
				try {
					fis = new FileInputStream(new File(Constants.datasheetPath + "Datasheet.xlsx"));
					workbook = new XSSFWorkbook(fis);
				} catch (Exception e) {
//					Log.error(e);
				}
				Sheetcount = workbook.getNumberOfSheets();
				Excel_Handling excel = new Excel_Handling();
				TestNG testNG = new TestNG();
				XmlSuite suite = suites.get(0);
				suite.setName(new Common_Functions_old().GetXMLTagValue(Constants.configPath + "Config.xml",
						"Regression_Suite_Name"));
//				suite.addListener("com.haud.qa.svalinn.api.report.TestListener");
				testNG.setXmlSuites(Arrays.asList(suite));

				for (int k = 0; k < Sheetcount; k++) {
					SheetnameTest = workbook.getSheetName(k);
					// Log.info(SheetnameTest);
					excel.ExcelReader(Constants.datasheetPath + "Datasheet.xlsx", SheetnameTest,
							Constants.datasheetPath + "Datasheet_Result.xlsx", SheetnameTest);
					try {
						excel.getExcelDataAll(SheetnameTest, "Execute", (System.getProperty("suite")==null)?"Y":System.getProperty("suite"), "testCaseName");
						if (Excel_Handling.TestData.isEmpty()) {
							continue;
						}
					} catch (Exception e) {
//						Log.error(e);
					}
					@SuppressWarnings({ "rawtypes", "static-access" })
					Map<String, HashMap> map = Excel_Handling.TestData;
					Map.Entry<String, HashMap> entry = map.entrySet().iterator().next();
					HashMap<String, String> firstKey = entry.getValue();
					Map.Entry<String, String> excelvalue = firstKey.entrySet().iterator().next();
					String key = excelvalue.getKey();
//					Log.info("Map-Map -------- " + key);

//					for (String key: map.keySet()){
					// creation of the testng xml based on parameters/data

					XmlTest test = new XmlTest(suite);
					test.setName(SheetnameTest);
					test.setPreserveOrder("true");
					test.addParameter("serviceType", Excel_Handling.Get_Data(key, "Service_Type"));
					test.addParameter("tcID", key);
					test.addParameter("appURL",
							new Common_Functions_old().GetXMLTagValue(Constants.configPath + "Config.xml", "AppUrl"));
					suite.setVerbose(8);
					ArrayList<XmlInclude> methodsToRun = new ArrayList<XmlInclude>();

					XmlClass testClass = new XmlClass();
					testClass.setName("com.Tests." + SheetnameTest);
					for (String key2 : map.keySet()) {
						XmlInclude xmlInclude = new XmlInclude(Excel_Handling.Get_Data(key2, "testCaseName"));
						methodsToRun.add(xmlInclude);
						testClass.setIncludedMethods(methodsToRun);
					}
					test.getXmlClasses().add(testClass);
//						testNG.setXmlSuites(Arrays.asList(suites.get(0)));
					// com.haud.qa.svalinn.api.report.Report_Setup.InitializeReport(key);
//						testNG.run();
//					}
//					Report_Setup.extent.endTest(Report_Setup.test);
//					HammurabiReport.singleton().end();    
				}
//				com.haud.qa.svalinn.api.report.Report_Setup.InitializeReport(key);
//				Log.info(suite.toXml());
//				testNG.run();
//			}
				//Report_Setup.extent.endTest(Report_Setup.test);

			}

		}
	}
}
