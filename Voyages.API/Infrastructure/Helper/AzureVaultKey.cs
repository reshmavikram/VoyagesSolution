using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoyagesAPIService.Infrastructure.Helper
{
    public class AzureVaultKey
    {
        public static string DbServer = "";
        public static string DbSchema = "";
        public static string DbUser = "";
        public static string DbPass = "";
        public static string ApplicationInsight = "c219e8a2-7676-4c9d-8eeb-3f17eecf9b83";
        public static string RedisCacheServer = "";
        public static string RedisCachePass = "";
        public static string SendGridUser = "";
        public static string SendGridPass = "";
        public static string StorageSASToken = "";
        public static string ScorpioWebmailUser = "";
        public static string ScorpioWebmailPass = "";
        public static string AlertReceiver1EmailId = "";
        public static string AlertReceiver2EmailId = "";
        public static string DistanceApiEndpointLogin = "";
        public static string DistanceApiEndpointDistance = "";
        public static string DistanceApiEndpointUser = "";
        public static string DistanceApiEndpointPass = "";
        public static string AuthServertokenEndpoint = "";
        public static string MeteostratumBaseURLForWetherAPI = "";
        public static string MeteoWeatherImageBaseURLForToken = "";
        public static string MeteoStratumWeatherAPIAuthorization = "";
        public static string MeteoWeatherImageUsername = "";
        public static string MeteoWeatherImagePassword = "";
        public static string BlockStorageImageUrl = "";
        public static string ResetPasswordlink = "";
        public static string SendGridAPIKey = "";
        public static string MarineWeatherImageBaseUrl = "";
        public static string CacheExpiry = "";
        public static string CachePort = "";
        public static string ResetPasswordExpiryMinutes = "";
        public static string DbServerPort = "";
        public static string Analyzedweathercalculationhour = "";
        public static string SmtpHostName = "";
        public static string SmtpFromEmail = "";
        public static string SmtpPort = "";
        public static string SfpmApiUrl = "";
        public static string SmtpToEmail = "";
        public static string SmtpPassword = "";
        public static string CommandTimeout="";

        public static void SetVaultValue()
        {
            IConfigurationSection section = Startup.StaticConfig.GetSection("AzureVaultCredentials");
            KeyVaultClient client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessToken));
            var vaultAddress = section["vaultAddress"].ToString();
            //DESKTOP-ETL7FE4\\SQLEXPRESS
            DbServer = "DESKTOP-ATHA8AK\\SQLEXPRESS";
            //VPS_DB_New_Logic  DESKTOP-ATHA8AK\SQLEXPRESS
            DbSchema = "VPS_DB_New_Logic";
            DbUser = "sa";
            DbPass = "p@ssw0rd";
            ApplicationInsight = client.GetSecretAsync(vaultAddress, "ApplicationInsight").GetAwaiter().GetResult().Value;
            RedisCacheServer = client.GetSecretAsync(vaultAddress, "RedisCacheServer").GetAwaiter().GetResult().Value;
            RedisCachePass = client.GetSecretAsync(vaultAddress, "RedisCachePass").GetAwaiter().GetResult().Value;
            SendGridUser = client.GetSecretAsync(vaultAddress, "SendGridUser").GetAwaiter().GetResult().Value;
            SendGridPass = client.GetSecretAsync(vaultAddress, "SendGridPass").GetAwaiter().GetResult().Value;
            StorageSASToken = client.GetSecretAsync(vaultAddress, "StorageSASToken").GetAwaiter().GetResult().Value;
            ScorpioWebmailUser = client.GetSecretAsync(vaultAddress, "ScorpioWebmailUser").GetAwaiter().GetResult().Value;
            ScorpioWebmailPass = client.GetSecretAsync(vaultAddress, "ScorpioWebmailPass").GetAwaiter().GetResult().Value;
            AlertReceiver1EmailId = client.GetSecretAsync(vaultAddress, "AlertReceiver1EmailId").GetAwaiter().GetResult().Value;
            AlertReceiver2EmailId = client.GetSecretAsync(vaultAddress, "AlertReceiver2EmailId").GetAwaiter().GetResult().Value;
            DistanceApiEndpointDistance = client.GetSecretAsync(vaultAddress, "DistanceApiEndpointDistance").GetAwaiter().GetResult().Value;
            DistanceApiEndpointUser = client.GetSecretAsync(vaultAddress, "DistanceApiEndpointUser").GetAwaiter().GetResult().Value;
            DistanceApiEndpointPass = client.GetSecretAsync(vaultAddress, "DistanceApiEndpointPass").GetAwaiter().GetResult().Value;
            AuthServertokenEndpoint = client.GetSecretAsync(vaultAddress, "AuthServertokenEndpoint").GetAwaiter().GetResult().Value;
            MeteostratumBaseURLForWetherAPI = client.GetSecretAsync(vaultAddress, "MeteostratumBaseURLForWetherAPI").GetAwaiter().GetResult().Value;
            MeteoWeatherImageBaseURLForToken = client.GetSecretAsync(vaultAddress, "MeteoWeatherImageBaseURLForToken").GetAwaiter().GetResult().Value;
            MeteoStratumWeatherAPIAuthorization = client.GetSecretAsync(vaultAddress, "MeteoStratumWeatherAPIAuthorization").GetAwaiter().GetResult().Value;
            MeteoWeatherImageUsername = client.GetSecretAsync(vaultAddress, "MeteoWeatherImageUsername").GetAwaiter().GetResult().Value;
            MeteoWeatherImagePassword = client.GetSecretAsync(vaultAddress, "MeteoWeatherImagePassword").GetAwaiter().GetResult().Value;
            BlockStorageImageUrl = client.GetSecretAsync(vaultAddress, "BlockStorageImageUrl").GetAwaiter().GetResult().Value;
            ResetPasswordlink = client.GetSecretAsync(vaultAddress, "ResetPasswordlink").GetAwaiter().GetResult().Value;
            SendGridAPIKey = client.GetSecretAsync(vaultAddress, "SendGridAPIKey").GetAwaiter().GetResult().Value;
            MarineWeatherImageBaseUrl = client.GetSecretAsync(vaultAddress, "MarineWeatherImageBaseUrl").GetAwaiter().GetResult().Value;
            CacheExpiry = client.GetSecretAsync(vaultAddress, "CacheExpiry").GetAwaiter().GetResult().Value;
            CachePort = client.GetSecretAsync(vaultAddress, "CachePort").GetAwaiter().GetResult().Value;
            ResetPasswordExpiryMinutes = client.GetSecretAsync(vaultAddress, "ResetPasswordExpiryMinutes").GetAwaiter().GetResult().Value;
            DbServerPort = client.GetSecretAsync(vaultAddress, "DbServerPort").GetAwaiter().GetResult().Value;
            Analyzedweathercalculationhour = client.GetSecretAsync(vaultAddress, "Analyzedweathercalculationhour").GetAwaiter().GetResult().Value;
            DistanceApiEndpointLogin = client.GetSecretAsync(vaultAddress, "DistanceApiEndpointLogin").GetAwaiter().GetResult().Value;
            SmtpHostName = client.GetSecretAsync(vaultAddress, "SmtpHostName").GetAwaiter().GetResult().Value;
            SmtpFromEmail = client.GetSecretAsync(vaultAddress, "SmtpFromEmail").GetAwaiter().GetResult().Value;
            SmtpPort = client.GetSecretAsync(vaultAddress, "SmtpPort").GetAwaiter().GetResult().Value;
            SfpmApiUrl = client.GetSecretAsync(vaultAddress, "SfpmApiUrl").GetAwaiter().GetResult().Value;
            //added by prashant
            SmtpToEmail = client.GetSecretAsync(vaultAddress, "SmtpTo").GetAwaiter().GetResult().Value; 
            SmtpPassword = client.GetSecretAsync(vaultAddress, "SmtpPassword").GetAwaiter().GetResult().Value;
            CommandTimeout = client.GetSecretAsync(vaultAddress, "CommandTimeout").GetAwaiter().GetResult().Value;

        }

        public static async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            IConfigurationSection section = Startup.StaticConfig.GetSection("AzureVaultCredentials");
            var clientId = section["clientId"].ToString();
            var clientSecret = section["clientSecret"].ToString();
            ClientCredential credential = new ClientCredential(clientId, clientSecret);
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, credential);
            return result.AccessToken;
        }

        public static string GetVaultValue(string secretKeyName)
        {
            switch (secretKeyName)
            {
                case "DbServer":
                    return DbServer;
                case "DbSchema":
                    return DbSchema;
                case "DbUser":
                    return DbUser;
                case "DbPass":
                    return DbPass;
                case "ApplicationInsight":
                    return ApplicationInsight;
                case "RedisCacheServer":
                    return RedisCacheServer;
                case "RedisCachePass":
                    return RedisCachePass;
                case "SendGridUser":
                    return SendGridUser;
                case "SendGridPass":
                    return SendGridPass;
                case "StorageSASToken":
                    return StorageSASToken;
                case "ScorpioWebmailUser":
                    return ScorpioWebmailUser;
                case "ScorpioWebmailPass":
                    return ScorpioWebmailPass;
                case "AlertReceiver1EmailId":
                    return AlertReceiver1EmailId;
                case "AlertReceiver2EmailId":
                    return AlertReceiver2EmailId;
                case "DistanceApiEndpointLogin":
                    return DistanceApiEndpointLogin;
                case "DistanceApiEndpointDistance":
                    return DistanceApiEndpointDistance;
                case "DistanceApiEndpointUser":
                    return DistanceApiEndpointUser;
                case "DistanceApiEndpointPass":
                    return DistanceApiEndpointPass;
                case "AuthServertokenEndpoint":
                    return AuthServertokenEndpoint;
                case "MeteostratumBaseURLForWetherAPI":
                    return MeteostratumBaseURLForWetherAPI;
                case "MeteoWeatherImageBaseURLForToken":
                    return MeteoWeatherImageBaseURLForToken;
                case "MeteoStratumWeatherAPIAuthorization":
                    return MeteoStratumWeatherAPIAuthorization;
                case "MeteoWeatherImageUsername":
                    return MeteoWeatherImageUsername;
                case "MeteoWeatherImagePassword":
                    return MeteoWeatherImagePassword;
                case "BlockStorageImageUrl":
                    return BlockStorageImageUrl;
                case "ResetPasswordlink":
                    return ResetPasswordlink;
                case "SendGridAPIKey":
                    return SendGridAPIKey;
                case "MarineWeatherImageBaseUrl":
                    return MarineWeatherImageBaseUrl;
                case "CacheExpiry":
                    return CacheExpiry;
                case "CachePort":
                    return CachePort;
                case "ResetPasswordExpiryMinutes":
                    return ResetPasswordExpiryMinutes;
                case "DbServerPort":
                    return DbServerPort;
                case "Analyzedweathercalculationhour":
                    return Analyzedweathercalculationhour;
                case "SmtpHostName":
                    return SmtpHostName;
                case "SmtpFromEmail":
                    return SmtpFromEmail;
                case "SmtpPort":
                    return SmtpPort;
                case "SfpmApiUrl":
                    return SfpmApiUrl;
                //added by prashant
                case "SmtpToEmail":
                    return SmtpToEmail;
                case "SmtpPassword":
                    return SmtpPassword;
                case "CommandTimeout":
                    return CommandTimeout;
            }

            return "";
        }
    }
}
