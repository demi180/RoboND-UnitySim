namespace Uml.Robotics.XmlRpc
{
    public delegate void XmlRpcFunc(XmlRpcValue parms, XmlRpcValue result);

    public class XmlRpcServerMethod
    {
        public XmlRpcServerMethod(XmlRpcServer server, string functionName, XmlRpcFunc func = null)
        {
            this.Name = functionName;
            this.Server = server;
            this.Func = func ?? Execute;
        }

        public XmlRpcServer Server { get; private set; }
        public string Name { get; private set; }
        public XmlRpcFunc Func { get; private set; }

        public virtual void Execute(XmlRpcValue parms, XmlRpcValue result)
        {
            Func(parms, result);
        }

        public virtual string Help()
        {
            return "no help";
        }
    }
}
