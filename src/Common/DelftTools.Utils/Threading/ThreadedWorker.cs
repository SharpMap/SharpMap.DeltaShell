using System;
using System.Threading;

namespace DelftTools.Utils.Threading
{
    /// <summary>
    /// Todo: move to framework (remove this class completely, probably should happen in NetCdfFunctionStore)
    /// For now this class sits in WFM, but if it works well we can move this to framework.
    /// DelfTools.Utils.Threading seems ideal, but unfortunately it has a SWF reference 
    /// which is hard to get rid of...
    /// </summary>
    public class ThreadedWorker
    {
        private readonly ManualResetEvent allDone = new ManualResetEvent(false);
        public static Action WaitMethod { get; set; }
        private int itemsTodo;

        public ThreadedWorker()
        {
            Reset();
        }

        private void Reset()
        {
            allDone.Reset();
            itemsTodo = 1; //1 

            //fake work item to make sure:
            //--we don't get at 0 before we're done adding items
            //--we don't deadlock if no items were added before wait
            //---(typical reason why no items are added is if the enumeration over which it is called is empty)
        }

        public void ProcessWorkItemAsync(Action workItem)
        {
            Interlocked.Increment(ref itemsTodo);
            ThreadPool.QueueUserWorkItem(
                s =>
                    {
                        try
                        {
                            workItem();
                        }
                        finally
                        {
                            ReportWorkItemDone();
                        }
                    }
                );
        }
        
        public void WaitTillAllWorkItemsDone()
        {
            ReportWorkItemDone(); //report fake work item done (to prevent we get at 0 before we're done)

            WaitTillAllWorkItemsDoneInternal();

            Reset();
        }

        private void ReportWorkItemDone()
        {
            if (Interlocked.Decrement(ref itemsTodo) == 0)
            {
                allDone.Set();
            }
        }

        private void WaitTillAllWorkItemsDoneInternal()
        {
            //if the current thread runs the (windows) message loop, we have to call DoEvents from time to time to prevent deadlocks
            if (WaitMethod != null)
            {
                while (true)
                {
                    if (allDone.WaitOne(1)) //wait with a timeout
                    {
                        break; //allDone was signaled
                    }
                    WaitMethod(); //process any 'invoke' calls (otherwise we could deadlock)
                }
            }
            else
            {
                allDone.WaitOne();
            }
        }
    }
}