using System.IO;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Resources;
using Sceelix.Helpers;

namespace Sceelix.Core.Parameters
{
    /// <summary>
    /// Enum IOOperation: describes if we are performing a loading or saving operation.
    /// </summary>
    public enum IOOperation
    {
        Load,
        Save
    }


    public class FileParameter : PrimitiveParameter<string>
    {
        /// <summary>
        /// FileParameter constructor.
        /// </summary>
        /// <param name="label">Label of the parameter. May contain spaces and special chars. </param>
        /// <param name="defaultValue">Default value (a file path) of the parameter.</param>
        public FileParameter(string label, string defaultValue)
            : base(label, defaultValue)
        {
        }



        /// <summary>
        /// FileParameter constructor.
        /// </summary>
        /// <param name="label">Label of the parameter. May contain spaces and special chars. </param>
        /// <param name="defaultValue">Default value (a file path) of the parameter.</param>
        /// <param name="extensions">Array of file extensions and/or descriptions (Ex. ".txt", "Image|.jpg", "Image file |.bmp, .png")</param>
        public FileParameter(string label, string defaultValue, params string[] extensions)
            : base(label, defaultValue)
        {
            ExtensionFilter = extensions;
        }



        internal FileParameter(FileParameterInfo fileParameterInfo)
            : base(fileParameterInfo)
        {
            IOOperation = fileParameterInfo.IOOperation;
        }



        /// <summary>
        /// Array of file extensions and/or descriptions (Ex. ".txt", "Image|.jpg", "Image file |.bmp, .png") 
        /// </summary>
        public string[] ExtensionFilter
        {
            get;
            set;
        } = new string[0];


        /// <summary>
        /// Indicates the type of operation, i.e. if we are loading or saving the file.
        /// </summary>
        public IOOperation IOOperation
        {
            get;
            set;
        } = IOOperation.Load;


        /*public T Load<T>()
        {
            return Procedure.LoadEnvironment.Load<T>(Value);
        }*/



        public bool ResourceExists()
        {
            return ProcedureEnvironment.GetService<IResourceManager>().Exists(Value);
        }



        /// <summary>
        /// Sets the value of the parameter.
        /// </summary>
        /// <param name="value">A string value containing a valid path. If the IOOperation is Load and the path does not exist, an exception is thrown.</param>
        protected override void SetData(object value)
        {
            string path = (string) value;

            /*if (!String.IsNullOrWhiteSpace(path) && !Path.IsPathRooted(path))
            {
                path = ProcedureEnvironment.Resources.GetFullPath(path);
            }
            else*/
            {
                path = PathHelper.ToUniversalPath(path);
            }

            if (IOOperation == IOOperation.Load && !string.IsNullOrWhiteSpace(path) && !ProcedureEnvironment.GetService<IResourceManager>().Exists(path))
                throw new FileNotFoundException("File or resource path '" + path + "' does not exist.", path);

            //base.SetData(path);
            base.SetData(PathHelper.ToUniversalPath((string) value));
        }



        /// <summary>
        /// Creates a ParameterInfo based on this paramter.
        /// </summary>
        /// <returns></returns>
        protected internal override ParameterInfo ToParameterInfo()
        {
            return new FileParameterInfo(this);
        }



        /*public String FullPath
        {
            get
            {
                
            }
        }*/
    }
}