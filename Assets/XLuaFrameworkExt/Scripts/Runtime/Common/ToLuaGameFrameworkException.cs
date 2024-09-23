
namespace XLuaFrameworkExt{

    [System.Serializable]
    public class XLuaFrameworkExtException : System.Exception
    {
        public XLuaFrameworkExtException() { }
        public XLuaFrameworkExtException(string message) : base(message) { }
        public XLuaFrameworkExtException(string message, System.Exception inner) : base(message, inner) { }
        protected XLuaFrameworkExtException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
