using DigitalRune.Game.UI.Controls;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class OpenDocumentForm
    {
        private readonly Window _form;



        public OpenDocumentForm(Window form)
        {
            _form = form;
        }



        public Window Form
        {
            get { return _form; }
        }
    }
}