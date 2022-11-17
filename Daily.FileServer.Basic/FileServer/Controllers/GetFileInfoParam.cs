using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileServer.Controllers
{
    public class GetFileInfoParam
    {
        public List<string> filemd5s
        {
            get; set;  
        }
    }
}
