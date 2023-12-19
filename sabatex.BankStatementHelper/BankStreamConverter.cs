using sabatex.Extensions.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace sabatex.V1C8.BankHelper
{
    public class BankStreamConverter
    {
        protected const int BufferSize = 1024;
        protected virtual string delimiter { get; } = "\r\n";

        public int LineDoc { get; set; } = 0;


        //protected int LineDoc = 0;
        public List<DocumentSection> Documents { get;}
        public BankStreamConverter()
        {
            Documents = new List<DocumentSection>();
        }

        protected const string UnknownFormatFile = "Unknown format file, check file format!";
        public virtual string HeaderErrorMsg { get => "The input file header {0} not mach in original file header {1}"; }
 
        public string GetAsXML()
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
                result.AppendLine(string.Format("        <КодВалюты>{0}</КодВалюты>", doc.КодВалюты));
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

        public virtual DocumentSection GetDocument(string s, string AccCode) { return null; }

        /// <summary>
        /// Convert text format to xml string 1C
        /// </summary>
        /// <param name="stream">Stream to file</param>
        /// <param name="AccNumber">bank account number to convert</param>
        /// <returns></returns>
        public async Task<string> ConvertTo1CFormat(Stream stream, string AccNumber)
        {
            using (StreamReader reader = new StreamReader(stream, new Encoding1251()))
            {
                LineDoc = 1;
                chars = 0;
                _1CClientBankExchange _1CClientBank = new _1CClientBankExchange();
                try
                {
                    do
                    {
                        var lineStr = await GetLineFromStreamAsync(reader).ConfigureAwait(false);
                        if (chars == 0 || lineStr.Length == 0)
                            continue;
                        var doc = GetDocument(lineStr, AccNumber);
                        if (doc != null) _1CClientBank.Documents.Add(doc);
                    } while (chars != 0);

                    //var result = await GetDocumentsAsync(reader, AccNumber).ConfigureAwait(false);
                    return _1CClientBank.GetAsXML();
                }
                catch (Exception e)
                {
                        throw new Exception(ErrorStrings.InLine(LineDoc, e.Message));
                }
            }
        }
 


        char[] buffer = new char[BufferSize];
        // chars in buffer
        int chars = 0;
        protected virtual async Task<string>  GetLineFromStreamAsync(StreamReader stream)
        {
            bool checkEnd(int position)
            {
                foreach (var c in delimiter)
                {
                     if (buffer[position++] != c) return false;
                }
                return true;
            }

            int readChars = await stream.ReadAsync(buffer, chars, BufferSize - chars).ConfigureAwait(false);
            if (readChars == 0 && chars == 0)
                return string.Empty;
            int pos = 0;
            chars += readChars;
            var result = new StringBuilder();
            while (pos < chars - 1)
            {
                if (pos >= BufferSize - 24)
                    throw new Exception(ErrorStrings.StrinLenght(BufferSize - 24));


                if (checkEnd(pos))
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

            //throw new Exception(Localize("Not find end string in file !!!"));
        }

    }
}
