using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace Plugin.Windows10
{
    class DialogueManager
    {

        private XDocument doc = null;
        private Frame frame = null;
        private Connector connector = null;
        public DialogueManager(StorageFile file)
        {
            this.doc = XDocument.Load(file.Path);
            this.frame = new Frame();
            connector = new Connector();

        }

        public class Frame
        {

            public FrameItem Action = new FrameItem(null);
            public FrameItem Subtype = new FrameItem(null, true);
            public FrameItem ExpectedType = new FrameItem(null, true);
            public FrameItem DestType = new FrameItem(null);
            public FrameItem DestName = new FrameItem(null);
            public FrameItem Prox = new FrameItem(null, true);



            public Frame Reset()
            {
                return new Frame();
            }
        }

        public class FrameItem
        {
            private string _value;
            public string Value
            {
                get { return _value; }
                set { _value = value; }
            }

            public readonly bool Optional = false;
            public FrameItem(string value, bool optional = false)
            {
                Value = value;
                Optional = optional;
            }
        }


        public Dictionary<string, string> tagMeaning = new Dictionary<string, string>() { { "COFFEE_SHOP", "coffee shop" },
            { "SHOE_SHOP", "shoe shop"} };

        // what if we need to reset the frame?
        // what if we already have the value?
        // need a preerve/reset mechanism
        public void FillFrame(Dictionary<string, string> input)
        {
            frame.Action.Value = input.ContainsKey("ACTION") ? input["ACTION"] : null;
            if(frame.DestType.Value == null)
            frame.DestType.Value = input.ContainsKey("DESTINATION") ? input["DESTINATION"] : null;
            if(frame.DestName.Value == null)
            frame.DestName.Value = input.ContainsKey("DEST_NAME") ? input["DEST_NAME"] : null;
            frame.Prox.Value = input.ContainsKey("PROX") ? input["PROX"] : null;
        }

        


        public string GenerateResponse(Dictionary<string, string> input, ref VoiceBot.Result state)
        {
            connector.Connect();
            FillFrame(input);
            Dictionary<string, string> templateValues = new Dictionary<string, string>();

            switch (frame.Action.Value)
            {
                // greeting does not require the frame to be filled
                case "GREETING":
                    if (frame.ExpectedType.Value != null && frame.ExpectedType.Value != "GREETING")
                    {
                        goto default;
                    }
                    else {
                        frame = frame.Reset();
                        return GetResponseTemplate("GREETING");
                    }
                case "DESTINATION":
                    // check if shop exist
                    if(frame.DestType.Value != null)
                    {
                        var list = connector.FindShopByType(frame.DestType.Value);
                        // if there's only a single shop
                        if (list.Count == 1)
                        {
                            // check if the shop name matches
                            if(frame.DestName.Value != null)
                            {
                                var item = list.Where(x => x.Name == frame.DestName.Value).FirstOrDefault();
                                if(item != null)
                                {
                                    // great we found our goal
                                    templateValues.Add("DESTINATION", tagMeaning[frame.DestType.Value]);
                                    templateValues.Add("DEST_NAME", frame.DestName.Value);
                                    return FillTemplate(GetResponseTemplate("DESTINATION"), templateValues);
                                    
                                } else
                                {
                                    // suggest user the shop
                                    // update the frame for confirmation
                                    return null;
                                }
                            } else
                            {
                                templateValues.Add("DESTINATION", tagMeaning[frame.DestType.Value]);
                                templateValues.Add("DEST_NAME", "");
                                return FillTemplate(GetResponseTemplate("DESTINATION"), templateValues);
                            }
                        } else
                        {
                            if(frame.DestName.Value != null)
                            {
                                var item = list.Where(x => x.Name == frame.DestName.Value).FirstOrDefault();
                                if (item != null)
                                {
                                    templateValues.Add("DESTINATION", tagMeaning[frame.DestType.Value]);
                                    templateValues.Add("DEST_NAME", frame.DestName.Value);
                                    return FillTemplate(GetResponseTemplate("DESTINATION"), templateValues);
                                }
                                else
                                {
                                    // shop does not exist
                                    return null;
                                }
                            } else
                            {
                                // we need more info
                                frame.Subtype.Value = "CLARIFY";
                                frame.ExpectedType.Value = "DEST_NAME";
                                templateValues.Add("DESTINATION", GetTagMeaning(frame.DestType.Value));
                                var shop_list = "";
                                foreach (var item in list)
                                    shop_list += " " + item.Name;
                                templateValues.Add("SHOP_LIST", shop_list);
                                return FillTemplate(GetResponseTemplate("CLARIFY"), templateValues);
                            }
                        }
                    } else
                    {
                        // need more information
                        return null;
                    }
                default:
                    return null;
            }

        }

        private string GetResponseTemplate(string type)
        {
            var response = doc.Descendants("response").FirstOrDefault(x => x.Attribute("type").Value == type.ToLower());

            var elements = response.Elements();
            var elemnList = elements.ToList();
            var elem = elemnList[0];
            string val = elem.Value;
            return val;
        }

        private string FillTemplate(string template, Dictionary<string, string> input)
        {
            string tmpl = template;
            foreach (var key in input.Keys)
            {
                Regex rgx = new Regex("\\$" + key);
                tmpl = rgx.Replace(tmpl, input[key]);
            }
            return tmpl;
        }


        private string GetTagMeaning(string tag)
        {
            if (tagMeaning.ContainsKey(tag))
                return tagMeaning[tag];
            return "";

        }





    }
}
