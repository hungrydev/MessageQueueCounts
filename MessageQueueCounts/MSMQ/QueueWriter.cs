
using System;
using System.Collections.Generic;
using System.Messaging;
using System.Security.Principal;

namespace MessageQueueCounts.MSMQ
{
    /// <summary>
    /// use this to write to the message queue
    /// </summary>
    public class QueueWriter:QueueBase
    {
        /// <summary>
        /// Message queue
        /// </summary>
        /// <param name="queuename"></param>
        public QueueWriter(string queuename)
        {
            QueueName = queuename;
        }

        #region Purge
        /// <summary>
        /// Purges the Message queue
        /// </summary>
        public void Purge()
        {
            ////
            if(IsCurrentUserAdmin()) throw new AccessViolationException("You must be an administrator to purge the message queue.");
            ////
            InitaliseMessageQueue(QueueAccessMode.ReceiveAndAdmin);
            _MessageQueue.Purge();
            // dispose of the message queue as we dont want to
            // leave it connected as admin.
            _MessageQueue.Dispose();
        }
        #endregion

      
        


        /// <summary>
        /// Send a list of messages to the queue
        /// </summary>
        /// <param name="messages"></param>
        public void SendMessage(List<Message> messages)
        {
            //exit if no messages to transmitt.
            if (messages == null || messages.Count < 1) return;

            InitaliseMessageQueue(QueueAccessMode.Send);

            foreach (Message message in messages)
            {
                _MessageQueue.Send(message);
            }
        }

        /// <summary>
        /// Is the current user an administrator
        /// </summary>
        /// <returns></returns>
        public static bool IsCurrentUserAdmin()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
