using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.Messaging
{
    public class CompoundMessage
    {
        private readonly List<object> _messages;



        public CompoundMessage(IEnumerable<Object> messages)
        {
            _messages = messages.ToList();
        }



        public IEnumerable<Object> Messages
        {
            get { return _messages; }
        }



        protected bool Equals(CompoundMessage other)
        {
            return _messages.SequenceEqual(other._messages);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CompoundMessage) obj);
        }



        public override int GetHashCode()
        {
            return (_messages != null ? _messages.GetHashCode() : 0);
        }



        public static bool operator ==(CompoundMessage left, CompoundMessage right)
        {
            return Equals(left, right);
        }



        public static bool operator !=(CompoundMessage left, CompoundMessage right)
        {
            return !Equals(left, right);
        }
    }
}
