using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;

namespace Sceelix.Documentation
{
    public class ProcedureDocumenter
    {

        private static readonly Size _imageSize = new Size(700, 300);

        private readonly Type _systemProcedureType;
        private readonly SystemNode _systemNode;
        private Color _shadowColor = Color.FromArgb(255, 146, 155, 151);



        public ProcedureDocumenter(Type systemProcedureType)
        {
            _systemProcedureType = systemProcedureType;

            SystemProcedure procedure = (SystemProcedure) Activator.CreateInstance(_systemProcedureType);
            _systemNode = new SystemNode(procedure, new Core.Graphs.Point());
        }



        private ProcedureAttribute ProcedureAttribute
        {
            get { return _systemNode.ProcedureAttribute; }
        }



        public bool IsObsolete
        {
            get { return _systemNode.IsObsolete; }
        }



        public String Category
        {
            get { return _systemNode.ProcedureAttribute.Category; }
        }



        public String Generate(String destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            var fileName = _systemProcedureType.FullName.Replace(".", "-") + "_" + _systemNode.ProcedureAttribute.Guid + ".mdx";
            var fileLocation = Path.Combine(destinationFolder, fileName);
            var imageFileLocation = "images/NodeImage_" + _systemNode.ProcedureAttribute.Guid + ".png";

            File.WriteAllText(fileLocation, GetMdxDocCode(imageFileLocation));

            //now, generate the image
            GenerateImage(Path.Combine(destinationFolder, imageFileLocation));

            return "[" + _systemNode.DefaultLabel + "](" + fileName + ")";
        }



        private String ReplaceNewLinesWithBr(String str)
        {
            return str.Replace("\n\n", "<br/>").Replace("\n", "");
        }



        private string GetMdxDocCode(String imagePath)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("---");
            builder.AppendLine("title: " + _systemNode.DefaultLabel);
            builder.AppendLine("---");
            builder.AppendLine();

            builder.AppendLine("import {ParameterTree} from '@site/src/components/TreeContent'").AppendLine();
            builder.AppendLine(ReplaceNewLinesWithBr(ProcedureAttribute.Description)).AppendLine();
            

            builder.AppendLine("![" + Path.GetFileNameWithoutExtension(imagePath) + "](" + imagePath + ")");

            var tags = _systemNode.Tags;

            if (!String.IsNullOrWhiteSpace(tags))
            {
                builder.AppendLine("<b>Tags:</b> " + tags).AppendLine();
            }


            if (!String.IsNullOrWhiteSpace(ProcedureAttribute.Remarks))
            {
                builder.AppendLine("## Remarks");
                builder.AppendLine(ReplaceNewLinesWithBr(ProcedureAttribute.Remarks));
                builder.AppendLine();
            }


            List<PortReference> portReferences = new List<PortReference>();

            if (_systemNode.Parameters.Any())
            {
                builder.AppendLine("## Parameters").AppendLine();
                builder.AppendLine("export const Parameters = ({children}) => (<>");

                
                WriteParameters(builder, 0, _systemNode.Parameters, portReferences);

                builder.AppendLine("</>);").AppendLine();
                builder.AppendLine("<Parameters />");
            }

            WritePorts(builder, "Inputs", _systemNode.InputPorts.Where(x => !x.IsParameterPort).ToList(), portReferences.Where(x => x.Type == "Input").ToList());

            builder.AppendLine("<br/>");

            WritePorts(builder, "Outputs", _systemNode.OutputPorts.Where(x => !x.IsParameterPort).ToList(), portReferences.Where(x => x.Type == "Output").ToList());


            return builder.ToString();
        }



        private void WriteParameters(StringBuilder builder, int level, List<ParameterInfo> parameters, List<PortReference> portReferences)
        {
            if (parameters.Any())
            {
                foreach (var parameter in parameters)
                {
                    String description = parameter.Description;

                    List<int> portNumbers = new List<int>();

                    foreach (InputPort inputPort in parameter.InputPorts)
                        portNumbers.Add(CreatePortReference("Input",inputPort, portReferences));
                        
                    foreach (OutputPort outputPort in parameter.OutputPorts)
                        portNumbers.Add(CreatePortReference("Output", outputPort, portReferences));
                    
                    for (int i = 0; i < level; i++)
                        builder.Append("\t");

                    
                    builder.Append("<ParameterTree name=" + parameter.Label.Quote() + " type=" + parameter.MetaType.Quote());

                    if (portNumbers.Any())
                        builder.Append(" ports={[" + String.Join(",", portNumbers) + "]}");

                    if (level == 0)
                        builder.Append(" open={true}");


                    if(String.IsNullOrWhiteSpace(description))
                        Console.WriteLine($"WARN: Parameter {parameter.Label} does not have a description.");
                    else
                    {
                        builder.Append(" description={<>" + description.Replace("\"", "&quot;") + "</>}");
                    }


                    if (parameter.ItemModels.Any())
                    {
                        builder.AppendLine(">");
                        WriteParameters(builder, level + 1, parameter.ItemModels, portReferences);
                        builder.AppendLine("</ParameterTree>");
                    }
                    else
                        builder.AppendLine("/>");
                }
            }
        }



        private void WritePorts(StringBuilder builder, String portType, List<Port> ports, List<PortReference> portDescriptions)
        {
            builder.AppendLine();
            builder.AppendLine("## " + portType );

            if (!ports.Any())
            {
                builder.AppendLine("<i>This node has no native " + portType.ToLower() + ".</i>");
                builder.AppendLine();
            }
            else
            {
                foreach (var port in ports)
                    builder.AppendLine("* " + GetMdxDocCode(port));

                builder.AppendLine();
            }

            if (portDescriptions.Any())
            {
                builder.AppendLine("### Parameter " + portType + ":");

                foreach (var portReference in portDescriptions)
                    builder.AppendLine(String.Format("* <a name=\"port{0}\">[{0}]</a> {1}", portReference.Id,portReference.Description));
            }
        }



        private String GetMdxDocCode(Port port)
        {
            if(port is InputPort)
                return String.Format("<b>{0}</b> [{1} | <i>{2}</i>]: {3}", port.Label, port.Nature, Entity.GetDisplayName(port.ObjectType), port.Description);

            return String.Format("<b>{0}</b> [<i>{1}</i>]: {2}", port.Label, Entity.GetDisplayName(port.ObjectType), port.Description);
        }



        private int CreatePortReference(String type, Port port, List<PortReference> portReferences)
        {
            var description = GetMdxDocCode(port);

            var portReference = portReferences.FirstOrDefault(x => x.Description == description);
            if(portReference == null)
                portReferences.Add(portReference = new PortReference(type, description, portReferences.Count + 1));

            //return String.Format("{0}<a href=\"#port{1}\"><sup>[{1}]</sup></a>", port.Label, portReference.Id);
            return portReference.Id; //String.Format("<a href=\"#port{0}\">{0}</a>", portReference.Id);
        }



        private void GenerateImage(string imagePath)
        {
            //create the image folder first
            var imageDirectory = Path.GetDirectoryName(imagePath);
            if (!Directory.Exists(imageDirectory))
                Directory.CreateDirectory(imageDirectory);


            Font font = new Font("Arial", 18);
            Size padding = new Size(25, 25);

            Bitmap bitmap = new Bitmap(_imageSize.Width, _imageSize.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                //set the background color
                graphics.Clear(Color.FromArgb(255, 240, 255, 248));

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                graphics.InterpolationMode = InterpolationMode.High;

                //determine the color for node and text
                var color = FromHex(ProcedureAttribute.HexColor);
                var textColor = Extreme(color);

                var labelSize = graphics.MeasureString(_systemNode.DefaultLabel, font);

                var nodeSize = new SizeF(labelSize.Width + padding.Width*2, labelSize.Height + padding.Height*2);
                //var nodePosition = new PointF(nodeSize.Width/2f - labelSize.Width/2f, nodeSize.Height/2f - labelSize.Height/2f);
                var nodePosition = new PointF(_imageSize.Width / 2f - nodeSize.Width / 2f, _imageSize.Height / 2f - nodeSize.Height / 2f);
                var textPosition = new PointF(_imageSize.Width / 2f - labelSize.Width / 2f, _imageSize.Height / 2f - labelSize.Height / 2f);

                //draw shadow
                graphics.FillRectangle(new SolidBrush(_shadowColor), new RectangleF(nodePosition.X+20, nodePosition.Y+20, nodeSize.Width, nodeSize.Height));

                //draw the input ports
                DrawPorts(graphics, _systemNode.InputPorts, new PointF(nodePosition.X + 20, nodePosition.Y + 20), nodeSize, true);
                DrawPorts(graphics, _systemNode.OutputPorts, new PointF(nodePosition.X + 20, nodePosition.Y + 20), nodeSize, true);

                //draw the rectangle fill and border
                graphics.FillRectangle(new SolidBrush(color), new RectangleF(nodePosition.X, nodePosition.Y, nodeSize.Width, nodeSize.Height));
                graphics.DrawRectangle(new Pen(Color.Black, 5), nodePosition.X, nodePosition.Y, nodeSize.Width, nodeSize.Height);

                //draw the node label
                graphics.DrawString(_systemNode.DefaultLabel, font,new SolidBrush(textColor), textPosition);

                //draw the input ports
                DrawPorts(graphics, _systemNode.InputPorts, nodePosition, nodeSize, false);
                DrawPorts(graphics, _systemNode.OutputPorts, nodePosition,nodeSize,false);
            }

            bitmap.Save(imagePath, ImageFormat.Png);
        }



        private void DrawPorts(Graphics graphics, List<Port> ports, PointF nodePosition, SizeF nodeSize, bool isShadow)
        {
            var borderPen = new Pen(Color.Black, 3);
            var fillBrush = isShadow ? new SolidBrush(_shadowColor) : new SolidBrush(Color.White);
            var portSize = new Size(20,20);
            var portStartingX = 10;


            float xSpacing = (nodeSize.Width - portStartingX * 2f) / ports.Count;
            
            for (int i = 0; i < ports.Count; i++)
            {
                float xPosition = nodePosition.X + portStartingX + i * xSpacing + xSpacing / 2f - portSize.Width/2f;
                float yPosition = ports[i] is InputPort ? nodePosition.Y - portSize.Height/2f : nodePosition.Y + nodeSize.Height - portSize.Height/2f;

                graphics.FillEllipse(fillBrush,new RectangleF(xPosition, yPosition, portSize.Width,portSize.Height));

                if(!isShadow)
                    graphics.DrawEllipse(borderPen, new RectangleF(xPosition, yPosition, portSize.Width, portSize.Height));
            }
        }



        private Color Extreme(Color color)
        {
            if (color.R >= 192 || color.G >= 192 || color.B >= 192)
                return Color.FromArgb(255, 0, 0, 0);

            return Color.FromArgb(255, 255, 255, 255);
        }



        public static Color FromHex(String hexString)
        {
            if (hexString.StartsWith("#"))
                hexString = hexString.Replace("#", "");

            var components = hexString.SplitSize(2).ToArray();

            var red = (byte) Int32.Parse(components[0], NumberStyles.HexNumber);
            var green = (byte) Int32.Parse(components[1], NumberStyles.HexNumber);
            var blue = (byte) Int32.Parse(components[2], NumberStyles.HexNumber);

            return Color.FromArgb(255, red, green, blue);
        }



        public class PortReference
        {
            public string Type
            {
                get;
                set;
            }



            public String Description
            {
                get;
                set;
            }

            public int Id
            {
                get;
                set;
            }

            public PortReference(String type, string description, int id)
            {
                Type = type;
                Description = description;
                Id = id;
            }
        }
    }
}