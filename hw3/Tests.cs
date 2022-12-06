namespace laba
{
    class Tests
    {
        public class ValueThread
        {
            public int Value;
            public int ThreadNumber; 

            public ValueThread(int value, int threadNumber)
            {
                Value = value;
                ThreadNumber = threadNumber;
            }
        }

        public static void PushValuesToStack(IEnumerable<int> values, LockFreeStack<int> stack)
        {
            foreach (var value in values)
            {
                stack.Push(value);
            }
        }

        public static void PopValuesFromStack(List<ValueThread> values, LockFreeStack<int> stack)
        {
            int value;
            while (stack.TryPop(out value))
            {
                lock (values)
                {
                    values.Add(new ValueThread(value, Thread.CurrentThread.ManagedThreadId));
                }
            }
        }

        public static List<int> GetOddNumbers(int maxNumber)
        {
            var numbers = new List<int>();
            var current = 1;
            while (current < maxNumber)
            {
                numbers.Add(current);
                current += 2;
            }

            return numbers;
        }

        public static List<int> GetEvenNumbers(int maxNumber)
        {
            var numbers = new List<int>();
            var current = 0;
            while (current < maxNumber)
            {
                numbers.Add(current);
                current += 2;
            }

            return numbers;
        }

        public static List<int> GetSequentialNumbers(int maxNumber)
        {
            var numbers = new List<int>();
            var current = 0;
            while (current < maxNumber)
            {
                numbers.Add(current);
                current += 1;
            }

            return numbers;
        }

        public static bool IsDecreasinSequence(IEnumerable<int> values)
        {
            var current = values.First();
            foreach (var value in values.Skip(1))
            {
                if (value >= current)
                {
                    return false;
                }
                current = value;
            }

            return true;
        }

        public static void StartTasksAndWait(IEnumerable<Task> tasks)
        {
            foreach (var task in tasks)
            {
                task.Start();
            }
            Task.WaitAll(tasks.ToArray());
        }

        public static bool RunPushTest()
        {
            var stack = new LockFreeStack<int>();
            var oddNumbers = GetOddNumbers(99);
            var evenNumbers = GetEvenNumbers(100);

            // pushing values to stack concurrenly
            var pushTasks = new List<Task>();
            pushTasks.Add(new Task(() => PushValuesToStack(oddNumbers, stack)));
            pushTasks.Add(new Task(() => PushValuesToStack(evenNumbers, stack)));
            StartTasksAndWait(pushTasks);

            var stackValues = new List<int>();
            int value;
            while (stack.TryPop(out value))
            {
                stackValues.Add(value);
            }

            var stackEvenValues = stackValues.Where(x => x % 2 == 0).Reverse().ToList<int>();
            var stackOddValues = stackValues.Where(x => x % 2 == 1).Reverse().ToList<int>();

            // order of the elements should be the same
            return stackEvenValues.SequenceEqual(evenNumbers) && stackOddValues.SequenceEqual(oddNumbers);
        }

        public static bool RunPopTests()
        {
            var stack = new LockFreeStack<int>();
            var evenNumbers = GetSequentialNumbers(100);
            PushValuesToStack(evenNumbers, stack);

            // popping values from stack concurrently 
            var poppedValues = new List<ValueThread>();
            var tasksNumber = 10;
            var poppingTasks = new List<Task>();
            for (var i = 0; i < tasksNumber; i++)
            {
                poppingTasks.Add(new Task(() => PopValuesFromStack(poppedValues, stack)));
            }
            StartTasksAndWait(poppingTasks);

            // values popped from each thread should be in decreasing order (LIFO order)
            return poppedValues
                .GroupBy(x => x.ThreadNumber)
                .Select(x => IsDecreasinSequence(x.Select(y => y.Value)))
                .All(x => x);
        }
    }
}