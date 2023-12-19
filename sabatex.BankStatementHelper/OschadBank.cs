﻿using sabatex.Extensions.ClassExtensions;
using sabatex.Extensions.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;



namespace Sabatex.BankStatementHelper;

public class OschadBank : ClientBankTo1CFormatConversion
{
    const int BufferSize = 1024;
    const string delimiter = "\n";
    string getValue(ref int pos, string s, string valueName)
    {
        if (pos == s.Length)
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
            {
                throw new Exception(ErrorStrings.DetermineEndDelimiterForValue(valueName));
            }
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
                        throw new Exception(ErrorStrings.StringEndedBeforeReadValue(valueName));

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
    string getDateValue(ref int pos, string s, string valueName)
    {
        var ts = getValue(ref pos, s, valueName).Trim();
        if (!ts.TryDateTo1C8Date(out string result))
            throw new Exception(ErrorStrings.ConvertDataTo1C8FormatForField("'Дата операції'", ts));
        else
            return result;
    }
    decimal getDecimalValue(ref int pos, string s, string valueName)
    {
        var ts = getValue(ref pos, s, valueName).Trim();
        if (ts.Length == 0)
        {
            return 0;
        }
        else
        {
            if (!ts.TryToDecimal(out decimal result))
                throw new Exception(ErrorStrings.DoubleParse(ts));
            else
                return result/100;
        }
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
        var document = new DocumentSection();
        int pos = 0;
        document.Номер = getValue(ref pos, s, "Номер платіжного документу (ndoc)").Trim();
        document.ДокументИД = document.Номер;
        string DocumentDate = getDateValue(ref pos, s, "Дата документу, дд.мм.рррр (dt)").Trim();
        document.Дата = getDateValue(ref pos, s, "Дата валютування, дд.мм.рррр (dv)").Trim();
        document.ПлательщикСчет = getValue(ref pos, s, "Рахунок відправника (acccli)");
        document.ПолучательСчет = getValue(ref pos, s, "Рахунок отримувача (acccor)");
        document.ПолучательОКПО = getValue(ref pos, s, "Податковий код отримувача (ІПН, ЄДРПОУ, ЗКПО)** (okpocor)").Trim();
        document.Получатель = getValue(ref pos, s, "Назва отримувача (namecor)").Trim();
        document.Сумма = getDecimalValue(ref pos, s, "Сума платежу    (у копійках) (summa)");
        document.КодВалюты = getValue(ref pos, s, "Валюта, ISO 4217 (val)").Trim();
        document.НазначениеПлатежа = getValue(ref pos, s, "Призначення платежу (nazn);Код країни-нерезидента отримувача (ISO 3166-1 numeric) (cod_cor)").Trim();
        string extra = getValue(ref pos, s, "Додаткові реквізити (add_req)").Trim();
        return document;
    }
    async Task ImportFromCSV(Stream stream, string AccNumber)
    {
        using (StreamReader reader = new StreamReader(stream, new Encoding1251()))
        {
            var lineDoc = 1;
            //char[] buffer = new char[BufferSize];
            try
            {
                do
                {
                    var line = await reader.ReadLineAsync();
                    if (line.Length == 0)
                        continue;
                    if (!int.TryParse(line[0]+"0",out int r)) continue; // start line

                    var doc = GetDocument(line.TrimEnd()+";", AccNumber);
                    if (doc != null) this.Documents.Add(doc);
                    lineDoc++;
                } while (!reader.EndOfStream);
            }
            catch (Exception e)
            {
                throw new Exception(ErrorStrings.InLine(lineDoc, e.Message));
            }
        }
    }

    public override async Task ImportFromFileAsync(Stream stream, string fileExt, string accNumber = "")
    {
        switch (fileExt.ToUpper())
        {
            case ".CSV":
                await ImportFromCSV(stream, accNumber);
                break;
            default:
                throw new Exception($"Для клінтбанка УКРГАЗБанк, файл з розширенням {fileExt} не підтримується. Можливі типи файлів CSV");

        }
    }
}
