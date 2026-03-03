using SmartParking.CustomerPackages.QRGenerator;

namespace SmartParking.CodeGeneration;

public interface ICodeGenerationAppService
{
    string CodeGenerationAsync(string? lastCode, string key);
}

