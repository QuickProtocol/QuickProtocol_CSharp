using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Quick.Protocol
{
    public class CommandExecuterManager
    {
        private Dictionary<string, Delegate> commandExecuterDict = new Dictionary<string, Delegate>();

        public CommandExecuterManager()
        {

        }

        /// <summary>
        /// 获取全部注册的命令请求类型名称
        /// </summary>
        public string[] GetRegisterCommandRequestTypeNames() => commandExecuterDict.Keys.ToArray();

        public void Register(string cmdRequestTypeName, Delegate commandExecuter)
        {
            commandExecuterDict[cmdRequestTypeName] = commandExecuter;
        }

        public void Register<TCmdRequest, TCmdResponse>(Func<QpChannel, TCmdRequest, TCmdResponse> commandExecuter)
            where TCmdRequest : class, new()
            where TCmdResponse : class, new()
        {
            var cmdRequestTypeName = typeof(TCmdRequest).FullName;
            Register(cmdRequestTypeName, commandExecuter);
        }

        public void Register<TCmdRequest, TCmdResponse>(TCmdRequest request, Func<QpChannel, TCmdRequest, TCmdResponse> commandExecuter)
            where TCmdRequest : class, IQpCommandRequest<TCmdResponse>, new()
            where TCmdResponse : class, new()
        {
            var cmdRequestTypeName = request.GetType().FullName;
            Register(cmdRequestTypeName, commandExecuter);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="cmdRequestTypeName"></param>
        /// <param name="cmdRequestModel"></param>
        /// <returns></returns>
        public virtual object ExecuteCommand(QpChannel handler, string cmdRequestTypeName, object cmdRequestModel)
        {
            if (!CanExecuteCommand(cmdRequestTypeName))
                throw new IOException($"Command Request Type[{cmdRequestTypeName}] has no executer.");
            Delegate commandExecuter = commandExecuterDict[cmdRequestTypeName];
            return commandExecuter.DynamicInvoke(new object[] { handler, cmdRequestModel });
        }

        /// <summary>
        /// 能否执行指定类型的命令
        /// </summary>
        /// <param name="cmdRequestTypeName"></param>
        /// <returns></returns>
        public virtual bool CanExecuteCommand(string cmdRequestTypeName)
        {
            return commandExecuterDict.ContainsKey(cmdRequestTypeName);
        }
    }
}
