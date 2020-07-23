namespace BinaryTree
{
    public class BinaryTreeNode<T> 
    {
        public BinaryTreeNode(T value)
        {
            Value = value;
        }

        public BinaryTreeNode<T> Left { get; set; }
        public BinaryTreeNode<T> Right { get; set; }
        public T Value { get; private set; }

    }
}
