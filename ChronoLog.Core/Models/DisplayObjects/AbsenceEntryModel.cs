namespace ChronoLog.Core.Models.DisplayObjects;

public class AbsenceEntryModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string AbsenceTypes { get; set; }
    public int DurationInDays { get; set; }
}