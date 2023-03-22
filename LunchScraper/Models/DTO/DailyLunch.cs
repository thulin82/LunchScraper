namespace LunchScraper.Models
{
    public class DailyLunch
    {
        public string DayOfTheWeek { get; set; }
        public List<LunchItem> Items { get; set; }
        public string Price { get; set; }
    }
}