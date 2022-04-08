package com.Utilities;

import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;


public class PropertiesFile {


	public PropertiesFile() {
	}


	public Properties load (String propertyFileName) {
		Properties prop = new Properties();
		InputStream input = null;

		try {
//			input = new FileInputStream(propertyFileName);
			input = this.getClass().getResourceAsStream("/"+ propertyFileName);
			prop.load(input);
		}
		catch (IOException e) {
//			Log.error(e);
		}
		finally {
			if (input != null) {
				try {
					input.close();
				} catch (IOException e) {
//					Log.error(e);
				}
			}
		}
		return prop;
	}

	public void save () {

	}

}
