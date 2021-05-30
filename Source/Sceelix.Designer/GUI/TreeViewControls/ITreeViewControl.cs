using DigitalRune.Collections;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public interface ITreeViewControl
    {
        NotifyingCollection<TreeViewItem> Items
        {
            get;
        }
    }
}