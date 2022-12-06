namespace laba
{
    class StackNode<T>
    {
        public T? value;
        public StackNode<T>? next;

        public StackNode()
        {
            this.value = default(T);
            this.next = null;
        }

        public StackNode(T? value, StackNode<T>? next)
        {
            this.value = value;
            this.next = next;
        }
    }
}