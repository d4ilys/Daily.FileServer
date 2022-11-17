using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileServer.Controllers
{
    public class FileStaticConfiguration
    {
        /// <summary>
        /// 文件类型白名单
        /// </summary>
        public static List<string> FileWhiteList = new List<string>();
        
        //是否启动鉴权
        public static bool IsValid = false;

        //是否显示web上传页面
        public static bool IsShowViews = false;
    }
}
