using System;
using System.Collections.Generic;
using System.Text;

namespace Uml.Robotics.Ros
{
    [IgnoreRosMessage]
    public class InnerActionMessage : RosMessage
    {
        // Base class for Action Message Generics. Used for Generic Constraints, so that only action messages can get plugged
        // into the Action Message Generics.
    }
}
