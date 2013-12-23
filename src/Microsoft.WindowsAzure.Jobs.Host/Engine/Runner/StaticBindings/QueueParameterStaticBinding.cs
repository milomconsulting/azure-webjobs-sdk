﻿using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.Jobs
{
    internal class QueueParameterStaticBinding : ParameterStaticBinding
    {
        // Is this enqueue or dequeue?
        public bool IsInput { get; set; }

        [JsonIgnore]
        private string _queueName;

        public string QueueName
        {
            get
            {
                return _queueName;
            }
            set
            {
                string name = value.ToLower(); // must be lowercase. coerce here to be nice.
                QueueClient.ValidateQueueName(name);
                this._queueName = name;
            }
        }

        public override ParameterRuntimeBinding Bind(IRuntimeBindingInputs inputs)
        {
            string invokeString = null;
            if (this.IsInput)
            {
                var inputQueueMsg = (ITriggerNewQueueMessage)inputs;
                invokeString = inputQueueMsg.QueueMessageInput.AsString;
            }
            return this.BindFromInvokeString(inputs, invokeString);
        }

        public override ParameterRuntimeBinding BindFromInvokeString(IRuntimeBindingInputs inputs, string invokeString)
        {
            if (this.IsInput)
            {
                return new QueueInputParameterRuntimeBinding { Content = invokeString };
            }
            else
            {
                // invokeString is ignored. 
                // Will set on out parameter.
                return new QueueOutputParameterRuntimeBinding
                {
                    QueueOutput = new CloudQueueDescriptor
                    {
                        AccountConnectionString = inputs.AccountConnectionString,
                        QueueName = this.QueueName
                    }
                };
            }
        }

        public override string Description
        {
            get
            {
                if (this.IsInput)
                {
                    return string.Format("dequeue from '{0}'", this.QueueName);
                }
                else
                {
                    return string.Format("enqueue to '{0}'", this.QueueName);
                }
            }
        }
    }
}