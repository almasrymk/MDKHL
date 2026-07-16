using SmartParking.Enums.PosTerminal;
using SmartParking.Payments.PosTerminal;
using Shouldly;
using Xunit;

namespace SmartParking.Payments;

public class GeideaEventMapperTests
{
    [Theory]
    [InlineData("OnConnect", GeideaCallbackEvent.OnConnect)]
    [InlineData("OnDisConnect", GeideaCallbackEvent.OnDisConnect)]
    [InlineData("OnError", GeideaCallbackEvent.OnError)]
    [InlineData("OnTerminalAction", GeideaCallbackEvent.OnTerminalAction)]
    [InlineData("OnTerminalStatus", GeideaCallbackEvent.OnTerminalStatus)]
    [InlineData("OnDataReceive", GeideaCallbackEvent.OnDataReceive)]
    [InlineData("OnWrite", GeideaCallbackEvent.OnWrite)]
    public void ParseCallbackEvent_ShouldMapCorrectly(string rawEvent, GeideaCallbackEvent expected)
    {
        // Act
        var result = GeideaEventMapper.ParseCallbackEvent(rawEvent);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "XUNIT0013:Theory methods must have test data", Justification = "<Pending>")]
    public void ParseCallbackEvent_ShouldFallbackToOnWrite_ForUnknownEvent()
    {
        // Act
        var result = GeideaEventMapper.ParseCallbackEvent("UnknownEvent");

        // Assert
        result.ShouldBe(GeideaCallbackEvent.OnWrite);
    }

    [Theory]
    [InlineData("SWIPE_OR_INSERT", GeideaTerminalAction.SwipeOrInsert)]
    {
        // Act & Assert
        // We'll test standard actions, Error Codes 1 and Error Codes 2
    }
*/
    [Theory]
    [InlineData("SWIPE_OR_INSERT", GeideaTerminalAction.SwipeOrInsert)]
    [InlineData("CARD_INSERTED", GeideaTerminalAction.CardInserted)]
    [InlineData("CARD_REMOVED", GeideaTerminalAction.CardRemoved)]
    [InlineData("PIN_ENTRY", GeideaTerminalAction.PinEntry)]
    [InlineData("PIN_QUIT", GeideaTerminalAction.PinQuit)]
    [InlineData("IDLE_SCREEN", GeideaTerminalAction.IdleScreen)]
    [InlineData("CONNECTING", GeideaTerminalAction.Connecting)]
    [InlineData("RECONNECT", GeideaTerminalAction.ReConnect)]
    [InlineData("SENDING_REQUEST", GeideaTerminalAction.SendingRequest)]
    [InlineData("WAITING_RESPONSE", GeideaTerminalAction.WaitingResponse)]
    [InlineData("SAF_PROCESSING", GeideaTerminalAction.SafProcessing)]
    [InlineData("CONNECTION_MADE", GeideaTerminalAction.ConnectionMade)]
    [InlineData("RECONCILIATION_SUCCESSFUL", GeideaTerminalAction.ReconciliationSuccessful)]
    [InlineData("CONNECTION_CLOSED", GeideaTerminalAction.ConnectionClosed)]
    // Error Codes 1
    [InlineData("USER_CANCELLED_AND_TIMEOUT", GeideaTerminalAction.UserCancelledAndTimeout)]
    [InlineData("CARD_READ_ERROR", GeideaTerminalAction.CardReadError)]
    [InlineData("DATA_ERROR", GeideaTerminalAction.DataError)]
    [InlineData("BLOCKED_CARD", GeideaTerminalAction.BlockedCard)]
    [InlineData("NO_ACTIVE_APPLICATION_FOUND", GeideaTerminalAction.NoActiveApplicationFound)]
    [InlineData("INSERT_CARD_ONLY_USE_SMART_CARD_READER", GeideaTerminalAction.InsertCardOnlyUseSmartCardReader)]
    [InlineData("MAXIMUM_AMOUNT_LIMIT_EXCEEDED", GeideaTerminalAction.MaximumAmountLimitExceeded)]
    [InlineData("TRANSACTION_CANCELLED", GeideaTerminalAction.TransactionCancelled)]
    // Error Codes 2
    [InlineData("WRONG_PIN", GeideaTerminalAction.WrongPin)]
    [InlineData("RETRY_LIMIT_EXCEEDED", GeideaTerminalAction.RetryLimitExceeded)]
    [InlineData("MAXIMUM_SAF_REACHED", GeideaTerminalAction.MaximumSafReached)]
    [InlineData("MAG_STRIPE_NOT_SUPPORTED", GeideaTerminalAction.MagStripeNotSupported)]
    [InlineData("CARD_SCHEME_NOT_SUPPORTED", GeideaTerminalAction.CardSchemeNotSupported)]
    [InlineData("DE_SAF_FAILED", GeideaTerminalAction.DeSafFailed)]
    [InlineData("RECONCILIATION_NO_BUSINESS", GeideaTerminalAction.ReconciliationNoBusiness)]
    [InlineData("RECONCILIATION_FAILED", GeideaTerminalAction.ReconciliationFailed)]
    [InlineData("NO_TXN_TO_REVERSE", GeideaTerminalAction.NoTxnToReverse)]
    [InlineData("TRY_CONTACTING_INTERFACE", GeideaTerminalAction.TryContactingInterface)]
    [InlineData("SUPERVISOR_PASSWORD_QUIT", GeideaTerminalAction.SupervisorPasswordQuit)]
    [InlineData("GPRS_DOWN", GeideaTerminalAction.GprsDown)]
    [InlineData("REFUND_PAN_NOT_MATCHING", GeideaTerminalAction.RefundPanNotMatching)]
    [InlineData("INVALID_PASSWORD", GeideaTerminalAction.InvalidPassword)]
    [InlineData("CARD_NOT_DETECTED", GeideaTerminalAction.CardNotDetected)]
    public void ParseTerminalAction_ShouldMapCorrectly(string rawAction, GeideaTerminalAction expected)
    {
        // Act
        var result = GeideaEventMapper.ParseTerminalAction(rawAction);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void ParseTerminalAction_ShouldFallbackToUnknown_ForUnknownAction()
    {
        // Act
        var result = GeideaEventMapper.ParseTerminalAction("INVALID_ACTION_NAME");

        // Assert
        result.ShouldBe(GeideaTerminalAction.Unknown);
    }

    [Theory]
    [InlineData("BUSY", GeideaTerminalStatus.Busy)]
    [InlineData("AVAILABLE", GeideaTerminalStatus.Available)]
    public void ParseTerminalStatus_ShouldMapCorrectly(string rawStatus, GeideaTerminalStatus expected)
    {
        // Act
        var result = GeideaEventMapper.ParseTerminalStatus(rawStatus);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void ParseTerminalStatus_ShouldFallbackToBusy_ForUnknownStatus()
    {
        // Act
        var result = GeideaEventMapper.ParseTerminalStatus("UNKNOWN_STATUS");

        // Assert
        result.ShouldBe(GeideaTerminalStatus.Busy);
    }

    [Theory]
    [InlineData("000", true, "مقبول")]
    [InlineData("000", false, "Approved")]
    [InlineData("116", true, "الرصيد غير كافٍ")]
    [InlineData("116", false, "Insufficient Funds")]
    [InlineData("117", true, "الرقم السري خاطئ")]
    [InlineData("117", false, "Incorrect PIN")]
    [InlineData("101", true, "بطاقة منتهية الصلاحية")]
    [InlineData("101", false, "Expired Card")]
    [InlineData("121", true, "تجاوز الحد المسموح به")]
    [InlineData("121", false, "Withdrawal Limit Exceeded")]
    [InlineData("200", true, "بطاقة غير صالحة")]
    [InlineData("200", false, "Invalid Card")]
    [InlineData("909", true, "عطل في النظام")]
    [InlineData("909", false, "System Malfunction")]
    [InlineData("999", true, "عملية مرفوضة")]
    [InlineData("999", false, "Declined")]
    public void GetSamaResponseDescription_ShouldLocalizeCorrectly(string code, bool isArabic, string expected)
    {
        // Act
        var result = GeideaEventMapper.GetSamaResponseDescription(code, isArabic);

        // Assert
        result.ShouldBe(expected);
    }
}
