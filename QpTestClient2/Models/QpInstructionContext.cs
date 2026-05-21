using Quick.Protocol;
using System.Collections.ObjectModel;

namespace QpTestClient.Models
{
    public class QpInstructionContext
    {
        public QpInstruction Model { get; set; }
        public ObservableCollection<TreeNode> Children { get; set; }

        public QpInstructionContext(QpInstruction model)
        {
            Model = model;
            Children = new ObservableCollection<TreeNode>();
            if (Model.NoticeInfos != null && Model.NoticeInfos.Length > 0)
                Children.Add(new TreeNode()
                {
                    Name = "通知",
                    Children = Model.NoticeInfos
                });
            if (Model.CommandInfos != null && Model.CommandInfos.Length > 0)
                Children.Add(new TreeNode()
                {
                    Name = "命令",
                    Children = Model.CommandInfos
                });
        }
    }
}
