using HtmlAgilityPack;
using LunchScraper.Models;
using Microsoft.AspNetCore.Mvc;

namespace LunchScraper.Controllers
{
    public class LunchController : Controller
    {
        public ActionResult Index()
        {
            var malmoArenaMenu = GetMalmoArenaLunch();
            var edgeKitchenMenu = GetEdgeKitchenLunch();

            ViewBag.MalmoArenaMenu = malmoArenaMenu;
            ViewBag.EdgeKitchenMenu = edgeKitchenMenu;

            return View();
        }

        /// <summary>
        /// Get lunch menu from Edge Kitchen
        /// </summary>
        /// <returns></returns>
        private List<DailyLunch> GetEdgeKitchenLunch()
        {
            var web = new HtmlWeb();
            var doc = web.LoadFromWebAsync("https://edgekitchen.se/meny", System.Text.Encoding.UTF8).Result;
            var nodes = doc.DocumentNode.SelectNodes("//body/div[4]/div[4]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/p[position()>4 and position()<24]");

            List<DailyLunch> items = new List<DailyLunch>();

            for (int i = 0; i < nodes.Count; i += 4)
            {
                // Get the day of the week
                var dayOfTheWeekNode = nodes[i];
                var dayOfTheWeek = HtmlEntity.DeEntitize(dayOfTheWeekNode.InnerText.ToUpper());

                // Get the 1st dish
                var firstDishNode = nodes[i + 1];
                var firstDish = HtmlEntity.DeEntitize(firstDishNode.InnerText);

                // Get the 2nd dish
                var secondDishNode = nodes[i + 2];
                var secondDish = HtmlEntity.DeEntitize(secondDishNode.InnerText);

                items.Add(new DailyLunch()
                {
                    DayOfTheWeek = dayOfTheWeek,
                    Items = new List<LunchItem>()
                    {
                        new LunchItem() { Name = firstDish },
                        new LunchItem() { Name = secondDish }
                    },
                    Price = "119 kr"
                });
            }

            return items;
        }


        /// <summary>
        /// Get lunch menu from Malmö Arena
        /// </summary>
        /// <returns></returns>
        private List<DailyLunch> GetMalmoArenaLunch()
        {
            var web = new HtmlWeb();
            var doc = web.LoadFromWebAsync("https://www.malmoarena.com/mat-dryck/lunch", System.Text.Encoding.UTF8).Result;
            var nodes = doc.DocumentNode.SelectNodes("//body/div[4]/div[1]/div[1]/div[1]/div[3]/div[1]/div[1]/div[3]/p[position()>1]");

            List<DailyLunch> items = new List<DailyLunch>();

            foreach (var node in nodes)
            {
                var lunchItems = new List<LunchItem>();

                var firstDishNode = node.SelectSingleNode("br[1]").NextSibling;
                var secondDishNode = node.SelectSingleNode("br[2]");
                var thirdDishNode = node.SelectSingleNode("br[3]");

                lunchItems.Add(new LunchItem()
                {
                    Name = HtmlEntity.DeEntitize(firstDishNode.InnerText)
                });

                if (secondDishNode != null && secondDishNode.NextSibling != null)
                {
                    lunchItems.Add(new LunchItem()
                    {
                        Name = HtmlEntity.DeEntitize(secondDishNode.NextSibling.InnerText)
                    });
                }

                if (thirdDishNode != null && thirdDishNode.NextSibling != null)
                {
                    lunchItems.Add(new LunchItem()
                    {
                        Name = HtmlEntity.DeEntitize(thirdDishNode.NextSibling.InnerText)
                    });
                }

                items.Add(new DailyLunch()
                {
                    DayOfTheWeek = HtmlEntity.DeEntitize(node.SelectSingleNode("strong").InnerText),
                    Items = lunchItems,
                    Price = HtmlEntity.DeEntitize(node.SelectSingleNode("span").InnerText),
                });
            }
            return items;
        }
    }
}
