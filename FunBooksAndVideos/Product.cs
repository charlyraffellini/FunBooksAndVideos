namespace FunBooksAndVideos.Test
{
    public class Product
    {
        public string Title { get; }
        public bool IsPhysical { get; }
        private readonly string _type;

        private Product(string type, string title, bool isPhysical)
        {
            Title = title;
            IsPhysical = isPhysical;
            _type = type;
        }

        public static Product CreatePhysicalBook(string title)
        {
            return new Product("Book", title, true);
        }

        public static Product CreateDigitalBook(string title)
        {
            return new Product("Book", title, false);
        }
    }
}