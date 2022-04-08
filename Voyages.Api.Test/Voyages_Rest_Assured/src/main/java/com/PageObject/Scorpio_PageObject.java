package com.PageObject;

import org.openqa.selenium.WebDriver;

public class Scorpio_PageObject {
	//changed3
	public WebDriver driver;
	public String TC_ID = "";
	public Scorpio_PageObject(WebDriver driver, String TC_ID)
	{
		this.driver = driver;
		this.TC_ID=TC_ID;
	}
	
	public final String voyageNumber="voyageNumber";
	public final String departurePort="departurePort";
	public final String departureTimezone="departureTimezone";
	public final String arrivalPort="arrivalPort";
	public final String arrivalTimezone="arrivalTimezone";
	public final String actualStartOfSeaPassage="actualStartOfSeaPassage";
	public final String actualEndOfSeaPassage="actualEndOfSeaPassage";
	public final String sfpM_VoyagesId="sfpM_VoyagesId";
	public final String voyagesId="voyagesId";
	//public final String status="status";
	public final String initialApprovedBy="initialApprovedBy";
	public final String isInitialApproved="isInitialApproved";
	public final String voyageNo="voyageNo";
	public final String formIdentifier="formIdentifier";
	public final String reportDateTime="reportDateTime";
	public final String latitude="latitude";
	public final String longitude="longitude";
	public final String steamingHrs="steamingHrs";
	public final String fwdDraft="fwdDraft";
	public final String distanceToGO="distanceToGO";
	public final String engineDistance="engineDistance";
	public final String slip="slip";
	public final String shaftPower="shaftPower";
	public final String windDirection="windDirection";
	public final String seaDir="seaDir";
	public final String fluidType="fluidType";
	public final String unit="unit";
	public final String category="category";
	public final String consumption="consumption";
	public final String formId="formId";
	public final String  robId="robId";
	public final String robsSFPM_RobsId="robsSFPM_RobsId";
	public final String eventType="eventType";
	public final String eventROBObsDistance="eventROBObsDistance";
	public final String eventROBRemarks="eventROBRemarks";
	public final String eventROBStartDateTime="eventROBStartDateTime";
	public final String eventROBEndDateTime="eventROBEndDateTime";
	public final String eventROBStartLatitude="eventROBStartLatitude";
	public final String eventROBStartLongitude="eventROBStartLongitude";
	public final String eventROBEndLatitude="eventROBEndLatitude";
	public final String  eventROBEndLongitude="eventROBEndLongitude";
	public final String  eventROBsRowId="eventROBsRowId";
	public final String status="status";
	public final String isActive="isActive";
	public final String vesselCode="vesselCode";
	public final String imoNumber="imoNumber";
	public final String showAll="showAll";
	public final String isFinalApproved="isFinalApproved";
	public final String finalApprovedBy="finalApprovedBy";
	public final String positionWarningId="positionWarningId";
	public final String isApproved="isApproved";
	public final String passageWarningId="passageWarningId";
	public final String reviewedBy="reviewedBy";
	public final String eventRobsRowId="eventRobsRowId";
	public final String eventId="eventId";
	public final String sfpM_EventROBsRowId="sfpM_EventROBsRowId";

	
	
	

	
	
	
	
	
	
}
