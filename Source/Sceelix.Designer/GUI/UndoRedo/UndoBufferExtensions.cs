using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Undo;

namespace Sceelix.Designer.GUI.UndoRedo
{
    public static class UndoBufferExtensions
    {
        public static void AddAndDo(this UndoBuffer undoBuffer, IUndoableOperation undoableOperation)
        {
            undoBuffer.Add(undoableOperation);
            undoableOperation.Do();
        }

        public static T AddAndReturn<T>(this UndoBuffer undoBuffer, T undoableOperation) where  T: IUndoableOperation
        {
            undoBuffer.Add(undoableOperation);
            return undoableOperation;
        }
    }
}
