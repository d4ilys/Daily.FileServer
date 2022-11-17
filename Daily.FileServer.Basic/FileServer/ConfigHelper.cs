using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FileServer.Controllers;

namespace FileServer
{
    public class ConfigHelper
    {
        public static IConfiguration Configuration = null;

        public static void Init()
        {
            ChangeToken.OnChange(() => Configuration.GetReloadToken(), () =>
            {
                //热更新配置文件
                InitFileWhiteList();
                Console.WriteLine("文件白名单已经更新！");
            });
            //初始化文件类型白名单
            InitFileWhiteList();
        }

        public static string GetAppSetting(string key)
        {
            return Configuration.GetSection(key).Value;
        }

        public static void InitFileWhiteList()
        {
            List<string> list = new List<string>();
            var VdioWhiteList = GetAppSetting("VdioWhiteList").Split("|").ToList();
            var ImageWhiteList = GetAppSetting("ImageWhiteList").Split("|").ToList();
            var DocumentWhiteList = GetAppSetting("DocumentWhiteList").Split("|").ToList();
            var AccessoryWhiteList = GetAppSetting("AccessoryWhiteList").Split("|").ToList();
            list.AddRange(VdioWhiteList);
            list.AddRange(DocumentWhiteList);
            list.AddRange(ImageWhiteList);
            list.AddRange(AccessoryWhiteList);
            FileStaticConfiguration.FileWhiteList = list;
            //是否启动鉴权
            FileStaticConfiguration.IsValid = GetAppSetting("IsValid").ToBoolean();
            //是否启动Web
            FileStaticConfiguration.IsShowViews = GetAppSetting("IsShowViews").ToBoolean();
        }
    }
}