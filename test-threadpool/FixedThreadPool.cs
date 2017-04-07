using System;
using System.Threading;
using System.Collections.Generic;

namespace FixedThreadPool
{
    public class ThreadPool 
    {
        readonly object _locker;
        readonly Thread[] _workers;
        bool _stopped;
        readonly Queue<ITask> _tasksHP;
        readonly Queue<ITask> _tasksNP;
        readonly Queue<ITask> _tasksLP;
        uint _taskCount;
        const int HighPriorityRespite = 3;
        uint _HPRespiteCount;
        
        public ThreadPool(uint threadcount) 
        {
            _locker = this;
            _workers = new Thread[threadcount];
            for (int i = 0; i < threadcount; i++)
            {
                (_workers [i] = new Thread (Consume)).Start();
            }
            _tasksHP = new Queue<ITask>();
            _tasksNP = new Queue<ITask>();
            _tasksLP = new Queue<ITask>();
            _taskCount = 0;
            _HPRespiteCount = HighPriorityRespite;
        }
        
        public bool Execute(ITask task, Priority priority)
        {
            if (task == null)
            {
                return false;
            }
            lock (_locker)
            {
                if (_stopped)
                {
                    return false;
                }
                switch (priority) 
                {
                    case Priority.High:
                        _tasksHP.Enqueue(task);
                        break;
                    case Priority.Normal:
                        _tasksNP.Enqueue(task);
                        break;
                    case Priority.Low:
                        _tasksLP.Enqueue(task);
                        break;
                }
                _taskCount++;
                Monitor.Pulse (_locker);
                return true;
            }
        }

        public void Stop()
        {
            lock (_locker)
            {
                _stopped = true;
                foreach (var worker in _workers)
                {
                    _tasksLP.Enqueue(null);
                    _taskCount++;
                }
            }
            foreach (var worker in _workers)
            {
                worker.Join();
            }
        }
        

        void Consume()
        {
            while (true)
            {
                ITask item;
                lock (_locker)
                {
                    while (_taskCount == 0) 
                    {
                        Monitor.Wait (_locker);
                    }
                    _taskCount--;
                    if (_tasksHP.Count > 0 && _HPRespiteCount > 0) {
                        item = _tasksHP.Dequeue();
                        _HPRespiteCount--;
                    } else if (_tasksNP.Count > 0) {
                        item = _tasksNP.Dequeue();
                        _HPRespiteCount = HighPriorityRespite;
                    } else {
                        item = _tasksLP.Dequeue();
                        if (item == null)
                        {
                            return; // This signals our exit.
                        }
                    }
                }
                item.Execute();                          
            }
        }
    }
    
    public enum Priority 
    {
        Low, Normal, High
    }
    
    public interface ITask 
    {
        void Execute();
    }
}