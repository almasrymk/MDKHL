namespace SmartParking.CodeGeneration;

public interface IReadFilesAppService
{
    CodeGenerationConfig ReadFromJson(string key);
}
