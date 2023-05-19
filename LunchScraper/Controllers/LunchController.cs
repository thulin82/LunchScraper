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
            var malmoOperaMenu = GetMalmoOperaLunch();

            ViewBag.MalmoArenaMenu = malmoArenaMenu;
            ViewBag.EdgeKitchenMenu = edgeKitchenMenu;
            ViewBag.MalmoOperaMenu = malmoOperaMenu;

            ViewBag.Highlight = "raggmunk";

            return View();
        }

        /// <summary>
        /// Get lunch menu from Malmö Opera
        /// </summary>
        /// <returns></returns>
        private static List<DailyLunch> GetMalmoOperaLunch()
        {
            var web = new HtmlWeb();
            var doc = web.LoadFromWebAsync("https://www.malmoopera.se/mat-och-dryck/lunchmeny", System.Text.Encoding.UTF8).Result;
            var nodes = doc.DocumentNode.SelectNodes("//body/div[@id='page-wrapper']/div[1]/div[2]/div[1]/section[1]/div[1]/p[position()>2 and position()<7]");

            List<DailyLunch> items = new();

            var firstDishNode = SplitStringtoDailyLunch(nodes[0].InnerHtml);
            var secondDishNode = SplitStringtoDailyLunch(nodes[1].InnerHtml);
            var thirdDishNode = SplitStringtoDailyLunch(nodes[2].InnerHtml);
            var fourthDishNode = SplitStringtoDailyLunch(nodes[3].InnerHtml);

            items.Add(firstDishNode);
            items.Add(secondDishNode);
            items.Add(thirdDishNode);
            items.Add(fourthDishNode);

            return items;
        }

        /// <summary>
        /// Split string to DailyLunch Object
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static DailyLunch SplitStringtoDailyLunch(string item)
        {
            var split = item.Split("<br>");
            var type = split[0].Split(">")[1].Split("<")[0];
            var price = split[0].Split(">")[2].Split("<")[0];
            var name = split[1][1..];

            var lunchItem = new LunchItem
            {
                Type = type,
                Name = name
            };

            var dailyLunch = new DailyLunch
            {
                Items = new List<LunchItem> { lunchItem },
                Price = price
            };

            return dailyLunch;
        }

        /// <summary>
        /// Get lunch menu from Edge Kitchen
        /// </summary>
        /// <returns></returns>
        private static List<DailyLunch> GetEdgeKitchenLunch()
        {
            var web = new HtmlWeb();
            var doc = web.LoadFromWebAsync("https://edgekitchen.se/meny", System.Text.Encoding.UTF8).Result;
            var nodes = doc.DocumentNode.SelectNodes("//p[@class=\"mobile-undersized-upper\"][position()>2 and position()<22]");

            List<DailyLunch> items = new();

            for (int i = 0; i < nodes.Count; i += 4)
            {
                // Get the day of the week
                var dayOfTheWeekNode = nodes[i];
                var dayOfTheWeek = HtmlEntity.DeEntitize(dayOfTheWeekNode.InnerText.ToUpper());

                // Get the 1st dish
                var firstDishNode = nodes[i + 1];
                var firstDish = HtmlEntity.DeEntitize(firstDishNode.InnerText).TrimStart();

                // Get the 2nd dish
                var secondDishNode = nodes[i + 2];
                var secondDish = HtmlEntity.DeEntitize(secondDishNode.InnerText).TrimStart();

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
        private static List<DailyLunch> GetMalmoArenaLunch()
        {
            var web = new HtmlWeb();
            var doc = web.LoadFromWebAsync("https://www.malmoarena.com/mat-dryck/lunch", System.Text.Encoding.UTF8).Result;
            var nodes = doc.DocumentNode.SelectNodes("//h3[contains(text(),\"Dagens lunch\")]/../p[position()>1]");

            List<DailyLunch> items = new();

            foreach (var node in nodes)
            {
                List<LunchItem> lunchItems = new();

                var firstDishNode = node.SelectSingleNode("br[1]");
                var secondDishNode = node.SelectSingleNode("br[2]");
                var thirdDishNode = node.SelectSingleNode("br[3]");

                if (firstDishNode != null && firstDishNode.NextSibling != null)
                {
                    lunchItems.Add(new LunchItem()
                    {
                        Name = HtmlEntity.DeEntitize(firstDishNode.NextSibling.InnerText)
                    });
                }

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

                var strongNode = node.SelectSingleNode("strong");
                var spanNode = node.SelectSingleNode("span");
                if (strongNode != null && spanNode != null)
                {
                    items.Add(new DailyLunch()
                    {
                        DayOfTheWeek = HtmlEntity.DeEntitize(strongNode.InnerText),
                        Items = lunchItems,
                        Price = HtmlEntity.DeEntitize(spanNode.InnerText).TrimStart(),
                    });
                }
            }
            return items;
        }
    }
}
