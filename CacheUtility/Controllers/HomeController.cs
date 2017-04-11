using CacheUtility.Models;
using LNF.Impl.Cache;
using LNF.Impl.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CacheUtility.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index(HomeModel model)
        {
            ViewBag.ErrorMessage = string.Empty;
            ViewBag.SuccessMessage = string.Empty;
            ViewBag.WarningMessage = string.Empty;

            if (!string.IsNullOrEmpty(model.Command))
            {
                try
                {
                    HandleCommand(model);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = ex.Message;
                }
            }

            TimeSpan? ping = RedisCache.Default.Ping();
            model.Ping = ping.GetValueOrDefault(TimeSpan.Zero);
            model.Host = string.Format("{0}:{1}", RedisConnection.Configuration.Host, RedisConnection.Configuration.Port);
            model.DatabaseId = RedisConnection.Configuration.DatabaseId;

            return View(model);
        }

        private void HandleCommand(HomeModel model)
        {
            if (string.IsNullOrEmpty(model.Key))
                throw new ArgumentException("Key is required.");

            if (model.Command == "get-value")
            {
                ModelState.Clear();

                // true if not null, otherwise false
                if (!model.GetValue())
                    ViewBag.WarningMessage = string.Format("Cache key not found: {0}", model.Key);
            }
            else if (model.Command == "set-value")
            {
                model.SetValue();
                ViewBag.SuccessMessage = "Cache value set OK!";
            }
            else if (model.Command == "delete-key")
            {
                ModelState.Clear();
                model.DeleteKey();
                ViewBag.SuccessMessage = "Cache key deleted OK!";
            }
            else
                throw new ArgumentException(string.Format("Unknown command: {0}", model.Command));
        }

        [Route("browse/{database?}")]
        public ActionResult Browse(int database = 0)
        {
            ViewBag.Database = database;
            return View();
        }

        [HttpPost, Route("ajax")]
        public ActionResult Ajax(string command)
        {
            if (command == "load")
            {
                int databaseId;

                if (int.TryParse(Request.Form["database"], out databaseId))
                    return Json(new { database = databaseId, keys = GetKeys(databaseId) });
                else
                    return Json(new { error = "invalid database" });
            }
            else if (command == "get")
            {
                string type = Request.Form["type"];
                string key = Request.Form["key"];

                RedisType redisType = (RedisType)Enum.Parse(typeof(RedisType), type);

                int databaseId;

                if (int.TryParse(Request.Form["database"], out databaseId))
                {
                    var db = GetDatabase(databaseId);
                    var ttl = db.KeyTimeToLive(key);

                    switch (redisType)
                    {
                        case RedisType.String:
                            return Json(new { database = databaseId, value = new[] { db.StringGet(key).ToString() }, ttl = ttl.ToString() });
                        case RedisType.Set:
                            return Json(new { database = databaseId, value = db.SetMembers(key).Select(x => x.ToString()).ToArray(), ttl = ttl.ToString() });
                        default:
                            return Json(new { error = "invalid type" });
                    }
                }
                else
                    return Json(new { error = "invalid database" });
            }
            else
            {
                return Json(new { error = "invalid command" });
            }
        }

        private IServer GetServer()
        {
            return RedisConnection.Multiplexer.GetServer(RedisConnection.Configuration.Host, RedisConnection.Configuration.Port);
        }

        private IDatabase GetDatabase(int databaseId)
        {
            return RedisConnection.Multiplexer.GetDatabase(databaseId);
        }

        private IDictionary<string, IList<string>> GetKeys(int databaseId)
        {
            var server = GetServer();
            var result = new Dictionary<string, IList<string>>();

            foreach (var key in server.Keys(databaseId).Select(x => x.ToString()))
            {
                string type = GetKeyType(databaseId, key);

                if (!result.ContainsKey(type))
                    result.Add(type, new List<string>());

                result[type].Add(key);
            }

            return result.OrderBy(x => x.Key).ToDictionary(x => x.Key, v => (IList<string>)v.Value.OrderBy(x => x).ToList());
        }


        private string GetKeyType(int databaseId, string key)
        {
            var type = GetDatabase(databaseId).KeyType(key);
            return type.ToString();
        }
    }
}