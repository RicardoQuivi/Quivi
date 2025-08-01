using System.Runtime.Serialization;

namespace Paybyrd.Api.Models
{
    public enum PaymentMethod
    {
        [EnumMember(Value = "card")]
        Card,
        [EnumMember(Value = "ideal")]
        iDeal,
        [EnumMember(Value = "multibanco")]
        Multibanco,
        [EnumMember(Value = "mbway")]
        MbWay,
        [EnumMember(Value = "multicaixa")]
        MultiCaixa,
        [EnumMember(Value = "pickup")]
        Pickup,
        [EnumMember(Value = "paypal")]
        PayPal,
        [EnumMember(Value = "banktransfer")]
        BankTransfer,
        [EnumMember(Value = "floa")]
        Floa,
        [EnumMember(Value = "pix")]
        Pix,
    }
}