using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using VoyagesAPIService.Infrastructure.Helper;

namespace VoyagesAPIService.Helper
{
    public static class RedisCacheHelper
    {
        //private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() => {
        //    return ConnectionMultiplexer.Connect(Startup.StaticConfig.GetSection("redis")["connectionString"]);
        //});

        //public static ConnectionMultiplexer Connection
        //{
        //    get
        //    {
        //        return lazyConnection.Value;
        //    }
        //}
        public static ConnectionMultiplexer connectionMultiplexer;
        public static T Get<T>(string cacheKey)
        {
            return Deserialize<T>(GetDatabase().StringGet(cacheKey));
        }

        public static object Get(string cacheKey)
        {
            return Deserialize<object>(GetDatabase().StringGet(cacheKey));
        }

        public static void Set(string cacheKey, object cacheValue)
        {
            TimeSpan? expiry = null;
            var expiryMin = AzureVaultKey.GetVaultValue("CacheExpiry");// Startup.StaticConfig.GetSection("redis")["expiryMinutes"];
            if (!string.IsNullOrEmpty(expiryMin))
                expiry = TimeSpan.FromMinutes(Convert.ToDouble(expiryMin));
            GetDatabase().StringSet(cacheKey, Serialize(cacheValue), expiry);
        }

        private static byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            BinaryFormatter objBinaryFormatter = new BinaryFormatter();
            using (MemoryStream objMemoryStream = new MemoryStream())
            {
                objBinaryFormatter.Serialize(objMemoryStream, obj);
                byte[] objDataAsByte = objMemoryStream.ToArray();
                return objDataAsByte;
            }
        }

        private static T Deserialize<T>(byte[] bytes)
        {
            BinaryFormatter objBinaryFormatter = new BinaryFormatter();
            if (bytes == null)
                return default(T);

            using (MemoryStream objMemoryStream = new MemoryStream(bytes))
            {
                T result = (T)objBinaryFormatter.Deserialize(objMemoryStream);
                return result;
            }
        }

        public static bool IsKeyExists(string Key)
        {
            return GetDatabase().KeyExists(Key);
        }

        public static bool IsKeyExistsMatchingPattern(string keyword)
        {
            bool isExists = false;
            IServer server = null;
            IConfigurationSection section = Startup.StaticConfig.GetSection("redis");
            if (connectionMultiplexer != null && connectionMultiplexer.IsConnected)
            {
                server = connectionMultiplexer.GetServer(AzureVaultKey.GetVaultValue("RedisCacheServer") + ":" + AzureVaultKey.GetVaultValue("CachePort") );
            }
            else
            {
                connectionMultiplexer = ConnectionMultiplexer.Connect( "" +AzureVaultKey.GetVaultValue("RedisCacheServer") + ":"+ AzureVaultKey.GetVaultValue("CachePort") + ",password=" + AzureVaultKey.GetVaultValue("RedisCachePass") + ",abortConnect=false,ssl=true");
                if (connectionMultiplexer.IsConnected)
                    server = connectionMultiplexer.GetServer(AzureVaultKey.GetVaultValue("RedisCacheServer") + ":" + AzureVaultKey.GetVaultValue("CachePort") );
            }
            if (server != null)
            {
                foreach (var key in server.Keys(pattern: keyword + ".*"))
                {
                    isExists = connectionMultiplexer.GetDatabase().KeyExists(key);
                    if (isExists) break;
                }
            }
            return isExists;
        }

        public static void DeleteKey(string Key)
        {
            GetDatabase().KeyDelete(Key);
        }

        public static void DeleteKeyMatchingPattern(string keyword)
        {
            IServer server = null;
            IConfigurationSection section = Startup.StaticConfig.GetSection("redis");
            if (connectionMultiplexer != null && connectionMultiplexer.IsConnected)
            {
                server = connectionMultiplexer.GetServer(AzureVaultKey.GetVaultValue("RedisCacheServer") + ":" + AzureVaultKey.GetVaultValue("CachePort"));
            }
            else
            {
                connectionMultiplexer = ConnectionMultiplexer.Connect("" + AzureVaultKey.GetVaultValue("RedisCacheServer") + ":" + AzureVaultKey.GetVaultValue("CachePort") + ",password=" + AzureVaultKey.GetVaultValue("RedisCachePass") + ",abortConnect=false,ssl=true");
                if (connectionMultiplexer.IsConnected)
                    server = connectionMultiplexer.GetServer(AzureVaultKey.GetVaultValue("RedisCacheServer") + ":" + AzureVaultKey.GetVaultValue("CachePort"));
            }

            if (server != null)
            {
                foreach (var key in server.Keys(pattern: keyword + ".*"))
                {
                    connectionMultiplexer.GetDatabase().KeyDelete(key);
                }
            }
        }

        public static IDatabase GetDatabase()
        {
            IDatabase databaseReturn = null;
            if (connectionMultiplexer!=null && connectionMultiplexer.IsConnected)
                databaseReturn = connectionMultiplexer.GetDatabase();
            else
            {
                var connectionString = "" + AzureVaultKey.GetVaultValue("RedisCacheServer") + ":" + AzureVaultKey.GetVaultValue("CachePort") + ",password=" + AzureVaultKey.GetVaultValue("RedisCachePass") + ",abortConnect=false,ssl=true";
                connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
                if (connectionMultiplexer.IsConnected)
                    databaseReturn = connectionMultiplexer.GetDatabase();
            }
            return databaseReturn;
        }
    }
}
