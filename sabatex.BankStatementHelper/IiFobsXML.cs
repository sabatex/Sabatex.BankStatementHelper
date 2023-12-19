using sabatex.Extensions.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace sabatex.V1C8.BankHelper
{
    public interface IiFobsXML:I1CClientBankExchange
    {
        string convertDate(string str)
        {
            return str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2);
        }
        DocumentSection GetDocument(XmlNode node, string AccNumber,string docDate)
        {

            //ARCDATE
            // Дата создания документа 655-660
            var documentCreateDate = node.Attributes["ARCDATE"].Value;
            // Номер документа 134-143 len=10
            var documentNumber = node.Attributes["DOCUMENTNO"].Value;
            // Номер рахунку платника 10-28 len=19
            string accountNumberOfPayer = node.Attributes["ACCOUNTNO"].Value; 
            // IBAN платника 29-57 len=29
            string iBANOfPayer = node.Attributes["IBAN"].Value;
            // Код валюти 144-146
            var codeOfCurrency = node.Attributes["CURRENCYID"].Value;
            // МФО банку платника 1-9 len=9
            string bankID = node.Attributes["BANKID"].Value;
            // МФО банку одержувача 58-66 len= 9
            string beneficiarysDankID = node.Attributes["CORRBANKID"].Value;
            // Номер рахунку одержувача 67-85 len=19
            string accountNumberOfBeneficiary = node.Attributes["CORRACCOUNTNO"].Value;
            // IBAN одержувача 86-114 len=29
            var iBANofBeneficiary = "";
            try
            {
                iBANofBeneficiary = node.Attributes["CORRIBAN"].Value;
            }
            catch { }
            // Тип фінансової операції. 1 (кредит) 0 дебет 115-115 len=1
            var typeOfFinancialOperation = node.Attributes["OPERATIONID"].Value;
            // Дата отримання документа в банку 153-158
            var dateWhenDocumentWasReceivedInTheBank = node.Attributes["BANKDATE"].Value;
            // символьна назва валюти
            var symbolCodeOfCurrency = node.Attributes["CURRSYMBOLCODE"].Value;
            
            //node.Attributes["DOCSUBTYPhSNAME"].Value;

            // Призначення платежу 235-395
            var purposeOfPayment = node.Attributes["PLATPURPOSE"].Value;
            // Дата документа 147-152
            var documentDate = node.Attributes["DOCUMENTDATE"].Value;

            //node.Attributes["CORRBANKNAME"].Value;

            // Ідентифікаційний код одержувача (код ЄДРПОУ) 474-487
            var taxpayerNumberOfBeneficiary = node.Attributes["CORRIDENTIFYCODE"].Value;
            // Найменування одержувача 197-234
            var nameOfBeneficiary = node.Attributes["CORRCONTRAGENTSNAME"].Value;
            // Ідентифікаційний код платника (код ЄДРПОУ) 460-473
            var taxpayerNumberOfPayer = node.Attributes["IDENTIFYCODE"].Value;
            // ACCDESCR="ТОВ &quot;ЗНЦ&quot;" 
            // Найменування платника 159-196
            var nameOfPayer = node.Attributes["CONTRAGENTSNAME"].Value;
            // ACCOUNTID="508443"
            // Унікальний номер документа 488-496
            var documentIdentifier = node.Attributes["ACCOUNTID"].Value;

            // BOOKEDDATE="20210104"
            // Вид документа 132-133 len=2
            var documentType = node.Attributes["DOCUMENTTYPEID"].Value;
            // DATAVERSION=""
            // SUMMAEQ="75000"
            // Сума (у копійках) 116-131 len=16
            var amount = node.Attributes["SUMMA"].Value;
            // KODPLATPURPOSE=""
            // ACC_OPERATOR="KLIMCHUKO"
            // ACC_OPERATOR_FIO="Клімчук Оксана Миколаївна"
            // COMMISSION=""
            // AMOUNTEQ="75000"
            // Додаткові реквізити 396-454
            //var additionalRequisites = GetSubstring(s, 396, 454);
            // Код призначення платежу 455-457
            //var codeOfPurposeOfPayment = GetSubstring(s, 455, 457);
            // ------------ 458-459

            if (documentType == "14" || documentType == "30" || documentType == "6")
            {
                DocumentSection doc = new DocumentSection();
                doc.Номер = documentNumber; //Номер документа
                doc.КодВалюты = codeOfCurrency;// Код валюты
                doc.НазначениеПлатежа = purposeOfPayment; //Назначение платежа
                doc.Дата = docDate;
                
                //if (documentCreateDate.Length == 8)
                //{
                //    doc.Дата = convertDate(documentCreateDate);
                //}
                //else if (documentDate.Length == 8)
                //{
                //    doc.Дата = convertDate(documentDate); ;
                //}
                //else
                //{
                //    throw new Exception(ErrorDateParse(documentCreateDate));
                //}

                //doc.ДокументИД = 
                if (typeOfFinancialOperation == "0") //Тип финансовой операции 
                {
                    doc.Плательщик = nameOfPayer; //Наименование плательщика 
                    doc.ПлательщикМФО = bankID;//МФО банка плательщика
                    doc.ПлательщикОКПО = taxpayerNumberOfPayer;//Идентификационный код плательщика (код ЕГРПОУ)
                    doc.ПлательщикСчет = iBANOfPayer.Length == 0 ? accountNumberOfPayer : iBANOfPayer; //Номер счета плательщика

                    doc.Получатель = nameOfBeneficiary; //Наименование получателя
                    doc.ПолучательМФО = beneficiarysDankID;//МФО банка получателя
                    doc.ПолучательОКПО = taxpayerNumberOfBeneficiary;//Идентификационный код плательщика (код ЕГРПОУ) 
                    doc.ПолучательСчет = iBANofBeneficiary.Length == 0 ? accountNumberOfBeneficiary : iBANofBeneficiary;//Номер счета получателя
                }
                else
                {
                    doc.Плательщик = nameOfBeneficiary; //Наименование получателя
                    doc.ПлательщикМФО = beneficiarysDankID;//МФО банка получателя
                    doc.ПлательщикОКПО = taxpayerNumberOfBeneficiary;//Идентификационный код плательщика (код ЕГРПОУ) 
                    doc.ПлательщикСчет = iBANofBeneficiary.Length == 0 ? accountNumberOfBeneficiary : iBANofBeneficiary;//Номер счета получателя


                    doc.Получатель = nameOfPayer;//Наименование плательщика
                    doc.ПолучательМФО = bankID;//МФО банка плательщика
                    doc.ПолучательОКПО = taxpayerNumberOfPayer;//Идентификационный код плательщика (код ЕГРПОУ)
                    doc.ПолучательСчет = iBANOfPayer.Length == 0 ? accountNumberOfPayer : iBANOfPayer; //Номер счета плательщика

                }
                if (decimal.TryParse(amount, out decimal sum))
                {
                    doc.Сумма = sum * 0.01m; //Сумма (в копейках)
                }
                else
                {
                    throw new Exception(ErrorStrings.DoubleParse(amount));
                }

                return doc;
            }
            return null;


        }
        /// <summary>
        /// Convert zip file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="AccNumber"></param>
        /// <returns></returns>
        public string ConvertTo1CFormat(Stream stream, string AccNumber)
        {
            using (ZipArchive zip = new ZipArchive(stream))
            {
                foreach (var xml in zip.Entries)
                {
                    var day = convertDate(new string(xml.Name.Where(f=>Char.IsDigit(f)).ToArray()));
                    using (StreamReader reader = new StreamReader(xml.Open(), new Encoding1251()))
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.Load(reader);

                         var LineDoc = 1;
                         try
                         {
                            foreach (XmlNode row in xmlDoc.SelectNodes("ROWDATA/ROW"))
                            {
                                var document = GetDocument(row, AccNumber,day);
                                if (document!=null)
                                    AddDocument(document);
                                LineDoc++;
                            } 
                            
                         }
                        catch (Exception e)
                        {
                            throw new Exception(ErrorStrings.InLine(LineDoc, e.Message));
                        }
                    }
                }
                return GetAsXML();
            }




        }
    
    
    
    
    
    }
}
