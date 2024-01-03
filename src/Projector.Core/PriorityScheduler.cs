using System.Collections.Concurrent;

namespace ProjectoR.Core;

internal sealed class PriorityScheduler(ThreadPriority priority) : TaskScheduler
{
    public static PriorityScheduler Highest = new(ThreadPriority.Highest);
    public static PriorityScheduler AboveNormal = new(ThreadPriority.AboveNormal);
    public static PriorityScheduler Normal = new(ThreadPriority.Normal);
    public static PriorityScheduler BelowNormal = new(ThreadPriority.BelowNormal);
    public static PriorityScheduler Lowest = new(ThreadPriority.Lowest);

    public override int MaximumConcurrencyLevel { get; } = Math.Max(1, Environment.ProcessorCount);
    
    private readonly ConcurrentQueue<Task> _tasks = new();
    private Thread[]? _threads;
    
    protected override IEnumerable<Task> GetScheduledTasks() => _tasks;

    protected override void QueueTask(Task task)
    {
        _tasks.Enqueue(task);

        if (_threads != null) 
            return;
        
        _threads = new Thread[MaximumConcurrencyLevel];
        for (var index = 0; index < _threads.Length; index++)
        {
            _threads[index] = new Thread(() =>
            {
                while (!_tasks.IsEmpty)
                {
                    if (_tasks.TryDequeue(out var taskToProcess))
                        TryExecuteTask(taskToProcess);
                }
            })
            {
                Name = $"PriorityScheduler: {index}",
                Priority = priority,
                IsBackground = true,
            };
            _threads[index].Start();
        }
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
}