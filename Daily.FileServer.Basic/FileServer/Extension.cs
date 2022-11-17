using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace FileServer
{
    public static class Extension
    {
        public static byte[] ToBytes(this Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            try
            {
                stream.Read(bytes, 0, bytes.Length);
                // 设置当前流的位置为流的开始
                stream.Seek(0, SeekOrigin.Begin);
                return bytes;
            }
            catch (Exception ex)
            {
            }
            return bytes;
        }

        public static IServiceCollection AddHeatConfig(this IServiceCollection service)
        {
            ConfigHelper.Configuration = service.BuildServiceProvider().GetService<IConfiguration>();
            ConfigHelper.Init();
            return service;
        }
    }
}
