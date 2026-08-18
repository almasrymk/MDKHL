using System.ComponentModel.DataAnnotations;

namespace SmartParking.Mobile.Keepers.Dtos;

public class SendUnlinkedAsEntryDto
{
    [Required]
    public int UnlinkedTransactionId { get; set; }

    [Required]
    public int OrganizationId { get; set; }

    [Required]
    public string WatcherCode { get; set; } = string.Empty;

    public string? CorrectedPlateNumber { get; set; }

    public string? TicketNumber { get; set; }

    public bool TriggerGateOpen { get; set; } = true;
}
