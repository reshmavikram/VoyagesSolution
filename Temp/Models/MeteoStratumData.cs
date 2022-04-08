using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class MeteoStratumData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_MeteoStratumId { get; set; }
        //For Meteo
        public string ReportId { get; set; }
        public string IMONumber { get; set; }
        public DateTime? GPSTimeStamp { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Cog { get; set; }
        public string Sog { get; set; }
        public string PollCategory { get; set; }
        public string PollMessage { get; set; }
        public string MMSI { get; set; }
        public long? Id { get; set; }
        public DateTime? TimeStamp { get; set; }
        public string ShipType { get; set; }
        public long? DimensionToBow { get; set; }
        public long? DimensionToStern { get; set; }
        public long? DimensionToPort { get; set; }
        public long? DimensionToStarboard { get; set; }
        public long? EtaMonth { get; set; }
        public long? EtaDay { get; set; }
        public long? EtaHour { get; set; }
        public long? EtaMinute { get; set; }
        public string Draught { get; set; }
        public string CallSign { get; set; }
        public string Name { get; set; }
        public string Destination { get; set; }
        //Stratum
        public string ValidAt { get; set; }
        public string Source { get; set; }
        public decimal AirPressureInHectoPascal { get; set; }
        public decimal WindSpeedInKnots { get; set; }
        public long WindDirectionInDegrees { get; set; }
        public decimal SeaHeightInMeters { get; set; }
        public long SeaPeriodInSeconds { get; set; }
        public long SwellDirectionInDegrees { get; set; }
        public decimal SwellHeightInMeters { get; set; }
        public long SwellPeriodInSeconds { get; set; }
        public long VisibilityCode { get; set; }
        public string Visibility { get; set; }
        public long WeatherCode { get; set; }
        public string Weather { get; set; }
        public long PrecipitationProbabilityInPercent { get; set; }
        public long HeightOf500HectoPascalLevelInMeters { get; set; }
        public long SeaTemperatureInCelsius { get; set; }
        public long AirTemperatureInCelsius { get; set; }
        public long WindSpeedAt50MetersInKnots { get; set; }
        public long WindDirectionAt50MetersInDegrees { get; set; }
        public long IcingClass { get; set; }
        public string Icing { get; set; }
        public long RiskWindSpeedInKnots { get; set; }
        public long WindGustInKnots { get; set; }
        public long WindGustAt50MetersInKnots { get; set; }
        public decimal TotalWaveHeightInMeters { get; set; }
        public long TotalWaveDirectionInDegrees { get; set; }
        public decimal RiskWaveHeightInMeters { get; set; }
        public decimal SeaCurrentSpeedInKnots { get; set; }
        public long SeaCurrentDirectionInDegrees { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }

        public long StratumBrngFromVesselInDegree { get; set; }
        public decimal StratumDistFromVesselInNmi { get; set; }
        public long StratumRelHumidityinPercentage { get; set; }
        public decimal StratumMSLPressureInHectoPascal { get; set; }
        public long StratumCloudCoverInPercentage { get; set; }
        public long StratumPrecipitationInMM { get; set; }
        public long StratumWavePeriodInSecond { get; set; }
        public long StratumSigWaveHeightInMeter { get; set; }
        public long StratumOceanCurrentDirectionInDegree { get; set; }
        public long StratumOceanCurrentSpeedInKnot { get; set; }
        public decimal StratumWindDirectionInDegree { get; set; }
        public decimal StratumWindSpeedInKnot { get; set; }
        public decimal StratumAirTemperatureInDegreeCelcius { get; set; }
        public decimal StratumSeaTemperatureInDegreeCelcius { get; set; }
        public decimal StratumWaveDirectionInDegree { get; set; }
        public decimal StratumWaveHeightInMeter { get; set; }
        public decimal StratumSwellDirectionInDegree { get; set; }
        public decimal StratumSwellPeriodInSecond { get; set; }
        public decimal StratumSwellHeightInMeter { get; set; }

    }
}
