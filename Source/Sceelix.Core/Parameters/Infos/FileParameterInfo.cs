using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sceelix.Conversion;
using Sceelix.Core.Environments;
using Sceelix.Core.Resources;
using Sceelix.Extensions;
using Sceelix.Helpers;

namespace Sceelix.Core.Parameters.Infos
{
    public class FileParameterInfo : PrimitiveParameterInfo<string>
    {
        //for project files, we can store the guid to keep references



        public FileParameterInfo(string label)
            : base(label)
        {
            ExtensionFilter = new string[0];
            FixedValue = string.Empty;
        }



        public FileParameterInfo(FileParameter parameter)
            : base(parameter)
        {
            //create a clone of the strings
            ExtensionFilter = parameter.ExtensionFilter.ToArray();
            IOOperation = parameter.IOOperation;
            //_searchScope = parameter.SearchScope;
        }



        public FileParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
            FixedValue = PathHelper.ToPlatformPath(FixedValue);

            IOOperation = xmlNode.GetAttributeOrDefault("FileOperation", IOOperation);
            FileGuid = xmlNode.GetAttributeOrDefault<string>("Guid");
            //_searchScope = xmlNode.GetAttributeOrDefault("SearchScope", _searchScope);

            List<string> choiceList = new List<string>();

            foreach (XmlElement xmlElement in xmlNode["Extensions"].GetElementsByTagName("Extension")) choiceList.Add(xmlElement.InnerText);

            ExtensionFilter = choiceList.ToArray();
        }



        /// <summary>
        /// Array of file extensions, with (Ex. .txt, .jpg)
        /// </summary>
        public string[] ExtensionFilter
        {
            get;
            set;
        }


        public string FileGuid
        {
            get;
            set;
        }


        public IOOperation IOOperation
        {
            get;
            set;
        } = IOOperation.Load;


        public override string MetaType => "File";


        public override string ValueLiteral => FixedValue?.Quote();



        public override object Clone()
        {
            var fileParameterInfo = (FileParameterInfo) base.Clone();

            fileParameterInfo.ExtensionFilter = ExtensionFilter.Select(x => x).ToArray();

            return fileParameterInfo;
        }



        public override IEnumerable<string> GetReferencedPaths()
        {
            yield return FixedValue;
        }



        public override void ReadArgumentXML(XmlNode xmlNode, IProcedureEnvironment procedureEnvironment)
        {
            base.ReadArgumentXML(xmlNode, procedureEnvironment);

            FileGuid = xmlNode.GetAttributeOrDefault<string>("Guid");

            //if it was empty, don't care
            if (string.IsNullOrWhiteSpace(FixedValue))
                return;

            //if this is not a rooted path, it is a project path, so check if the file exists
            //it if doesn't, look for the new path
            if (!Path.IsPathRooted(FixedValue))
            {
                var resources = procedureEnvironment.GetService<IResourceManager>();
                if (FileGuid != null)
                {
                    //if the resource no linger exists, look for it through the guid
                    if (!resources.Exists(FixedValue))
                    {
                        var newRelativePath = resources.Lookup(FileGuid);
                        if (!string.IsNullOrEmpty(newRelativePath))
                            FixedValue = newRelativePath;
                    }
                }
                else
                {
                    //if the guid is not set, we need to look for it
                    //so that we can use it to recover the file later on
                    var newGuid = resources.GetGuid(FixedValue);
                    if (!string.IsNullOrEmpty(newGuid))
                        FileGuid = newGuid;
                }
            }
        }



        internal override bool RefactorReferencedFile(IProcedureEnvironment procedureEnvironment, string originalPath, string replacementPath)
        {
            //if it was empty, don't care
            if (string.IsNullOrWhiteSpace(FixedValue))
                return false;

            //if this path is rooted, make the origin and destination rooted as well
            if (Path.IsPathRooted(FixedValue))
            {
                var resources = procedureEnvironment.GetService<IResourceManager>();
                originalPath = resources.GetFullPath(originalPath);
                replacementPath = resources.GetFullPath(replacementPath);
            }

            //simple case
            if (FixedValue == originalPath)
            {
                FixedValue = replacementPath;
                return true;
            }

            return false;
        }



        internal override bool RefactorReferencedFolder(IProcedureEnvironment loadEnvironment, string originalPath, string replacementPath)
        {
            //if it was empty, don't care
            if (string.IsNullOrWhiteSpace(FixedValue))
                return false;

            //if this path is rooted, make the origin and replacement rooted as well
            if (Path.IsPathRooted(FixedValue))
            {
                var resourceManager = loadEnvironment.GetService<IResourceManager>();
                originalPath = resourceManager.GetFullPath(originalPath);
                replacementPath = resourceManager.GetFullPath(replacementPath);
            }

            if (FixedValue.StartsWith(originalPath + PathHelper.UniversalDirectorySeparator))
            {
                FixedValue = Path.Combine(replacementPath, FixedValue.Substring(originalPath.Length + 1));
                return true;
            }

            return false;
        }



        public override Parameter ToParameter()
        {
            return new FileParameter(this);
        }



        protected override void WriteFixedValue(XmlWriter writer)
        {
            writer.WriteAttributeString("FixedValue", PathHelper.ToUniversalPath(ConvertHelper.Convert<string>(FixedValue)));

            if (FileGuid != null)
                writer.WriteAttributeString("Guid", FileGuid);
        }



        public override void WriteXMLDefinition(XmlWriter writer)
        {
            writer.WriteAttributeString("FileOperation", IOOperation.ToString());
            //writer.WriteAttributeString("SearchScope", _searchScope.ToString());

            base.WriteXMLDefinition(writer);

            writer.WriteStartElement("Extensions");
            {
                foreach (string extension in ExtensionFilter) writer.WriteElementString("Extension", extension);
            }
            writer.WriteEndElement();
        }
    }
}