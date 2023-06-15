namespace TrainingRestApi;

public class CompanyLedger
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Liquidity { get; set; }
    public decimal Profit { get; set; }
    public int Workers { get; set; }
    public decimal Wage { get; set; }
    public int Sales { get; set; }
    public int Stock { get; set; }
    public int Lifetime { get; set; }
    public bool Extinct { get; set; }
}