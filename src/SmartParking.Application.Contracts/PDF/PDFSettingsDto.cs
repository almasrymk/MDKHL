using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF;

public class PdfSettings
{
    public string PaperSize { get; set; } = "A4";
    public PdfMargins Margins { get; set; } = new();
}

public class PdfMargins
{
    public float Top { get; set; } = 10;
    public float Right { get; set; } = 10;
    public float Bottom { get; set; } = 10;
    public float Left { get; set; } = 10;
}
