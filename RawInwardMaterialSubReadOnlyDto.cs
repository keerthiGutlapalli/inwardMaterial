namespace ERP_API.Moduls
{
    public class RawInwardMaterialSubReadOnlyDto
    {
        public int? InMatSubId { get; set; }
        public int InMatId { get; set; }
        public int? SlNo { get; set; }
        public int? ItemId { get; set; }

        public double? Qty { get; set; }

        public int? NoOfBags { get; set; }

        public string? BatchNo { get; set; }

    }
}
