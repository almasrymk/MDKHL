using QRCoder;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.QRCode;

public class QRCodeAppService : SmartParkingAppService, IQRCodeAppService
{
    private string folderQrImage = "QRCodeImage";
    private string folderImages = "image";
    public async Task<Result<string>> CreateQRCOdeImageAsync(string? qrCodedto)
    {
        if (qrCodedto.IsNullOrEmpty())
            return " ";
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())

        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodedto, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            string imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderQrImage, folderImages);
            if (!Directory.Exists(imageDirectoryPath))
            {
                Directory.CreateDirectory(imageDirectoryPath);
            }
            string fileName = $"{Guid.NewGuid()}{DateTime.Now:yyyyMMddHHmmssfff}.png";
            string fullPath = Path.Combine(imageDirectoryPath, fileName);
            await File.WriteAllBytesAsync(fullPath, qrCodeImage);
            return $"{folderQrImage}/{folderImages}/{fileName}";
        }
    }
}
