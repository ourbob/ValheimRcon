using System.Linq;
using System.Text;

namespace ValheimRcon.Commands
{
    internal class ShowPlayers : RconCommand
    {
        private StringBuilder _builder = new StringBuilder();

        public override string Command => "players";

        public override string Description => "Show all online players with their positions and zones";

        protected override string OnHandle(CommandArgs args)
        {
            _builder.Clear();
            var online = ZNet.instance.GetPeers().Count;
            _builder.AppendFormat("Online {0}\n", online);

            foreach (var player in ZNet.instance.GetPeers())
            {
                //var data = string.Join("&", player.m_serverSyncedPlayerData
                //    .Select(pair => $"{pair.Key}={pair.Value}"));

                player.m_serverSyncedPlayerData.TryGetValue("platformDisplayName", out var displayName);
                player.m_socket.GetConnectionQuality(
                    out float localQ,
                    out float remoteQ,
                    out int ping,
                    out float outBps,
                    out float inBps
                    );
                _builder.AppendFormat("{0}:{1} - {2}({3}), {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}",
                    player.GetSteamId(),
                    player.m_playerName,
                    player.GetRefPos(),
                    ZoneSystem.GetZone(player.GetRefPos()),
                    player.m_publicRefPos,
                    displayName,
                    player.m_socket.GetEndPointString(),
                    localQ,
                    remoteQ,
                    ping,
                    outBps,
                    inBps
                    );

                _builder.AppendLine();
            }
            return _builder.ToString();
        }
    }
}
