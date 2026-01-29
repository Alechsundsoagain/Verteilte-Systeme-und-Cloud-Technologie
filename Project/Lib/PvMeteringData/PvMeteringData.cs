namespace PvMeteringData
{
    public class PvMetering
    {
        public string PlantId { get; set; }
        public DateTime Timestamp { get; set; }
        public double PowerKw { get; set; }
        public double Voltage { get; set; }
        public double Current { get; set; }
    }

}
