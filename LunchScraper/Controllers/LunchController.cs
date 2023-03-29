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

            return View();
        }

        /// <summary>
        /// Get lunch menu from Malmö Opera
        /// </summary>
        /// <returns></returns>
        private List<DailyLunch> GetMalmoOperaLunch()
        {
            var web = new HtmlWeb();
            var doc = web.LoadFromWebAsync("https://www.malmoopera.se/mat-och-dryck/lunchmeny", System.Text.Encoding.UTF8).Result;
            var nodes = doc.DocumentNode.SelectNodes("//body/div[@id='page-wrapper']/div[1]/div[2]/div[1]/section[1]/div[1]/p[position()>2 and position()<7]");

            List<DailyLunch> items = new List<DailyLunch>();

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
        private DailyLunch SplitStringtoDailyLunch(string item)
        {

            var lunchItem = new LunchItem();
            var split = item.Split("<br>");
            var type = split[0].Split(">")[1].Split("<")[0];
            var price = split[0].Split(">")[2].Split("<")[0];
            var name = split[1].Substring(1);
            lunchItem.Type = type;
            lunchItem.Name = name;
            var dailyLunch = new DailyLunch()
            {
                Items = new List<LunchItem>()
                    {
                        new LunchItem() { Name = name, Type = type }
                    },
                Price = price
            };
            return dailyLunch;
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
                    Price = HtmlEntity.DeEntitize(node.SelectSingleNode("span").InnerText).TrimStart(),
                });
            }
            return items;
        }
    }
}
