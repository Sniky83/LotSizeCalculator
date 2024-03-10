namespace API
{
    public class OrderGetInfoDto
    {
        public double Capital { get; set; }
        public string Symbol { get; set; } = "";
        public bool IsCash { get; set; }
        public double MaxPercentCapital { get; set; }
        public double TheoricalTpPips { get; set; }
        public double TheoricalSlPips { get; set; }
        public double? LotSize { get; set; }
    }
}
