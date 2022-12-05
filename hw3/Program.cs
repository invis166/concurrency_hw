using System;


namespace laba
{
    public interface IStack<T>
    {
        void Push(T item);
        bool TryPop(out T item);
        int Count { get; }
    }
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

    public class LockFreeStack<T> : IStack<T>
    {
        private StackNode<T> head;
        private int itemsCount;
        public int Count { get => itemsCount;}

        public LockFreeStack()
        {
            head = new StackNode<T>();
        }

        public void Push(T item)
        {
            var newNode = new StackNode<T>(item, head);
            do 
            {
                newNode.next = head.next;
            } 
            while (!CompareAndExchange(ref head.next, newNode, newNode.next));
            Interlocked.Increment(ref itemsCount);
        }

        public bool TryPop(out T item)
        {
            StackNode<T> top;
            do
            {
                top = head.next;

                if (top == null) {
                    item = default(T);
                    return false;
                }
            }
            while (!CompareAndExchange(ref head.next, top.next, top));

            item = top.value;
            Interlocked.Decrement(ref itemsCount);

            return true;
        }

        static bool CompareAndExchange(ref StackNode<T> location, StackNode<T> value, StackNode<T> comparand)
        {
            return comparand == Interlocked.CompareExchange<StackNode<T>>(ref location, value, comparand);
        }
    }

    class Program
    {
        public static void Main()
        {
            var stack = new LockFreeStack<int>();
            stack.Push(3);
            stack.Push(4);
            stack.Push(5);
            stack.Push(6);
            stack.Push(7);
            int top;
            while (stack.TryPop(out top))
            {
                Console.WriteLine(top);
            }
        }
    }
}