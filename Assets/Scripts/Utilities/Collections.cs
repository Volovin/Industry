using System;

namespace Industry.Utilities.Collections
{
    /// <summary>
    /// Represents a generic priority queue collection 
    /// with insertion and removal complexity less than O(log N).
    /// </summary>
    /// <typeparam name="TValue">The type of items.</typeparam>
    /// <typeparam name="TPriority">The comparable type of the priority values.</typeparam>
    public class PriorityQueue<TValue, TPriority> where TPriority : IComparable<TPriority>
    {
        /// <summary>
        /// Represents the queue node.
        /// </summary>
        private class Node
        {
            public Node(TValue v, TPriority p)
            {
                value = v;
                priority = p;
            }

            public TValue value;
            public TPriority priority;

            public Node parent;
            public Node left;
            public Node right;

            public override string ToString()
            {
                return $"{value}: {priority}";
            }
        }

        public PriorityQueue()
        {

        }


        private int m_count;

        private Node m_root;
        private Node m_minNode;

        /// <summary>
        /// Total count of items in the queue.
        /// </summary>
        public int Count
        {
            get { return m_count; }
        }

        /// <summary>
        /// Adds the <paramref name="value"/> to the queue with priority of <paramref name="priority"/>.
        /// </summary>
        public void Enqueue(TValue value, TPriority priority)
        {
            Node node = new Node(value, priority);

            m_count++;

            if (m_root == null)
            {
                m_root = node;
                m_minNode = node;

                return;
            }

            if (priority.CompareTo(m_minNode.priority) < 0)
            {
                m_minNode.left = node;
                node.parent = m_minNode;
                m_minNode = node;

                return;
            }

            VisitChildren(m_root, node);
        }

        /// <summary>
        /// Removes the item with the least priority from the queue.
        /// </summary>
        /// <returns></returns>
        public TValue Dequeue()
        {
            var node = m_minNode;
            var right = m_minNode.right;

            if (right == null)
            {
                if (m_minNode == m_root)
                {
                    m_root = null;
                    m_minNode = null;
                }
                else
                {
                    m_minNode = m_minNode.parent;
                    m_minNode.left = null;
                }
            }
            else
            {
                if (m_minNode == m_root)
                {
                    m_root = right;
                    m_root.parent = null;
                }
                else
                {
                    m_minNode.parent.left = right;
                    right.parent = m_minNode.parent;
                }

                m_minNode = right;

                while (m_minNode.left != null)
                    m_minNode = m_minNode.left;

            }

            m_count--;

            return node.value;
        }

        /// <summary>
        /// Recursively traverses between the nodes in search of an appropriate position for <paramref name="newNode"/>.
        /// </summary>
        private void VisitChildren(Node curr, Node newNode)
        {
            if (newNode.priority.CompareTo(curr.priority) < 0)
            {
                if (curr.left == null)
                {
                    newNode.parent = curr;
                    curr.left = newNode;
                }
                else
                {
                    VisitChildren(curr.left, newNode);
                }
            }
            else
            {
                if (curr.right == null)
                {
                    newNode.parent = curr;
                    curr.right = newNode;
                }
                else
                {
                    VisitChildren(curr.right, newNode);
                }
            }
        }
    }

}
