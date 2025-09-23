using System;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace ValheimRcon.Commands
{
    internal class ModifyObjects : RconCommand
    {
        public override string Command => "modifyObjects";

        public override string Description => "Modify objects matching all search criteria. " +
            "Usage (with optional arguments): modifyObjects " +
            "-creator <creator id> " +
            "-id <id> <userid> " +
            "-tag <tag>" +
            "-sethealth <health> " +
            "-enable " +
            "-disable " +
            "-preventremoval (not working yet)";

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
            string prefab = string.Empty;

            bool setHealth = false;
            float newHealth = 0;
            bool preventRemoval = false;
            bool disable = false;
            bool enable = false;

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
                    case "-prefab":
                        prefab = args.GetString(index + 1);
                        break;
                    case "-sethealth":
                        setHealth = true;
                        newHealth = args.GetFloat(index + 1);
                        break;
                    case "-preventremoval":
                        preventRemoval = true;
                        break;
                    case "-enable":
                        enable = true;
                        break;
                    case "-disable":
                        disable = true;
                        break;
                    default:
                        return $"Unknown argument: {argument}";
                }
            }

            var objects = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => ZdoUtils.MatchesCriteria(zdo, creatorId, id, tag, prefab))
                .ToArray();

            if (objects.Length == 0)
            {
                return "No objects found matching the provided criteria.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Modifying {objects.Length} objects:");
            foreach (ZDO zdo in objects)
            {
                var prefabName = ZdoUtils.GetPrefabName(zdo.GetPrefab());
                sb.AppendLine($"- Prefab: {prefabName}");
                ZdoUtils.AppendZdoStats(zdo, sb);
                if (enable) {
                    zdo.Set(ZDOVars.s_enabled, true);
                }
                if (disable)
                {
                    zdo.Set(ZDOVars.s_enabled, false);
                }
                if (setHealth)
                {
                    zdo.Set(ZDOVars.s_health, newHealth);
                }
                if (preventRemoval)
                {
                    zdo.Persistent = true;
                    /*
                    ZNetView znv = ZNetScene.instance.FindInstance(zdo); // TODO: NOT WORKING
                    if (znv == null)
                    {
                        sb.AppendLine();
                        sb.AppendLine("[ERROR: Cannot find object instance]");
                    }
                    else
                    {
                        Piece piece = znv.GetComponent<Piece>();
                        if (piece == null)
                        {
                            sb.AppendLine();
                            sb.AppendLine("[ERROR: Cannot find Piece object]");
                        }
                        else
                        {
                            piece.m_canBeRemoved = false;
                        }
                        WearNTear wearAndTear = znv.GetComponent<WearNTear>();
                        if (wearAndTear == null)
                        {
                            sb.AppendLine();
                            sb.AppendLine("[ERROR: Cannot find WearNTear object]");
                        }
                        else
                        {
                            wearAndTear.m_ashDamageImmune = true;
                            wearAndTear.m_noRoofWear = true;
                            wearAndTear.m_noSupportWear = true;
                        }
                    }
                    */
                }
                sb.AppendLine();
                sb.AppendLine("->");
                ZdoUtils.AppendZdoStats(zdo, sb);
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }
    }
}
