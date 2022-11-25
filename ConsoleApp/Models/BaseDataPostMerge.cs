using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    public class BaseDataPostMerge
    {
        public List<BaseDataPost> baseDataPosts { get; set;}
        public List<BaseDataPostContact> baseDataPostContacts { get; set; }
    }
}
