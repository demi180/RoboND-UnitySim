using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using YAMLParser;

namespace FauxMessages
{
    public class SrvFile
    {
        private string GUTS;
        public string GeneratedDictHelper;
        public bool HasHeader;
        public string Name;
        public string Namespace = "Messages";
        public MsgFile Request;
        public MsgFile Response;
        public List<SingleType> Stuff = new List<SingleType>();
        public string backhalf;
        public string classname;
        private List<string> def = new List<string>();
        public string dimensions = "";
        public string fronthalf;
        private bool meta;
        public string requestbackhalf;
        public string requestfronthalf;
        public string responsebackhalf;
        public string resposonebackhalf;

        internal MsgFileLocation msgfilelocation;

        public SrvFile(MsgFileLocation filename)
        {
            msgfilelocation = filename;
            // read in srv file
            var lines = File.ReadAllLines(filename.Path);
            classname = filename.basename;
            Namespace += "." + filename.package;
            Name = filename.package + "." + filename.basename;
            
            // def is the list of all lines in the file
            def = new List<string>();
            int mid = 0;
            bool found = false;
            var request = new List<string>();
            var response = new List<string>();
            
            // Search through for the "---" separator between request and response
            for (; mid < lines.Length; mid++)
            {
                lines[mid] = lines[mid].Replace("\"", "\\\"");
                if (lines[mid].Contains('#'))
                {
                    lines[mid] = lines[mid].Substring(0, lines[mid].IndexOf('#'));
                }
                lines[mid] = lines[mid].Trim();
                if (lines[mid].Length == 0)
                {
                    continue;
                }
                def.Add(lines[mid]);
                if (lines[mid].Contains("---"))
                {
                    found = true;
                    continue;
                }
                if (found)
                    response.Add(lines[mid]);
                else
                    request.Add(lines[mid]);
            }

            // treat request and response like 2 message files, each with a partial definition and extra stuff tagged on to the classname
            Request = new MsgFile(new MsgFileLocation(filename.Path.Replace(".srv", ".msg"), filename.searchroot), true, request, "\t");
            Response = new MsgFile(new MsgFileLocation(filename.Path.Replace(".srv", ".msg"), filename.searchroot), false, response, "\t");
        }

        public void ParseAndResolveTypes()
        {
            this.Request.ParseAndResolveTypes();
            this.Response.ParseAndResolveTypes();
        }

        public void Write(string outdir)
        {
            string[] chunks = Name.Split('.');
            for (int i = 0; i < chunks.Length - 1; i++)
                outdir = Path.Combine(outdir, chunks[i]);
            if (!Directory.Exists(outdir))
                Directory.CreateDirectory(outdir);
            string contents = this.ToString();
            if (contents != null)
                File.WriteAllText(Path.Combine(outdir, msgfilelocation.basename + ".cs"), contents.Replace("FauxMessages", "Messages"));
        }

        public override string ToString()
        {
            if (requestfronthalf == null)
            {
                requestfronthalf = "";
                requestbackhalf = "";
                string[] lines = Templates.SrvPlaceHolder.Split('\n');
                int section = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    //read until you find public class request... do everything once.
                    //then, do it again response
                    if (lines[i].Contains("$$REQUESTDOLLADOLLABILLS"))
                    {
                        section++;
                        continue;
                    }
                    if (lines[i].Contains("namespace"))
                    {
                        requestfronthalf +=
                          "\nusing Messages.std_msgs;\nusing String=System.String;\nusing Messages.geometry_msgs;\n\n"; //\nusing Messages.roscsharp;
                        requestfronthalf += "namespace " + Namespace + "\n";
                        continue;
                    }
                    if (lines[i].Contains("$$RESPONSEDOLLADOLLABILLS"))
                    {
                        section++;
                        continue;
                    }
                    switch (section)
                    {
                        case 0:
                            requestfronthalf += lines[i] + "\n";
                            break;
                        case 1:
                            requestbackhalf += lines[i] + "\n";
                            break;
                        case 2:
                            responsebackhalf += lines[i] + "\n";
                            break;
                    }
                }
            }

            GUTS = requestfronthalf + Request.GetSrvHalf() + requestbackhalf + Response.GetSrvHalf() + "\n" +
                   responsebackhalf;
            /***********************************/
            /*       CODE BLOCK DUMP           */
            /***********************************/

            #region definitions

            for (int i = 0; i < def.Count; i++)
            {
                while (def[i].Contains("\t"))
                    def[i] = def[i].Replace("\t", " ");
                while (def[i].Contains("\n\n"))
                    def[i] = def[i].Replace("\n\n", "\n");
                def[i] = def[i].Replace('\t', ' ');
                while (def[i].Contains("  "))
                    def[i] = def[i].Replace("  ", " ");
                def[i] = def[i].Replace(" = ", "=");
                def[i] = def[i].Replace("\"", "\"\"");
            }

            StringBuilder md = new StringBuilder();
            StringBuilder reqd = new StringBuilder();
            StringBuilder resd = null;
            foreach (string s in def)
            {
                if (s == "---")
                {
                    //only put this string in md, because the subclass defs don't contain it
                    md.AppendLine(s);

                    //we've hit the middle... move from the request to the response by making responsedefinition not null.
                    resd = new StringBuilder();
                    continue;
                }

                //add every line to MessageDefinition for whole service
                md.AppendLine(s);

                //before we hit ---, add lines to request Definition. Otherwise, add them to response.
                if (resd == null)
                    reqd.AppendLine(s);
                else
                    resd.AppendLine(s);
            }

            string MessageDefinition = md.ToString().Trim();
            string RequestDefinition = reqd.ToString().Trim();
            string ResponseDefinition = "";
            if (resd != null)
                ResponseDefinition = resd.ToString().Trim();

            #endregion

            #region THE SERVICE

            GUTS = GUTS.Replace("$WHATAMI", classname);
            GUTS = GUTS.Replace("$MYSRVTYPE", Namespace.Replace("Messages.", "") + "/" + classname);
            GUTS = GUTS.Replace("$MYSERVICEDEFINITION", "@\"" + MessageDefinition + "\"");

            #endregion

            #region request

            string RequestDict = Request.GenFields();
            meta = Request.meta;
            GUTS = GUTS.Replace("$REQUESTMYISMETA", meta.ToString().ToLower());
            GUTS = GUTS.Replace("$REQUESTMYMSGTYPE", Namespace.Replace("Messages.", "") + "/" + classname);
            GUTS = GUTS.Replace("$REQUESTMYMESSAGEDEFINITION", "@\"" + RequestDefinition + "\"");
            GUTS = GUTS.Replace("$REQUESTMYHASHEADER", Request.HasHeader.ToString().ToLower());
            GUTS = GUTS.Replace("$REQUESTMYFIELDS", RequestDict.Length > 5 ? "{{" + RequestDict + "}}" : "()");
            GUTS = GUTS.Replace("$REQUESTNULLCONSTBODY", "");
            GUTS = GUTS.Replace("$REQUESTEXTRACONSTRUCTOR", "");

            #endregion

            #region response

            string ResponseDict = Response.GenFields();
            GUTS = GUTS.Replace("$RESPONSEMYISMETA", Response.meta.ToString().ToLower());
            GUTS = GUTS.Replace("$RESPONSEMYMSGTYPE", Namespace.Replace("Messages.", "") + "/" + classname);
            GUTS = GUTS.Replace("$RESPONSEMYMESSAGEDEFINITION", "@\"" + ResponseDefinition + "\"");
            GUTS = GUTS.Replace("$RESPONSEMYHASHEADER", Response.HasHeader.ToString().ToLower());
            GUTS = GUTS.Replace("$RESPONSEMYFIELDS", ResponseDict.Length > 5 ? "{{" + ResponseDict + "}}" : "()");
            GUTS = GUTS.Replace("$RESPONSENULLCONSTBODY", "");
            GUTS = GUTS.Replace("$RESPONSEEXTRACONSTRUCTOR", "");

            #endregion

            #region MD5

            GUTS = GUTS.Replace("$REQUESTMYMD5SUM", MD5.Sum(Request));
            GUTS = GUTS.Replace("$RESPONSEMYMD5SUM", MD5.Sum(Response));
            string GeneratedReqDeserializationCode = "", GeneratedReqSerializationCode = "", GeneratedResDeserializationCode = "", GeneratedResSerializationCode = "", GeneratedReqRandomizationCode = "", GeneratedResRandomizationCode = "", GeneratedReqEqualizationCode = "", GeneratedResEqualizationCode = "";
            //TODO: service support
            GeneratedReqEqualizationCode += string.Format("{0}.Request other = (Messages.{0}.Request)____other;\n", Request.Name);
            GeneratedResEqualizationCode += string.Format("{0}.Response other = (Messages.{0}.Response)____other;\n", Response.Name);
            for (int i = 0; i < Request.Stuff.Count; i++)
            {
                GeneratedReqDeserializationCode += Request.GenerateDeserializationCode(Request.Stuff[i], 1);
                GeneratedReqSerializationCode += Request.GenerateSerializationCode(Request.Stuff[i], 1);
                GeneratedReqRandomizationCode += Request.GenerateRandomizationCode(Request.Stuff[i], 1);
                GeneratedReqEqualizationCode += Request.GenerateEqualityCode(Request.Stuff[i], 1);
            }
            for (int i = 0; i < Response.Stuff.Count; i++)
            {
                GeneratedResDeserializationCode += Response.GenerateDeserializationCode(Response.Stuff[i], 1);
                GeneratedResSerializationCode += Response.GenerateSerializationCode(Response.Stuff[i], 1);
                GeneratedResRandomizationCode += Response.GenerateRandomizationCode(Response.Stuff[i], 1);
                GeneratedResEqualizationCode += Response.GenerateEqualityCode(Response.Stuff[i], 1);
            }
            GUTS = GUTS.Replace("$REQUESTSERIALIZATIONCODE", GeneratedReqSerializationCode);
            GUTS = GUTS.Replace("$REQUESTDESERIALIZATIONCODE", GeneratedReqDeserializationCode);
            GUTS = GUTS.Replace("$REQUESTRANDOMIZATIONCODE", GeneratedReqRandomizationCode);
            GUTS = GUTS.Replace("$REQUESTEQUALITYCODE", GeneratedReqEqualizationCode);
            GUTS = GUTS.Replace("$RESPONSESERIALIZATIONCODE", GeneratedResSerializationCode);
            GUTS = GUTS.Replace("$RESPONSEDESERIALIZATIONCODE", GeneratedResDeserializationCode);
            GUTS = GUTS.Replace("$RESPONSERANDOMIZATIONCODE", GeneratedResRandomizationCode);
            GUTS = GUTS.Replace("$RESPONSEEQUALITYCODE", GeneratedResEqualizationCode);

            string md5 = MD5.Sum(this);
            if (md5 == null)
                return null;
            GUTS = GUTS.Replace("$MYSRVMD5SUM", md5);

            #endregion

            /********END BLOCK**********/
            return GUTS;
        }
    }
}
