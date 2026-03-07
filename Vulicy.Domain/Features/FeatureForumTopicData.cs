namespace Vulicy.Domain;

public class FeatureForumTopicData
{
    public FeatureType Type { get; set; }
    public string Name { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string? ForumRelativeLink { get; set; }
}
