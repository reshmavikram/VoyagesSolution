package com.TestData;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import org.apache.poi.hssf.usermodel.HSSFCell;
import org.apache.poi.hssf.usermodel.HSSFRow;
import org.apache.poi.hssf.usermodel.HSSFSheet;
import org.apache.poi.hssf.usermodel.HSSFWorkbook;


public class MyExcelReader {

	private HSSFWorkbook workbook;
	private HSSFCell cell;
	@SuppressWarnings("unused")
	private HSSFRow row;
	private FileInputStream fis = null;
	public static HashMap<String, HashMap<String, String>> TestData;

	public MyExcelReader(String fileName) {
		try {
			fis = new FileInputStream(new File(fileName));
			workbook = new HSSFWorkbook(fis);
		} catch (FileNotFoundException fnfEx) {
			System.out.println(fileName + " is not Found. please check the file name.");
			System.exit(0);
		} catch (IOException ioEx) {
			System.out.println(fis + " is not Found. please check the path.");
		} catch (Exception ex) {
			System.out.println("There is error reading/loading xls file, due to " + ex);
		}
	}

	public List<HashMap<String, String>> getExcelData(String sheetName) {
		HSSFSheet sheet = workbook.getSheet(sheetName);
		int lastRow = sheet.getLastRowNum();
		List<HashMap<String, String>> result = new ArrayList<HashMap<String, String>>(lastRow);
		for (int i = 1; i <= sheet.getLastRowNum(); i++) {
			HashMap<String, String> testdata = new HashMap<String, String>();
			for (int j = 0; j < sheet.getRow(i).getLastCellNum(); j++)
				testdata.put(sheet.getRow(0).getCell(j).getStringCellValue(), sheet.getRow(i).getCell(j).getStringCellValue());
			result.add(testdata);
		}
		return result;
	}

	public String[] getRowData(String sheetName) throws Exception {
		int rowNum = 0;
		  HSSFSheet sheet = workbook.getSheet(sheetName);
      @SuppressWarnings("unused")
	HSSFRow row = sheet.getRow(0);
		String[] temp = new String[sheet.getRow(rowNum).getLastCellNum()];
		for (int i = 0; i < temp.length; i++)
			temp[i] = getCellData(rowNum, i,sheetName);
		return temp;
	}
	
	public String getCellData(int rowNum, int colNum, String sheetName) throws Exception {
		try {
			HSSFSheet sheet = workbook.getSheet(sheetName);
			cell = sheet.getRow(rowNum).getCell(colNum);
			String cellData = cell.getStringCellValue();
			return cellData;
		} catch (Exception e) {
			return "";
		}
	}
	public HashMap<String, String> getRowTestData(String sheetName, String testCaseName) {
		HSSFSheet sheet = workbook.getSheet(sheetName);
		HashMap<String, String> testdata = new HashMap<String, String>();
		for (int i = 1; i <= sheet.getLastRowNum(); i++) {
			if (sheet.getRow(i).getCell(0).getStringCellValue().equalsIgnoreCase(testCaseName)) {
				for (int j = 0; j < sheet.getRow(i).getLastCellNum(); j++)
					testdata.put(sheet.getRow(0).getCell(j).getStringCellValue(), sheet.getRow(i).getCell(j).getStringCellValue());
				break;
			}
		}
		return testdata;
	}
}
