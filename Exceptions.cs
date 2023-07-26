namespace butters{
    [System.Serializable]
    public class ButtersException : System.Exception
    {
        public ButtersException() { }
        public ButtersException(string message) : base(message) { }
        public ButtersException(string message, System.Exception inner) : base(message, inner) { }
        protected ButtersException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class InvalidTokenException : ButtersException
    {
        public InvalidTokenException() {}
        public InvalidTokenException(string message) : base(message) {}
        public InvalidTokenException(string message, ButtersException inner) : base(message, inner) {}
        protected InvalidTokenException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) {}
    }

    [System.Serializable]
    public class CompileException : ButtersException
    {
        public CompileException() { }
        public CompileException(string message) : base(message) { }
        public CompileException(string message, System.Exception inner) : base(message, inner) { }
        protected CompileException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class RuntimeException : ButtersException
    {
        public RuntimeException() { }
        public RuntimeException(string message) : base(message) { }
        public RuntimeException(string message, System.Exception inner) : base(message, inner) { }
        protected RuntimeException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class InvalidVariableException : ButtersException
    {
        public InvalidVariableException() { }
        public InvalidVariableException(string message) : base(message) { }
        public InvalidVariableException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidVariableException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}