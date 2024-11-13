using Quick.Protocol;
using Quick.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QpTestClient.Utils
{
    public class QpdFileUtils
    {
        public static string QpbFileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(QpTestClient));

        public static string GetQpbFilePath(TestConnectionInfo connectionInfo) => Path.Combine(QpbFileFolder, connectionInfo.Name + ".qpd");

        public static void SaveQpbFile(TestConnectionInfo connectionInfo, string file = null)
        {
            var content = Quick.Xml.XmlConvert.Serialize(connectionInfo);
            if (!Directory.Exists(QpbFileFolder))
                Directory.CreateDirectory(QpbFileFolder);
            if (string.IsNullOrEmpty(file))
                file = GetQpbFilePath(connectionInfo);
            if (File.Exists(file))
                File.Delete(file);
            File.WriteAllText(file, content, Encoding.UTF8);
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

        public static Quick.Xml.XmlConvertOptions XmlConvertOptions { get; private set; }

        public static TestConnectionInfo Load(string file)
        {
            if (XmlConvertOptions == null)
            {
                XmlConvertOptions = new XmlConvertOptions()
                {
                     InstanceFactory = QpClientTypeManager.Instance.GetAll().ToDictionary(t => t.OptionsType, t =>
                     {
                         return new Func<object>(() => t.CreateOptionsInstanceFunc());
                     })
                };
                XmlConvertOptions.InstanceFactory.Add(typeof(TestConnectionInfo), () => new TestConnectionInfo());
                XmlConvertOptions.InstanceFactory.Add(typeof(QpInstruction), () => new QpInstruction());
            }
            var content = File.ReadAllText(file);
            return Quick.Xml.XmlConvert.Deserialize<TestConnectionInfo>(content, XmlConvertOptions);
        }
    }
}
