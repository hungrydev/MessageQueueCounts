using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Management;
namespace MessageQueueCounts.MSMQ
{
    /// <summary>
    /// Count the queue
    /// </summary>
    internal class QueueCount
    {
        private readonly MessageQueue _messageQueue;
        private readonly string _queueName;

        internal QueueCount()
        {
            CountMode = eCountMode.GetAllMessages;
        }

        internal QueueCount(MessageQueue messagequeue)
        {
            _queueName = messagequeue.Path;
            _messageQueue = new MessageQueue(_queueName, QueueAccessMode.Peek);
            CountMode = eCountMode.GetAllMessages;

        }

        public QueueCount(string queuenameAsReader)
        {
            
            _queueName = queuenameAsReader;
            _messageQueue = new MessageQueue(_queueName, QueueAccessMode.Peek);
            CountMode = eCountMode.GetAllMessages;
        }

        internal eCountMode CountMode { get; set; }
        internal enum eCountMode
        {
            /// <summary>
            /// Method proviced by John Opincar uses a cursor
            /// </summary>
            Cursor,
            GetEnumerator2,
            GetAllMessages,
            Powershell
        }
        /// <summary>
        /// Count the message queue
        /// </summary>
        /// <returns></returns>
        internal int Count()
        {
            switch (CountMode)
            {
                case eCountMode.Cursor:
                    return CountCursor();
                case eCountMode.GetEnumerator2:
                    return CountGetEnumerator2();
                case eCountMode.GetAllMessages:
                    return CountGetAllMessages();
                case eCountMode.Powershell:
                    return GetPowerShellCount();
                default:
                    throw new ApplicationException("Count mode has not been specified.");
            }
        }
        #region CountGetAllMessages
        /// <summary>
        /// Count all messages ias a filter.
        /// </summary>
        /// <returns></returns>
        private int CountGetAllMessages()
        {
            var countFilter = new MessagePropertyFilter
                                  {
                                      AdministrationQueue = false,
                                      ArrivedTime = false,
                                      CorrelationId = false,
                                      Priority = false,
                                      ResponseQueue = false,
                                      SentTime = false,
                                      Body = false,
                                      Label = false,
                                      Id = false
                                  };
            _messageQueue.MessageReadPropertyFilter = countFilter;
            var r = _messageQueue.GetAllMessages().Length;
            return r;
        }
        #endregion

        #region CountGetEnumerator2
        /// <summary>
        /// Count the messages using the Message queue method GetMessageEnumerator2
        /// </summary>
        /// <returns></returns>
        private int CountGetEnumerator2()
        {
            var x = _messageQueue.GetMessageEnumerator2();
            int iCount = 0;
            while (x.MoveNext())
            {
                iCount++;
            }
            return iCount;
        }
        #endregion
        #region Cursor
        /// <summary>
        /// http://jopinblog.wordpress.com/2008/03/12/counting-messages-in-an-msmq-messagequeue-from-c/
        /// </summary>
        /// <returns></returns>
        private int CountCursor()
        {
            int count = 0;
            Cursor cursor = _messageQueue.CreateCursor();
            Message m = CursorPeekWithoutTimeout( cursor, PeekAction.Current);
            if (m != null)
            {
                count = 1;
                while ((m = CursorPeekWithoutTimeout(cursor, PeekAction.Next)) != null)
                {
                    count++;
                }
                if (m != null) m.Dispose();
            }
            cursor.Dispose();
            return count;
        }


        /// <summary>
        /// http://jopinblog.wordpress.com/2008/03/12/counting-messages-in-an-msmq-messagequeue-from-c/
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected Message CursorPeekWithoutTimeout( Cursor cursor, PeekAction action)
        {
            Message ret = null;
            try
            {
                ret = _messageQueue.Peek(new TimeSpan(1), cursor, action);
            }
            catch (MessageQueueException mqe)
            {
                if (mqe.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                {
                    throw;
                }
            }
            return ret;
        }


        #endregion

        private int GetPowerShellCount()
        {
            return GetPowerShellCount(_queueName, Environment.MachineName, "", "");
        }

        /// <summary>
        /// uses powershell can also be used on remote machines if you have access.
        /// http://www.meadow.se/wordpress/?p=648
        /// </summary>
        /// <param name="queuePath"></param>
        /// <param name="machine"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private  int GetPowerShellCount(string queuePath, string machine,string username, string password)
        {
            var path = string.Format(@"\\{0}\root\CIMv2", machine);
            ManagementScope scope;
            if (string.IsNullOrEmpty(username))
            {
                 scope = new ManagementScope(path);
            }
            else
            {
                var options = new ConnectionOptions {Username = username, Password = password};
                scope = new ManagementScope(path, options);
            }
            scope.Connect();
            if (queuePath.StartsWith(".\\")) queuePath=queuePath.Replace(".\\",string.Format("{0}\\",machine));

           // string queryString = String.Format("SELECT * FROM Win32_PerfFormattedData_msmq_MSMQQueue WHERE Name = '{0}'", queuePath);
            string queryString = String.Format("SELECT * FROM Win32_PerfFormattedData_msmq_MSMQQueue");
            var query = new ObjectQuery(queryString);

            var searcher = new ManagementObjectSearcher(scope, query);

            IEnumerable<int> messageCountEnumerable =
              from ManagementObject queue in searcher.Get()
              select (int)(UInt64)queue.GetPropertyValue("MessagesInQueue");
            //IEnumerable<string> messageCountEnumerable =
            //  from ManagementObject queue in searcher.Get()
            //  select (string)queue.GetPropertyValue("Name");

            var x = messageCountEnumerable.First();
            return x;
        }

    }
}

