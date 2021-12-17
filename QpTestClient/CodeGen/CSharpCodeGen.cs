using NJsonSchema.CodeGeneration.CSharp;
using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QpTestClient.CodeGen
{
    public class CSharpCodeGen
    {
        private static async Task genCSharpCode(string instructionId, string typeFullName, string typeJsonSchema, string baseFolder, bool isCommandRequestType = false, string responseTypeFullName = null)
        {
            var typeInfo = CodeGenTypeInfo.Parse(typeFullName);
            var schema = await NJsonSchema.JsonSchema.FromJsonAsync(typeJsonSchema);

            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings()
            {
                Namespace=typeInfo.Namespace,
                DateType=nameof(DateTime),
                DateTimeType=nameof(DateTime)
            });
            var code = generator.GenerateFile(typeInfo.TypeName);
            StringBuilder sb = new StringBuilder(code);

            //如果没有属性，产生类
            if (schema.Properties.Count == 0)
            {
                sb.Insert(code.Length-2, $"public partial class {typeInfo.TypeName}{Environment.NewLine}    {{{Environment.NewLine}    }}");
            }

            //如果是命令请求类型
            if (isCommandRequestType)
            {
                //添加using
                sb.Insert(0, "using Quick.Protocol;" + Environment.NewLine);
                //添加接口实现
                var responseTypeName = "Response";
                if (!string.IsNullOrEmpty(responseTypeFullName))
                {
                    var responseTypeInfo = CodeGenTypeInfo.Parse(responseTypeFullName);
                    responseTypeName=responseTypeInfo.TypeName;
                }
                var keywords = $"public partial class {typeInfo.TypeName}";
                sb.Replace(keywords, keywords + $" : IQpCommandRequest<{responseTypeName}>");
            }
            //添加using
            sb.Insert(0, "using System;" + Environment.NewLine);

            code = sb.ToString();
            //保存到文件
            var namespaceFolder = typeInfo.Namespace;
            if (namespaceFolder.StartsWith(instructionId))
                namespaceFolder=namespaceFolder.Substring(instructionId.Length+1);

            List<string> pathList = new List<string>();
            pathList.Add(baseFolder);
            pathList.AddRange(namespaceFolder.Split('.'));
            var outFolder = Path.Combine(pathList.ToArray());
            if (!Directory.Exists(outFolder))
                Directory.CreateDirectory(outFolder);
            File.WriteAllText(Path.Combine(outFolder, typeInfo.TypeName + ".cs"), code);
        }

        public static async Task Generate(QpInstruction instruction, string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            //生成通知类代码
            if (instruction.NoticeInfos!=null && instruction.NoticeInfos.Length>0)
            {
                foreach (var item in instruction.NoticeInfos)
                {
                    await genCSharpCode(instruction.Id, item.NoticeTypeName, item.NoticeTypeSchema, folder);
                }
            }
            //生成指令类代码
            if (instruction.CommandInfos!=null && instruction.CommandInfos.Length>0)
            {
                foreach (var item in instruction.CommandInfos)
                {
                    //请求命令类
                    await genCSharpCode(instruction.Id, item.RequestTypeName, item.RequestTypeSchema, folder, true, item.ResponseTypeName);
                    //响应命令类
                    await genCSharpCode(instruction.Id, item.ResponseTypeName, item.ResponseTypeSchema, folder);
                }
            }
            //生成Instruction类
            StringBuilder sbInstruction = new StringBuilder();
            sbInstruction.AppendLine("using Quick.Protocol;");
            sbInstruction.AppendLine();
            sbInstruction.AppendLine($"namespace {instruction.Id}");
            sbInstruction.AppendLine("{");
            sbInstruction.AppendLine("    public class Instruction");
            sbInstruction.AppendLine("    {");
            sbInstruction.AppendLine("        public static QpInstruction Instance = new QpInstruction()");
            sbInstruction.AppendLine("        {");
            sbInstruction.AppendLine("            Id = typeof(Instruction).Namespace,");
            sbInstruction.AppendLine($"            Name = \"{instruction.Name}\",");
            if (instruction.NoticeInfos!=null && instruction.NoticeInfos.Length>0)
            {
                sbInstruction.AppendLine("            NoticeInfos = new[]");
                sbInstruction.AppendLine("            {");

                for (var i = 0; i<instruction.NoticeInfos.Length; i++)
                {
                    var item = instruction.NoticeInfos[i];
                    sbInstruction.Append($"                QpNoticeInfo.Create<{item.NoticeTypeName}>()");
                    if (i<instruction.NoticeInfos.Length-1)
                        sbInstruction.Append(",");
                    sbInstruction.AppendLine();
                }
                sbInstruction.AppendLine("            },");
            }
            if (instruction.CommandInfos!=null && instruction.CommandInfos.Length>0)
            {
                sbInstruction.AppendLine("            CommandInfos = new[]");
                sbInstruction.AppendLine("            {");
                for (var i = 0; i<instruction.CommandInfos.Length; i++)
                {
                    var item = instruction.CommandInfos[i];
                    sbInstruction.Append($"                QpCommandInfo.Create(new {item.RequestTypeName}())");
                    if (i<instruction.CommandInfos.Length-1)
                        sbInstruction.Append(",");
                    sbInstruction.AppendLine();
                }
                sbInstruction.AppendLine("            },");
            }
            sbInstruction.AppendLine("        };");
            sbInstruction.AppendLine("    }");
            sbInstruction.AppendLine("}");
            File.WriteAllText(Path.Combine(folder, "Instruction.cs"), sbInstruction.ToString());
            var qpVersion = typeof(QpChannel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute >().InformationalVersion;
            File.WriteAllText(Path.Combine(folder, instruction.Id + ".csproj"), @$"
<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Quick.Protocol"" Version=""{qpVersion}"" />
  </ItemGroup>

</Project>
");

        }
    }
}
