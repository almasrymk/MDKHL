using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SmartParking.CodeGeneration;

public class ReadFilesAppService : ApplicationService, IReadFilesAppService
{

    public ReadFilesAppService()
    {
    }

    [RemoteService(false)]
    public CodeGenerationConfig ReadFromJson(string key)
    {
        
        string jsonString = File.ReadAllText(CodeGenerationConsts.Directory);
        
        var data = JsonSerializer.Deserialize<
        Dictionary<string, CodeGenerationConfig>>(jsonString);

        if (!data.ContainsKey(key))
            throw new Exception($"Key '{key}' not found in json file.");

        return data[key];
    }
}

