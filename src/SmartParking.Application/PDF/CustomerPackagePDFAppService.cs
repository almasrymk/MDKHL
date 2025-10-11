using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Font;
using iText.Layout.Properties;
using SmartParking.CustomerPackages.QRGenerator;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.PDF;

public class CustomerPackagePDFAppService : ApplicationService , ICustomerPackagePDFAppService
{
    public async Task<string> GeneratePdfForCustomerPackage(CreateUpdateQrCodeDto input, byte[] qrCodeImage)
    {

        await Task.Yield();
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), PathHelper.WwwRoot, PathHelper.CustomerPackageQRCodeFolder, input.PhoneNumber, input.OrganizationId.ToString());
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        var fileName = Path.Combine(directoryPath, $"{input.PhoneNumber}.{"pdf"}");
        FileInfo file = new FileInfo(fileName);
        using (PdfWriter writer = new PdfWriter(file.FullName))
        {
            using (PdfDocument pdfDoc = new PdfDocument(writer))
            {
                iText.Kernel.Geom.PageSize smallPage = new iText.Kernel.Geom.PageSize(300, 300);
                Document document = new Document(pdfDoc, smallPage);
                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//fonts//NotoNaskhArabic-Regular.ttf");
                PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
                FontSet set = new FontSet();
                set.AddFont(fontPath);
                document.SetMargins(20, 20, 20, 20);
                Style styleBold = new Style();
                Style Center = new Style();
                Table tableimages = new Table(3).UseAllAvailableWidth();
                Image QrCodeImage = new Image(ImageDataFactory.Create(qrCodeImage));
                // 4. Make the image fit the page (minus margins)
                QrCodeImage.ScaleToFit(250, 250)
                       .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                // 5. Add the image directly (no table)
                document.Add(QrCodeImage);
                document.Close();
                var pdfPath = file.FullName;
                pdfPath = pdfPath.Substring(pdfPath.IndexOf("wwwroot"));

                return pdfPath.Replace("wwwroot", "").Replace("\\", "/");

            }
        }
    }

}
