using System;
using Shouldly;
using SmartParking.Entities.Payments;
using SmartParking.Enums.PosTerminal;
using Xunit;

namespace SmartParking.Payments;

public class PosTerminalTransactionDetailTests
{
    [Fact]
    public void Create_ShouldSetPosTransactionTypeCorrectly_ForPurchase()
    {
        var paymentId = Guid.NewGuid();
        var detail = PosTerminalTransactionDetail.Create(
            paymentId,
            "000",
            SamaResponseOutcome.Approved,
            "{}",
            PosTransactionType.Purchase,
            "TERM123",
            "000001",
            "123456789012",
            "123456",
            "411111****1111",
            "MADA"
        );

        detail.PaymentId.ShouldBe(paymentId);
        detail.TransactionType.ShouldBe(PosTransactionType.Purchase);
        detail.SamaResponseCode.ShouldBe("000");
        detail.Outcome.ShouldBe(SamaResponseOutcome.Approved);
    }

    [Fact]
    public void Create_ShouldSetPosTransactionTypeCorrectly_ForRefund()
    {
        var paymentId = Guid.NewGuid();
        var detail = PosTerminalTransactionDetail.Create(
            paymentId,
            "000",
            SamaResponseOutcome.Approved,
            "{}",
            PosTransactionType.Refund,
            "TERM123",
            "000002",
            "123456789013",
            "123457",
            "411111****1111",
            "VISA"
        );

        detail.PaymentId.ShouldBe(paymentId);
        detail.TransactionType.ShouldBe(PosTransactionType.Refund);
    }

    [Fact]
    public void Create_ShouldSetPosTransactionTypeCorrectly_ForReversal()
    {
        var paymentId = Guid.NewGuid();
        var detail = PosTerminalTransactionDetail.Create(
            paymentId,
            "000",
            SamaResponseOutcome.Approved,
            "{}",
            PosTransactionType.Reversal,
            "TERM123",
            "000003",
            "123456789014",
            "123458",
            "411111****1111",
            "MASTER"
        );

        detail.PaymentId.ShouldBe(paymentId);
        detail.TransactionType.ShouldBe(PosTransactionType.Reversal);
    }
}
