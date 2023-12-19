using System;
using System.Collections.Generic;
using System.Text;

namespace sabatex.V1C8.BankHelper
{
    public interface I1CClientBankExchange
    {
        int LineDoc { get; set; }
        List<string> Errors { get; set; }
        List<string> Warnings { get; set; }
        string GetAsXML();
        List<DocumentSection> Documents { get; set; }
        void AddDocument(DocumentSection documentSection);
        int Count();
    }
}
