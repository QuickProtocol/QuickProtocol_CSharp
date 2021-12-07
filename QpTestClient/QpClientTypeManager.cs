using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QpTestClient
{
    public class QpClientTypeManager
    {
        public static QpClientTypeManager Instance { get; } = new QpClientTypeManager();
        private Dictionary<string, QpClientTypeInfo> dict = null;
        public void Init()
        {
            dict = new Dictionary<string, QpClientTypeInfo>();
            foreach (var dllFile in Directory.GetFiles(".", $"{nameof(Quick)}.{nameof(Quick.Protocol)}.*.dll"))
            {
                var assembly = Assembly.Load(Path.GetFileNameWithoutExtension(dllFile));
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || !typeof(QpClient).IsAssignableFrom(type))
                        continue;

                    var typeConstructor = type.GetConstructors()[0];
                    var typeConstructorParameters = typeConstructor.GetParameters();
                    if (typeConstructorParameters == null || typeConstructorParameters.Length != 1)
                        continue;
                    var optionsType = typeConstructorParameters[0].ParameterType;
                    var name = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? type.Name;

                    dict[type.FullName] = new QpClientTypeInfo()
                    {
                        Name = name,
                        QpClientType = type,
                        QpClientOptionsType = optionsType
                    };
                }
            }
        }

        public QpClientTypeInfo Get(string qpClientTypeName)
        {
            if (dict.ContainsKey(qpClientTypeName))
                return dict[qpClientTypeName];
            return null;
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public QpClientTypeInfo[] GetAll() => dict.Values.ToArray();
    }
}
