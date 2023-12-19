using sabatex.V1C8.BankHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace sabatex.Tests.BankHelper
{
    public class BankStreamConverterTests
    {
        const string testFilePath = @"C:\Users\serhi\OneDrive\DataBases\BankHelper";

        [Theory]
        [InlineData("26005034006185", testFilePath + "/PrimaBankSK.csv", EBankType.PrimaBankSK, "PrimaBankSK")]
        [InlineData("26005034006185", testFilePath + "/OtpBankSK.csv", EBankType.OtpBankSK, "OtpBankSK")]
        [InlineData("UA923052990000026008050334389", testFilePath + "/Privat24.csv", EBankType.PrivatUA, "Privat24")]
        [InlineData("26005034006185", testFilePath + "/iBankUA.csv", EBankType.iBankUA_TXT, "IBankUA")]
        [InlineData("", testFilePath+ "/OTPBank_210211.zip", EBankType.iFobsUA_XML, "iFobsXML")]
        [InlineData("", testFilePath + "/Львів/CB_to_1C_20210101-20210309.zip", EBankType.iFobsUA_XML, "iFobsXML")]
        [InlineData("26005034006185", testFilePath + "/iFobsEximBank.dat", EBankType.iFobsUA_TXT, "EXIMBank")]
        [InlineData("", testFilePath + "/iFobsEximBank.dat", EBankType.iFobsUA_TXT, "EXIMBank without accouunt")]
        [InlineData("26005034006186", testFilePath + "/iFobsExim191004.dat", EBankType.iFobsUA_TXT, "EXIMBank wrong account")]
        [InlineData("", testFilePath + @"\GASBank\account_statement_21122021-21122021_221220211532.csv", EBankType.GAZBank_CSV, "GazBank with account")]

        public void ConvertTo1CFormatNew(string accNumber, string FileName, EBankType bankType, string name)
        {
            using (Stream stream = File.OpenRead(FileName))
            {
                        var result = _1CClientBankExchange.ConvertTo1CFormat(bankType,stream, accNumber);
                        Assert.NotNull(result);
            }
 
        }


        [Theory]
        [InlineData(testFilePath + "/OTPBank_210211.zip")]
        [InlineData(testFilePath + "/Львів/CB_to_1C_20210101-20210309.zip")]
        [InlineData(testFilePath + "/iFobsEximBank.dat")]
        [InlineData(testFilePath + "/iFobsExim191004.dat")]

        public async Task Test_iFobs(string fileName)
        {
            using (Stream stream = File.OpenRead(fileName))
            {
                var iFobs = (new _1CClientBankExchange()) as IiFobs;
                await iFobs.ImportFromFileAsync(stream, Path.GetExtension(fileName));

                string s = iFobs.GetAsXML();
                Assert.True(iFobs.Count()>0);
            }

        }



        //[Theory]
        //[InlineData("Try get value from end stream !!!", "Спроба прочитати значення в кінці потоку !!!")]
        //[InlineData("Try get value from end stream !!", "Try get value from end stream !!")]
        //public void TestLocalizer(string engValue,string ukrValue)
        //{
        //    CultureInfo.CurrentCulture = new CultureInfo("uk");
        //    var localizedString = Localizer.Localize(engValue);
        //    Assert.Equal(localizedString, ukrValue);

        //}

    }
}