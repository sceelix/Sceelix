using System;

namespace Sceelix.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyEmailAttribute : Attribute
    {
        public AssemblyEmailAttribute(string email)
        {
            Email = email;
        }



        public string Email
        {
            get;
        }
    }
}