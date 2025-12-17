using BepInEx;
using System.IO;
using System.Linq;
using System.Text;
using static WorldGenerator;

namespace ValheimRcon.Commands
{
    internal class dumpAllUserObjects : RconCommand
    {
        public override string Command => "dumpAllUserObjects";

        public override string Description => "Dump all user objects to a file";
        private string escapeCSV(string str)
        {
            str = str.Replace("\n", "\\n");
            str = str.Replace("\r", "\\r");
            str = str.Replace("\"", "\\\"");
            return $"\"{str}\"";
        }
        protected override string OnHandle(CommandArgs args)
        {
            string pluginFolder = Paths.PluginPath;
            string filePath = Path.Combine(pluginFolder, "objects.csv");
            File.WriteAllText(filePath, string.Empty);
            var objects = ZDOMan.instance.m_objectsByID.Values;
            foreach (var zdo in objects)
            {
                if (zdo.GetLong(ZDOVars.s_creator) != 0 || zdo.GetLong(ZDOVars.s_owner) != 0) {
                    var prefabName = ZdoUtils.GetPrefabName(zdo.GetPrefab());
                    string textContent = zdo.GetString(ZDOVars.s_text);
                    int textContentLength = textContent.Length;
                    string row = string.Format(
                        "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}\n",
                        zdo.m_uid.ID,
                        zdo.m_uid.UserID,
                        zdo.m_position.x,
                        zdo.m_position.y,
                        zdo.m_position.z,
                        zdo.GetLong(ZDOVars.s_creator),
                        escapeCSV(zdo.GetString(ZDOVars.s_creatorName)),
                        zdo.GetLong(ZDOVars.s_owner),
                        escapeCSV(zdo.GetString(ZDOVars.s_ownerName)),
                        zdo.GetZDOID("owner"),
                        escapeCSV(prefabName),
                        escapeCSV(zdo.GetTag()),
                        escapeCSV(zdo.GetString(ZDOVars.s_data)),
                        textContentLength,
                        escapeCSV(textContent)
                    );
                    File.AppendAllText(filePath, row);
                }
            }
            return $"Dumped to {filePath}";
        }
    }
}
