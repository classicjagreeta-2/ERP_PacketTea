using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace PacketTea.Models.Enum
{
    public enum EnumVoucherType
    {
        [Description("Contra Voucher")]
        CashPayment = 0,

        [Description("Cash Voucher")]
        Cash = 1,

        [Description("Bank Voucher")]
        Bank = 2,

        [Description("Journal Voucher")]
        Journal = 3,
    }

    public enum EnumPaymentType
    {
        [Description("Auto")]
        Auto = 0,

        [Description("Payment")]
        Payment = 1,

        [Description("Receipt")]
        Receipt = 2,

        [Description("Other")]
        Other = 3,
    }
    public enum DRCR
    {
        [Description("Debit")]
        D = 1,

        [Description("Credit")]
        C = 2,
    }
    public enum RefType
    {
        [Description("Debit Reference")]
        Debit = 11,

        [Description("Credit")]
        Credit = 12,
    }
    public enum NewVouType
    {
        [Description("Cash Voucher")]
        Cash = 01,

        [Description("Bank Voucher")]
        Bank = 02,
        [Description("Journal Voucher")]
        Journal = 03,
    }
    public enum TrueOrFalse
    {
        [Description("True")]
        T = 1,
        [Description("False")]
        F = 0
    }
    public enum YesOrNo
    {
        [Description("Yes")]
        Y = 1,
        [Description("No")]
        N = 0
    }
    public enum BhvtProvisionalType
    {
        [Description("Normal")]
        N = 0,
        [Description("Memorandum")]
        M = 1,
        [Description("Provisional")]
        Y = 2
    }
}