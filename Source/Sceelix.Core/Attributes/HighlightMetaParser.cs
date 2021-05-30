using Sceelix.Core.Annotations;

namespace Sceelix.Core.Attributes
{
    [MetaManager("Highlight")]
    public class HighlightMetaParser : IMetaParser
    {
        //private static int _hightlightCode;

        //private readonly Dictionary<String, int> _colors = new Dictionary<string, int>();



        //the response code + attributeString is stored on a dictionary, so that this does not have to be called again
        public object Parse(string metaToken, string[] metaStringParts)
        {
            return new HighlightMeta();

            /*if (metaToken.StartsWith("highlight"))
            {
                if (metaToken.Contains("#"))
                {
                    var colorString = metaToken.Split('#').Last();
                    var colorMetaCode = AttributeMetaKeyManager.GetNewCode(this);
                    _colors.Add(colorString, colorMetaCode);

                    return new {_hightlightCode, colorMetaCode};
                }
            }

            return null;*/
        }



        /*public void GetHighlightMeta(AttributeKey key)
        {
            var codesOfMetaManager = AttributeMetaKeyManager.GetCodesOfMetaManager<HighlightMetaManager>(key.MetaData);

            foreach (var code in codesOfMetaManager)
            {
                var colorString = _colors.FirstOrDefault(x => x.Value == code);

            }
        }*/


        /*public static int HightlightCode
        {
            get { return _hightlightCode; }
        }*/
    }
}