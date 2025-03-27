
using DAM.Backend.Controllers;
using DAM.Backend.Data.Models;
using DAM.Backend.Data;

public static class DummyDataGenerator
{
    public static List<Image> images = new List<Image>();
    public static List<Product> products = new List<Product>();

    private static string[] SampleProductIds = new[]
    {
        "dce69c0d-39e0-48bf-a193-d452da142b1c",
        "cb7e6c6d-15c7-48e3-b355-31b77114d60f",
        "99f5aa6c-e5cf-41a5-8c7e-f4fabeab3ccf",
        "a29c372e-1ef0-4f3c-b0ce-62dad345f037",
        "11e00953-bc06-4e9f-bc09-df9a6793baa8",
        "9efac388-b05f-4c46-9cdc-9b1bf0346d39",
        "0d359178-8c8c-494b-83a9-f68da3a186c7",
        "a57583ac-3d4e-404c-84e7-602cae3097c3",
        "e40add7e-c42a-4ce4-a7f7-3da530bb6e9e",
        "c5fb57c9-5634-4fbb-936f-c40d6983d044"
    };
    
    
    public async static void CreateProducts()
    {
        for(int i = 0; i < SampleProductIds.Length; i++)
        {
            Product product = new Product();
            product.UUID = Guid.Parse(SampleProductIds[i]);
            product.Name = "Product " + i;
            products.Add(product);
        }
        foreach(Product product in products)
        {
            await Database.Instance.Create(product);
        }
    }


    
    
    
    // public static void LoadData()
    // {
    //     LoadImages();
    //     LoadSampleData();
    // }
    //
    // private static void LoadImages()
    // {
    //     for (int i = 0; i < 10; i++)
    //     {
    //         CreateImageRequest data = new CreateImageRequest();
    //         data.Content = [];
    //         data.Width = RandomNumber(100, 2000);
    //         data.Height = RandomNumber(100, 2000);
    //         data.Priority = 0;
    //         data.IsShown = true;
    //
    //         Product product = CreateProduct();
    //         products.Add(product);
    //
    //         data.ProductId = product.UUID;
    //
    //         CreateImage(data);
    //     }
    // }
    //
    //
    // private static Product CreateProduct() {
    //     Product product = new Product();
    //     product.UUID = Guid.NewGuid();
    //     product.Name = "Product " + RandomNumber(0, 100);
    //     return product;
    // }
    //
    // private static void LoadSampleData() {
    //     Product sample = new Product();
    //     sample.UUID = Guid.Parse("12345678-1234-1234-1234-123456789abc");
    //     sample.Name = "SampleProduct";
    //     products.Add(sample);
    //
    //     Image sampleImage = new Image();
    //     sampleImage.UUID = sample.UUID;;
    //     sampleImage.Content = new byte[0];
    //     sampleImage.IsShown = true;
    //     images.Add(sampleImage);
    // }
    //
    //
    // public static Image CreateImage(CreateImageRequest requestParams)
    // {
    //     Image image = new Image();
    //     image.UUID = Guid.NewGuid();
    //     image.Content = requestParams.Content;
    //     image.Width = requestParams.Width ?? 0;
    //     image.Height = requestParams.Height ?? 0;
    //     image.Priority = requestParams.Priority ?? 0;
    //     image.IsShown = requestParams.IsShown;
    //     
    //     image.Product = new Product();
    //     image.Product.UUID = Guid.NewGuid();
    //     image.CreatedAt = DateTime.Now;
    //     image.UpdatedAt = DateTime.Now;
    //     
    //     images.Add(image);
    //     return image;
    // }
    //
    // public static Image? GetImageByProductAndPriority(Guid productId, int priority)
    // {
    //     Image? foundImage = images.Find(img => img.Product.UUID == productId && img.Priority == priority);
    //     return foundImage;
    // }
    // public static Image? GetRandomImage() 
    // {
    //     Random random = new Random();
    //     int index = random.Next(0, images.Count);
    //     return images[index];
    // }
    //
    // public static void CreateTag()
    // {
    //     Tag tag = new Tag("Your daddy", false);
    //     tag.TagID = 123456;
    // }
    //
    // public static void CreateTagImageGroup()
    // {
    //     TagImageGroup tagImageGroupID = new TagImageGroup();    
    //     
    //     tagImageGroupID.Tag = new Tag("your grandma", false);
    //     tagImageGroupID.Image = new Image();
    // }
    // private static int RandomNumber(int min, int max)
    // {
    //     Random random = new Random();
    //     return random.Next(min, max);
    // }
}