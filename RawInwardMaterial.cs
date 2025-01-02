namespace ERP_API.Data
{
    public class RawInwardMaterial

    {
        public RawInwardMaterial()
        {
            RawInwardMaterialSubs = new HashSet<RawInwardMaterialSub>();
            Storeadds = new HashSet<Storeadd>();
        }

        public int InMatId { get; set; }
        public int? VendId { get; set; }
        public int? POId { get; set; }
        public string? InvNo { get; set; }
        public DateTime? InvDate { get; set; }
        public int? StoreAddId { get; set; }
        public int? StoreId { get; set; }
      
        public string? GRNNo { get; set; }
        public DateTime? GRNDate { get; set; }
       
        public ICollection<RawInwardMaterialSub> RawInwardMaterialSubs { get; set; }
        public ICollection<Storeadd> Storeadds { get; set; }
       
    }


}

