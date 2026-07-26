using System.Text.Json.Serialization;

namespace SmartParking.Payments.PosTerminal.WireContracts;

// ── Outbound Requests ────────────────────────────────────────────────────────

public sealed class ConnectComRequestDto
{
    [JsonPropertyName("Event")] public string Event { get; init; } = "CONNECTION";
    [JsonPropertyName("Operation")] public string Operation { get; init; } = "CONNECT";
    [JsonPropertyName("ConnectionMode")] public string ConnectionMode { get; init; } = "COM";
    [JsonPropertyName("ComName")] public string ComName { get; init; }
    [JsonPropertyName("BraudRate")] public string BraudRate { get; init; } // Vendor typo "BraudRate" is intentional
    [JsonPropertyName("DataBits")] public string DataBits { get; init; }
    [JsonPropertyName("Parity")] public string Parity { get; init; }
}

public sealed class ConnectTcpRequestDto
{
    [JsonPropertyName("Event")] public string Event { get; init; } = "CONNECTION";
    [JsonPropertyName("Operation")] public string Operation { get; init; } = "CONNECT";
    [JsonPropertyName("ConnectionMode")] public string ConnectionMode { get; init; } = "TCP";
    [JsonPropertyName("IpAddress")] public string IpAddress { get; init; }
    [JsonPropertyName("Port")] public int Port { get; init; }
}

public sealed class DisconnectRequestDto
{
    [JsonPropertyName("Event")] public string Event { get; init; } = "CONNECTION";
    [JsonPropertyName("Operation")] public string Operation { get; init; } = "DISCONNECT";
}

public sealed class GetTerminalSettingsRequestDto
{
    [JsonPropertyName("Event")] public string Event { get; init; } = "CONNECTION";
    [JsonPropertyName("Operation")] public string Operation { get; init; } = "GET_TERMINAL_SETTINGS";
}

public sealed class TransactionNoPayloadRequestDto
{
    [JsonPropertyName("Event")] public string Event { get; init; } = "TRANSACTION";
    [JsonPropertyName("Operation")] public string Operation { get; init; }
    [JsonPropertyName("PrintSettings")] public string? PrintSettings { get; init; }
}

public sealed class ReconciliationRequestDto
{
    [JsonPropertyName("Event")] public string Event { get; init; } = "TRANSACTION";
    [JsonPropertyName("Operation")] public string Operation { get; init; } = "RECONCILIATION";
    [JsonPropertyName("PrintSettings")] public string PrintSettings { get; init; } = "1";
}

public sealed class PurchaseRequestDto
{
    [JsonPropertyName("Event")] public string Event { get; init; } = "TRANSACTION";
    [JsonPropertyName("Operation")] public string Operation { get; init; } = "PURCHASE";
    [JsonPropertyName("Amount")] public string Amount { get; init; }
    [JsonPropertyName("ECRNumber")] public string ECRNumber { get; init; }
    [JsonPropertyName("PrintSettings")] public string PrintSettings { get; init; } = "0";
    [JsonPropertyName("AppId")] public string AppId { get; init; } = "11";
}

public sealed class RefundRequestDto
{
    [JsonPropertyName("Event")] public string Event { get; init; } = "TRANSACTION";
    [JsonPropertyName("Operation")] public string Operation { get; init; } = "REFUND";
    [JsonPropertyName("Amount")] public string Amount { get; init; }
    [JsonPropertyName("ECRNumber")] public string ECRNumber { get; init; }
    [JsonPropertyName("PrintSettings")] public string PrintSettings { get; init; } = "1";
    [JsonPropertyName("AppId")] public string AppId { get; init; } = "11";
    [JsonPropertyName("RRNNumber")] public string RRNNumber { get; init; }
    [JsonPropertyName("TransactionDate")] public string TransactionDate { get; init; } // DDMMYYYY
}

public sealed class RefundWithCardNumberRequestDto
{
    [JsonPropertyName("Event")] public string Event { get; init; } = "TRANSACTION";
    [JsonPropertyName("Operation")] public string Operation { get; init; } = "REFUND_WITH_CARDNUMBER";
    [JsonPropertyName("Amount")] public string Amount { get; init; }
    [JsonPropertyName("ECRNumber")] public string ECRNumber { get; init; }
    [JsonPropertyName("PrintSettings")] public string PrintSettings { get; init; } = "1";
    [JsonPropertyName("AppId")] public string AppId { get; init; } = "11";
    [JsonPropertyName("RRNNumber")] public string RRNNumber { get; init; }
    [JsonPropertyName("TransactionDate")] public string TransactionDate { get; init; } // DDMMYYYY
    [JsonPropertyName("CardNumber")] public string CardNumber { get; init; } // Masked PAN
}

public sealed class ReversalRequestDto
{
    [JsonPropertyName("Event")] public string Event { get; init; } = "TRANSACTION";
    [JsonPropertyName("Operation")] public string Operation { get; init; } = "REVERSAL";
    [JsonPropertyName("ECRNumber")] public string ECRNumber { get; init; }
    [JsonPropertyName("Stan")] public string Stan { get; init; }
    [JsonPropertyName("AppId")] public string AppId { get; init; } = "11";
    [JsonPropertyName("PrintSettings")] public string PrintSettings { get; init; } = "1";
}

// ── Inbound Responses ─────────────────────────────────────────────────────────

public sealed class ConnectionStatusResponseDto
{
    [JsonPropertyName("IsConnected")] public string IsConnected { get; init; } // "True" | "False"
    [JsonPropertyName("ConnectionMode")] public string ConnectionMode { get; init; }
    [JsonPropertyName("Status")] public string Status { get; init; }
    [JsonPropertyName("Message")] public string Message { get; init; }
    [JsonPropertyName("Event")] public string Event { get; init; }
}

public sealed class ErrorResponseDto
{
    [JsonPropertyName("Message")] public string Message { get; init; }
    [JsonPropertyName("Reason")] public string Reason { get; init; }
    [JsonPropertyName("StackTrace")] public string? StackTrace { get; init; }
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnError";
}

public sealed class TerminalActionResponseDto
{
    [JsonPropertyName("EventName")] public string EventName { get; init; } = "TERMINAL_ACTION";
    [JsonPropertyName("TerminalAction")] public string TerminalAction { get; init; }
    [JsonPropertyName("OptionalMessage")] public string OptionalMessage { get; init; }
    [JsonPropertyName("JsonResult")] public string JsonResult { get; init; } // Stringified inner JSON
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnTerminalAction";
}

public sealed class TerminalStatusResponseDto
{
    [JsonPropertyName("EventName")] public string EventName { get; init; } = "TERMINAL_STATUS";
    [JsonPropertyName("TerminalStatus")] public string TerminalStatus { get; init; }
    [JsonPropertyName("OptionalMessage")] public string OptionalMessage { get; init; }
    [JsonPropertyName("JsonResult")] public string JsonResult { get; init; } // Stringified inner JSON
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnTerminalStatus";
}

public sealed class TerminalSettingsResponseDto
{
    [JsonPropertyName("EventName")] public string EventName { get; init; } = "TERMINAL_SETTINGS";
    [JsonPropertyName("JsonResult")] public string JsonResult { get; init; }
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnDataReceive";
}

public sealed class TerminalSettingsInnerDto
{
    [JsonPropertyName("IsMenuEnabled")] public string IsMenuEnabled { get; init; } // "0" | "1"
    [JsonPropertyName("IsPrinterEnabled")] public string IsPrinterEnabled { get; init; } // "0" | "1"
    [JsonPropertyName("BraudRate")] public string BraudRate { get; init; } // Typo "BraudRate"
}

public sealed class TerminalResponseDto
{
    [JsonPropertyName("EventName")] public string EventName { get; init; } = "TERMINAL_RESPONSE";
    [JsonPropertyName("JsonResult")] public string JsonResult { get; init; } // Stringified inner JSON
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnDataReceive";
}

public sealed class TerminalResponseInnerDto
{
    [JsonPropertyName("TransactionResponseEnglish")] public string TransactionResponseEnglish { get; init; }
    [JsonPropertyName("PrimaryAccountNumber")] public string PrimaryAccountNumber { get; init; }
    [JsonPropertyName("TransactionAmount")] public string TransactionAmount { get; init; }
    [JsonPropertyName("TransactionSTAN")] public string TransactionSTAN { get; init; }
    [JsonPropertyName("TransactionDateTime")] public string TransactionDateTime { get; init; }
    [JsonPropertyName("TransactionExpiryDate")] public string TransactionExpiryDate { get; init; }
    [JsonPropertyName("RetrievalReferenceNumber")] public string RetrievalReferenceNumber { get; init; }
    [JsonPropertyName("TransactionAuthCode")] public string TransactionAuthCode { get; init; }
    [JsonPropertyName("TransactionResponse")] public string TransactionResponse { get; init; } // 3-digit SAMA code
    [JsonPropertyName("CardAcceptorTerminalId")] public string CardAcceptorTerminalId { get; init; }
    [JsonPropertyName("CardAcceptorName")] public string CardAcceptorName { get; init; }
    [JsonPropertyName("CardAcceptorIdCode")] public string CardAcceptorIdCode { get; init; }
    [JsonPropertyName("AdditionalAmount")] public string AdditionalAmount { get; init; }
    [JsonPropertyName("CardAcceptorBusinessCode")] public string CardAcceptorBusinessCode { get; init; }
    [JsonPropertyName("BankId")] public string BankId { get; init; }
    [JsonPropertyName("DateTime")] public string DateTime { get; init; }
    [JsonPropertyName("EMVTagsIssuer")] public string EMVTagsIssuer { get; init; }
    [JsonPropertyName("TransactionType")] public string TransactionType { get; init; }
    [JsonPropertyName("MerchantInfo")] public string MerchantInfo { get; init; }
    [JsonPropertyName("ResponseCodeDescription")] public string ResponseCodeDescription { get; init; }
    [JsonPropertyName("ECRReferenceNumber")] public string ECRReferenceNumber { get; init; }
    [JsonPropertyName("TransactionResponseArabic")] public string TransactionResponseArabic { get; init; }
    [JsonPropertyName("TransactionTypeAsReadable")] public string TransactionTypeAsReadable { get; init; }
    [JsonPropertyName("TerminalVersion")] public string TerminalVersion { get; init; }
    [JsonPropertyName("MerchantNameEnglish")] public string MerchantNameEnglish { get; init; }
    [JsonPropertyName("MerchantNameArabic")] public string MerchantNameArabic { get; init; }
    [JsonPropertyName("MerchantAddress1English")] public string MerchantAddress1English { get; init; }
    [JsonPropertyName("MerchantAddress1Arabic")] public string MerchantAddress1Arabic { get; init; }
    [JsonPropertyName("MerchantAddress2English")] public string? MerchantAddress2English { get; init; }
    [JsonPropertyName("TransacitonRequestDateTime")] public string TransacitonRequestDateTime { get; init; } // Vendor typo "Transaciton"
    [JsonPropertyName("TransactionResponseDateTime")] public string TransactionResponseDateTime { get; init; }
    [JsonPropertyName("CardScheme")] public string CardScheme { get; init; }
    [JsonPropertyName("POSEntryMode")] public string POSEntryMode { get; init; }
    [JsonPropertyName("AID")] public string AID { get; init; }
    [JsonPropertyName("TVR")] public string TVR { get; init; }
    [JsonPropertyName("TSI")] public string TSI { get; init; }
    [JsonPropertyName("CryptResult")] public string CryptResult { get; init; }
    [JsonPropertyName("CVMResult")] public string CVMResult { get; init; }
    [JsonPropertyName("ApplicationCryptogram")] public string ApplicationCryptogram { get; init; }
    [JsonPropertyName("CardHolderVerificationMethod")] public string CardHolderVerificationMethod { get; init; }
    [JsonPropertyName("CardHolderVerificationMethodArabic")] public string? CardHolderVerificationMethodArabic { get; init; }
    [JsonPropertyName("CardCode")] public string CardCode { get; init; }
    [JsonPropertyName("CardNameEnglish")] public string CardNameEnglish { get; init; }
    [JsonPropertyName("CardNameArabic")] public string CardNameArabic { get; init; }
    [JsonPropertyName("TransactionResultReasonArabic")] public string? TransactionResultReasonArabic { get; init; }
    [JsonPropertyName("TransactionResultReasonEnglish")] public string TransactionResultReasonEnglish { get; init; }
}

public sealed class WriteEventResponseDto
{
    [JsonPropertyName("EventName")] public string EventName { get; init; }
    [JsonPropertyName("JsonResult")] public string JsonResult { get; init; }
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnWrite";
}

public sealed class GenericEventDataResponseDto
{
    [JsonPropertyName("EventName")] public string EventName { get; init; }
    [JsonPropertyName("JsonResult")] public string? JsonResult { get; init; }
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnDataReceive";
}

public sealed class MerchantInfoResponseDto
{
    [JsonPropertyName("EventName")] public string EventName { get; init; } = "MERCHANT_INFO";
    [JsonPropertyName("JsonResult")] public string JsonResult { get; init; }
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnDataReceive";
}

public sealed class TerminalIdResponseDto
{
    [JsonPropertyName("EventName")] public string EventName { get; init; } = "TERMINAL_ID";
    [JsonPropertyName("JsonResult")] public string JsonResult { get; init; }
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnDataReceive";
}

public sealed class AvailablePortsResponseDto
{
    [JsonPropertyName("EventName")] public string EventName { get; init; } = "AVAILABLE_PORTS";
    [JsonPropertyName("JsonResult")] public string JsonResult { get; init; }
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnDataReceive";
}

public sealed class CheckStatusResponseDto
{
    [JsonPropertyName("EventName")] public string EventName { get; init; } = "CHECK_STATUS";
    [JsonPropertyName("JsonResult")] public string JsonResult { get; init; }
    [JsonPropertyName("Event")] public string Event { get; init; } = "OnDataReceive";
}
