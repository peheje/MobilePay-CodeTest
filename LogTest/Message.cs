namespace LogTest
{
    class Message
    {
        public MessageType MessageType { get; }
        public string Data { get; }
        public Message(MessageType messageType, string data)
        {
            MessageType = messageType;
            Data = data;
        }
    }
}
