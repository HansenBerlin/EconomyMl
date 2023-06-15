namespace TrainingRestApi;

public class CompanyEvent
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal ActPrice { get; set; }
    public decimal ActWage { get; set; }
    public int ActWorker { get; set; }
}