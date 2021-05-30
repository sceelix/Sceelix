using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Data;
using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters;

namespace Sceelix.Core.IO
{
    public class InputReference
    {
        public InputReference(Input input)
        {
            Input = input;
        }



        public int Count => Input.QueueCount;


        public Type EntityType => Input.EntityType;


        public string Identifier => Input.Identifier;


        internal Input Input
        {
            get;
        }


        public InputPort InputPort => Input.ToInputPort(Input.Parent is Parameter);


        public string Label => Input.Label;



        public void Enqueue(params IEntity[] data)
        {
            Input.Enqueue(data);
        }



        public void Enqueue(IEnumerable<IEntity> data)
        {
            Input.Enqueue(data);
        }



        public void Enqueue<T>(params T[] data) where T : IEntity
        {
            Input.Enqueue(data.Cast<IEntity>());
        }



        public void Enqueue<T>(IEnumerable<T> data) where T : IEntity
        {
            Input.Enqueue((IEnumerable<IEntity>) data);
        }



        public T Peek<T>() where T : IEntity
        {
            return (T) Input.CurrentObjects.Peek();
        }



        public IEnumerable<IEntity> PeekAll()
        {
            return new List<IEntity>(Input.CurrentObjects);
        }



        public IEnumerable<T> PeekAll<T>() where T : IEntity
        {
            return new List<IEntity>(Input.CurrentObjects).Cast<T>();
        }
    }
}