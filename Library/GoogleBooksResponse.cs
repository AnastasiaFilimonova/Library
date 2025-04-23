namespace Library
{
    public class GoogleBooksResponse
    {
        public List<GoogleBookItem> Items { get; set; }
    }
    public class GoogleBookItem
    {
        public VolumeInfo VolumeInfo { get; set; }
    }
    public class VolumeInfo
    {
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public string Description { get; set; }
        public int PageCount { get; set; }
        public ImageLinks ImageLinks { get; set; }
        public List<string> Categories { get; set; }
    }
    public class ImageLinks
    {
        public string Thumbnail { get; set; }
    }
}

