using System.Linq;
using System.Text;

namespace ValheimRcon.Commands
{
    internal class DeleteObjects : RconCommand
    {
        public override string Command => "deleteObjects";

        public override string Description => "Delete objects matching all search criteria. " +
            "Usage (with optional arguments): deleteObjects " +
            "-creator <creator id> " +
            "-id <id> <userid> " +
            "-tag <tag> " +
            "-override";

        protected override string OnHandle(CommandArgs args)
        {
            var optionalArgs = args.GetOptionalArguments();

            if (!optionalArgs.Any())
            {
                return "At least one criteria must be provided.";
            }

            long? creatorId = null;
            ObjectId? id = null;
            var tag = string.Empty;
            bool overrideAllowed = false;

            foreach (var index in optionalArgs)
            {
                var argument = args.GetString(index);
                switch (argument.ToLower())
                {
                    case "-creator":
                        creatorId = args.GetLong(index + 1);
                        break;
                    case "-id":
                        id = args.GetObjectId(index + 1);
                        break;
                    case "-tag":
                        tag = args.GetString(index + 1);
                        break;
                    case "-override":
                        overrideAllowed = true;
                        break;
                    default:
                        return $"Unknown argument: {argument}";
                }
            }

            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => ZdoUtils.MatchesCriteria(zdo, creatorId, id, tag))
                .ToArray();

            if (objects.Length == 0)
            {
                return "No objects found matching the provided criteria.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Deleting {objects.Length} objects:");
            foreach (var zdo in objects)
            {
                var prefabName = ZdoUtils.GetPrefabName(zdo.GetPrefab());
                sb.Append($"- Prefab: {prefabName}");
                ZdoUtils.AppendZdoStats(zdo, sb);

                if (ZdoUtils.CanDeleteZdo(zdo))
                {
                    ZdoUtils.DeleteZDO(zdo);
                    sb.AppendLine(" [deleted]");
                }
                else
                {
                    if (overrideAllowed)
                    {
                        sb.AppendLine(" [WARNING: NOT ALLOWED TO DELETE, OVERRISE SET SO DOING IT ANYWAY!]");
                        zdo.SetOwner(0);
                        ZDOMan.instance.m_destroySendList.Add(zdo.m_uid);
                    }
                    else
                    {
                        sb.AppendLine(" [ERROR: NOT ALLOWED TO DELETE, try with -override if you are sure you want to delete this]");
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }
    }
}
