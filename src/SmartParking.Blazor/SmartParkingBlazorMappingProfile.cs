using AutoMapper;
using SmartParking.Blazor.Dtos;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;

namespace SmartParking.Blazor
{
    public class SmartParkingBlazorMappingProfile : Profile
    {
        public SmartParkingBlazorMappingProfile()
        {
            //Define your AutoMapper configuration here for the Blazor project.
            CreateMap<GarageTransactionDto, GarageTransactionUiDto>()
                .ReverseMap();
            CreateMap<CreateUpdateInvoiceDto, CreateUpdateInvoiceUiDto>()
                .ReverseMap();
            CreateMap<InvoiceDto, InvoiceUiDto>()
                .ReverseMap();
        }
    }
}
