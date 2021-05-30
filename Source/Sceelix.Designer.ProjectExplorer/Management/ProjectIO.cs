using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.ProjectExplorer.Management
{
    public static class ProjectIO
    {
        #region Loading

        public static Project Load(IServiceLocator services, String projectFilePath)
        {
            projectFilePath = Path.GetFullPath(projectFilePath);
            String name = Path.GetFileNameWithoutExtension(projectFilePath);
            String folderPath = Path.GetDirectoryName(projectFilePath);

            Project loadedProject = new Project(services, folderPath, name);

            var fileHandlerManager = services.Get<FileHandlerManager>();
            
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(projectFilePath));

            //get the "Extensions" xml node
            XmlElement root = doc.DocumentElement;

            //load all assemblies through their names
            if (root != null)
            {
                loadedProject.Author = root["Author"] != null ? root["Author"].InnerText : "";
                loadedProject.Version = root["Version"] != null ? root["Version"].InnerText : "";
                loadedProject.Company = root["Company"] != null ? root["Company"].InnerText : "";

                LoadReferences(fileHandlerManager, root["Items"], loadedProject.BaseFolder);

                loadedProject.BaseFolder.SortItems();
            }

            return loadedProject;
        }



        private static void LoadReferences(FileHandlerManager fileHandlerManager, XmlElement containerElement, FolderItem container)
        {
            foreach (XmlElement element in containerElement.GetChildren().Where(x => x.Name == "Item"))
            {
                String nature = element.Attributes["Nature"].InnerText;
                String fileName = element.Attributes["Path"].InnerText;
                //String color =  xmlElement.Attributes["Color"].InnerText;
                Guid guid = element.GetAttributeOrDefault("Guid", Guid.NewGuid());

                try
                {
                    ProjectItem projectItem;

                    if (nature == "File")
                    {
                        FileItem fileItem = new FileItem(fileName, container, guid);

                        //if the file does not exist on the disk, do not load this file
                        //TODO: in the future, present a "false" or "warning" file, so that the link is not lost
                        //if (!fileItem.ExistsPhysically)
                        //    continue;

                        IFileHandler fileHandler = fileHandlerManager.GetFileHandler(fileItem);
                        if (fileHandler != null)
                        {
                            Guid? otherGuid = fileHandler.GetGuid(fileItem);
                            if (otherGuid.HasValue)
                                fileItem.Guid = otherGuid.Value;
                        }

                        container.SubItems.Add(fileItem);

                        projectItem = fileItem;
                    }
                    else
                    {
                        FolderItem folderItem = new FolderItem(fileName, container, guid);

                        //if the folder does not exist on the disk, do not load this file
                        //TODO: in the future, present a "false" or "warning" folder, so that the link is not lost
                        if (!folderItem.ExistsPhysically)
                            continue;

                        if (element.Attributes["Expanded"] != null)
                            folderItem.Expanded = Convert.ToBoolean(element.Attributes["Expanded"].InnerText);

                        container.SubItems.Add(folderItem);

                        LoadReferences(fileHandlerManager, element, folderItem);

                        folderItem.SortItems();

                        projectItem = folderItem;
                    }

                    LoadProperties(element, projectItem);
                    //projectItem.Color = color;
                }
                catch (Exception ex)
                {
                    DesignerProgram.Log.Error("Could Not Load Project " + fileName + ".", ex);
                }
            }
        }



        private static void LoadProperties(XmlElement containerElement, ProjectItem projectItem)
        {
            foreach (XmlElement element in containerElement.GetChildren().Where(x => x.Name == "Property"))
            {
                String key = element.Attributes["Key"].InnerText;
                String value = element.Attributes["Value"].InnerText;

                projectItem.Properties.Add(key,value);
            }
        }

        #endregion

        #region Saving

        public static void Save(this Project project)
        {
            StringBuilder builder = new StringBuilder();

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings {Indent = true};

            //XmlWriter.
            using (XmlWriter writer = XmlWriter.Create(builder, xmlWriterSettings))
            {
                writer.WriteStartDocument();
                {
                    writer.WriteStartElement("Project");
                    {
                        writer.WriteElementString("Author", project.Author);
                        writer.WriteElementString("Company", project.Company);
                        writer.WriteElementString("Version", SceelixApplicationInfo.CurrentVersion.ToString());

                        writer.WriteStartElement("Items");
                        {
                            foreach (ProjectItem item in project.BaseFolder.SubItems)
                            {
                                SaveReference(writer, item);
                            }
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndDocument();
            }

            File.WriteAllText(project.ProjectFilePath, builder.ToString());
        }



        private static void SaveReference(XmlWriter writer, ProjectItem item)
        {
            writer.WriteStartElement("Item");

            writer.WriteAttributeString("Path", item.FileName);
            writer.WriteAttributeString("Nature", item.Nature);
            writer.WriteAttributeString("Guid",item.Guid.ToString());

            foreach (var property in item.Properties)
            {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("Key", property.Key);
                writer.WriteAttributeString("Value", property.Value);
                writer.WriteEndElement();
            }

            if (item is FolderItem)
            {
                var folderItem = (FolderItem) item;

                //perhaps this property should be saved in an auxiliary project file, but anyway...
                writer.WriteAttributeString("Expanded", folderItem.Expanded.ToString());

                foreach (ProjectItem subItem in folderItem.SubItems)
                {
                    SaveReference(writer, subItem);
                }
            }

            writer.WriteEndElement();
        }

        #endregion
    }
}