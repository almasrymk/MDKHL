using Abp.Json;
using Microsoft.Extensions.Logging;
using SmartParking.Cameras;
using SmartParking.GarageTransactions;
using SmartParking.OpeningGate;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.OpenGate;

public class OpenCameraGateService : SmartParkingAppService, IOpeningGateService
{
    private readonly ICamerasAppService _cameraAppService;
    private readonly ILogger<OpenCameraGateService> _logger;
    private readonly HttpClient _httpClient;
    public string Name => "Camera";




    public OpenCameraGateService(ICamerasAppService cameraAppService, ILogger<OpenCameraGateService> logger, HttpClient httpClient)
    {
        _cameraAppService = cameraAppService;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> OpenGate(OpenGateRequestDto camera, string GarageUrl)
    {

        var content = new StringContent(camera.ToJsonString(), Encoding.UTF8, "application/json");
        _logger.LogInformation($"Open Gate Request body => {camera.ToJsonString()}");
        _logger.LogInformation($"Open Gate Request Header => {content.Headers.ToJsonString()}");
        var response = await _httpClient.PostAsync(GarageUrl, content);
        _logger.LogInformation($"Open Gate Response body => {response.Headers.ToJsonString()}");
        return response.IsSuccessStatusCode;

    }

    public async Task<bool> OpenGateAsync(GarageTransactionDto garageTransactionDto)
    {
        try
        {

            var camera = await _cameraAppService.GetAsync(garageTransactionDto.CameraId.Value);
            var openGateBody = new OpenGateRequestDto
            {
                ip = camera.CameraIP,
                port = 80,
                username = camera.UserName ?? "",
                password = camera.Password ?? ""
            };
            return await OpenGate(openGateBody, camera.Garage.OpenGateURL);
        }
        catch(Exception ex)
        {
            _logger.LogError("Failed To call camera");
            _logger.LogError(garageTransactionDto.ToJsonString());
            return false;
        }
    }

}
