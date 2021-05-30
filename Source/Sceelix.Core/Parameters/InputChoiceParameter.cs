using System.Collections.Generic;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters
{
    #region Abstract Definition

    public abstract class InputParameter<T> : CompoundParameter where T : IEntity
    {
        protected InputParameter(string label)
            : base(label)
        {
        }



        public abstract IEnumerable<T> Read();
    }

    #endregion

    #region Single Input

    /// <summary>
    /// Provides a single input.
    /// </summary>
    /// <typeparam name="T">Type of entity that this input will process.</typeparam>
    /// <seealso cref="InputParameter{T}" />
    public class SingleInputParameter<T> : InputParameter<T> where T : IEntity
    {
        /// <summary>
        /// Overridden.
        /// </summary>
        private readonly SingleInput<T> _input = new SingleInput<T>("Single");



        public SingleInputParameter()
            : base("Single")
        {
            _input.Description = "Processes one " + Entity.GetDisplayName(typeof(T)) + " entity at a time.";
        }



        public override IEnumerable<T> Read()
        {
            return new List<T> {_input.Read()};
        }
    }

    #endregion

    #region Dual Input

    /// <summary>
    /// Provides two single inputs.
    /// </summary>
    /// <typeparam name="T">Type of entities that these inputs will process.</typeparam>
    /// <seealso cref="InputParameter{T}" />
    public class DualInputParameter<T> : InputParameter<T> where T : IEntity
    {
        /// <summary>
        /// Overridden.
        /// </summary>
        private readonly SingleInput<T> _input1 = new SingleInput<T>("First");

        /// <summary>
        /// Overridden.
        /// </summary>
        private readonly SingleInput<T> _input2 = new SingleInput<T>("Second");



        public DualInputParameter()
            : base("Dual")
        {
            _input1.Description = "Processes one " + Entity.GetDisplayName(typeof(T)) + " entity at a time.";
            _input2.Description = "Processes one " + Entity.GetDisplayName(typeof(T)) + " entity at a time.";
        }



        public override IEnumerable<T> Read()
        {
            return new List<T> {_input1.Read(), _input2.Read()};
        }
    }

    #endregion

    #region Collective Input

    /// <summary>
    /// Provides a collective input.
    /// </summary>
    /// <typeparam name="T">Type of entity that this input will process.</typeparam>
    /// <seealso cref="InputParameter{T}" />
    public class CollectiveInputParameter<T> : InputParameter<T> where T : IEntity
    {
        /// <summary>
        /// Overridden.
        /// </summary>
        private readonly CollectiveInput<T> _input = new CollectiveInput<T>("Collective");



        public CollectiveInputParameter()
            : base("Collective")
        {
            _input.Description = "Processes all " + Entity.GetDisplayName(typeof(T)) + " entities at a time.";
        }



        public override IEnumerable<T> Read()
        {
            return _input.Read();
        }
    }

    #endregion

    #region Single Or Collective Input

    /// <summary>
    /// Gives the choice between a "Single" or a "Collective" input.
    /// </summary>
    /// <typeparam name="T">Type of entity that these inputs will process.</typeparam>
    /// <seealso cref="ListParameter" />
    public class SingleOrCollectiveInputChoiceParameter<T> : SelectListParameter where T : IEntity
    {
        /// <summary>
        /// Constructor of this compound input choice.
        /// </summary>
        /// <param name="label">Label of the parameter</param>
        /// <param name="defaultSelection">"Single" or "Collective"</param>
        public SingleOrCollectiveInputChoiceParameter(string label, string defaultSelection)
            : base(label, () => new SingleInputParameter<T> {Description = "Processes one " + Entity.GetDisplayName(typeof(T)) + " entity at a time."},
                () => new CollectiveInputParameter<T> {Description = "Processes all " + Entity.GetDisplayName(typeof(T)) + " entities at a time."})
        {
            base.Set(defaultSelection);
        }



        /// <summary>
        /// gets the input data.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Read()
        {
            return SelectedItem.CastTo<InputParameter<T>>().Read();
        }
    }

    #endregion

    #region Dual Or Collective Input

    /// <summary>
    /// Gives the choice between a "Dual" or a "Collective" input.
    /// </summary>
    /// <typeparam name="T">Type of entity that these inputs will process.</typeparam>
    /// <seealso cref="ListParameter" />
    public class DualOrCollectiveInputChoiceParameter<T> : SelectListParameter where T : IEntity
    {
        /// <summary>
        /// Constructor of this compound input choice.
        /// </summary>
        /// <param name="label">Label of the parameter</param>
        /// <param name="defaultSelection">"Dual" or "Collective"</param>
        public DualOrCollectiveInputChoiceParameter(string label, string defaultSelection)
            : base(label, () => new DualInputParameter<T> {Description = "Processes two " + Entity.GetDisplayName(typeof(T)) + " entities at a time."},
                () => new CollectiveInputParameter<T> {Description = "Processes all " + Entity.GetDisplayName(typeof(T)) + " entities at a time."})
        {
            base.Set(defaultSelection);
        }



        /// <summary>
        /// gets the input data.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Read()
        {
            return SelectedItem.CastTo<InputParameter<T>>().Read();
        }
    }

    #endregion

    /*#region Single Or Collective Input

    /// <summary>
    /// Gives the choice between a "Single", a "Collective" or no input at all.
    /// </summary>
    /// <typeparam name="T">Type of entity that these inputs will process.</typeparam>
    /// <seealso cref="ListParameter" />
    public class SingleCollectiveOrNoneInputChoiceParameter<T> : SelectListParameter where T : IEntity
    {
        /// <summary>
        /// Constructor of this compound input choice.
        /// </summary>
        /// <param name="label">Label of the parameter</param>
        /// <param name="defaultSelection">"Single" or "Collective"</param>
        public SingleCollectiveOrNoneInputChoiceParameter(string label, string defaultSelection)
            : base(label,   () => new CompoundParameter("None") { Description = "Does not receive input entities." },
                            () => new SingleInputParameter<T>() { Description = "Processes one " + Entity.GetDisplayName(typeof(T)) + " entity at a time." },
                            () => new CollectiveInputParameter<T>() { Description = "Processes all " + Entity.GetDisplayName(typeof(T)) + " entities at a time." })
        {
            base.Set(defaultSelection);
        }



        /// <summary>
        /// gets the input data.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Read()
        {
            return Items.First().CastTo<InputParameter<T>>().Read();
        }
    }

    #endregion*/
}