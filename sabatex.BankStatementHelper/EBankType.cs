using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sabatex.V1C8.BankHelper
{
    public enum EBankType
    {
        [Display(Name ="iFobs, file with TXT format")]
        iFobsUA_TXT,
        [Display(Name = "iFobs,  file with XML format")]
        iFobsUA_XML,
        [Display(Name = "iBankUA,  file with CSV format")]
        iBankUA_TXT,
        [Display(Name = "Prima Banka SK, file with CSV format")]
        PrimaBankSK,
        [Display(Name = "OTP Bank SK' file with CSV format")]
        OtpBankSK,
        [Display(Name = "Privat Bank, file with CSV format")]
        PrivatUA,
        iFobs,
        GAZBank_CSV

    }
}
