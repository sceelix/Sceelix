using System;

namespace Sceelix.Designer.Interfaces
{
    public interface IUpdateableElement
    {
        void Update(TimeSpan deltaTime);
    }
}
