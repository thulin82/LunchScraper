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

            // Loop through all the nodes, and add the lunch items to the list, all the nodes are p nodes, and the ones
            // that we are interested in are in the format of "Day of the week, 1st dish, 2nd dish, empty" and then it repeats for days monday-friday
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
            var document = web.LoadFromWebAsync("https://www.malmoarena.com/mat-dryck/lunch", System.Text.Encoding.UTF8).Result;
            var nodes = document.DocumentNode.SelectNodes("//body/div[4]/div[1]/div[1]/div[1]/div[3]/div[1]/div[1]/div[3]/p[position()>1]");

            List<DailyLunch> items = new List<DailyLunch>();

            foreach (var node in nodes)
            {
                var lunchItems = new List<LunchItem>();

                var firstItemNode = node.SelectSingleNode("br[1]").NextSibling;
                var secondItemNode = node.SelectSingleNode("br[2]");
                var thirdItemNode = node.SelectSingleNode("br[3]");

                lunchItems.Add(new LunchItem()
                {
                    Name = HtmlEntity.DeEntitize(firstItemNode.InnerText)
                });

                if (secondItemNode != null && secondItemNode.NextSibling != null)
                {
                    lunchItems.Add(new LunchItem()
                    {
                        Name = HtmlEntity.DeEntitize(secondItemNode.NextSibling.InnerText)
                    });
                }

                if (thirdItemNode != null && thirdItemNode.NextSibling != null)
                {
                    lunchItems.Add(new LunchItem()
                    {
                        Name = HtmlEntity.DeEntitize(thirdItemNode.NextSibling.InnerText)
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
