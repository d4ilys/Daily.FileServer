using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace FileServer.Models
{
    [Table(Name = "bbs_resources")]
    public class ResourcesInfo
    {
        [Column(StringLength = 64)] public string id { get; set; }

        [Column(StringLength = 255)] public string file_name { get; set; }

        [Column(StringLength = 500)] public string file_path { get; set; }

        public int file_isfinish { get; set; }

        public string file_size { get; set; }

        public DateTime file_upload_time { get; set; }

        public string file_host { get; set; }

        public int downloadCount { get; set; } = 0;
    }
}