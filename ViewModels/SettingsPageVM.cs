using MovieHall.Models;
using MovieHall.SaveModels;

namespace MovieHall.ViewModels
{
    public class SettingsPageVM
    {
        public IEnumerable<Genre> Genres { get; set; }
        public IEnumerable<Setting> Settings { get; set; }
        public IEnumerable<WatchedWith> WatchedWiths { get; set; }
        public IEnumerable<SaveCustomLink> CustomLink { get; set; }
    }
}
