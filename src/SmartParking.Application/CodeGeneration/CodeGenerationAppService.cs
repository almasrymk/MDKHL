using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SmartParking.CodeGeneration;

public class CodeGenerationAppService : ApplicationService, ICodeGenerationAppService
{
    private readonly IReadFilesAppService _readFilesAppService;

    public CodeGenerationAppService(IReadFilesAppService readFilesAppService)
    {
        _readFilesAppService = readFilesAppService;
    }
    [RemoteService(false)]
    public string CodeGenerationAsync(string? lastCode, string key)
    {
        if (key == null)
        {
            return null;
        }

        var config = _readFilesAppService.ReadFromJson(key);

        string code = config.Code;
        int zerosNumber = config.ZerosNumber;
        if (lastCode == null)
        {
            return code + "-" + "1".PadLeft(zerosNumber, '0');
        }
        string suffix = lastCode[^zerosNumber..];
        int suffixNumber = int.Parse(suffix);
        suffixNumber++;
        suffix = suffixNumber.ToString();

        return code + "-" + suffix.ToString().PadLeft(zerosNumber, '0');
    }
}
