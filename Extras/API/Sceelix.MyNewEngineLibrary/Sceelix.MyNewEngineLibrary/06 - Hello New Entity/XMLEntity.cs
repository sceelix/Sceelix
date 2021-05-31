using System.Collections.Generic;
using System.Xml;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// This shows how to create an entity. 
    /// 
    /// All that you need to do is inherit from BaseEntity (which already has the base features covered) 
    /// or implement the IEntity interface.
    /// 
    /// The EntityInfo attribute is not exactly mandatory, but you should add it with a more proper display
    /// name. This is used in a few procedures for entity filtering/selection. This display name should not 
    /// conflict with existing names.
    /// </summary>
    [Entity("XML Entity")]
    public class XMLEntity : Entity, IXMLThing
    {
        private XmlDocument _document;

        public XMLEntity(XmlDocument document)
        {
            _document = document;
        }


        public XmlDocument Document
        {
            get { return _document; }
        }


        /// <summary>
        /// If this entity had SubEntities (such as mesh, which has faces, vertices...) 
        /// they should be enumerated here. This is important to guarantee that the
        /// attributes of SubEntities can be accessed.
        /// 
        /// In this case, there's not need to override the function.
        /// </summary>
        public override IEnumerable<IEntity> SubEntityTree
        {
            get { return base.SubEntityTree; }
        }


        /// <summary>
        /// This function should be implemented.
        /// </summary>
        /// <returns></returns>
        public override IEntity DeepClone()
        {
            //this function performs a MemberwiseClone
            //at this point, all the attributes are cloned
            var clone = (XMLEntity) base.DeepClone();

            clone._document = (XmlDocument)_document.Clone();
            
            return clone;
        }
    }


    //This is an optional approach. You can define interfaces that
    //inherit from IEntity and use it generically in inputs/outputs.
    [Entity("XML Thing")]
    interface IXMLThing : IEntity
    {
    }
}
