using QpTestClient2.ViewModels;
using System;

namespace QpTestClient2.Controls
{
    public class MessageBoxViewModel : PropertyNotifyModel
    {
        public enum MessageBoxType
        {
            Message,
            Loading,
            Prompt
        }

        private MessageBoxType _Type = MessageBoxType.Message;
        public MessageBoxType Type
        {
            get { return _Type; }
            set
            {
                _Type = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsTypePrompt));
            }
        }

        public bool IsTypePrompt => Type == MessageBoxType.Prompt;

        private bool _IsVisible = false;
        public bool IsVisible
        {
            get { return _IsVisible; }
            set
            {
                _IsVisible = value;
                RaisePropertyChanged();
            }
        }

        private bool _ButtonOkVisiable = false;
        public bool ButtonOkVisiable
        {
            get { return _ButtonOkVisiable; }
            set
            {
                _ButtonOkVisiable = value;
                RaisePropertyChanged();
            }
        }

        private string _ButtonOkText = "OK";
        public string ButtonOkText
        {
            get { return _ButtonOkText; }
            set
            {
                _ButtonOkText = value;
                RaisePropertyChanged();
            }
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                RaisePropertyChanged();
            }
        }

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set
            {
                _Message = value;
                RaisePropertyChanged();
            }
        }

        private string _PromptValue;
        public string PromptValue
        {
            get { return _PromptValue; }
            set
            {
                _PromptValue = value;
                RaisePropertyChanged();
            }
        }

        private Action<string> PromptHandler;

        public DelegateCommand OkCommand { get; set; }

        public MessageBoxViewModel()
        {
            OkCommand = new DelegateCommand() { ExecuteCommand = executeCommand_OkCommand };
        }

        public void Show(string title, string message)
        {
            Type = MessageBoxType.Message;
            Title = title;
            Message = message;
            ButtonOkVisiable = true;
            IsVisible = true;
        }

        public void Loading(string title, string message)
        {
            Type = MessageBoxType.Loading;
            Title = title;
            Message = message;
            ButtonOkVisiable = false;
            IsVisible = true;
        }

        public void Prompt(string title, string message, string promptValue, Action<string> promptHandler)
        {
            Type = MessageBoxType.Prompt;
            Title = title;
            Message = message;
            PromptValue = promptValue;
            PromptHandler = promptHandler;
            ButtonOkVisiable = true;
            IsVisible = true;
        }

        public void Close()
        {
            IsVisible = false;
        }

        private void executeCommand_OkCommand(object e)
        {
            switch (Type)
            {
                case MessageBoxType.Prompt:
                    if (PromptHandler != null)
                    {
                        PromptHandler.Invoke(PromptValue);
                        PromptHandler = null;
                    }
                    break;
            }
            Close();
        }
    }
}
