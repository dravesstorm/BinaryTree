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

            // Находим удаляемый узел.
            current = FindWithParent(item, out BinaryTreeNode<T> parent);

            if (current == null)
            {
                return false;
            }

            Count--;

            // Случай 1: Если нет детей справа, левый ребенок встает на место удаляемого.
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
                        // Если значение родителя больше текущего,
                        // левый ребенок текущего узла становится левым ребенком родителя.
                        parent.Left = current.Left;
                    }
                    else if (result < 0)
                    { // Если значение родителя меньше текущего, 
                      //левый ребенок текущего узла становится правым ребенком родителя. 
                        parent.Right = current.Left;
                    }
                }
            }
            // Случай 2: Если у правого ребенка нет детей слева, то он занимает место удаляемого узла. 
            else if (current.Right.Left == null)
            {
                current.Right.Left = current.Left; if (parent == null) { root = current.Right; }
                else
                {
                    int result = Comparer.Compare(parent.Value, current.Value);
                    if (result > 0)
                    {
                        // Если значение родителя больше текущего,
                        // правый ребенок текущего узла становится левым ребенком родителя.
                        parent.Left = current.Right;
                    }
                    else if (result < 0)
                    { // Если значение родителя меньше текущего, 
                        // правый ребенок текущего узла становится правым ребенком родителя. 
                        parent.Right = current.Right;
                    }
                }
            }
            // Случай 3: Если у правого ребенка есть дети слева, крайний левый ребенок 
            // из правого поддерева заменяет удаляемый узел. 
            else
            { // Найдем крайний левый узел. 
                BinaryTreeNode<T> leftmost = current.Right.Left;
                BinaryTreeNode<T> leftmostParent = current.Right;
                while (leftmost.Left != null)
                {
                    leftmostParent = leftmost;
                    leftmost = leftmost.Left;
                }
                // Левое поддерево родителя становится правым поддеревом крайнего левого узла.
                leftmostParent.Left = leftmost.Right;
                // Левый и правый ребенок текущего узла становится левым и правым ребенком крайнего левого. 
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
                        // Если значение родителя больше текущего,
                        // крайний левый узел становится левым ребенком родителя.
                        parent.Left = leftmost;
                    }
                    else if (result < 0)
                    {
                        // Если значение родителя меньше текущего,
                        // крайний левый узел становится правым ребенком родителя.
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
            // Поиск узла осуществляется другим методом.
            return FindWithParent(data, out _) != null;
        }


        /// 
        /// Находит и возвращает первый узел с заданным значением. Если значение
        /// не найдено, возвращает null. Также возвращает родителя найденного узла (или null)
        /// для использования в методе Remove.
        /// 
        private BinaryTreeNode<T> FindWithParent(T value, out BinaryTreeNode<T> parent)
        {
            // Попробуем найти значение в дереве.
            BinaryTreeNode<T> current = root;
            parent = null;

            // До тех пор, пока не нашли...
            while (current != null)
            {
                int result = Comparer.Compare(current.Value, value);

                if (result > 0)
                {
                    // Если искомое значение меньше, идем налево.
                    parent = current;
                    current = current.Left;
                }
                else if (result < 0)
                {
                    // Если искомое значение больше, идем направо.
                    parent = current;
                    current = current.Right;
                }
                else
                {
                    // Если равны, то останавливаемся
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
            switch (traverseType)
            {
                case TraverseType.InOrder:
                    InOrderTraversal((root) => { });
                    return this;
                case TraverseType.PreOrder:
                    PreOrderTraversal((root) => { });
                    return this;
                case TraverseType.PostOrder:
                    PostOrderTraversal((root) => { });
                    return this;
                default:
                    throw new ArgumentException("traverseType");
            }
        }

        private void InOrderTraversal(Action<T> action)
        {
            InOrderTraversal(action, root);
        }

        private void InOrderTraversal(Action<T> action, BinaryTreeNode<T> node)
        {
            if (node != null)
            {
                InOrderTraversal(action, node.Left);

                action(node.Value);

                InOrderTraversal(action, node.Right);
            }
        }

        private void PreOrderTraversal(Action<T> action)
        {
            PreOrderTraversal(action, root);
        }

        private void PreOrderTraversal(Action<T> action, BinaryTreeNode<T> node)
        {
            if (node != null)
            {
                action(node.Value);
                PreOrderTraversal(action, node.Left);
                PreOrderTraversal(action, node.Right);
            }
        }

        private void PostOrderTraversal(Action<T> action)
        {
            PostOrderTraversal(action, root);
        }

        private void PostOrderTraversal(Action<T> action, BinaryTreeNode<T> node)
        {
            if (node != null)
            {
                PostOrderTraversal(action, node.Left);
                PostOrderTraversal(action, node.Right);
                action(node.Value);
            }
        }

        public IEnumerator<T> InOrderTraversal()
        {
            // Это нерекурсивный алгоритм.
            // Он использует стек для того, чтобы избежать рекурсии.
            if (root != null)
            {
                // Стек для сохранения пропущенных узлов.
                Stack stack = new Stack();

                BinaryTreeNode<T> current = root;

                // Когда мы избавляемся от рекурсии, нам необходимо
                // запоминать, в какую стороны мы должны двигаться.
                bool goLeftNext = true;

                // Кладем в стек корень.
                stack.Push(current);

                while (stack.Count > 0)
                {
                    // Если мы идем налево...
                    if (goLeftNext)
                    {
                        // Кладем все, кроме самого левого узла на стек.
                        // Крайний левый узел мы вернем с помощю yield.
                        while (current.Left != null)
                        {
                            stack.Push(current);
                            current = current.Left;
                        }
                    }

                    // Префиксный порядок: left->yield->right.
                    yield return current.Value;

                    // Если мы можем пойти направо, идем.
                    if (current.Right != null)
                    {
                        current = current.Right;

                        // После того, как мы пошли направо один раз,
                        // мы должным снова пойти налево.
                        goLeftNext = true;
                    }
                    else
                    {
                        // Если мы не можем пойти направо, мы должны достать родительский узел
                        // со стека, обработать его и идти в его правого ребенка.
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
