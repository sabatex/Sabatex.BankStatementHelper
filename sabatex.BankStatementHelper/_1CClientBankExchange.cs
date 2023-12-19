using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;
using System.Text.RegularExpressions;
using sabatex.Extensions.Text;
using sabatex.Extensions.ClassExtensions;
using System.Xml.Serialization;

namespace sabatex.V1C8.BankHelper
{

    /// <summary>
    /// Внутренний признак файла обмена
    /// </summary>
    public class _1CClientBankExchange: I1CClientBankExchange,IiFobsXML,IiFobsTXT,IiBankUA_TXT, IPrivatUA, IOtpBankSK,IPrimaBankSk,IiFobs,IGAZBankCSV
    {
        public _1CClientBankExchange()
        {
            Documents = new List<DocumentSection>();
            CurrencyCode = new Dictionary<string, int>()
            {
                {"EUR", 978 },
                {"RUB", 643 },
                {"UAH", 980 },
                {"USD", 840 }
            };
        }
        /// <summary>
        /// Поточна лінія обробляємого документа
        /// </summary>
        public int LineDoc { get; set; } = 1;

        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<DocumentSection> Documents { get; set; }
        private static Dictionary<string,int> CurrencyCode { get; set; }

        public static string[] supportFileTupe = new string[]
        {
            "XML","DAT","CSV"
        };

        public int Count() => Documents.Count();
        public string GetAsXML1()
        {
            // Процедура выгружает платежные поручения в XML.
            StringBuilder result = new StringBuilder();
            result.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            result.AppendLine("<_1CClientBankExchange xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            result.AppendLine("<ВерсияФормата>2.00</ВерсияФормата>");
            result.AppendLine("<Отправитель>bilart.co</Отправитель>");
            result.AppendLine("<Получатель>1C8.X</Получатель>");
            foreach (DocumentSection doc in Documents)
            {
                result.AppendLine("    <СекцияДокумент>");
                result.AppendLine(string.Format("        <ВидДокумента>{0}</ВидДокумента>", doc.ВидДокумента));
                result.AppendLine(string.Format("        <Номер>{0}</Номер>", doc.Номер));
                result.AppendLine(string.Format("        <Дата>{0}</Дата>", doc.Дата));
                result.AppendLine(string.Format("        <ДокументИД>{0}</ДокументИД>", doc.ДокументИД));
                result.AppendLine(string.Format("        <Сумма>{0}</Сумма>", doc.Сумма.ToString("#############0.00")));
                result.AppendLine(string.Format("        <КодВалюты>{0}</КодВалюты>", CurrencyCode.TryGetValue(doc.КодВалюты, out int value) ? value.ToString() : doc.КодВалюты));
                result.AppendLine(string.Format("        <ПлательщикСчет>{0}</ПлательщикСчет>", doc.ПлательщикСчет));
                result.AppendLine(string.Format("        <Плательщик>{0}</Плательщик>", doc.Плательщик));
                result.AppendLine(string.Format("        <ПлательщикОКПО>{0}</ПлательщикОКПО>", doc.ПлательщикОКПО));
                result.AppendLine(string.Format("        <ПлательщикМФО>{0}</ПлательщикМФО>", doc.ПлательщикМФО));
                result.AppendLine(string.Format("        <ПолучательСчет>{0}</ПолучательСчет>", doc.ПолучательСчет));
                result.AppendLine(string.Format("        <Получатель>{0}</Получатель>", doc.Получатель));
                result.AppendLine(string.Format("        <ПолучательБанк>{0}</ПолучательБанк>", doc.ПолучательБанк));
                result.AppendLine(string.Format("        <ПолучательМФО>{0}</ПолучательМФО>", doc.ПолучательМФО));
                result.AppendLine(string.Format("        <ПолучательОКПО>{0}</ПолучательОКПО>", doc.ПолучательОКПО));
                result.AppendLine(string.Format("        <НазначениеПлатежа>{0}</НазначениеПлатежа>", doc.НазначениеПлатежа));
                //result.AppendLine(string.Format("        <ДатаПоступило>{0}</ДатаПоступило>", doc.ДатаПоступило));

                result.AppendLine("    </СекцияДокумент>");
            }


            result.AppendLine("</_1CClientBankExchange>");
            return result.ToString();
        }
        public string GetAsXML()
        {
            try
            {
                StringWriter sw = new StringWriter();
                XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Encoding = Encoding.UTF8 });
                xmlWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\"");
                xmlWriter.WriteStartElement("_1CClientBankExchange");
                xmlWriter.WriteAttributeString("xmlns","xsi",null, "http://www.w3.org/2001/XMLSchema-instance");
                xmlWriter.WriteElementString("ВерсияФормата", "2.0");
                xmlWriter.WriteElementString("Отправитель", "sabatex");
                xmlWriter.WriteElementString("Получатель", "1C8.3");
                foreach (DocumentSection doc in Documents)
                {
                    xmlWriter.WriteStartElement("СекцияДокумент");
                    xmlWriter.WriteElementString("ВидДокумента", doc.ВидДокумента);
                    xmlWriter.WriteElementString("Номер", doc.Номер);
                    xmlWriter.WriteElementString("Дата", doc.Дата);
                    xmlWriter.WriteElementString("ДокументИД", doc.ДокументИД);
                    xmlWriter.WriteElementString("Сумма", doc.Сумма.ToString("#############0.00"));
                    xmlWriter.WriteElementString("КодВалюты", CurrencyCode.TryGetValue(doc.КодВалюты, out int value) ? value.ToString() : doc.КодВалюты);
                    xmlWriter.WriteElementString("ПлательщикСчет", doc.ПлательщикСчет);
                    xmlWriter.WriteElementString("Плательщик", doc.Плательщик);
                    xmlWriter.WriteElementString("ПлательщикОКПО", doc.ПлательщикОКПО);
                    xmlWriter.WriteElementString("ПлательщикМФО", doc.ПлательщикМФО);
                    xmlWriter.WriteElementString("ПолучательСчет", doc.ПолучательСчет);
                    xmlWriter.WriteElementString("Получатель", doc.Получатель);
                    xmlWriter.WriteElementString("ПолучательБанк", doc.ПолучательБанк);
                    xmlWriter.WriteElementString("ПолучательМФО", doc.ПолучательМФО);
                    xmlWriter.WriteElementString("ПолучательОКПО", doc.ПолучательОКПО);
                    xmlWriter.WriteElementString("НазначениеПлатежа", doc.НазначениеПлатежа);
                    xmlWriter.WriteEndElement();
                }



                xmlWriter.WriteEndElement();

                xmlWriter.Close();
                return sw.ToString();
            }
            catch (Exception e)
            {
                var s = e.Message;
                throw new Exception(s);
            }
        }

        public static string ConvertTo1CFormat(EBankType bankType,  Stream stream, string AccNumber = "")
        {
            var converter = new _1CClientBankExchange();
            switch (bankType)
            {
                case EBankType.iFobsUA_XML:
                    return (converter as IiFobsXML).ConvertTo1CFormat(stream, AccNumber);
                case EBankType.iFobsUA_TXT:
                    return (converter as IiFobsTXT).ConvertTo1CFormat(stream, AccNumber);
                case EBankType.iBankUA_TXT:
                    return (converter as IiBankUA_TXT).ConvertTo1CFormat(stream, AccNumber);
                case EBankType.PrivatUA:
                    return (converter as IPrivatUA).ConvertTo1CFormat(stream, AccNumber);
                case EBankType.OtpBankSK:
                    return (converter as IOtpBankSK).ConvertTo1CFormat(stream, AccNumber);
                case EBankType.PrimaBankSK:
                    return (converter as IPrimaBankSk).ConvertTo1CFormat(stream, AccNumber);
                case EBankType.GAZBank_CSV:
                    return (converter as IGAZBankCSV).ConvertTo1CFormat(stream, AccNumber);

                default:
                    throw new Exception(ErrorStrings.ErrorUnsupportBank(bankType));
            }
        }
        public async Task<string> ImportFromSlovakBankCSV(Stream stream,string AccCode)
        {
            string GetValueFromLine(string s, ref int pos)
            {
                if (pos == s.Length - 1) return "";

                StringBuilder res = new StringBuilder();
                bool br = false;
                while (pos < s.Length)
                {
                    char c = s[pos++];
                    switch (c)
                    {
                        case ';':
                            if (!br) return res.ToString().Trim();
                            break;
                        case '"':
                            if (!br)
                            {
                                br = true;
                                continue;
                            }
                            else
                            {
                                br = false;
                                continue;
                            }
                    }
                    res.Append(c);

                }
                return res.ToString().Trim();
            }
            string HeaderFile = "Dátum útovania;Dátum valuty;Protistrana;Kód banky;Komentár;CS;VS;SS;DB/CR;Suma;Mena";

            Documents.Clear();
            using (StreamReader reader = new StreamReader(stream, new Encoding1251()))
            {
                // get header
                string str = await reader.ReadLineAsync();
                if (str != HeaderFile)
                    return await Task.FromResult("The input file not mach header - " + HeaderFile);

                // first line
                while (!reader.EndOfStream)
                {
                    var doc = new DocumentSection();
                    str = await reader.ReadLineAsync();
                    int pos = 0;
                    string Dátumútovania = GetValueFromLine(str, ref pos);
                    string Dátumvaluty = GetValueFromLine(str, ref pos);
                    string Protistrana = GetValueFromLine(str, ref pos);
                    string Kódbanky = GetValueFromLine(str, ref pos);
                    string Komentár = GetValueFromLine(str, ref pos);
                    string CS = GetValueFromLine(str, ref pos);
                    string VS = GetValueFromLine(str, ref pos);
                    string SS = GetValueFromLine(str, ref pos);
                    string DB_CR = GetValueFromLine(str, ref pos).ToUpper();
                    string Suma = GetValueFromLine(str, ref pos).Replace(" ","");
                    string Mena = GetValueFromLine(str, ref pos).ToUpper();

                    try
                    {
                        doc.Дата = Dátumvaluty.DateTo1C8Date();
                    }
                    catch (Exception e)
                    {
                        return await Task.FromResult(string.Format(e.Message + " in line {0}", str));
                    }

                    doc.КодВалюты = Mena;
                    decimal summ;
                    if (Suma.TryToDecimal(out summ))
                        doc.Сумма = summ;
                    //doc.ДокументИД = 
                    doc.НазначениеПлатежа = Komentár;
                    //doc.Номер =  doc.Дата + summ.ToString();
                    if (DB_CR == "DEBIT")
                    {
                        doc.ПлательщикСчет = Protistrana;
                        doc.ПолучательСчет = AccCode;
                    }
                    else
                    {
                        doc.ПолучательСчет = Protistrana;
                        doc.ПлательщикСчет = AccCode;
                    }
                    Documents.Add(doc);

                }
            }
            return await  Task.FromResult("ok");
        }

        /// <summary>
        /// continue import from prima bank sk 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="AccCode"></param>
        public async void ImportFrompPrimaBankSK_CSV(Stream stream, string AccCode)
        {
            string GetValueFromLine(string s, ref int pos)
            {
                if (pos == s.Length - 1) return "";

                StringBuilder res = new StringBuilder();
                bool br = false;
                while (pos < s.Length)
                {
                    char c = s[pos++];
                    switch (c)
                    {
                        case ';':
                            if (!br) return res.ToString().Trim();
                            break;
                        case '"':
                            if (!br)
                            {
                                br = true;
                                continue;
                            }
                            else
                            {
                                br = false;
                                continue;
                            }
                    }
                    res.Append(c);

                }
                return res.ToString().Trim();
            }

            Documents.Clear();
            using (StreamReader reader = new StreamReader(stream))
            {
                // get header
                string st = await reader.ReadToEndAsync();
                var slrarray = st.Split(new string[] { "\r\n" },StringSplitOptions.RemoveEmptyEntries);
                Regex iban = new Regex(@"^\w{2}\d{10}\d*");
                // first line
                foreach (string str in slrarray)
                {
                    if (str.Trim() == string.Empty) continue;
                    if (str.Substring(1, 5) == "Datum") continue;
                    var doc = new DocumentSection();
                    //str = await reader.ReadLineAsync();
                    int pos = 0;
                    string Datum = GetValueFromLine(str, ref pos).Replace("\"","");
                    string Valuta = GetValueFromLine(str, ref pos).Replace("\"", "");
                    string Suma = GetValueFromLine(str, ref pos).Replace(" ", "");
                    string Mena = GetValueFromLine(str, ref pos).Replace("\"", "").ToUpper();
                    string Popis = GetValueFromLine(str, ref pos).Replace("\"", "");

                    if (Valuta == string.Empty) continue;
                   

                    try
                    {
                        doc.Дата = Valuta.DateTo1C8Date();
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format(e.Message + " in line {0}", str));
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
                    Documents.Add(doc);

                }
            }
            await Task.Yield();
        }
        public void AddDocument(DocumentSection documentSection)
        {
            Documents.Add(documentSection);
        }

        const string OtpBankSKFileHeader = "Dátum útovania;Dátum valuty;Protistrana;Kód banky;Komentár;CS;VS;SS;DB / CR;Suma;Mena";
        const string PrimaBankSKFileHeader = "Dátum útovania;Dátum valuty;Protistrana;Kód banky;Komentár;CS;VS;SS;DB / CR;Suma;Mena";
        const string iBankUAFileHeader = "Dátum útovania;Dátum valuty;Protistrana;Kód banky;Komentár;CS;VS;SS;DB / CR;Suma;Mena";
    }



}
