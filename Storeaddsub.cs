using System;
using System.Collections.Generic;

namespace ERP_API.Data;

public partial class Storeaddsub
{
    public int? StoreAddId { get; set; }
    public int? storeAddSubId { get; set; }
    public int? SlNo { get; set; }
    public int? ItemId { get; set; }
    public float? Qty { get; set; }
    public float? BalQty { get; set; }
    public string? Trak { get; set; }

    public int? BagNo { get; set; }

    public string? BatchNo { get; set; }

    public Storeadd Storeadd { get; set; }
}
