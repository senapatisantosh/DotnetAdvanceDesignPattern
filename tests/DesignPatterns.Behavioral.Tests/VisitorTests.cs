using DesignPatterns.Behavioral.Visitor;
using DesignPatterns.Behavioral.Visitor.Transactions;
using DesignPatterns.Behavioral.Visitor.Visitors;
using FluentAssertions;

namespace DesignPatterns.Behavioral.Tests;

public class VisitorTests
{
    [Fact]
    public void GeoAnomalyVisitor_DetectsHighRiskCountry()
    {
        var visitor = new GeoAnomalyVisitor("US");
        var transaction = new CardTransaction
        {
            TransactionId = "T-001",
            Amount = 100,
            OriginCountry = "XX", // High-risk
            CardLastFour = "1234",
            MerchantCategory = "Retail"
        };

        var result = transaction.Accept(visitor);

        result.IsSuspicious.Should().BeTrue();
        result.Risk.Should().Be(RiskLevel.Critical);
    }

    [Fact]
    public void GeoAnomalyVisitor_AcceptsNormalCountry()
    {
        var visitor = new GeoAnomalyVisitor("US");
        var transaction = new CardTransaction
        {
            TransactionId = "T-002",
            Amount = 50,
            OriginCountry = "US",
            CardLastFour = "5678",
            MerchantCategory = "Grocery"
        };

        var result = transaction.Accept(visitor);

        result.IsSuspicious.Should().BeFalse();
        result.Risk.Should().Be(RiskLevel.None);
    }

    [Fact]
    public void GeoAnomalyVisitor_DetectsHighRiskWireTransfer()
    {
        var visitor = new GeoAnomalyVisitor("US");
        var wire = new WireTransfer
        {
            TransactionId = "W-001",
            Amount = 50_000,
            OriginCountry = "US",
            DestinationCountry = "XX",
            SenderBankCode = "BANK-US",
            ReceiverBankCode = "BANK-XX"
        };

        var result = wire.Accept(visitor);

        result.IsSuspicious.Should().BeTrue();
        result.Risk.Should().Be(RiskLevel.Critical);
    }

    [Fact]
    public void AmountAnomalyVisitor_DetectsStructuring()
    {
        var visitor = new AmountAnomalyVisitor();
        var wire = new WireTransfer
        {
            TransactionId = "W-002",
            Amount = 9_800, // Just below $10,000 CTR threshold
            OriginCountry = "US",
            DestinationCountry = "US",
            SenderBankCode = "BANK-A",
            ReceiverBankCode = "BANK-B"
        };

        var result = wire.Accept(visitor);

        result.IsSuspicious.Should().BeTrue();
        result.Risk.Should().Be(RiskLevel.Critical);
        result.Reason.Should().Contain("structuring");
    }

    [Fact]
    public void AmountAnomalyVisitor_DetectsLargeOnlineCardPurchase()
    {
        var visitor = new AmountAnomalyVisitor();
        var card = new CardTransaction
        {
            TransactionId = "T-003",
            Amount = 7_500,
            OriginCountry = "US",
            CardLastFour = "9999",
            MerchantCategory = "Electronics",
            IsOnline = true
        };

        var result = card.Accept(visitor);

        result.IsSuspicious.Should().BeTrue();
        result.Risk.Should().Be(RiskLevel.Medium);
    }

    [Fact]
    public void AmountAnomalyVisitor_DetectsRoundCryptoAmount()
    {
        var visitor = new AmountAnomalyVisitor();
        var crypto = new CryptoTransaction
        {
            TransactionId = "C-001",
            Amount = 5_000,
            OriginCountry = "US",
            WalletAddress = "0xABCDEF1234567890",
            CryptoCurrency = "BTC",
            ExchangeName = "Binance"
        };

        var result = crypto.Accept(visitor);

        result.IsSuspicious.Should().BeTrue();
        result.Reason.Should().Contain("round-number");
    }

    [Fact]
    public void VelocityCheckVisitor_DetectsRapidTransactions()
    {
        var visitor = new VelocityCheckVisitor(TimeSpan.FromMinutes(10), maxTransactionsInWindow: 3);
        var now = DateTime.UtcNow;

        // Send 4 transactions rapidly — 4th should trigger
        for (int i = 1; i <= 4; i++)
        {
            var card = new CardTransaction
            {
                TransactionId = $"T-{i:D3}",
                Amount = 50,
                Timestamp = now.AddSeconds(i),
                OriginCountry = "US",
                CardLastFour = "1111",
                MerchantCategory = "Retail"
            };

            var result = card.Accept(visitor);

            if (i <= 3)
                result.IsSuspicious.Should().BeFalse($"transaction {i} should be within velocity limit");
            else
                result.IsSuspicious.Should().BeTrue($"transaction {i} should exceed velocity limit");
        }
    }

    [Fact]
    public void MultipleVisitors_CanAnalyzeSameTransaction()
    {
        var geoVisitor = new GeoAnomalyVisitor("US");
        var amountVisitor = new AmountAnomalyVisitor();
        var velocityVisitor = new VelocityCheckVisitor();

        var transaction = new CardTransaction
        {
            TransactionId = "T-MULTI",
            Amount = 200,
            OriginCountry = "US",
            CardLastFour = "4444",
            MerchantCategory = "Retail"
        };

        IFraudDetectionVisitor[] visitors = [geoVisitor, amountVisitor, velocityVisitor];
        var results = visitors.Select(v => transaction.Accept(v)).ToList();

        results.Should().HaveCount(3);
        results.Select(r => r.RuleName).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void CryptoTransaction_AcceptsAllVisitors()
    {
        var crypto = new CryptoTransaction
        {
            TransactionId = "C-002",
            Amount = 30_000,
            OriginCountry = "XX",
            WalletAddress = "0x1234567890ABCDEF",
            CryptoCurrency = "ETH",
            ExchangeName = "Coinbase"
        };

        var geoResult = crypto.Accept(new GeoAnomalyVisitor());
        var amountResult = crypto.Accept(new AmountAnomalyVisitor());

        geoResult.IsSuspicious.Should().BeTrue();
        amountResult.IsSuspicious.Should().BeTrue();
    }
}
