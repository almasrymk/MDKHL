using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Analysis;

public class AnalysisNamesCountDto : EntityDto
{
    public  List<string> Names { get; set; } = new List<string>();
    public List<double> Count { get; set; } = new List<double>();

}
