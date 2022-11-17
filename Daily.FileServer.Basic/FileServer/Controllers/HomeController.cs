using FreeSql;
using Furion.DataEncryption;
using Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Resources;
using System.Threading.Tasks;
using DailyHelper.Extends;
using DailyHelper.ResultInfo;
using Furion;
using Masuit.Tools.AspNetCore.Mime;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Serializers;
using FileServer;
using FileServer.Controllers;
using FileServer.Models;
using static System.Convert;
using static FileServer.ConfigHelper;

[Route("[controller]/[action]")]
public class HomeController : Controller
{
    private string temSuffix = ".upload.tmp";

    private string basePath = GetAppSetting("FilePath");

    private string secretKey = GetAppSetting("SecretKey");

    //如果这个文件存在是否覆盖
    private bool coverFile = true;

    private IFreeSql db = null;

    [AllowAnonymous]
    public ActionResult Index()
    {
        return View();
    }

    public HomeController(IFreeSql _db)
    {
        db = _db;
        GetHostIP();
        string yearfolder = Path.Combine(basePath, DateTime.Now.Year.ToString());
        string datefolder = Path.Combine(yearfolder, DateTime.Now.ToString("yyyyMMdd"));
        if (!Directory.Exists(yearfolder))
        {
            Directory.CreateDirectory(yearfolder);
        }

        if (!Directory.Exists(datefolder))
        {
            Directory.CreateDirectory(datefolder);
        }

        basePath = datefolder;
    }

    [HttpPost]
    public object GetCurrChunk(IFormCollection form)
    {
        string fileName = form["fileName"];
        string filePath = Path.Combine(basePath, fileName);
        string fileExtension = Path.GetExtension(fileName);
        //文件后缀名校验
        if (!FileStaticConfiguration.FileWhiteList.Any(s => s == fileExtension.ToLower()))
        {
            return new
            {
                code = 403,
                msg = $"该文件类型不支持上传"
            };
        }

        string fileSize = form["fileSize"];
        string tmpFileName = filePath + temSuffix;
        long chunkIndex = 0;
        //如果存在临时文件
        if (System.IO.File.Exists(tmpFileName))
        {
            System.IO.FileInfo file = new System.IO.FileInfo(tmpFileName);
            long fileInfoLength = file.Length;
            chunkIndex = fileInfoLength / 3145728;
        }

        //加密临时文件 返回给前端
        string secretPath = DESCEncryption.Encrypt(tmpFileName, secretKey);
        Hashtable hashResult = new Hashtable();
        hashResult.Add("chunkIndex", chunkIndex);
        hashResult.Add("path", secretPath);
        return hashResult;
    }

    [HttpPost]
    public object Upload(IFormCollection form)
    {
        try
        {
            string fileName = form["name"];
            string fileExtension = Path.GetExtension(fileName);
            StringValues secretPath = form["path"];
            string tmpPath = DESCEncryption.Decrypt(secretPath, secretKey);
            string ultimatelyPath = tmpPath.Replace(temSuffix, string.Empty);
            int index = ToInt32(form["chunk"]);
            int maxChunk = ToInt32(form["maxChunk"]);
            IFormFile file = form.Files["file"];
            if (index == maxChunk + 1)
            {
                //如果存在这个名称的文件
                if (System.IO.File.Exists(ultimatelyPath))
                {
                    var extensIndex = ultimatelyPath.LastIndexOf(".");
                    ultimatelyPath =
                        $"{ultimatelyPath.Substring(0, extensIndex)}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{Path.GetExtension(ultimatelyPath)}";
                }

                //覆盖的写法 参数三为 false 则不覆盖文件
                System.IO.File.Move(tmpPath, ultimatelyPath, true);
                return new
                {
                    msg = "ok",
                };
            }

            using (FileStream fs = new FileStream(tmpPath, (index == 0) ? FileMode.Create : FileMode.Append))
            {
                using Stream filereader = file.OpenReadStream();
                byte[] bytes = new byte[filereader.Length];
                filereader.Read(bytes, 0, bytes.Length);
                fs.Write(bytes, 0, bytes.Length);
            }

            return new
            {
                msg = "next"
            };
        }
        catch (Exception)
        {
            return new
            {
                msg = "error"
            };
        }
    }

    //[HttpGet]
    //public IActionResult Filedownload(string filemd5)
    //{
    //    ResourcesInfo fileInfo = (from t in db.Select<ResourcesInfo>()
    //        where t.file_md5 == filemd5
    //        select t).ToOne();
    //    string filePath = fileInfo.file_path;
    //    FileStream stream = System.IO.File.OpenRead(filePath);
    //    MimeMapper mimeMapper = new MimeMapper();
    //    var contentType = mimeMapper.GetMimeFromExtension(Path.GetExtension(basePath));
    //    return new ResumeFileStreamResult(stream, contentType)
    //    {
    //        FileInlineName = fileInfo.file_name
    //    };
    //}
    //[HttpGet]
    //public IActionResult FileSee(string id)
    //{
    //    ResourcesInfo fileInfo = (from t in db.Select<ResourcesInfo>()
    //        where t.file_md5 == id
    //                              select t).ToOne();
    //    string filePath = fileInfo.file_path;
    //    FileStream stream = System.IO.File.OpenRead(filePath);
    //    MimeMapper mimeMapper = new MimeMapper();
    //    var contentType = mimeMapper.GetMimeFromExtension(Path.GetExtension(filePath));
    //    return Path.GetExtension(fileInfo.file_name).ToLower() == ".pdf"
    //        ? new FileContentResult(stream.ToBytes(), contentType)
    //        : new FileContentResult(stream.ToBytes(), contentType)
    //        {
    //            FileDownloadName = fileInfo.file_name
    //        };
    //}

    //[HttpPost]
    //public object GetFileInfo([FromBody] GetFileInfoParam param)
    //{
    //    return db.Select<ResourcesInfo>()
    //        .Where(t => param.filemd5s.Contains(t.file_md5) && t.file_isfinish == 0)
    //        .ToList(
    //            t => new
    //            {
    //                fileName = t.file_name,
    //                fileMd5 = t.file_md5,
    //                fileSize = t.file_size,
    //                fileUploadTime = t.file_upload_time
    //            });
    //}

    [HttpGet]
    public string test()
    {
        return "hello";
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<dynamic> NormalUpload([FromForm] IFormFile file)
    {
        var endPath = "";
        var hashResult = new Hashtable();
        var visitPath = "";
        try
        {
            var fileName = file.FileName;
            hashResult.Add("fileName", fileName);
            string fileExtension = Path.GetExtension(fileName);
            var firstfloor = $"TyporaImage";
            var twofloor = $"{EGet.DNow().ToString("yyyy")}";
            var threefloor = $"{EGet.DNow().ToString("yyyy-MM")}";
            var fourfloor = $"{EGet.DNow().ToString("yyyy-MM-dd")}";

            var firstfloor_path = firstfloor;
            CheckFolder(firstfloor);
            var twofloor_path = Path.Combine(firstfloor_path, twofloor);
            CheckFolder(twofloor_path);
            var threefloor_path = Path.Combine(twofloor_path, threefloor);
            CheckFolder(threefloor_path);
            var fourfloor_path = Path.Combine(threefloor_path, fourfloor);
            CheckFolder(fourfloor_path);
            var encryptionFile = $"{EGet.GetID()}{fileExtension}";
            endPath = Path.Combine(App.WebHostEnvironment.WebRootPath, fourfloor_path, encryptionFile);
            visitPath = Path.Combine(fourfloor_path, encryptionFile);
            using (var filestream = new FileStream(endPath, FileMode.Create))
            {
                //将文件复制到指定文件夹中
                await file.CopyToAsync(filestream);
            }

            visitPath = $"/{visitPath}";
            hashResult.Add("basePath", visitPath.Replace("\\", "/"));
        }
        catch (Exception ex)
        {
            return ResultInfo.ResultError(ex.ToString());
        }

        return !CheckFile(endPath) ? ResultInfo.ResultError("上传文件失败.") : ResultInfo.Result(hashResult);
    }

    [NonAction]
    public bool CheckFile(string path)
    {
        var BasePath = App.WebHostEnvironment.WebRootPath;
        return System.IO.File.Exists(Path.Combine(BasePath, path));
    }

    [NonAction]
    public string CheckFolder(string path)
    {
        var BasePath = App.WebHostEnvironment.WebRootPath;
        if (!Directory.Exists(Path.Combine(BasePath, path)))
        {
            Directory.CreateDirectory(Path.Combine(BasePath, path));
        }

        return path;
    }

    /// <summary>
    /// 获取到当前的IP地址
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public string GetHostIP()
    {
        string name = Dns.GetHostName();
        IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
        foreach (IPAddress ipa in ipadrlist)
        {
            if (ipa.AddressFamily == AddressFamily.InterNetwork)
                return ipa.ToString();
        }

        return string.Empty;
    }
}