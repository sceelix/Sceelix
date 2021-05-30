using System.Text;

namespace Sceelix.Core.Code
{
    public class CodeBuilder
    {
        private readonly StringBuilder _footerStringBuilder = new StringBuilder();
        private readonly StringBuilder _stringBuilder = new StringBuilder();


        public int TabIndentation
        {
            get;
            set;
        } = 0;


        public int VarCounter
        {
            get;
            set;
        } = 0;



        public void AppendLine(string str)
        {
            for (int i = 0; i < TabIndentation; i++) _stringBuilder.Append("\t");

            _stringBuilder.AppendLine(str);
        }



        public void AppendLineToFooter(string str)
        {
            _footerStringBuilder.AppendLine(str);
        }



        public override string ToString()
        {
            if (_footerStringBuilder.Length > 0)
                return _stringBuilder + "\n\n" + _footerStringBuilder;

            return _stringBuilder.ToString();
        }
    }
}