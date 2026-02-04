namespace SmartParking.Blazor.Components.State;

public class NormalPackagePropertiesDto
{
    public int DurationOfPackage { get; set; }
    public bool Published { get; set; }
    public TimeOnly? PublishFrom { get; set; }
    public TimeOnly? PublishTo { get; set; }

}
