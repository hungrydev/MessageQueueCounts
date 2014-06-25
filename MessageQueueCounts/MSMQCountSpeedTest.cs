using System;
using System.Collections.Generic;
using System.Messaging;
using System.Text;
using MessageQueueCounts.MSMQ;

namespace MessageQueueCounts
{
    public class MSMQCountSpeedTest
    {
        public MSMQCountSpeedTest()
        {
            CountUsingPowerShell = true;
        }

        const int _columnWidth = 20;
        private StringBuilder _sb;


        public bool CountUsingPowerShell { get; set; }

        public void Go()
        {
            _sb = new StringBuilder();
            string queueName = GetPrivateQueueName();
            InformUser( string.Format("Queue : {0}", queueName));

            QueueWriter qw = new QueueWriter(queueName);
            qw.Purge();

            string header = string.Empty;
            header += "Cursor".PadRight(_columnWidth);
            header += "GetAllMessages".PadRight(_columnWidth);
            header += "GetEnumerator2".PadRight(_columnWidth);
            if(CountUsingPowerShell) header += "PowerShell".PadRight(_columnWidth);

            InformUser(header);

            for (int i = 5000; i <= 100000; i += 5000)
            {
                Generate(i, queueName);
                CountMessagesOnQueue(queueName);
            }

            // write out the contents of the string builder.
            System.Diagnostics.Debug.Print(_sb.ToString());
        }

        private void InformUser(string message)
        {
            Console.WriteLine(message);
            _sb.AppendLine(message);
        }
        /// <summary>
        /// Count the messages on the queue
        /// </summary>
        /// <param name="queueName"></param>
        private void CountMessagesOnQueue(string queueName)
        {
            for (int i = 0; i < 1; i++)
            {
            string line = string.Empty;

            line += BuildCount(queueName, QueueCount.eCountMode.Cursor).PadRight(_columnWidth);
            line += BuildCount(queueName, QueueCount.eCountMode.GetAllMessages).PadRight(_columnWidth);
            line += BuildCount(queueName, QueueCount.eCountMode.GetEnumerator2).PadRight(_columnWidth);
            if(CountUsingPowerShell) line += BuildCount(queueName, QueueCount.eCountMode.Powershell).PadRight(_columnWidth);

            InformUser(line);
            }
        }
        /// <summary>
        /// Count the messages on the queue and return a formated count and time.
        /// </summary>
        /// <param name="queuename"></param>
        /// <param name="countMode"></param>
        /// <returns></returns>
        private static string BuildCount(string queuename, QueueCount.eCountMode countMode)
        {
            try
            {

                QueueCount qc = new QueueCount(queuename) {CountMode = countMode};
                DateTime dtStart = DateTime.Now;
                int count = qc.Count();
                DateTime dtEnd = DateTime.Now;
                qc = null;

                TimeSpan ts = dtEnd.Subtract(dtStart);
                string sReturn = string.Format("{0} : {1}ms",
                                                 count.ToString("#,##0"),
                                                 ts.TotalMilliseconds.ToString("#,##0")); 
                //string sReturn = string.Format("{0} : {1}ms",
                //  count.ToString("#,##0").PadLeft(8),
                //  ts.TotalMilliseconds.ToString("#,##0").PadLeft(6));
                return sReturn;

            }
            catch (Exception)
            {
                return "Error";

            }
        }

        /// <summary>
        /// Get the first private queuename on this machine
        /// </summary>
        /// <returns></returns>
        private static string GetPrivateQueueName()
        {
            // Get the first private queue name
            QueueBase qb = new QueueBase();
            MessageQueue[] queues = qb.GetPrivateQueues();
            if (queues.Length < 1)
                throw new ApplicationException("No Private Queues on this machine.");
            //take the first private queue to play with
            MessageQueue q = queues[0];
            string queueName = ".\\" + q.QueueName;
            qb.Dispose();

            return queueName;
        }
        /// <summary>
        /// Generate test messages
        /// </summary>
        /// <param name="messagesToGenerate">number of messages to generate</param>
        /// <param name="queueName">Which queue to write to</param>
        private static void Generate(int messagesToGenerate, string queueName)
        {


            string body = "It was the best of times, it was the worst of times ".PadRight(5000, 'q');
            const string label = "MSMQ Count Speed Test Message";

            //build the messages
            Message msg = new Message { Body = body, Label = label };
            List<Message> messages = new List<Message>();
            for (int i = 0; i < messagesToGenerate; i++)
            {
                messages.Add(msg);
            }

            // Write to the queue as a batch;
            QueueWriter qw = new QueueWriter(queueName);
            qw.Purge();
            qw.SendMessage(messages);

            qw.Dispose();

        }
    }
}
