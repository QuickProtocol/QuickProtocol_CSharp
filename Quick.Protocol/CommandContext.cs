using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Protocol
{
    public class CommandContext
    {
        public string Id { get; private set; }
        private CommandException commandException;
        private bool isTimeout = false;
        private CommandResponseTypeNameAndContent response;
        public Task<CommandResponseTypeNameAndContent> ResponseTask { get; private set; }

        public CommandContext(string typeName)
        {
            Id = Guid.NewGuid().ToString("N").ToLower();
            ResponseTask = new Task<CommandResponseTypeNameAndContent>(() =>
            {
                if (isTimeout)
                    throw new TimeoutException($"Command[Id:{Id},Type:{typeName}] is timeout.");
                if (commandException != null)
                    throw commandException;
                return response;
            });
        }

        public virtual void SetResponse(CommandException commandException)
        {
            if (isTimeout)
                return;
            this.commandException = commandException;
            if (ResponseTask.Status == TaskStatus.Created)
                ResponseTask.Start();
        }

        public virtual void SetResponse(string responseTypeName,string responseContent)
        {
            if (isTimeout)
                return;

            this.response = new CommandResponseTypeNameAndContent()
            {
                TypeName = responseTypeName,
                Content = responseContent
            };
            
            if (ResponseTask.Status == TaskStatus.Created)
                ResponseTask.Start();
        }

        public virtual void Timeout()
        {
            isTimeout = true;
            if (ResponseTask.Status == TaskStatus.Created)
                ResponseTask.Start();
        }
    }
}
