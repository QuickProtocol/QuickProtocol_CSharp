using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QpTestClient.CodeGen
{
    public class CodeGenTypeInfo
    {
        public string Namespace { get; set; }
        public string TypeName { get; set; }

        public static CodeGenTypeInfo Parse(string typeFullName)
        {
            var segments = typeFullName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var _namespace = string.Join(".", segments, 0, segments.Length-1);
            var _typeName = segments[segments.Length-1];
            return new CodeGenTypeInfo()
            {
                Namespace = _namespace,
                TypeName = _typeName,
            };
        }
    }
}
