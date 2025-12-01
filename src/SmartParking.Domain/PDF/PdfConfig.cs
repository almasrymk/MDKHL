using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF
{
    public class PdfConfig
    {
        public string PaperSize { get; set; }
        public MarginConfig Margins { get; set; }
        public int FontSize { get; set; }
    }
}
