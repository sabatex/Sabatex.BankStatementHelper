﻿using sabatex.Extensions.ClassExtensions;
using sabatex.Extensions.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace sabatex.V1C8.BankHelper
{
    public interface IPrimaBankSk : I1CClientBankExchange
    {
        const string delimiter = "\"\r\n";
        const int BufferSize = 1024;

        bool checkEnd(int position, ref char[] buffer)
        {
            foreach (var c in delimiter)
            {
                if (buffer[position++] != c) return false;
            }
            return true;
        }


        string getValue(ref int pos, string s, string valueName)
        {
            if (pos==s.Length)
                throw new Exception(ErrorStrings.TryGetValueFromEndStream(valueName));
            if (s[pos] == '\n' || s[pos] == '\r')
                throw new Exception(ErrorStrings.TryGetValueFromEndStream(valueName));

           
            int start = pos;
            if (s[pos] == '"')
            {
                pos++;
                start = pos;
            }
            else
            {
                pos = s.IndexOf(';', start);
                if (pos == -1)
                    throw new Exception(ErrorStrings.DetermineEndDelimiterForValue(valueName));
                return s.Substring(start, pos++ - start);
            }

            var result = new StringBuilder();
            // special string
            while (pos < s.Length)
            {
                switch (s[pos])
                {
                    case '"':
                        pos++;
                        if (pos == s.Length)
                            return result.ToString(); 

                        if (s[pos] == ';')
                        {
                            pos++;
                            return result.ToString();
                        }

                        if (s[pos] == '"')
                        {
                            result.Append('"');
                            pos++;
                            break;
                        }
                        throw new Exception(ErrorStrings.StringFormatErrorForValue(valueName));
                    default:
                        result.Append(s[pos]);
                        pos++;
                        break;

                }
            }
            throw new Exception(ErrorStrings.DetermineEndDelimiterForValue(valueName));
        }



        DocumentSection GetDocument(string s, string AccCode)
        {
            var doc = new DocumentSection();
            Regex iban = new Regex(@"^\w{2}\d{10}\d*");
            int pos = 0;
            string Datum = getValue(ref pos,s, "Datum");
            if (Datum == "Datum") return null;//The header
            string Valuta = getValue(ref pos,s, "Valuta");
            string Suma = getValue(ref pos, s, "Suma").Replace(" ", "");
            string Mena = getValue(ref pos, s, "Mena").ToUpper();
            string Popis = getValue(ref pos, s, "Popis");
            string DopolnitelnaInformacia = getValue(ref pos, s, "Doplnujuca informacia");
            string Zostatok = getValue(ref pos, s, "Zostatok po transakcii");
            if (Valuta == string.Empty) return null;

            try
            {
                doc.Дата = Valuta.DateTo1C8Date();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("{0} for parametr Valuta", e.Message));
            }

            doc.КодВалюты = Mena;
            decimal summ;
            if (Suma.TryToDecimal(out summ))
                doc.Сумма = summ;
            //doc.ДокументИД = 
            doc.НазначениеПлатежа = Popis;
            //doc.Номер =  doc.Дата + summ.ToString();
            var r = iban.Match(Popis);
            string Protistrana = r.Success ? r.Value : "";

            if (summ >= 0)
            {
                doc.ПлательщикСчет = Protistrana;
                doc.ПолучательСчет = AccCode;
            }
            else
            {
                doc.ПолучательСчет = Protistrana;
                doc.ПлательщикСчет = AccCode;
            }

            return doc;

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
                    return result.Append('"').ToString();
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
