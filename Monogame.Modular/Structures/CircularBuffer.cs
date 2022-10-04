namespace MM.Core.Structures;

public class ScrollingBuffer<T>
{
    public int Capacity { get; }
    public int Count { get; private set; }

    private readonly T[] array;
    private int head;

    public ScrollingBuffer(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be at least 1");

        Capacity = capacity;
        array = new T[capacity];
    }

    public void Enqueue(in T value)
    {
        array[head] = value;
        head = (head + 1) % Capacity;
        if (Count < Capacity)
            Count++;
    }

    public bool Dequeue() => TryDequeue(out _);

    public ref T GetRootItem() => ref array[0];

    public int GetOffset() => head;

    public bool TryDequeue(out T dequeued)
    {
        if (Count == 0)
        {
            dequeued = default;
            return false;
        }

        dequeued = array[head];
        array[head] = default;
        Count--;
        head--;
        if (head < 0)
            head = Capacity - 1;
        return true;
    }
}
