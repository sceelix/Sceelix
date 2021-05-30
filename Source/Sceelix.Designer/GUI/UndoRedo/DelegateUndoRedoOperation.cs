using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Undo;

namespace Sceelix.Designer.GUI.UndoRedo
{
    public class DelegateUndoRedoOperation : IUndoableOperation
    {
        private readonly Action _doAction;
        private readonly Action _undoAction;
        private readonly string _description;



        public DelegateUndoRedoOperation(Action doAction, Action undoAction, String description = "")
        {
            _doAction = doAction;
            _undoAction = undoAction;
            _description = description;
        }



        public void Undo()
        {
            _undoAction();
        }



        public void Do()
        {
            _doAction();
        }



        public object Description
        {
            get { return _description; }
        }
    }
}
