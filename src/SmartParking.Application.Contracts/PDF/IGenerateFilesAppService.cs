using SmartParking.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.PDF;

public interface IGenerateFilesAppService :ITransientDependency
{
    Task ExportToPdf(TableData Data,string fileName);
    Task ExportToExcel(TableData Data,string pathFileName);
}
