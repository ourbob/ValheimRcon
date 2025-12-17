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
            "-setprefab " +
            "-preventremoval (not working yet) " +
            "-removeattachment (not working yet)";

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
            string setTag = null;
            bool preventRemoval = false;
            bool disable = false;
            bool enable = false;
            String setprefab = string.Empty;
            bool removeAttachment = false;
            
            
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
                    case "-settag":
                        setTag = args.GetString(index + 1);
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
                    case "-removeattachment":
                        removeAttachment = true;
                        break;
                    case "-setprefab":
                        setprefab = args.GetString(index + 1);
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
                if(setprefab != String.Empty)
                {
                    var newprefab = ZNetScene.instance.GetPrefab(setprefab);
                    if (newprefab == null) return $"Prefab {setprefab} not found";
                    zdo.SetPrefab(newprefab.name.GetStableHashCode());
                }
                if (setHealth)
                {
                    zdo.Set(ZDOVars.s_health, newHealth);
                }
                if(setTag != null)
                {
                    zdo.SetTag(setTag);
                }
                /*
                ZNetView znv = ZNetScene.instance.FindInstance(zdo);
                if (znv == null) {
                    sb.AppendLine("[ERROR: Cannot get ZNetView from ZNetScene for ZDO]");
                    continue;
                }
                if (removeAttachment) {  // TODO: Not working - Intended for removing trophies from spawn
                    sb.AppendLine("[INFO: Trying to remove attachment]");
                    ItemStand holder = znv.gameObject.GetComponentInChildren<ItemStand>();
                    if (holder == null)
                    {
                        sb.AppendLine("[ERROR: ItemStand component is null]");
                    }
                    else
                    {
                        sb.AppendLine("[INFO: REMOVING]");
                        holder.DropItem();
                    }
                }
                if (preventRemoval)  // TODO: Not working
                {
                    zdo.Persistent = true;
                    if (znv.gameObject == null)
                    {
                        sb.AppendLine();
                        sb.AppendLine("[ERROR: Cannot find prefab / gameobject]");
                    }
                    else
                    {
                        Piece piece = znv.gameObject.GetComponentInChildren<Piece>();
                        if (piece == null)
                        {
                            sb.AppendLine();
                            sb.AppendLine("[ERROR: Cannot find Piece object]");
                        }
                        else
                        {
                            piece.m_canBeRemoved = false;
                        }
                        WearNTear wearAndTear = znv.gameObject.GetComponentInChildren<WearNTear>();
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
                }
                */
                sb.AppendLine();
                sb.AppendLine("---->");
                ZdoUtils.AppendZdoStats(zdo, sb);
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }
    }
}
