using SmartParking.Entities.Analysis;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Entities.Organizations;

public interface IGarageAnalysisRepository : IRepository<Garage, int>
{
    Task<List<GarageAnalysis>> GetGarageStatusRange(DateTime startDate, DateTime endDate);
    Task<List<GarageAnalysis>> GetGarageStatusByDate(DateTime accurateDate);

}
