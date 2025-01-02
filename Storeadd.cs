using System;
using System.Collections.Generic;

namespace ERP_API.Data
{
    public partial class Storeadd
    {
        public Storeadd()
        {
            Storeaddsubs = new HashSet<Storeaddsub>();
        }

       
        public int? StoreAddId { get; set; } 
        public int? StoreAddNo { get; set; } 
        public DateTime? StoreAddDate { get; set; } 
        public int? RefDocId { get; set; }
        public int? StoreId { get; set; } 

        public int? Source { get; set; }
        public int? InMatId { get; set; }

        public RawInwardMaterial RawInwardMaterial { get; set; }

        public PmInwardMaterial PmInwardMaterial { get; set; }


        public ICollection<Storeaddsub> Storeaddsubs { get; set; } = new List<Storeaddsub>();
        

        
    }
}