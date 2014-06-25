using System;
using System.Messaging;

namespace MessageQueueCounts.MSMQ
{
    public class QueueBase:IDisposable
    {
        public QueueBase()
        {
            IsMessageFormated = false;
        }
        /// <summary>
        /// Connect to the specified message queue
        /// </summary>
        /// <param name="queuename">Queue name to connect to.</param>
        public QueueBase(string queuename)
        {
            QueueName = queuename;
            IsMessageFormated = false;
        }

        /// <summary>
        /// Get or Set the message queuename.
        /// </summary>
        public string QueueName { get; set; }
      

        /// <summary>
        /// Does the message use a message formatter
        /// </summary>
        public bool IsMessageFormated { get; set; }

        /// <summary>
        /// Expose the Message queue object
        /// </summary>
        internal MessageQueue _MessageQueue{ get; set; }

        /// <summary>
        /// Initalise the message queue.
        /// Setting up all options as required.
        /// </summary>
        internal void InitaliseMessageQueue(QueueAccessMode mode)
        {
            //Connect to the queue if needed
            if (_MessageQueue == null || _MessageQueue.AccessMode != mode)
            {
                if(string.IsNullOrEmpty(QueueName)) throw new Exception("Queuename has not been set.");
                _MessageQueue = new MessageQueue(QueueName, mode);
             
            }
        }

        /// <summary>
        /// Return a collection of messagequeues for the current machine.
        /// </summary>
        public MessageQueue[] GetPrivateQueues()
        {
            return GetPrivateQueues(Environment.MachineName);
        }
        /// <summary>
        /// Return a collection of private message queues on a remote machine
        /// </summary>
        /// <param name="computername"></param>
        /// <returns></returns>
        public MessageQueue[] GetPrivateQueues(string computername)
        {
            MessageQueue[] queueList = MessageQueue.GetPrivateQueuesByMachine(computername);

            return queueList;
        }

        /// <summary>
        /// Count the number of messages in the message queue
        /// </summary>
        /// <returns></returns>
        public int? Count()
        {
            if (_MessageQueue == null) return null;

            int? iReturn = 0;
            var qc = new QueueCount(_MessageQueue);
            iReturn = qc.Count();
            return iReturn;
        }
        /// <summary>
        /// Dispose of objects in correct order.
        /// </summary>
        public void Dispose()
        {
            if (_MessageQueue == null) return;
            _MessageQueue.Dispose();
            _MessageQueue = null;
        }

    }
}
