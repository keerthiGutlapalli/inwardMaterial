namespace ERP_API.Moduls
{
    public class RawInwardMaterialReadOnlyDto
    {
        public int InMatId { get; set; }
        public int? VendId { get; set; }

        public int? POId { get; set; }
        public string? InvNo { get; set; }
        public DateTime? InvDate { get; set; }
        public int? StoreId { get; set; }
        public int? StoreAddId { get; set; }
        public string? GRNNo { get; set; }
        public DateTime? GRNDate { get; set; }
        public List<RawInwardMaterialSubReadOnlyDto> RawInwardMaterialSubs { get; set; }
        public List<StoreAddReadOnlyDto> Storeadds { get; set; }
    }
}

