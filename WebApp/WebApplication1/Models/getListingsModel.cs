using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class mobileHelixItems
    {
        public string displayName { get; set; }
        public string globalKey { get; set; }
        public string parentDigest { get; set; }
        public bool isFile { get; set; }
        public string current { get; set; }
    }
    public class getListingsModel
    {
        public List<mobileHelixItems> data { get; set; }
    }
}