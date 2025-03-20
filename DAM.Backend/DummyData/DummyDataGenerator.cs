
using DAM.Backend.Controllers;
using DAM.Backend.Data.Models;

public static class DummyDataGenerator
{
    public static List<Image> images = new List<Image>();
    public static List<Product> products = new List<Product>();

    public static void LoadData()
    {
        LoadImages();
        LoadSampleData();
    }

    private static void LoadImages()
    {
        for (int i = 0; i < 10; i++)
        {
            CreateImageRequest data = new CreateImageRequest();
            data.Content = [];
            data.Width = RandomNumber(100, 2000);
            data.Height = RandomNumber(100, 2000);
            data.Priority = 0;
            data.IsShown = true;

            Product product = CreateProduct();
            products.Add(product);

            data.ProductId = product.UUID;

            CreateImage(data);
        }
    }


    private static Product CreateProduct() {
        Product product = new Product();
        product.UUID = Guid.NewGuid();
        product.Name = "Product " + RandomNumber(0, 100);
        return product;
    }

    private static void LoadSampleData() {
        Product sample = new Product();
        sample.UUID = Guid.Parse("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
        sample.Name = "SampleProduct";
        products.Add(sample);

        Image sampleImage = new Image();
        sampleImage.UUID = sample.UUID;;
        sampleImage.Content = new byte[0];
        sampleImage.IsShown = true;
        images.Add(sampleImage);
    }
    
    
    public static Image CreateImage(CreateImageRequest requestParams)
    {
        Image image = new Image();
        image.UUID = Guid.NewGuid();
        image.Content = requestParams.Content;
        image.Width = requestParams.Width ?? 0;
        image.Height = requestParams.Height ?? 0;
        image.Priority = requestParams.Priority ?? 0;
        image.IsShown = requestParams.IsShown;
        
        image.Product = new Product();
        image.Product.UUID = Guid.NewGuid();
        image.CreatedAt = DateTime.Now;
        image.UpdatedAt = DateTime.Now;
        
        images.Add(image);
        return image;
    }
    
    public static Image? GetImageByProductAndPriority(Guid productId, int priority)
    {
        Image? foundImage = images.Find(img => img.Product.UUID == productId && img.Priority == priority);
        return foundImage;
    }
    public static Image? GetRandomImage() 
    {
        Random random = new Random();
        int index = random.Next(0, images.Count);
        return images[index];
    }
    
    public static void CreateTag()
    {
        Tag tag = new Tag("Your daddy", false);
        tag.TagID = 123456;
    }
    
    public static void CreateTagImageGroup()
    {
        TagImageGroup tagImageGroupID = new TagImageGroup();    
        
        tagImageGroupID.Tag = new Tag("your grandma", false);
        tagImageGroupID.Image = new Image();
    }
    private static int RandomNumber(int min, int max)
    {
        Random random = new Random();
        return random.Next(min, max);
    }
}