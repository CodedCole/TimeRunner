using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    
    public class PriorityQueue<TItem,TPriority> where TPriority: IComparable
    {
        private KeyValuePair<TItem, TPriority>[] _heap;
        private int _count;

        public PriorityQueue(int maxSize) 
        {
            _heap = new KeyValuePair<TItem, TPriority>[maxSize];
            _count = 0;
        }

        public void Push(KeyValuePair<TItem, TPriority> pair)
        {
            //put pair at the bottom of the heap
            _heap[_count] = pair;

            int current = _count;   //offset count by 1
            int parent = (current - 1) / 2;
            while(parent > 0)
            {
                //move the pair up the heap if the parent has a greater priority value (lowest value comes first)
                if (pair.Value.CompareTo(_heap[parent].Value) < 0)
                {
                    KeyValuePair<TItem, TPriority> temp = _heap[parent];
                    _heap[parent] = pair;
                    _heap[current] = temp;
                    current = parent;
                }
                else
                {
                    break;
                }
                parent = (parent - 1) / 2;
            }
            _count++;
        }

        public TItem Pop()
        {
            //get top of heap
            TItem item = _heap[0].Key;

            //put bottom of the heap on top
            _heap[0] = _heap[_count - 1];
            _count--;

            //grab bottom of heap and push it back down
            int parent = 1;
            int child1 = 2;
            int child2 = 3;
            int indexToSwapTo;
            while (parent < _count)
            {
                indexToSwapTo = -1;
                if (_heap[parent].Value.CompareTo(_heap[child1].Value) > 0)
                {
                    indexToSwapTo = child1;
                }
                if (_heap[parent].Value.CompareTo(_heap[child2].Value) > 0)
                {
                    if ((indexToSwapTo == -1) || (indexToSwapTo == child1 && _heap[child1].Value.CompareTo(_heap[child2].Value) > 0))
                        indexToSwapTo = child2;
                }

                if (indexToSwapTo == -1)
                {
                    break;
                }
                else
                {
                    var temp = _heap[indexToSwapTo];
                    _heap[indexToSwapTo] = _heap[parent];
                    _heap[parent] = temp;

                    parent = indexToSwapTo;
                    child1 = (parent * 2) + 1;
                    child2 = (parent * 2) + 2;
                }
            }

            //return the original top of the heap
            return item;
        }

        public bool Empty()
        {
            return _count <= 0;
        }

        public void LogHeap()
        {
            string output = "";
            for (int i = 0; i < _count; i++)
            {
                output += _heap[i].ToString() + ", ";
            }
            Debug.Log(output);
        }
    }
}
