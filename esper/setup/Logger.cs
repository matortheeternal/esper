namespace esper.setup {
    [JSExport]
    public enum LogMessageType : byte {
        Debug,
        Info,
        Warning,
        Error
    }

    [JSExport]
    public class LogMessage {
        public LogMessageType messageType;
        public string text;

        public LogMessage(LogMessageType messageType, string text) {
            this.messageType = messageType;
            this.text = text;
        }

        public override string ToString() {
            return text;
        }
    }

    [JSExport]
    public class Logger {
        public readonly List<LogMessage> messages;
        public readonly DateTime created;
        public readonly string name;
        public readonly List<Action<LogMessage>> callbacks;

        public Logger(string name) {
            this.name = name;
            created = DateTime.Now;
            messages = new List<LogMessage>();
            callbacks = new List<Action<LogMessage>>();
            Info($"Logger initialized {created}");
        }

        private void Message(LogMessageType messageType, string text) {
            var message = new LogMessage(messageType, text);
            messages.Add(message);
            callbacks.ForEach(cb => cb(message));
        }

        public void Debug(string text) {
            Message(LogMessageType.Debug, text);
        }

        public void Info(string text) {
            Message(LogMessageType.Info, text);
        }

        public void Warn(string text) {
            Message(LogMessageType.Warning, text);
        }

        public void Error(string text) {
            Message(LogMessageType.Error, text);
        }

        public void Save() {
            // TODO
        }
    }
}
