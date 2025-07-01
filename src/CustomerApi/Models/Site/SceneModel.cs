namespace CustomerCustomerApi.Models.Site;
public class SceneModel
{
    private SceneModel() { }
    public SceneModel(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; set; }
    public string Name { get; set; }
}