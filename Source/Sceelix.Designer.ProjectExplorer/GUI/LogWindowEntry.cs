using System;
using Sceelix.Logging;

namespace Sceelix.Designer.ProjectExplorer.GUI
{
    public class LogWindowEntry
    {
        public LogWindowEntry()
        {
            Count = 1;
        }



        public int Id
        {
            get;
            set;
        }



        public LogType LogType
        {
            get;
            set;
        }



        public String Content
        {
            get;
            set;
        }



        public String Detail
        {
            get;
            set;
        }



        public Object ResponseMessage
        {
            get;
            set;
        }


        public int Count
        {
            get;
            set;
        }



        protected bool Equals(LogWindowEntry other)
        {
            return LogType == other.LogType && string.Equals(Content, other.Content) && string.Equals(Detail, other.Detail) && Equals(ResponseMessage, other.ResponseMessage);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LogWindowEntry)obj);
        }



        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)LogType;
                hashCode = (hashCode * 397) ^ (Content != null ? Content.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Detail != null ? Detail.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ResponseMessage != null ? ResponseMessage.GetHashCode() : 0);
                return hashCode;
            }
        }



        public static bool operator ==(LogWindowEntry left, LogWindowEntry right)
        {
            return Equals(left, right);
        }



        public static bool operator !=(LogWindowEntry left, LogWindowEntry right)
        {
            return !Equals(left, right);
        }
    }
}
