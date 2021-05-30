using Sceelix.Core.Annotations;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// A non-executable procedure for placing comments on graphs. 
    /// Text can be written on the node's label and placed next to other nodes, so as to provide further information
    /// or clarification of certain design choices.
    /// </summary>
    [Procedure("edd445f0-2dcd-41bb-912f-39b2e2d9daf7", Label = "Note", HexColor = "ffcc00", IsDummy = true)]
    public class NoteProcedure : SystemProcedure
    {
        protected override void Run()
        {
        }
    }
}