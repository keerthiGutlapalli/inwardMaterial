namespace ERP_API.Data
{
    public class PmInwardMaterial
    {

        public PmInwardMaterial()
        {
            PmInwardMaterialSubs = new HashSet<PmInwardMaterialSub>();
            Storeadds = new HashSet<Storeadd>();
        }
        public int InMatId { get; set; }
        public int? VendId { get; set; }
        public int? POId { get; set; }
        public string? InvNo { get; set; }
        public DateTime? InvDate { get; set; }

        public int? StoreId { get; set; }

        public string? GRNNo { get; set; }
        public DateTime? GRNDate { get; set; }

    
        public ICollection<PmInwardMaterialSub> PmInwardMaterialSubs { get; set; } = new List<PmInwardMaterialSub>();
        public ICollection<Storeadd> Storeadds { get; set; }
    }
}
