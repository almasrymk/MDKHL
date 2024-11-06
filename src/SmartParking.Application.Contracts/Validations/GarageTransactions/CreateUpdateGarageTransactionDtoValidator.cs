using FluentValidation;
using SmartParking.GarageTransactions;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.GarageTransactions
{
    public class CreateUpdateGarageTransactionDtoValidator : AbstractValidator<CreateUpdateGarageTransactionDto>
    {
        public CreateUpdateGarageTransactionDtoValidator()
        {
            RuleFor(x=>x.AccurateTime).NotEmpty();
            RuleFor(x=>x.SnapTime).NotEmpty();
            RuleFor(x=>x.PlateNumber);
            RuleFor(x => x.CameraId).NotEmpty();
            RuleFor(x => x.GarageId).NotEmpty();
            RuleFor(x => x.OrganizationID).NotEmpty();
            RuleFor(x => x.VehiclePicture).NotEmpty();
            RuleFor(x => x.CameraId).NotEmpty();

        }
    }
}
