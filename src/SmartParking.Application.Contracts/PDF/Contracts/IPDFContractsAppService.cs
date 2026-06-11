using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Layout;
using iText.Layout.Element;
using static System.Net.Mime.MediaTypeNames;

namespace SmartParking.PDF.Contracts;

public interface IPDFContractsAppService<DTO>
{
     void AddHeaderSection(Document doc, DTO data);

     void AddSubHeader(Document doc, DTO data);
     void AddVatAndTicket(Document doc, DTO data);
     void AddPaymentHeader(Document doc, DTO data);

     void AddOrganizationSection(Document doc, DTO data, bool isArabic);

     void AddSummarySection(Document doc, DTO data, bool isArabic);
     void AddQrAndTotalsSection(Document doc, DTO data, bool isArabic);

     Table BuildTotalsTable(DTO data, bool isArabic,Document doc);
}
