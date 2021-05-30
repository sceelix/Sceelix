using System;
using System.Collections.Generic;

namespace Sceelix.Collections
{
    /// <summary>
    /// Priority Queue data structure
    /// </summary>
    public class PriorityQueue<T> where T : IComparable
    {
        private readonly List<T> _storedValues;



        public PriorityQueue()
        {
            //Initialize the array that will hold the values
            _storedValues = new List<T>();

            //Fill the first cell in the array with an empty value
            _storedValues.Add(default(T));
        }



        /// <summary>
        /// Gets the number of values stored within the Priority Queue
        /// </summary>
        public virtual int Count => _storedValues.Count - 1;



        /// <summary>
        /// Restores the heap-order property between child and parent values going down towards the bottom
        /// </summary>
        protected virtual void BubbleDown(int startCell)
        {
            int cell = startCell;

            //Bubble down as long as either child is smaller
            while (IsLeftChildSmaller(cell) || IsRightChildSmaller(cell))
            {
                int child = CompareChild(cell);

                if (child == -1) //Left Child
                {
                    //Swap values
                    T parentValue = _storedValues[cell];
                    T leftChildValue = _storedValues[2 * cell];

                    _storedValues[cell] = leftChildValue;
                    _storedValues[2 * cell] = parentValue;

                    cell = 2 * cell; //move down to left child
                }
                else if (child == 1) //Right Child
                {
                    //Swap values
                    T parentValue = _storedValues[cell];
                    T rightChildValue = _storedValues[2 * cell + 1];

                    _storedValues[cell] = rightChildValue;
                    _storedValues[2 * cell + 1] = parentValue;

                    cell = 2 * cell + 1; //move down to right child
                }
            }
        }



        /// <summary>
        /// Restores the heap-order property between child and parent values going up towards the head
        /// </summary>
        protected virtual void BubbleUp(int startCell)
        {
            int cell = startCell;

            //Bubble up as long as the parent is greater
            while (IsParentBigger(cell))
            {
                //Get values of parent and child
                T parentValue = _storedValues[cell / 2];
                T childValue = _storedValues[cell];

                //Swap the values
                _storedValues[cell / 2] = childValue;
                _storedValues[cell] = parentValue;

                cell /= 2; //go up parents
            }
        }



        /// <summary>
        /// Compares the children cells of a parent cell. -1 indicates the left child is the smaller of the two,
        /// 1 indicates the right child is the smaller of the two, 0 inidicates that neither child is smaller than the parent.
        /// </summary>
        protected virtual int CompareChild(int parentCell)
        {
            bool leftChildSmaller = IsLeftChildSmaller(parentCell);
            bool rightChildSmaller = IsRightChildSmaller(parentCell);

            if (leftChildSmaller || rightChildSmaller)
            {
                if (leftChildSmaller && rightChildSmaller)
                {
                    //Figure out which of the two is smaller
                    int leftChild = 2 * parentCell;
                    int rightChild = 2 * parentCell + 1;

                    T leftValue = _storedValues[leftChild];
                    T rightValue = _storedValues[rightChild];

                    //Compare the values of the children
                    if (leftValue.CompareTo(rightValue) <= 0)
                        return -1; //left child is smaller
                    return 1; //right child is smaller
                }

                if (leftChildSmaller)
                    return -1; //left child is smaller
                return 1; //right child smaller
            }

            return 0; //both children are bigger or don't exist
        }



        /// <summary>
        /// Returns the minimum value inside the Priority Queue
        /// </summary>
        public virtual T Dequeue()
        {
            if (Count == 0) return default(T); //queue is empty

            //The smallest value in the Priority Queue is the first item in the array
            T minValue = _storedValues[1];

            //If there's more than one item, replace the first item in the array with the last one
            if (_storedValues.Count > 2)
            {
                T lastValue = _storedValues[_storedValues.Count - 1];

                //Move last node to the head
                _storedValues.RemoveAt(_storedValues.Count - 1);
                _storedValues[1] = lastValue;

                //Bubble down
                BubbleDown(1);
            }
            else
            {
                //Remove the only value stored in the queue
                _storedValues.RemoveAt(1);
            }

            return minValue;
        }



        /// <summary>
        /// Adds a value to the Priority Queue
        /// </summary>
        public virtual void Enqueue(T value)
        {
            //Add the value to the internal array
            _storedValues.Add(value);

            //Bubble up to preserve the heap property,
            //starting at the inserted value
            BubbleUp(_storedValues.Count - 1);
        }



        /// <summary>
        /// Returns whether the left child cell is smaller than the parent cell.
        /// Returns false if a left child does not exist.
        /// </summary>
        protected virtual bool IsLeftChildSmaller(int parentCell)
        {
            if (2 * parentCell >= _storedValues.Count)
                return false; //out of bounds
            return _storedValues[2 * parentCell].CompareTo(_storedValues[parentCell]) < 0;
            //return storedNodes[2 * parentCell].Key < storedNodes[parentCell].Key;
        }



        /// <summary>
        /// Returns if the value of a parent is greater than its child
        /// </summary>
        protected virtual bool IsParentBigger(int childCell)
        {
            if (childCell == 1)
                return false; //top of heap, no parent
            return _storedValues[childCell / 2].CompareTo(_storedValues[childCell]) > 0;
            //return storedNodes[childCell / 2].Key > storedNodes[childCell].Key;
        }



        /// <summary>
        /// Returns whether the right child cell is smaller than the parent cell.
        /// Returns false if a right child does not exist.
        /// </summary>
        protected virtual bool IsRightChildSmaller(int parentCell)
        {
            if (2 * parentCell + 1 >= _storedValues.Count)
                return false; //out of bounds
            return _storedValues[2 * parentCell + 1].CompareTo(_storedValues[parentCell]) < 0;
            //return storedNodes[2 * parentCell + 1].Key < storedNodes[parentCell].Key;
        }



        /// <summary>
        /// Returns the value at the head of the Priority Queue without removing it.
        /// </summary>
        public virtual T Peek()
        {
            if (Count == 0)
                return default(T); //Priority Queue empty
            return _storedValues[1]; //head of the queue
        }
    }

    public class PriorityQueue<T, TR> where T : IComparable
    {
        private readonly PriorityQueue<PriorityQueueItem> _priorityQueueItems = new PriorityQueue<PriorityQueueItem>();


        /// <summary>
        /// Gets the number of values stored within the Priority Queue
        /// </summary>
        public virtual int Count => _priorityQueueItems.Count - 1;



        /// <summary>
        /// Returns the minimum value inside the Priority Queue
        /// </summary>
        public virtual TR Dequeue()
        {
            return _priorityQueueItems.Dequeue().Value;
        }



        /// <summary>
        /// Adds a value to the Priority Queue
        /// </summary>
        public virtual void Enqueue(T comparable, TR value)
        {
            _priorityQueueItems.Enqueue(new PriorityQueueItem(comparable, value));
        }



        /// <summary>
        /// Returns the value at the head of the Priority Queue without removing it.
        /// </summary>
        public virtual TR Peek()
        {
            if (Count == 0)
                return default(TR); //Priority Queue empty
            return _priorityQueueItems.Peek().Value;
        }



        private class PriorityQueueItem : IComparable
        {
            private T _comparable;



            public PriorityQueueItem(T comparable, TR value)
            {
                _comparable = comparable;
                Value = value;
            }



            public TR Value
            {
                get;
            }



            public int CompareTo(object obj)
            {
                var item = (PriorityQueueItem) obj;
                return _comparable.CompareTo(item._comparable);
            }
        }
    }
}