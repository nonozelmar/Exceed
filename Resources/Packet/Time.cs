﻿using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Resources.Packet {
    public class Time {
        public const int packetID = 5;

        public int day;
        public int time;

        public Time() { }

        public Time(BinaryReader reader) {
            day = reader.ReadInt32();
            time = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer, bool writePacketID = true) {
            if(writePacketID) {
                writer.Write(packetID);
            }
            writer.Write(day);
            writer.Write(time);
        }

        public void Broadcast(Dictionary<ulong, Player> players, ulong toSkip) {
            foreach(Player player in new List<Player>(players.Values)) {
                if(player.entityData.guid != toSkip) {
                    try {
                        this.Write(player.writer);
                    }
                    catch (IOException) { }
                }
            }
        }
    }
}
