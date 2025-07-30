using AutoMapper;
using AutoMapper.Internal.Mappers;
using Microsoft.Extensions.FileProviders;
using QRCoder;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.FileProviders;
using SmartParking.Invoices;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages;
public class CustomerPackageQRGenerator : SmartParkingAppService, ICustomerPackageQRGenerator
{
    private readonly IFileProviderAppService _fileProvider;
    public CustomerPackageQRGenerator(IFileProviderAppService fileProvider)
    {
        _fileProvider = fileProvider;
    }

    #region Public methods

    ///<inheritdoc/>
    public async Task<(string base64Image, string relativePath)> CreateAsync(CreateUpdateQrCodeDto input)
    {
        (string base64Image, string relativePath) = await CreateImageQrCode(input);

        return (base64Image, relativePath);

    }
    #endregion

    #region Private methods
    /// <summary>
    /// Creates a QR code entity from the given input and saves it to the repository.
    /// </summary>
    /// <param name="input">The DTO containing QR code data.</param>
    /// <param name="code">The generated unique code for the QR code.</param>
    /// <param name="imageDirectoryPath">The relative or absolute path to the QR code image.</param>
    /// <returns>
    /// A tuple containing the created <see cref="QrCode"/> entity and a boolean indicating the success of the database operation.
    /// </returns>


    /// <summary>
    /// Generates a QR code image in PNG byte format based on the serialized JSON of the input data.
    /// </summary>
    /// <param name="input">The DTO to be encoded into the QR code.</param>
    /// <returns>A byte array representing the generated PNG QR code image.</returns>
    private byte[] GenerateQrCodeImage(CreateUpdateQrCodeDto input)
    {
        var jsonQrCode = JsonSerializer.Serialize(input.PackageCode);
        using (var qrGenerator = new QRCodeGenerator())
        using (var qrCodeData = qrGenerator.CreateQrCode(jsonQrCode, QRCodeGenerator.ECCLevel.Q))
        using (var qrCode = new PngByteQRCode(qrCodeData))
        {
            return qrCode.GetGraphic(20);
        }
    }

    /// <summary>
    /// Generates a unique QR code string and creates the corresponding image file.
    /// </summary>
    /// <param name="input">The DTO containing QR code metadata (e.g., address, zone, barrier info).</param>
    /// <returns>
    /// A tuple containing the generated QR code string and the relative file path where the image is saved.
    /// </returns>
    private async Task<(string imageBase, string relativePath)> CreateImageQrCode(CreateUpdateQrCodeDto input)
    {
        byte[] qrCodeImage = GenerateQrCodeImage(input);
        string base64Image = Convert.ToBase64String(qrCodeImage);

        string relativePath = await _fileProvider.SaveFileAsync(
            qrCodeImage,
            Path.Combine(Directory.GetCurrentDirectory(), PathHelper.WwwRoot, PathHelper.CustomerPackageQRCodeFolder, input.PhoneNumber, input.OrganizationId.ToString()),
             input.PackageCode + ".png"
        );
        return (base64Image, relativePath);
    }

    public async Task<string> ConvertToBase64(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
        return Convert.ToBase64String(fileBytes);
    }

    #endregion
}
