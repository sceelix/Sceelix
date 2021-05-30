using Sceelix.Core.Parameters.Infos;
using Sceelix.Helpers;

namespace Sceelix.Core.Parameters
{
    public class FolderParameter : PrimitiveParameter<string>
    {
        public FolderParameter(string label, string defaultValue)
            : base(label, defaultValue)
        {
        }



        internal FolderParameter(FolderParameterInfo folderParameterInfo)
            : base(folderParameterInfo)
        {
            IOOperation = folderParameterInfo.IOOperation;
        }



        public IOOperation IOOperation
        {
            get;
            set;
        } = IOOperation.Load;



        /// <summary>
        /// Sets the value of the parameter.
        /// </summary>
        /// <param name="value">A string value containing a valid path. If the IOOperation is Load and the path does not exist, an exception is thrown.</param>
        protected override void SetData(object value)
        {
            string path = (string) value;

            path = PathHelper.ToPlatformPath(path);

            //if (!System.IO.Path.IsPathRooted(path))
            //    path = ProcedureEnvironment.Resources.GetFullPath(path);

            base.SetData(path);
        }



        protected internal override ParameterInfo ToParameterInfo()
        {
            return new FolderParameterInfo(this);
        }
    }
}