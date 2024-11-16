using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace QpTestClient.Utils
{
    public class QpdFileUtils
    {
        public static string QpbFileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(QpTestClient));

        public static string GetQpbFilePath(TestConnectionInfo connectionInfo) => Path.Combine(QpbFileFolder, connectionInfo.Name + ".qpd");

        public static void SaveQpbFile(TestConnectionInfo connectionInfo, string file = null)
        {
            if (!Directory.Exists(QpbFileFolder))
                Directory.CreateDirectory(QpbFileFolder);
            if (string.IsNullOrEmpty(file))
                file = GetQpbFilePath(connectionInfo);
            if (File.Exists(file))
                File.Delete(file);

            using (var zipArchive = ZipFile.Open(file, ZipArchiveMode.Create))
            {
                //写入连接信息
                var entry = zipArchive.CreateEntry(typeof(TestConnectionInfo).FullName);
                using (var stream = entry.Open())
                    JsonSerializer.Serialize(stream, connectionInfo, TestConnectionInfoSerializerContext.Default.TestConnectionInfo);
                //写入客户端配置信息
                entry = zipArchive.CreateEntry(typeof(QpClientOptions).FullName);
                using (var stream = entry.Open())
                    connectionInfo.QpClientOptions.Serialize(stream);
            }
        }

        public static TestConnectionInfo[] GetConnectionInfosFromQpbFileFolder()
        {
            if (!Directory.Exists(QpbFileFolder))
                return null;
            List<TestConnectionInfo> list = new List<TestConnectionInfo>();
            foreach (var file in Directory.GetFiles(QpbFileFolder, "*.qpd"))
            {
                try
                {
                    var connectionInfo = Load(file);
                    list.Add(connectionInfo);
                }
                catch { }
            }
            return list.ToArray();
        }

        public static void DeleteQpbFile(TestConnectionInfo connectionInfo)
        {
            var file = GetQpbFilePath(connectionInfo);
            if (File.Exists(file))
                File.Delete(file);
        }


        public static TestConnectionInfo Load(string file)
        {
            TestConnectionInfo testConnectionInfo = null;
            using (var zipArchive = ZipFile.OpenRead(file))
            {
                //读取连接信息
                var entry = zipArchive.GetEntry(typeof(TestConnectionInfo).FullName);
                using (var stream = entry.Open())
                    testConnectionInfo = JsonSerializer.Deserialize(stream, TestConnectionInfoSerializerContext.Default.TestConnectionInfo);
                //读取客户端配置信息
                entry = zipArchive.GetEntry(typeof(QpClientOptions).FullName);
                using (var stream = entry.Open())
                {
                    var qpClientTypeInfo = QpClientTypeManager.Instance.Get(testConnectionInfo.QpClientTypeName);
                    QpClientOptions options = qpClientTypeInfo.DeserializeQpClientOptions(stream);
                    testConnectionInfo.QpClientOptions = options;
                }
            }
            return testConnectionInfo;
        }
    }
}
