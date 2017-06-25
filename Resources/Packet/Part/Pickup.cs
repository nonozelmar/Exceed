﻿using System.IO;

namespace Resources.Packet.Part {
    public class Pickup {
        public ulong guid;
        public Item item;

        public void Read(BinaryReader reader) {
            guid = reader.ReadUInt64();
            item = new Item(); item.Read(reader);
        }

        public void Write(BinaryWriter writer) {
            writer.Write(guid);
            item.Write(writer);
        }
    }
}
