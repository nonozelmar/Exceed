﻿using System.Collections.Generic;
using System.IO;

namespace Resources.Packet {
    public class ProtocolVersion {
        public const int packetID = 17;

        public int version;

        public ProtocolVersion() { }

        public ProtocolVersion(BinaryReader reader) {
            version = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer, bool writePacketID = true) {
            if(writePacketID) {
                writer.Write(packetID);
            }
            writer.Write(version);
        }

        public void Broadcast(Dictionary<ulong, Player> players, ulong toSkip) {
            foreach(var player in players.Values) {
                if(player.entityData.guid != toSkip) {
                    Write(player.writer);
                }
            }
        }
    }
}
