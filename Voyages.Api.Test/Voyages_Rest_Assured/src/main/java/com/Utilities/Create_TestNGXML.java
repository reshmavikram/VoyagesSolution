package com.Utilities;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileWriter;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;

import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import org.testng.TestNG;
import org.testng.annotations.Test;
import org.testng.xml.XmlClass;
import org.testng.xml.XmlInclude;
import org.testng.xml.XmlSuite;
import org.testng.xml.XmlTest;

//import com.Reporting.Report_Setup;
import com.TestData.Excel_Handling;

//import io.swagger.models.Xml;
//import xyz.trillian.hammurabi.report.HammurabiReport;


//import xyz.trillian.hammurabi.report.HammurabiReport;


public class Create_TestNGXML {	
	private static XSSFWorkbook workbook;
	private static FileInputStream fis = null;
	public  static int  Sheetcount ;
	public static String SheetnameTest ="";
	private static final String TASKLIST = "tasklist";
	private static final String KILL = "taskkill /F /IM ";
	public File f;
//	public static HammurabiReport report;
	public List<XmlInclude> constructIncludes (String... methodNames) {
		List<XmlInclude> includes = new ArrayList<XmlInclude> ();
		for (String eachMethod : methodNames) {
			includes.add (new XmlInclude (eachMethod));
		}
		return includes;
	}

	@SuppressWarnings("deprecation")
	@Test     
	public void createXMLfile () throws Exception {
//		Properties prop = new Properties();
//		FileInputStream stream = new FileInputStream(Constants.propertiesFile + "Hammurabi.properties");
//		prop.load(stream);

		//		Runtime.getRuntime().exec(Constants.deleteAllTempFileBatchlocation);		
		//		killProcessRunning("IEDriverServer.exe");
		//		killProcessRunning("iexplore.exe *32");
		//		killProcessRunning("iexplore.exe");
		//		killProcessRunning("ALM-Client.exe");
		//		//killProcessRunning("chromedriver.exe");	
		//		//killProcessRunning("chrome.exe");
		//		killProcessRunning("scalc.exe");	
//		report = HammurabiReport.singleton();		
//		report.sendMetadata("Version", prop.getProperty("appVersion"));		
		fis = new FileInputStream(new File(Constants.datasheetPath+"Datasheet.xlsx"));
		workbook = new XSSFWorkbook(fis);	
		Sheetcount= workbook.getNumberOfSheets();
		Excel_Handling excel = new Excel_Handling();	
		for(int k=0;k<Sheetcount;k++)
		{					
			SheetnameTest = workbook.getSheetName(k);
			excel.ExcelReader(Constants.datasheetPath+"Datasheet.xlsx", SheetnameTest, Constants.datasheetPath+"Datasheet_Result.xlsx", SheetnameTest);
			try {
				excel.getExcelDataAll(SheetnameTest, "Execute", "Y", "testCaseName");	
				if(Excel_Handling.TestData.isEmpty())
				{
					continue;
				}
			} catch (Exception e) {
				// TODO Auto-generated catch block
//				Log.error(e);
			}		
			@SuppressWarnings({ "rawtypes", "static-access" })
			Map<String, HashMap> map = Excel_Handling.TestData;  
			Map.Entry<String,HashMap> entry = map.entrySet().iterator().next();
			HashMap<String, String> firstKey = entry.getValue();
			Map.Entry<String,String> excelvalue = firstKey.entrySet().iterator().next();
			String key = excelvalue.getKey();
//			Log.info("Map-Map -------- " + key);

//			for (String key: map.keySet()){
				//creation of the testng xml based on parameters/data
				TestNG testNG = new TestNG();
				XmlSuite suite = new XmlSuite ();
				suite.setName (new Common_Functions_old().GetXMLTagValue(Constants.configPath+"Config.xml", "Regression_Suite_Name"));
				if(false/*Integer.parseInt(Excel_Handling.Get_Data(key, "Browser_Instance"))>1*/){
					suite.setParallel("tests");
					suite.setVerbose(8);
					suite.setThreadCount(Integer.parseInt(Excel_Handling.Get_Data(key, "Browser_Instance")));
					for(int i=1;i<=Integer.parseInt(Excel_Handling.Get_Data(key, "Browser_Instance"));i++){
						XmlTest test = new XmlTest (suite);        		
						test.setName (key+"_Instance_"+i);
						test.setPreserveOrder("false");
						test.addParameter("serviceType", Excel_Handling.Get_Data(key, "Service_Type"));
						test.addParameter("tcID", key);
						test.addParameter("appURL", new Common_Functions_old().GetXMLTagValue(Constants.configPath+"Config.xml", "AppUrl")); 	        
						test.addParameter("temp", "temp"+i);	        		
						suite.setVerbose(8);
						ArrayList<XmlClass> classes = new ArrayList<XmlClass>();
				        ArrayList<XmlInclude> methodsToRun = new ArrayList<XmlInclude>();		
						
						XmlClass testClass = new XmlClass ();
						XmlInclude xmlInclude = new XmlInclude(Excel_Handling.Get_Data(key, "testCaseName"));
						testClass.setName ("com.Tests."+SheetnameTest);
						methodsToRun.add(xmlInclude);

				        testClass.setIncludedMethods(methodsToRun);
				        classes.add(testClass);
				        test.setXmlClasses(classes);
					}        		
				}else{
					XmlTest test = new XmlTest (suite);
					test.setName (key);            	
					test.setPreserveOrder ("true");
					test.addParameter("serviceType", Excel_Handling.Get_Data(key, "Service_Type"));
					test.addParameter("tcID", key);
					test.addParameter("appURL", new Common_Functions_old().GetXMLTagValue(Constants.configPath+"Config.xml", "AppUrl")); 	        
					suite.setVerbose(8);
					ArrayList<XmlClass> classes = new ArrayList<XmlClass>();
			        ArrayList<XmlInclude> methodsToRun = new ArrayList<XmlInclude>();		
					
					XmlClass testClass = new XmlClass ();
					testClass.setName ("com.Tests."+SheetnameTest);
					for (String key2: map.keySet()){
					XmlInclude xmlInclude = new XmlInclude(Excel_Handling.Get_Data(key2, "testCaseName"));
					methodsToRun.add(xmlInclude);
					testClass.setIncludedMethods(methodsToRun);
					}
					classes.add(testClass);
			        test.setXmlClasses(classes);
					
					/*Log.info("excel class---------------->"+Excel_Handling.Get_Data(key, "Class_Name"));
					if(!(Excel_Handling.Get_Data(key, "Class_Name")==null)) {
						XmlClass testClass = new XmlClass ();
						testClass.setName ("com.haud.qa.svalinn.api.test."+Excel_Handling.Get_Data(key, "Class_Name"));
						test.setXmlClasses (Arrays.asList (new XmlClass[] { testClass}));
					}*/
					
				}
				List<String> suites = new ArrayList<String>();
				final File f1 = new File(Create_TestNGXML.class.getProtectionDomain().getCodeSource().getLocation().getPath());
				f = new File(f1+"\\testNG.xml");
				f.createNewFile();
				FileWriter fw = new FileWriter(f.getAbsoluteFile());
				BufferedWriter bw = new BufferedWriter(fw);
				bw.write(suite.toXml());
				bw.close();	        
				suites.add(f.getPath());	        
				testNG.setTestSuites(suites);
//				com.Reporting.Report_Setup.InitializeReport(key);
				testNG.run();
//			}
//			Report_Setup.extent.endTest(Report_Setup.test);
//			HammurabiReport.singleton().end();    
			f.delete();
		} 	
//		Report_Setup.extent.flush();    

	}

	public boolean killProcessRunning(String serviceName) throws Exception {
		boolean flag = false;
		try
		{

			Process p = Runtime.getRuntime().exec(TASKLIST);
			BufferedReader reader = new BufferedReader(new InputStreamReader(p.getInputStream()));
			String line;
			while ((line = reader.readLine()) != null) {
				if (line.contains(serviceName)) {
					Runtime.getRuntime().exec(KILL+serviceName);
					flag= true;
				}
			}
		}
		catch(Exception e)
		{
//			Log.error(e);
		}
		return flag;
	}



}
