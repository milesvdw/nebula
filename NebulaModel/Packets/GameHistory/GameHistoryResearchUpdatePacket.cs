﻿namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryResearchUpdatePacket
    {
        public int TechId { get; set; }
        public long HashUploaded { get; set; }
        public long HashNeeded { get; set; }

        public GameHistoryResearchUpdatePacket() { }

        public GameHistoryResearchUpdatePacket(int techId, long hashUploaded, long hashNeeded)
        {
            TechId = techId;
            HashUploaded = hashUploaded;
            HashNeeded = hashNeeded;
        }
    }
}
