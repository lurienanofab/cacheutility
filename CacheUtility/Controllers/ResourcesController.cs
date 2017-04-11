using CacheUtility.Models;
using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CacheUtility.Controllers
{
    public class ResourcesController : Controller
    {
        [Route("resources")]
        public ActionResult Index(ResourcesModel model)
        {
            model.Items = GetResourcesFromCache(model);
            return View(model);
        }

        [Route("resources/reload")]
        public ActionResult Reload()
        {
            CacheManager.Current.RemoveSessionValue("ResourceTree");
            return RedirectToAction("Index");
        }

        [Route("resources/delete")]
        public ActionResult Delete()
        {
            CacheManager.Current.RemoveSessionValue("ResourceTree");
            return RedirectToAction("Index");
        }

        [Route("resources/detail/{resourceId}")]
        public ActionResult Detail(int resourceId)
        {
            var model = CacheManager.Current.GetResource(resourceId);
            return View(model);
        }

        private IEnumerable<ResourceModel> GetResourcesFromCache(ResourcesModel model)
        {
            var list = CacheManager.Current.Resources();
            var result = Filter(list, model);
            return result;
        }

        private IEnumerable<ResourceModel> Filter(IEnumerable<ResourceModel> items, ResourcesModel model)
        {
            IEnumerable<ResourceModel> result = items;

            if (model.Active)
                result = result.Where(x => x.ResourceIsActive);

            if (!string.IsNullOrEmpty(model.ResourceName))
                result = result.Where(x => x.ResourceName.ToLower().Contains(model.ResourceName.ToLower()));

            if (!string.IsNullOrEmpty(model.ProcessTechName))
                result = result.Where(x => x.ProcessTechName.ToLower().Contains(model.ProcessTechName.ToLower()));

            if (!string.IsNullOrEmpty(model.LabDisplayName))
                result = result.Where(x => x.LabDisplayName.ToLower().Contains(model.LabDisplayName.ToLower()));

            if (!string.IsNullOrEmpty(model.BuildingName))
                result = result.Where(x => x.BuildingName.ToLower().Contains(model.BuildingName.ToLower()));

            return result;
        }
    }
}