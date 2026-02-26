using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Shifts.ShiftStaffDetail;

public class ShiftStaffDetailsDto : AuditedEntityDto<Guid>
{
    public Guid StaffId { get; set; }
    public string StaffEnName { get; set; }
    public string StaffArName { get; set; }
    public string StaffPhoneNumber { get; set; }
    public int OrganizationId { get; set; }

}
