using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Setup.TemplatesInstaller
{    
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
                return -1;

            try
            {
                var userSettingsPath = Path.Combine(args[0], "UserSettings.xml");
                var liveTemplatesPath = Path.Combine(args[0], args[1]);

                var userSettings = XDocument.Parse(File.ReadAllText(userSettingsPath));
                var liveTemplates = XDocument.Load(File.OpenRead(liveTemplatesPath));

                var userTemplates = userSettings.Descendants("LiveTemplatesManager")
                    .Elements("UserTemplates")
                    .Single();

                foreach (var liveTemplate in liveTemplates.Descendants("Template"))
                {
                    var installedTemplate = userTemplates.Elements("Template")
                        .SingleOrDefault(x => x.Attribute("uid").Value == liveTemplate.Attribute("uid").Value);

                    liveTemplate.SetAttributeValue("xunit-resharper", "true");

                    if (installedTemplate != null)
                        installedTemplate.ReplaceWith(liveTemplate);
                    else
                        userTemplates.Add(liveTemplate);
                }

                userSettings.Save(userSettingsPath);
            }
            catch
            {
                return 1;
            }

            return 0;
        }
    }
}