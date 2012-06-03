using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Setup.TemplatesInstaller
{
    static class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
                return -1;

            try
            {
                var installed =
                    InstallTemplates(args[0], "", args[1]) | //All versions
                    InstallTemplates(args[0], "vs10.0", args[1]) | // vs2010
                    InstallTemplates(args[0], "vs9.0", args[1]) |  // vs2008
                    InstallTemplates(args[0], "vs8.0", args[1]);  // vs2005

                if (!installed)
                    return 1;
            }
            catch
            {
                return 1;
            }

            return 0;
        }

        private static bool InstallTemplates(string path, string vsVersion, string templates)
        {
            var userSettingsPath = Path.Combine(Path.Combine(path, vsVersion), "UserSettings.xml");
            var liveTemplatesPath = Path.Combine(path, templates);

            if (!File.Exists(userSettingsPath))
                return false;

            var userSettings = XDocument.Parse(File.ReadAllText(userSettingsPath));
            var liveTemplates = XDocument.Load(new StreamReader(File.OpenRead(liveTemplatesPath)));

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

            return true;
        }
    }
}