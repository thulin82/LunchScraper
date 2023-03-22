using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;

namespace LunchScraper.Controllers
{
    public class LunchController : Controller
    {
        public ActionResult Index()
        {
            var malmoArenaMenu = GetMalmoArenaLunch();
            var edgeKitchenMenu = GetEdgeKitchenLunch();

            ViewBag.MalmoArenaMenu = malmoArenaMenu;
            ViewBag.EdgeKitchenMenu = RemoveCenter(edgeKitchenMenu);

            return View();
        }

        /// <summary>
        /// Get lunch menu from Malmo Arena
        /// </summary>
        /// <returns></returns>
        private List<string> GetMalmoArenaLunch()
        {
            var web = new HtmlWeb();
            var doc = web.LoadFromWebAsync("https://www.malmoarena.com/mat-dryck/lunch", encoding: System.Text.Encoding.UTF8).Result;
            var menu = doc.DocumentNode.SelectNodes("//div[@id='c5380']");
            return menu.Select(x => x.InnerHtml.Trim()).ToList();
        }

        /// <summary>
        /// Get lunch menu from Edge Kitchen
        /// </summary>
        /// <returns></returns>
        private List<string> GetEdgeKitchenLunch()
        {
            var web = new HtmlWeb();
            var doc = web.LoadFromWebAsync("https://edgekitchen.se/meny", System.Text.Encoding.UTF8).Result;
            var menu = doc.DocumentNode.SelectNodes("//body/div[4]/div[4]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]");
            return menu.Select(x => x.InnerHtml.Trim()).ToList();
        }

        /// <summary>
        /// Remove the string "style=\"text-align: center;\"" from the list
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        private List<string> RemoveCenter(List<string> menu)
        {
            return menu.Select(x => x.Replace("style=\"text-align: center;\"", "")).ToList();
        }
    }
}
