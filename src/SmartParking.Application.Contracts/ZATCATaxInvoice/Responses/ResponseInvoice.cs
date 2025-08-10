using System.Collections.Generic;

namespace SmartParking.ZATCATaxInvoice
{
    public class ResponseInvoice
    {
        public ValidationErrorResponseDto validationResults { get; set; }
        public object generalError { get; set; }
        public string clearanceStatus { get; set; }
        public string clearedInvoice { get; set; }
        public string invoiceGuid { get; set; }
        public string invoiceQRCode { get; set; }
        public bool status { get; set; }
    }

    public class InfoMessage
    {
        public string type { get; set; }
        public string code { get; set; }
        public string category { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }

    public class ValidationResults
    {
        public List<object> errorMessages { get; set; }
        public List<object> warningMessages { get; set; }
        public List<InfoMessage> infoMessages { get; set; }
        public string status { get; set; }
    }

    public class ValidationErrorResponseDto
    {
        public List<Error> errorMessages { get; set; }
        public List<Error> warningMessages { get; set; }
        public List<InfoMessage> infoMessages { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string TraceId { get; set; }
        public Dictionary<string, List<Error>> Errors { get; set; }
    }

    public class Error
    {
        public string Type { get; set; }
        public string Code { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }        
    }
}