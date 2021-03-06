﻿using System;
using System.ComponentModel;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using PostSharp.Aspects;
using log4net;

namespace DelftTools.Utils.Aop
{
    ///<summary>
    /// Implements thread-safe calls for Windows.Forms methods.
    ///</summary>
    [Serializable]
    [Synchronization]
    public class InvokeRequiredAttribute : MethodInterceptionAspect
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InvokeRequiredAttribute));

        private static int pendingInvokes; //number of invokes pending (global!)
        private static readonly object InvokeLock = new object(); //object to lock on
        private static readonly ManualResetEvent ZeroPendingInvokes = new ManualResetEvent(true); //true/signalled if there are no invokes pending
        
        public override sealed void OnInvoke(MethodInterceptionArgs args)
        {
            var synchronizeObject = InvokeRequiredInfo.SynchronizeObject ?? args.Instance as ISynchronizeInvoke;

            if (synchronizeObject == null || !synchronizeObject.InvokeRequired)
            {
                args.Proceed();
            }
            else
            {
                lock (InvokeLock)
                {
                    try
                    {
                        if (Interlocked.Increment(ref pendingInvokes) > 0)
                        {
                            ZeroPendingInvokes.Reset();
                        }
                        synchronizeObject.Invoke(new Action(args.Proceed), new object[0]);
                    }
                    catch (ObjectDisposedException e)
                    {
                        LogInvokeError(args.Method.Name, e, false);
                    }
                    finally
                    {
                        if (Interlocked.Decrement(ref pendingInvokes) == 0)
                        {
                            ZeroPendingInvokes.Set();
                        }
                    }
                }
            }
        }

        private static void LogInvokeError(string methodName, Exception e, bool beforeCall)
        {
            log.Error(string.Format("Thread synchronization error (call {1}): {0}", methodName,
                                    beforeCall ? "skipping" : "aborted"), e);
        }

        /// <summary>
        /// Call this method with a using statement around the content of the Dispose method you want to protected.
        /// </summary>
        /// <returns></returns>
        public static IDisposable BlockInvokeCallsDuringDispose()
        {
            return new InvokeBlocker();
        }

        private class InvokeBlocker : IDisposable
        {
            private bool hasLock;

            public InvokeBlocker()
            {
                int maxWaits = 15;
                do
                {
                    //while there are invokes pending
                    while (!ZeroPendingInvokes.WaitOne(0))
                    {
                        //block (call Application.DoEvents), so that other threads can finish their invokes
                        InvokeRequiredInfo.WaitMethod();

                        if (maxWaits-- <= 0) // prevent deadlocks
                            return; //hasLock = false
                    }
                } 
                while (!Monitor.TryEnter(InvokeLock)); //try to grab the invoke lock (to prevent additional invokes from occuring)
                
                hasLock = true;
            }

            public void Dispose()
            {
                //give up the lock; any invokes waiting for this lock are processed after this; but if the object is 
                //now correctly disposed, they are skipped
                if (hasLock)
                    Monitor.Exit(InvokeLock);
            }
        }
    }

    //In seperate class to make sure you don't need a postsharp reference to fill in
    public static class InvokeRequiredInfo 
    {
        //to be fill in by application:
        public static ISynchronizeInvoke SynchronizeObject { get; set; }
        public static Action WaitMethod;
        // end fill in
    }
}
