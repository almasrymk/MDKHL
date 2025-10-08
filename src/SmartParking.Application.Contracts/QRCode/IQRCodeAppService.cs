using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.QRCode;

public interface IQRCodeAppService : IApplicationService
{
    /// <summary>
    /// Asynchronously retrieves an image associated with an invoice, using the provided QR code DTO.
    /// </summary>
    /// <param name="invoice">The invoice for which the image is being retrieved.</param>
    /// <param name="qrCodedto">A QR code DTO to retrieve the corresponding image.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Result{string}"/> object with the image or an error message.</returns>
    Task<Result<string>> CreateQRCOdeImageAsync(string? qrCodedto);

}
