namespace DAM.Presentation.Services.API;

public class GetProductResponse {
    public string Name { get; set; }
    public Guid UUID { get; set; }

    public GetProductResponse(string name, Guid uuid)
    {
        Name = name;
        UUID = uuid;
    }

}