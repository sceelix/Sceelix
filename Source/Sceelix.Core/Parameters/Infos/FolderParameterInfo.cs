using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sceelix.Conversion;
using Sceelix.Core.Environments;
using Sceelix.Core.Resources;
using Sceelix.Extensions;
using Sceelix.Helpers;

namespace Sceelix.Core.Parameters.Infos
{
    public class FolderParameterInfo : PrimitiveParameterInfo<string>
    {
        //for project files, we can store the guid to keep references



        public FolderParameterInfo(string label)
            : base(label)
        {
            FixedValue = string.Empty;
        }



        public FolderParameterInfo(FolderParameter parameter)
            : base(parameter)
        {
            //_searchScope = parameter.SearchScope;
            IOOperation = parameter.IOOperation;
        }



        public FolderParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
            FixedValue = PathHelper.ToPlatformPath(FixedValue);

            IOOperation = xmlNode.GetAttributeOrDefault("FileOperation", IOOperation);
        }



        public string FolderGuid
        {
            get;
            set;
        }


        public IOOperation IOOperation
        {
            get;
            set;
        } = IOOperation.Load;


        public override string MetaType => "Folder";


        public override string ValueLiteral => FixedValue?.Quote();



        public override IEnumerable<string> GetReferencedPaths()
        {
            yield return FixedValue;
        }



        public override void ReadArgumentXML(XmlNode xmlNode, IProcedureEnvironment procedureEnvironment)
        {
            base.ReadArgumentXML(xmlNode, procedureEnvironment);

            FolderGuid = xmlNode.GetAttributeOrDefault<string>("Guid");

            //if it was empty, don't care
            if (string.IsNullOrWhiteSpace(FixedValue))
                return;

            //if this is not a rooted path, it is a project path, so check if the file exists
            //it if doesn't, look for the new path
            if (!Path.IsPathRooted(FixedValue))
            {
                var resources = procedureEnvironment.GetService<IResourceManager>();
                if (FolderGuid != null)
                {
                    //if the resource no linger exists, look for it through the guid
                    if (!resources.Exists(FixedValue))
                    {
                        var newRelativePath = resources.Lookup(FolderGuid);
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
                        FolderGuid = newGuid;
                }
            }
        }



        internal override bool RefactorReferencedFolder(IProcedureEnvironment loadEnvironment, string originalPath, string replacementPath)
        {
            //if it was empty, don't care
            if (string.IsNullOrWhiteSpace(FixedValue))
                return false;

            //if this path is rooted, make the origin and destination rooted as well
            if (Path.IsPathRooted(FixedValue))
            {
                var resources = loadEnvironment.GetService<IResourceManager>();

                originalPath = resources.GetFullPath(originalPath);
                replacementPath = resources.GetFullPath(replacementPath);
            }

            //first simple case
            if (FixedValue == originalPath)
            {
                FixedValue = replacementPath;
                return true;
            }

            if ((FixedValue + "/").StartsWith(originalPath + "/"))
            {
                FixedValue = Path.Combine(replacementPath, FixedValue.Substring(originalPath.Length + 1));
                return true;
            }

            return false;
        }



        public override Parameter ToParameter()
        {
            return new FolderParameter(this);
        }



        protected override void WriteFixedValue(XmlWriter writer)
        {
            writer.WriteAttributeString("FixedValue", ConvertHelper.Convert<string>(PathHelper.ToUniversalPath(FixedValue)));

            if (FolderGuid != null)
                writer.WriteAttributeString("Guid", FolderGuid);
        }



        public override void WriteXMLDefinition(XmlWriter writer)
        {
            writer.WriteAttributeString("FileOperation", IOOperation.ToString());

            base.WriteXMLDefinition(writer);
        }
    }
}