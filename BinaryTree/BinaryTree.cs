using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BinaryTree
{
    public class BinaryTree<T> : IEnumerable<T>
    {

        //Fields
        private BinaryTreeNode<T> root;


        /// <summary>
        /// Handles events for adding and removing elements
        /// </summary>
        /// <param name="sender">Instance of <see cref="BinaryTree<T>"/> that called the event</param>
        /// <param name="args">Arguments passed by sender for subscribers</param>
        public delegate void TreeEventHandler(object sender, TreeEventArgs<T> args);

        /// <summary>
        /// Event that should be called when new element is added
        /// </summary>
        public event TreeEventHandler ElementAdded;

        /// <summary>
        /// Event that should be called when element in tree is removed
        /// </summary>
        public event TreeEventHandler ElementRemoved;

        /// <summary>
        /// Defines how many elements tree contains
        /// </summary>
        public int Count { get; private set; }

        public IComparer<T> Comparer { get; }

        /// <summary>
        /// Checks if type T implements <see cref="IComparable<T>"/>
        /// If it does: saves and uses as default comparer
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when T doesn't implement <see cref="IComparable<T>"</exception>
        public BinaryTree()
        {
            if (typeof(T).GetInterfaces().Contains(typeof(IComparable<T>)))
            {
                Comparer = Comparer<T>.Default;
            }
            else
            {
                throw new ArgumentException("T not implemented IComparable<T>");
            }
        }

        /// <summary>
        /// Creates instance of tree and saves custom comparer passed by parameter
        /// </summary>
        /// <param name="comparer"><see cref="IComparer<T>"/></param>
        public BinaryTree(IComparer<T> comparer)
        {
            Comparer = comparer;
        }

        /// <summary>
        /// Adds element to the tree according to comparer
        /// </summary>
        /// <param name="item">Object that should be added in tree</param>
        /// <exception cref="ArgumentNullException">Thrown if parameter was null</exception>
        public void Add(T item)
        {
            if (item is null)
            {
                throw new ArgumentNullException("item");
            }

            var node = new BinaryTreeNode<T>(item);
            var EventArg = new TreeEventArgs<T>(item, "Item has been added");

            if (root == null)
                root = node;
            else
            {
                BinaryTreeNode<T> current = root, parent = null;

                while (current != null)
                {
                    parent = current;
                    if (Comparer.Compare(item, current.Value) < 0) current = current.Left;
                    else current = current.Right;
                }

                if (Comparer.Compare(item, parent.Value) < 0) parent.Left = node;
                else parent.Right = node;
            }
            Count++;
            ElementAdded?.Invoke(this, EventArg);
        }

        /// <summary>
        /// Removes element from tree by its reference
        /// </summary>
        /// <param name="item">Object that should be removed from tree</param>
        /// <returns>True if element was deleted succesfully, false if element wasn't found in tree</returns>
        public bool Remove(T item)
        {
            BinaryTreeNode<T> current;

            // Find removal node
            current = FindWithParent(item, out BinaryTreeNode<T> parent);

            if (current == null)
            {
                return false;
            }

            Count--;

            // Case 1: If there are no childs on right, left child will put on deleting place.
            if (current.Right == null)
            {
                if (parent == null)
                {
                    root = current.Left;
                }
                else
                {
                    int result = Comparer.Compare(parent.Value, current.Value);
                    if (result > 0)
                    {
                        // If value of parent higher than current,
                        // Left child of current node becomes left child of parent node.
                        parent.Left = current.Left;
                    }
                    else if (result < 0)
                    {   // If value of parent lower than current,
                        // Left child of current node becomes right child of parent node.
                        parent.Right = current.Left;
                    }
                }
            }
            //  Case 2: If right child has no childs on left side, so it will put on deleting place.
            else if (current.Right.Left == null)
            {
                current.Right.Left = current.Left; if (parent == null) { root = current.Right; }
                else
                {
                    int result = Comparer.Compare(parent.Value, current.Value);
                    if (result > 0)
                    {
                        // If value of parent higher than current,
                        // Right child of current node becomes left child of parent node.
                        parent.Left = current.Right;
                    }
                    else if (result < 0)
                    {   // If value of parent lower than current,
                        // Right child of current node becomes right child of parent node. 
                        parent.Right = current.Right;
                    }
                }
            }
            // Case 3: If right child has childs on left side, so last left child
            // from right subtree will put on deleting place. 
            else
            { // Find last left node. 
                BinaryTreeNode<T> leftmost = current.Right.Left;
                BinaryTreeNode<T> leftmostParent = current.Right;
                while (leftmost.Left != null)
                {
                    leftmostParent = leftmost;
                    leftmost = leftmost.Left;
                }
                // Left subtree of parent becomes right subtree of last left node.
                leftmostParent.Left = leftmost.Right;
                // Left or right child of current node becomes Left or right child of the last left node. 
                leftmost.Left = current.Left;
                leftmost.Right = current.Right;
                if (parent == null)
                {
                    root = leftmost;
                }
                else
                {
                    int result = Comparer.Compare(parent.Value, current.Value);
                    if (result > 0)
                    {
                        // If value of parent lower than current,
                        // last Left child of current node becomes Left child of parent node.
                        parent.Left = leftmost;
                    }
                    else if (result < 0)
                    {
                        // If value of parent lower than current,
                        // last Left child of current node becomes Right child of parent node.
                        parent.Right = leftmost;
                    }
                }
            }
            var EventArg = new TreeEventArgs<T>(item, "Item has been removed");
            ElementRemoved?.Invoke(this, EventArg);
            return true;
        }

        /// <summary>
        /// Returns item with the highest value
        /// </summary>
        /// <returns>The element with highest value</returns>
        /// <exception cref="InvalidOperationException">Thrown if tree is empty</exception> 
        public T TreeMax()
        {
            if (root == null)
            {
                throw new InvalidOperationException();
            }
            return TreeMax(root);
        }

        private T TreeMax(BinaryTreeNode<T> node)
        {
            if (node == null)
            {
                return default;
            }

            T res = node.Value;
            T lres = TreeMax(node.Left);
            T rres = TreeMax(node.Right);

            if (!((lres is null) || rres.Equals(default(T))) && Comparer.Compare(lres, res) > 0)
            {
                res = lres;
            }
            if (!((rres is null) || rres.Equals(default(T))) && Comparer.Compare(rres, res) > 0)
            {
                res = rres;
            }
            return res;
        }

        /// <summary>
        /// Returns item with the lowest value
        /// </summary>
        /// <returns>The element with lowest value</returns>
        /// <exception cref="InvalidOperationException">Thrown if tree is empty</exception>
        public T TreeMin()
        {
            if (root == null)
            {
                throw new InvalidOperationException();
            }

            return TreeMin(root);
        }

        private T TreeMin(BinaryTreeNode<T> node)
        {
            if (node == null)
            {
                return default;
            }
            T res = node.Value;
            T rres = TreeMin(node.Right);
            T lres = TreeMin(node.Left);


            if (!((lres is null) || rres.Equals(default(T))) && Comparer.Compare(lres, res) < 0)
            {
                res = lres;
            }
            if (!((rres is null) || rres.Equals(default(T))) && Comparer.Compare(rres, res) < 0)
            {
                res = rres;
            }
            return res;
        }

        /// <summary>
        /// Checks if tree contains element by its reference
        /// </summary>
        /// <param name="item">Object that should (or not) be found in tree</param>
        /// <returns>True if tree contains item, false if it doesn't</returns>
        public bool Contains(T data)
        {
            // ѕоиск узла осуществл€етс€ другим методом.
            return FindWithParent(data, out _) != null;
        }


        /// 
        /// Ќаходит и возвращает первый узел с заданным значением. ≈сли значение
        /// не найдено, возвращает null. “акже возвращает родител€ найденного узла (или null)
        /// дл€ использовани€ в методе Remove.
        /// 
        private BinaryTreeNode<T> FindWithParent(T value, out BinaryTreeNode<T> parent)
        {
            BinaryTreeNode<T> current = root;
            parent = null;

            // While not found...
            while (current != null)
            {
                int result = Comparer.Compare(current.Value, value);

                if (result > 0)
                {
                    // If value is lower, go left.
                    parent = current;
                    current = current.Left;
                }
                else if (result < 0)
                {
                    // If value is higher, go right.
                    parent = current;
                    current = current.Right;
                }
                else
                {
                    // If equals -> break
                    break;
                }
            }

            return current;
        }

        /// <summary>
        /// Makes tree traversal
        /// </summary>
        /// <param name="traverseType"><see cref="TraverseType"></param>
        /// <returns>Sequense of elements of tree according to traverse type</returns>
        public IEnumerable<T> Traverse(TraverseType traverseType)
        {
            List<T> res = new List<T>();
            switch (traverseType)
            {
                case TraverseType.InOrder:                    
                    InOrderTraversal(res);
                    return res.ToArray<T>();
                case TraverseType.PreOrder:
                    PreOrderTraversal(res);
                    return res.ToArray<T>();
                case TraverseType.PostOrder:
                    PostOrderTraversal(res);
                    return res.ToArray<T>();
                default:
                    throw new ArgumentException("traverseType");
            }
        }

        private void InOrderTraversal(List<T> container)
        {
            InOrderTraversal(container, root);
        }

        private void InOrderTraversal(List<T> container, BinaryTreeNode<T> node)
        {
            if (node != null)
            {
                InOrderTraversal(container, node.Left);

                container.Add(node.Value);

                InOrderTraversal(container, node.Right);
            }
        }

        private void PreOrderTraversal(List<T> container)
        {
            PreOrderTraversal(container, root);
        }

        private void PreOrderTraversal(List<T> container, BinaryTreeNode<T> node)
        {
            if (node != null)
            {
                container.Add(node.Value);
                PreOrderTraversal(container, node.Left);
                PreOrderTraversal(container, node.Right);
            }
        }

        private void PostOrderTraversal(List<T> container)
        {
            PostOrderTraversal(container, root);
        }

        private void PostOrderTraversal(List<T> container, BinaryTreeNode<T> node)
        {
            if (node != null)
            {
                PostOrderTraversal(container, node.Left);
                PostOrderTraversal(container, node.Right);
                container.Add(node.Value);
            }
        }

        public IEnumerator<T> InOrderTraversal()
        {
            // This is non-recursive algorithm
            // It uses stack to avoid recursion
            if (root != null)
            {
                // Stack to save missing nodes
                Stack stack = new Stack();

                BinaryTreeNode<T> current = root;

                // When we avoid recursion, we have to  
                // remember, which side we are moving
                bool goLeftNext = true;

                // Put root into stack
                stack.Push(current);

                while (stack.Count > 0)
                {
                    // If we go left
                    if (goLeftNext)
                    {
                        // Put all, except last left node into stack.
                        // Last left node we'll return with yield.
                        while (current.Left != null)
                        {
                            stack.Push(current);
                            current = current.Left;
                        }
                    }

                    // InOrder Traversal : left->yield->right.
                    yield return current.Value;

                    // If we have right nodes, we go.
                    if (current.Right != null)
                    {
                        current = current.Right;

                        // After we went right one time,
                        // we need to go left again.
                        goLeftNext = true;
                    }
                    else
                    {
                        // If we cant go to right path, we need to take parent node
                        // from stack, explore it and go to its right child.
                        current = stack.Pop() as BinaryTreeNode<T>;
                        goLeftNext = false;
                    }
                }
            }
        }

        /// <summary>
        /// Makes in-order traverse
        /// Serves as a default <see cref="TraverseType"/> for tree
        /// </summary>
        /// <returns>Enumerator for iterations in foreach cycle</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return InOrderTraversal();
        }

        /// <summary>
        /// Makes in-order traverse
        /// Serves as a default <see cref="TraverseType"/> for tree
        /// </summary>
        /// <returns>Enumerator for iterations in foreach cycle</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
