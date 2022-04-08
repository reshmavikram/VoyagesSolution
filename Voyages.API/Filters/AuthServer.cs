using Data.Solution.Helpers;
using Data.Solution.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VoyagesAPIService.Infrastructure.Helper;

namespace VoyagesAPIService.Filter
{
    public class AuthServer : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string connectionString;

        public AuthServer(IConfiguration configuration)
        {
            this.connectionString = "Server=tcp:" + AzureVaultKey.GetVaultValue("DbServer") + "," + AzureVaultKey.GetVaultValue("DbServerPort") + ";Initial Catalog=" + AzureVaultKey.GetVaultValue("DbSchema") + "; "
                    + " Persist Security Info=False;User ID=" + AzureVaultKey.GetVaultValue("DbUser") + ";Password=" + AzureVaultKey.GetVaultValue("DbPass") + ";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        }

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            bool status = false;
            string controller = (string)filterContext.RouteData.Values["Controller"];
            string method = filterContext.HttpContext.Request.Method;
            string Action = (string)filterContext.RouteData.Values["Action"];
            var authHeader = filterContext.HttpContext.Request.Headers["Authorization"].ToString();
            if (authHeader != null && authHeader.StartsWith("bearer", StringComparison.OrdinalIgnoreCase))
            {
                var accessToken = authHeader.Substring("Bearer".Length).Trim();
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(accessToken);
                string strPolicy = string.Empty;
                double time = 0;
                double userId = 0;
                foreach (var item in token.Claims)
                {
                    if (item.Type == "role")
                        strPolicy = item.Value;
                    if (item.Type == "exp")
                        time = Convert.ToDouble(item.Value);
                    if (item.Type == "sub")
                        userId = Convert.ToDouble(item.Value);
                }

                filterContext.RouteData.Values.Add("UserId", userId);

                DatabaseContext database = new DatabaseContext(connectionString);
                if (Enum.IsDefined(typeof(AccessibleAPIs), Action.ToLower()) && (strPolicy.Contains("Report") || strPolicy.Contains("Terms") || strPolicy.Contains("Passages")
               || strPolicy.Contains("Settings") || strPolicy.Contains("TrackingScreen")))
                {
                    status = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(strPolicy) && Action.ToLower() == "analyzedweathercal")
                        status = true;
                    else
                        status = ApiHelper.CheckAuthorization(strPolicy, controller, method, database);
                }
            }
            if (!status)
                filterContext.Result = new UnauthorizedResult();
        }

        public DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddTicks(Convert.ToInt64(timestamp));
        }
        enum AccessibleAPIs
        {
            deletemeteostratumdata,
            getpagesettings,
            getallvoyagesbyvessel,
            getfleetview,
            getmarineweatherimage,
            getecadata,
            getpassagedata,
            getpassagedatalatestreport,
            getpassagewarning,
            getpassagereportexclusionlog,
            getpassagereportexclusion,
            getpassagesapprovalauditlist,
            getreportsforvoyage,
            getpassagedataforchart,
            savepagesettings,
            createvoyage,
            getvoyage,
            getallfluidconsumption,
            getallfluidbunker,
            getpositionwarning,
            getvieworiginalemail,
            exportvoyage,
            createpassagereportexclusion
        }
    }

}
