﻿using sabatex.Extensions.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;



namespace sabatex.V1C8.BankHelper
{
    public interface IiFobsTXT : I1CClientBankExchange
    {
        const int BlockSize = 660;
        const int BufferSize = 1024;
        const string delimiter = "\r\n";

        string GetSubstring(string s, int start, int end)
        {
            return s.Substring(start - 1, end - start + 1).Trim();
        }
        string convertDate(string str)
        {
            return "20" + str.Substring(0, 2) + "-" + str.Substring(2, 2) + "-" + str.Substring(4, 2);
        }
        bool checkEnd(int position, ref char[] buffer)
        {
            foreach (var c in delimiter)
            {
                if (buffer[position++] != c) return false;
            }
            return true;
        }

        DocumentSection GetDocument(string s, string AccCode)
        {
            if (s.Length != BlockSize)
                throw new Exception(ErrorStrings.UnsupportedFormatFileForiFobs());

            // МФО банку платника 1-9 len=9
            string bankID = GetSubstring(s, 1, 9);
            // Номер рахунку платника 10-28 len=19
            string accountNumberOfPayer = GetSubstring(s, 10, 28);
            // IBAN платника 29-57 len=29
            string iBANOfPayer = GetSubstring(s, 29, 57);
            // МФО банку одержувача 58-66 len= 9
            string beneficiarysDankID = GetSubstring(s, 58, 66);
            // Номер рахунку одержувача 67-85 len=19
            string accountNumberOfBeneficiary = GetSubstring(s, 67, 85);
            // IBAN одержувача 86-114 len=29
            var iBANofBeneficiary = GetSubstring(s, 86, 114);
            // Тип фінансової операції. 1 (кредит) 0 дебет 115-115 len=1
            var typeOfFinancialOperation = GetSubstring(s, 115, 115);
            // Сума (у копійках) 116-131 len=16
            var amount = GetSubstring(s, 116, 131);
            // Вид документа 132-133 len=2
            var documentType = GetSubstring(s, 132, 133);
            // Номер документа 134-143 len=10
            var documentNumber = GetSubstring(s, 134, 143);
            // Код валюти 144-146
            var codeOfCurrency = GetSubstring(s, 144, 146);
            // Дата документа 147-152
            var documentDate = GetSubstring(s, 147, 152);
            // Дата отримання документа в банку 153-158
            var dateWhenDocumentWasReceivedInTheBank = GetSubstring(s, 153, 158);
            // Найменування платника 159-196
            var nameOfPayer = GetSubstring(s, 159, 196);
            // Найменування одержувача 197-234
            var nameOfBeneficiary = GetSubstring(s, 197, 234);
            // Призначення платежу 235-395
            var purposeOfPayment = GetSubstring(s, 235, 395);
            // Додаткові реквізити 396-454
            var additionalRequisites = GetSubstring(s, 396, 454);
            // Код призначення платежу 455-457
            var codeOfPurposeOfPayment = GetSubstring(s, 455, 457);
            // ------------ 458-459
            // Ідентифікаційний код платника (код ЄДРПОУ) 460-473
            var taxpayerNumberOfPayer = GetSubstring(s, 460, 473);
            // Ідентифікаційний код одержувача (код ЄДРПОУ) 474-487
            var taxpayerNumberOfBeneficiary = GetSubstring(s, 474, 487);
            // Унікальний номер документа 488-496
            var documentIdentifier = GetSubstring(s, 488, 496);
            // Дата создания документа 655-660
            var documentCreateDate = GetSubstring(s, 655, 660);

            if (documentType == "1")
            {
                DocumentSection doc = new DocumentSection();
                doc.Номер = documentNumber; //Номер документа
                doc.КодВалюты = codeOfCurrency;// Код валюты
                doc.НазначениеПлатежа = purposeOfPayment; //Назначение платежа

                if (documentCreateDate.Length == 6)
                {
                    doc.Дата = convertDate(documentCreateDate);
                }
                else if (documentDate.Length == 6)
                {
                    doc.Дата = convertDate(documentDate); ;
                }
                else
                {
                    throw new Exception();
                }

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
        string GetLineFromStream(StreamReader stream, char[] buffer, ref int chars)
        {
            int readChars = stream.Read(buffer, chars, BufferSize - chars);
            if (readChars == 0 && chars == 0)
                return string.Empty;
            int pos = 0;
            chars += readChars;
            var result = new StringBuilder();
            while (pos < chars - 1)
            {
                if (pos >= BufferSize - 24)
                    throw new Exception(ErrorStrings.StrinLenght(BufferSize - 24));


                if (checkEnd(pos, ref buffer))
                {
                    chars = chars - pos - delimiter.Length;
                    if (chars != 0)
                        Array.Copy(buffer, pos + delimiter.Length, buffer, 0, chars);
                    return result.ToString();
                }
                result.Append(buffer[pos]);
                pos++;
            }
            chars = 0;
            return result.ToString();
        }
        public string ConvertTo1CFormat(Stream stream, string AccNumber)
        {
            using (StreamReader reader = new StreamReader(stream, new Encoding1251()))
            {
                var lineDoc = 1;
                var chars = 0;
                char[] buffer = new char[BufferSize];
                _1CClientBankExchange _1CClientBank = new _1CClientBankExchange();
                try
                {
                    do
                    {
                        var lineStr = GetLineFromStream(reader, buffer, ref chars);
                        if (chars == 0 || lineStr.Length == 0)
                            continue;
                        var doc = GetDocument(lineStr, AccNumber);
                        if (doc != null) _1CClientBank.Documents.Add(doc);
                        lineDoc++;
                    } while (chars != 0);
                    return _1CClientBank.GetAsXML();
                }
                catch (Exception e)
                {
                    throw new Exception(ErrorStrings.InLine(lineDoc, e.Message));
                }
            }
        }
    }
}
