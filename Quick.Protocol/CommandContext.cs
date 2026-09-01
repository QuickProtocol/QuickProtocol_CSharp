using Quick.Protocol.Exceptions;

namespace Quick.Protocol
{
    public class CommandContext
    {
        public static string GenerateNewId() => Guid.NewGuid().ToString("n");
        public string Id { get; private set; }
        private readonly TaskCompletionSource<CommandResponseTypeNameAndContent> tcs;
        public Task<CommandResponseTypeNameAndContent> ResponseTask => tcs.Task;

        public CommandContext(string typeName)
        {
            Id = GenerateNewId();
            tcs = new TaskCompletionSource<CommandResponseTypeNameAndContent>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public virtual void SetResponse(CommandException commandException)
        {
            tcs.TrySetException(commandException);
        }

        public virtual void SetResponse(string responseTypeName, string responseContent)
        {
            tcs.TrySetResult(new CommandResponseTypeNameAndContent()
            {
                TypeName = responseTypeName,
                Content = responseContent
            });
        }

        public virtual void Timeout()
        {
            tcs.TrySetException(new TimeoutException($"Command[Id:{Id}] is timeout."));
        }
    }
}
