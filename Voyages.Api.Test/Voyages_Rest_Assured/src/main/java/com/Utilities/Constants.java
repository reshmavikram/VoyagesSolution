package com.Utilities;

import java.io.File;

public class Constants {
		
	public static final String configPath = System.getProperty("user.dir")+File.separator+"ScorpioTestData"+File.separator+"Config"+File.separator;
	public static final String drivePath =System.getProperty("user.dir")+File.separator+"ScorpioTestData"+File.separator+"Drivers"+File.separator;
	public static final String datasheetPath =System.getProperty("user.dir")+File.separator+"ScorpioTestData"+File.separator+"TestData"+File.separator;
	public static final String JsonFilePath =System.getProperty("user.dir")+File.separator+"ScorpioTestData"+File.separator+"JsonFiles"+File.separator;
	public static final String reportPath =System.getProperty("user.dir")+File.separator+"ScorpioTestData"+File.separator+"Result"+File.separator+"Graphical Reporting"+File.separator+"HTML"+File.separator+"Summery.html";
	public static final String snapshotsPath = System.getProperty("user.dir")+File.separator+"ScorpioTestData"+File.separator+"Result"+File.separator+"Graphical Reporting"+File.separator+"Sanpshots"+File.separator;
	public static final String propertiesFile = System.getProperty("user.dir")+File.separator+"ScorpioTestData"+File.separator+"PropertiesFile"+File.separator;
	public static final String imageFilePath = System.getProperty("user.dir")+File.separator+"ScorpioTestData"+File.separator+"imageFilePath"+File.separator;
	public static final String datasheetName = "Datasheet.xlsx";
	public static final String datasheetPathName = String.format("%s%s", Constants.datasheetPath, Constants.datasheetName);
	public static final String proprtiesSheetName = "Properties";
	//public static final String deleteAllTempFileBatchlocation = System.getProperty("user.dir")+"\\src\\test\\java\\DeleteAllTemporaryFiles.bat";
	public static String Resultfilename ="";
	
	
}
