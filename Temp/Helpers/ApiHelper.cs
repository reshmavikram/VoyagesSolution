using Data.Solution.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Formatting = Newtonsoft.Json.Formatting;
using Microsoft.Extensions.Options;

namespace Data.Solution.Helpers
{
    public class ApiHelper
    {
        public static string baseUrl = "https://api.test.veslink.com";
        public static async Task<T> GetObject<T>(string apiUrl)
        {
            T result = Activator.CreateInstance<T>();

            using (var client = new HttpClient())
            {
                var response = await GetAsyncApiResponse(client, apiUrl);
                await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
                {
                    if (x.IsFaulted)
                        throw x.Exception;

                    result = JsonConvert.DeserializeObject<T>(x.Result);
                });
            }

            return result;
        }

        public static async Task<T> GetObjectFromXML<T>(string apiUrl)
        {

            string test = "";
            T datalist = default(T);
            IDictionary<string, string> dic = new Dictionary<string, string>();
            using (var client = new HttpClient())
            {
                var response = await GetAsyncApiResponse(client, apiUrl);
                await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
                {
                    if (x.IsFaulted)
                        throw x.Exception;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(x.Result);
                    test = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc, Formatting.None, true).Replace("@", "").Replace("#", "");
                    datalist = JsonConvert.DeserializeObject<T>(test);
                });
            }

            return datalist;
        }




        public static async Task<List<T>> GetObjectList<T>(string apiUrl)
        {
            List<T> result = new List<T>();
            using (var client = new HttpClient())
            {
                var response = await GetAsyncApiResponse(client, apiUrl);
                await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
                {
                    if (x.IsFaulted)
                        throw x.Exception;

                    result = JsonConvert.DeserializeObject<List<T>>(x.Result);
                });
            }
            return result;
        }


        public static async Task<T> PostRequest<T>(string apiUrl, object postObject = null)
        {
            T result = Activator.CreateInstance<T>();
            using (var client = new HttpClient())
            {
                string json = string.Empty;
                if (postObject != null)
                    json = JsonConvert.SerializeObject(postObject, Newtonsoft.Json.Formatting.Indented);
                var httpContent = new StringContent(json);
                apiUrl = baseUrl + "/" + apiUrl;
                var response = await client.PostAsync(apiUrl, httpContent).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
                {
                    if (x.IsFaulted)
                        throw x.Exception;

                    result = JsonConvert.DeserializeObject<T>(x.Result);
                });
            }

            return result;
        }

        public static async Task PutRequest(string apiUrl, object putObject)
        {
            using (var client = new HttpClient())
            {
                apiUrl = baseUrl + "/" + apiUrl;
                string json = JsonConvert.SerializeObject(putObject, Newtonsoft.Json.Formatting.Indented);
                var httpContent = new StringContent(json);
                var response = await client.PostAsync(apiUrl, httpContent).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
            }
        }

        public static async Task<T> DeleteRequest<T>(string apiUrl)
        {
            T result = Activator.CreateInstance<T>();
            using (var client = new HttpClient())
            {
                apiUrl = baseUrl + "/" + apiUrl;
                var response = await client.DeleteAsync(apiUrl).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
                {
                    if (x.IsFaulted)
                        throw x.Exception;
                    result = JsonConvert.DeserializeObject<T>(x.Result);
                });
            }
            return result;
        }

        private static async Task<HttpResponseMessage> GetAsyncApiResponse(HttpClient client, string apiUrl)
        {
            apiUrl = baseUrl + "/" + apiUrl;
            var response = await client.GetAsync(apiUrl).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public class SingleOrArrayConverter<T> : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(List<T>));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JToken token = JToken.Load(reader);
                if (token.Type == JTokenType.Array)
                {
                    return token.ToObject<List<T>>();
                }
                return new List<T> { token.ToObject<T>() };
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                List<T> list = (List<T>)value;
                if (list.Count == 1)
                {
                    value = list[0];
                }
                serializer.Serialize(writer, value);
            }

            public override bool CanWrite
            {
                get { return true; }
            }
        }

        public static bool CheckAuthorization(string strPolicy, string controller, string method, DatabaseContext database)
        {
            DatabaseContext db = database;
            controller = controller.ToLower();
            strPolicy = strPolicy.Remove(strPolicy.Length - 1);
            List<string> lstPolicy = strPolicy.Split(',').ToList();
            for (int i = 0; i < lstPolicy.Count; i++)
            {
                long moduleId = Convert.ToInt64(lstPolicy[i].ToString().Split('_')[0]);
                string moduleName = Regex.Replace(lstPolicy[i].Split('_')[1], @"\s+", "").ToLower();
                Module module = db.Modules.Where(x => x.ModuleId == moduleId).FirstOrDefault();
                bool isExists = moduleName.ToLower().Equals(controller) || (!string.IsNullOrEmpty(module.AlternateModuleName) && module.AlternateModuleName.ToLower().Equals(controller));
                if (!isExists & moduleId != 0)
                {
                    if (moduleName == "usersettings" && (controller == "role" || controller == "users" || controller == "policy"))
                        return true;
                    else
                    {
                        //get inner modules and check if exists
                        isExists = db.Modules.Where(x => x.ParentId == moduleId && (x.ModuleName.ToLower().Equals(controller) ||
                            x.AlternateModuleName.ToLower().Equals(controller))).Any();

                        //isExists = db.Modules.Where(x => x.ParentId == moduleId && x.ModuleName.ToLower().Equals(controller) ||
                        //    x.AlternateModuleName.ToLower().Equals(controller)).Any();
                    }
                }
                if (isExists)
                {
                    if (lstPolicy[i].ToLower().Contains("full"))
                        return true;
                    else if (lstPolicy[i].ToLower().Contains("none"))
                        return false;
                    else if (method.ToLower() == "get")
                    {
                        if (lstPolicy[i].ToLower().Contains("read") || lstPolicy[i].ToLower().Contains("write"))
                            return true;
                    }
                    else if (method.ToLower() == "post" || method.ToLower() == "put")
                    {
                        if (lstPolicy[i].ToLower().Contains("write"))
                            return true;
                    }
                }
            }
            return false;
        }

        private static List<string> ReportControllers
        {
            get
            {
                return new List<string>()
                {
                    "home",
                    "bunkerreport",
                    "eventreport",
                    "general",
                    "passageperformancedetail",
                    "poolperformancedetailcontroller",
                    "vesseldailydetail",
                };
            }
        }
    }
}



