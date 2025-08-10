using System;

namespace SmartParking.ZATCATaxInvoice
{
    public class ResponseLogin
    {
        public bool Ok { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public Data Data { get; set; }
    }

    public class Data
    {
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        public User User { get; set; }
        public int PharmacyId { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public object Mobile { get; set; }
        public object AttachmentUrl { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; }
    }
}