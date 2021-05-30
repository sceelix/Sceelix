using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Game.UI.Controls;

namespace Sceelix.Designer.Settings.Types
{
    public class TimeApplicationField : PrimitiveApplicationField<TimeSpan>
    {
        public TimeApplicationField(TimeSpan defaultValue)
            : base(defaultValue)
        {
        }



        public override UIControl GetControl()
        {
            throw new NotImplementedException();
        }
    }
}