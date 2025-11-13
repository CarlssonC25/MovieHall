namespace MovieHall.SaveModels
{
    public class SaveCustomLink
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Belonging_to { get; set; }
        public string? Space { get; set; }
        public IFormFile? Img { get; set; }
        public string? ImgLink { get; set; }

    }
}
