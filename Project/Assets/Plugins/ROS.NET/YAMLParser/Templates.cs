using System;
using System.IO;

namespace YAMLParser
{
    public static class Templates
    {
        public static void LoadTemplateStrings(string templateProjectPath)
        {
            Templates.MessagesProj = File.ReadAllText(Path.Combine(templateProjectPath, "Messages._csproj"));
            Templates.MsgPlaceHolder = File.ReadAllText(Path.Combine(templateProjectPath, "PlaceHolder._cs"));
            Templates.SrvPlaceHolder = File.ReadAllText(Path.Combine(templateProjectPath, "SrvPlaceHolder._cs"));
            Templates.ActionMessagesPlaceHolder = File.ReadAllText(Path.Combine(templateProjectPath, "ActionMessagesPlaceHolder._cs"));
            Templates.ActionMessageTemplate = File.ReadAllText(Path.Combine(templateProjectPath, "ActionMessageTemplate._cs"));
            Templates.InnerMessageTemplate = File.ReadAllText(Path.Combine(templateProjectPath, "InnerMessageTemplate._cs"));
        }

        internal static string MessagesProj { get; set; }
        internal static string MsgPlaceHolder { get; set; }
        internal static string SrvPlaceHolder { get; set; }
        internal static string ActionPlaceHolder { get; set; }
        internal static string ActionMessagesPlaceHolder { get; set; }
        internal static string ActionMessageTemplate { get; set; }
        internal static string InnerMessageTemplate { get; set; }
    }
}
