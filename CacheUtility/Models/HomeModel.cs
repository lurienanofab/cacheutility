using LNF;
using LNF.Impl.Cache;
using System;

namespace CacheUtility.Models
{
    public class HomeModel
    {
        public string Command { get; set; }
        public string Host { get; set; }
        public int DatabaseId { get; set; }
        public TimeSpan Ping { get; set; }
        public string Key { get; set; }
        public TimeSpan? Expiry { get; set; }
        public TimeSpan? TTL { get; set; }
        public string ObjectJson { get; set; }

        public bool GetValue()
        {
            bool result;

            var db = RedisCache.Default.GetDatabase();

            var val = db.StringGetWithExpiry(Key);

            TimeSpan? expiry = val.Expiry;
            string json = val.Value;

            if (json == null)
            {
                Expiry = null;
                result = false;
            }
            else
            {
                Expiry = expiry;
                ObjectJson = json;
                TTL = db.KeyTimeToLive(Key);
                result = true;
            }

            return result;
        }

        public void SetValue()
        {
            var db = RedisCache.Default.GetDatabase();
            var setResult = db.StringSet(Key, ObjectJson, Expiry);

            TTL = Expiry;

            if (!setResult)
                throw new Exception("Unable to set value.");
        }

        public void DeleteKey()
        {
            RedisCache.Default.GetDatabase().KeyDelete(Key);
            Key = string.Empty;
            ObjectJson = string.Empty;
        }

        private string WrapWithQuotes(string value)
        {
            string result = "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            return result;
        }
    }
}