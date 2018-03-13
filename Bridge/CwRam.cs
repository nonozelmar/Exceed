﻿using ReadWriteProcessMemory;
using Resources;
using Resources.Utilities;
using System.Text;

namespace Bridge {
    static class CwRam {
        public static FormMain formMain;
        public static ProcessMemory memory;
        public static int EntityStart {
            get { return memory.ReadInt(memory.ReadInt(memory.baseAddress + 0x0036b1c8) + 0x39C); }
        }

        public static int ItemStart {
            get { return EntityStart + 0x1E8 + formMain.editor.listBoxItem.SelectedIndex * 280; }
        }
        public static int SlotStart {
            get { return ItemStart + 20 + 8 * formMain.editor.listBoxSlots.SelectedIndex; }
        }

        public static int SkillStart {
            get { return EntityStart + 0x1138; }
        }

        public static void SetMode(Mode mode, int timer) {
            CwRam.memory.WriteInt(CwRam.EntityStart + 0x6C, timer);//skill timer
            CwRam.memory.WriteInt(CwRam.EntityStart + 0x68, (int)mode);//skill
        }
        public static void SetName(string name) {
            var data = new byte[16];
            Encoding.ASCII.GetBytes(name).CopyTo(data, 0);
            CwRam.memory.WriteBytes(CwRam.EntityStart + 0x1168, data);
        }

        public static void Teleport(LongVector destination) {
            CwRam.memory.WriteLong(CwRam.EntityStart + 0x10, destination.x);
            CwRam.memory.WriteLong(CwRam.EntityStart + 0x18, destination.y); 
            CwRam.memory.WriteLong(CwRam.EntityStart + 0x20, destination.z);
        }

        public static void Freeze(int duration) {
            CwRam.memory.WriteInt(CwRam.EntityStart + 0x134, duration);//ice spirit effect
        }

        public static void Fear(int duration) {
            CwRam.memory.WriteInt(CwRam.EntityStart + 0x130, duration);//ice spirit effect
        }

        public static void RemoveFog() {
            memory.WriteBytes(memory.baseAddress + 0x89316, new byte[8] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });//fog change based on render dist
            memory.WriteBytes(memory.baseAddress + 0x89368, new byte[10] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });//fog change based on world change
            memory.WriteByte(memory.baseAddress + 0x894EE, 0);//loading screen
            memory.WriteSingle(memory.ReadInt(memory.baseAddress + 0x0036b1c8) + 0x1D4, -1000f);//fog
        }

        public static void ZoomHack(bool state) {
            if (state) memory.WriteBytes(memory.baseAddress + 0x7EFE9, new byte[10] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });//limit zoom distance to 14
            else memory.WriteBytes(memory.baseAddress + 0x7EFE9, new byte[10] { 0xC7, 0x81, 0xC0, 0x01, 0x00, 0x00, 0x00, 0x00, 0x60, 0x41 });//limit zoom distance to 14
        }

        public static void Knockback(FloatVector direction) {
            memory.WriteSingle(EntityStart + 0x4C, memory.ReadSingle(EntityStart + 0x4C) + direction.x);
            memory.WriteSingle(EntityStart + 0x50, memory.ReadSingle(EntityStart + 0x50) + direction.y);
            memory.WriteSingle(EntityStart + 0x54, memory.ReadSingle(EntityStart + 0x54) + direction.z);
        }
    }
}
