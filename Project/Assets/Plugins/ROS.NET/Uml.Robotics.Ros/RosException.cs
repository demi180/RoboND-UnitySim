using System;

public class RosException: Exception
{
    public RosException()
    {
    }

    public RosException(string message)
        : base(message)
    {
    }

    public RosException(string message, Exception inner)
        : base(message, inner)
    {
    }
}