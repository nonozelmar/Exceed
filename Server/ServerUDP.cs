﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

using Server.Addon;

using Resources;
using Resources.Datagram;
using Resources.Packet;
using Newtonsoft.Json;

namespace Server {
    class ServerUDP {
        UdpClient udpClient;
        TcpListener tcpListener;
        ServerUpdate worldUpdate = new ServerUpdate();
        Dictionary<ushort, Player> players = new Dictionary<ushort, Player>();
        List<string[]> bans; //MAC|IP
        Dictionary<string, string> accounts;
        const string bansFilePath = "bans.json";
        const string accountsFilePath = "accounts.json";

        public ServerUDP(int port) {
            if (File.Exists(bansFilePath)) {
                bans = JsonConvert.DeserializeObject<List<string[]>>(File.ReadAllText(bansFilePath));
            }
            else {
                Console.WriteLine("no bans file found");
                bans = new List<string[]>();
                File.WriteAllText(bansFilePath, JsonConvert.SerializeObject(bans));
            }
            if (File.Exists(accountsFilePath)) {
                accounts = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(accountsFilePath));
            }
            else {
                Console.WriteLine("no accounts file found");
                accounts = new Dictionary<string, string>();
                File.WriteAllText(accountsFilePath, JsonConvert.SerializeObject(accounts));
            }

            #region models
            //ZoxModel model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Fulcnix_exceedspawn.zox"));
            //model.Parse(worldUpdate, 8286883, 8344394, 200); 
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_Tavern2.zox"));
            //model.Parse(worldUpdate, 8287010, 8344432, 200);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_Tavern1.zox"));
            //model.Parse(worldUpdate, 8286919, 8344315, 212); 
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/arena/aster_arena.zox"));
            //model.Parse(worldUpdate, 8286775, 8344392, 207);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/michael_project1.zox"));
            //model.Parse(worldUpdate, 8286898, 8344375, 213); 
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/arena/fulcnix_hall.zox"));
            //model.Parse(worldUpdate, 8286885, 8344505, 208); 
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/arena/fulcnix_hall.zox"));
            //model.Parse(worldUpdate, 8286885, 8344629, 208); 
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Tiecz_MountainArena.zox"));
            //model.Parse(worldUpdate, 8286885, 8344759, 208);
            ////8397006, 8396937, 127 //near spawn
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay11.zox"));
            //model.Parse(worldUpdate, 8286770, 8344262, 207);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay12.zox"));
            //model.Parse(worldUpdate, 8286770, 8344136, 207);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay13.zox"));
            //model.Parse(worldUpdate, 8286770, 8344010, 207);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay14.zox"));
            //model.Parse(worldUpdate, 8286770, 8344010, 333);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay01.zox"));
            //model.Parse(worldUpdate, 8286644, 8344010, 333);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay02.zox"));
            //model.Parse(worldUpdate, 8286118, 8344010, 333);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay03.zox"));
            //model.Parse(worldUpdate, 8285992, 8344010, 333);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay04.zox"));
            //model.Parse(worldUpdate, 8285992, 8344136, 333);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay05.zox"));
            //model.Parse(worldUpdate, 8285992, 8344262, 333);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay06.zox"));
            //model.Parse(worldUpdate, 8286118, 8344262, 333);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay07.zox"));
            //model.Parse(worldUpdate, 8286118, 8344136, 333);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay08.zox"));
            //model.Parse(worldUpdate, 8286244, 8344136, 333);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay09.zox"));
            //model.Parse(worldUpdate, 8286244, 8344262, 333);
            //model = JsonConvert.DeserializeObject<ZoxModel>(File.ReadAllText("models/Aster_CloudyDay10.zox"));
            //model.Parse(worldUpdate, 8286770, 8344262, 333);
            #endregion

            Console.WriteLine("loading completed");

            udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            new Thread(new ThreadStart(ListenUDP)).Start();
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            new Thread(new ThreadStart(ListenTCP)).Start();
        }

        public void ListenTCP() {
            Player player = new Player(tcpListener.AcceptTcpClient());
            new Thread(new ThreadStart(ListenTCP)).Start();
            if (true) {

            }
            
            if (player.reader.ReadInt32() != Database.bridgeVersion) {
                player.writer.Write(false);
                return;
            }
            player.writer.Write(true);

            string username = player.reader.ReadString();
            if (!accounts.ContainsKey(username)) {
                player.writer.Write((byte)AuthResponse.unknownUser);
                return;
            }
            string password = player.reader.ReadString();
            if (accounts[username] != password) {
                player.writer.Write((byte)AuthResponse.wrongPassword);
                return;
            }
            player.writer.Write((byte)AuthResponse.success);
            player.admin = username == "BLACKROCK";

            player.MAC = player.reader.ReadString();
            string[] banEntry = bans.FirstOrDefault(x => x[(byte)BanEntry.mac] == player.MAC || x[(byte)BanEntry.ip] == player.IpEndPoint.Address.ToString());
            if (banEntry != null) {
                player.writer.Write(true);
                player.writer.Write(banEntry[(byte)BanEntry.reason]);
                return;
            }
            player.writer.Write(false);//not banned

            ushort newGuid = 1;
            while (players.ContainsKey(newGuid)) {//find lowest available guid
                newGuid++;
            }
            player.entityData.guid = newGuid;
            players.Add(newGuid, player);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(newGuid + " connected");
            
            while (true) {
                try {
                    byte packetID = player.reader.ReadByte();
                    ProcessPacket(packetID, player);
                } catch (IOException) {
                    players.Remove((ushort)player.entityData.guid);
                    var disconnect = new Disconnect() {
                        Guid = (ushort)player.entityData.guid
                    };
                    BroadcastUDP(disconnect.data);

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(newGuid + " disconnected");
                    break;
                }
            }
        }
        public void ListenUDP() {
            IPEndPoint source = null;
            while(true) {
                byte[] datagram = udpClient.Receive(ref source);
                var player = players.FirstOrDefault(x => x.Value.IpEndPoint.Equals(source)).Value;
                if (player != null) {
                    ProcessDatagram(datagram, player);
                }
            }
        }

        public void SendUDP(byte[] data, Player target) {
            udpClient.Send(data, data.Length, target.IpEndPoint);
        }
        public void BroadcastUDP(byte[] data, Player toSkip = null, bool includeNotPlaying = false) {
            foreach(var player in players.Values) {
                if(player != toSkip && (player.playing || includeNotPlaying)) {
                    SendUDP(data, player);
                }
            }
        }

        public void ProcessPacket(byte packetID, Player source) {
            switch (packetID) {
                case 0://query
                    var query = new Query("Exceed Official", 65535);

                    foreach(var player in players.Values) {
                        if(player.playing && player.entityData.name != null) {
                            query.players.Add((ushort)player.entityData.guid, player.entityData.name);
                        }
                    }
                    query.Write(source.writer);
                    break;
                default:
                    Console.WriteLine("unknown packet: " + packetID);
                    break;
            }
        }
        public void ProcessDatagram(byte[] datagram, Player source) {
            switch ((DatagramID)datagram[0]) {
                case DatagramID.entityUpdate:
                    #region entityUpdate
                    var entityUpdate = new EntityUpdate(datagram);

                    string ACmessage = AntiCheat.Inspect(entityUpdate);
                    if(ACmessage != "ok") {
                        //var kickMessage = new ChatMessage() {
                        //    message = "illegal " + ACmessage
                        //};
                        //kickMessage.Write(player.writer, true);
                        //Console.WriteLine(player.entityData.name + " kicked for illegal " + kickMessage.message);
                        //Thread.Sleep(100); //thread is about to run out anyway so np
                        //Kick(player);
                        //return;
                    }
                    if(entityUpdate.name != null) {
                        //Announce.Join(entityUpdate.name, player.entityData.name, players);
                    }

                    entityUpdate.entityFlags |= 1 << 5; //enable friendly fire flag for pvp
                    if(!source.entityData.IsEmpty) { //dont filter the first packet
                        //entityUpdate.Filter(player.entityData);
                    }
                    if(!entityUpdate.IsEmpty) {
                        //entityUpdate.Broadcast(players, 0);
                        BroadcastUDP(entityUpdate.Data, source);
                        if(entityUpdate.HP == 0 && source.entityData.HP > 0) {
                            BroadcastUDP(Tomb.Show(source).Data);
                        } else if(source.entityData.HP == 0 && entityUpdate.HP > 0) {
                            BroadcastUDP(Tomb.Hide(source).Data);
                        }
                        entityUpdate.Merge(source.entityData);
                    }
                    break;
                #endregion
                case DatagramID.attack:
                    #region attack
                    var attack = new Attack(datagram);
                    source.lastTarget = attack.Target;
                    if (players.ContainsKey(attack.Target)) {//in case the target is a tombstone
                        SendUDP(datagram, players[attack.Target]);
                    }
                    break;
                #endregion
                case DatagramID.shoot:
                    #region shoot
                    var shoot = new Resources.Datagram.Shoot(datagram);
                    BroadcastUDP(datagram, source); //pass to all players except source
                    break;
                #endregion
                case DatagramID.proc:
                    #region proc
                    var proc = new Proc(datagram);

                    switch (proc.Type) {
                        case ProcType.bulwalk:
                            SendUDP(new Chat(string.Format("bulwalk: {0}% dmg reduction", 1.0f - proc.Modifier)).data, source);
                            break;
                        case ProcType.poison:
                            var poisonTick = new Attack() {
                                Damage = proc.Modifier,
                                Target = proc.Target
                            };
                            Poison(players[proc.Target], poisonTick);
                            break;
                        case ProcType.manashield:
                            SendUDP(new Chat(string.Format("manashield: {0}", proc.Modifier)).data, source);
                            break;
                        case ProcType.warFrenzy:
                        case ProcType.camouflage:
                        case ProcType.fireSpark:
                        case ProcType.intuition:
                        case ProcType.elusivenes:
                        case ProcType.swiftness:
                            break;
                        default:

                            break;
                    }
                    BroadcastUDP(datagram, source); //pass to all players except source
                    break;
                #endregion
                case DatagramID.chat:
                    #region chat
                    var chat = new Chat(datagram);
                    if (chat.Text.StartsWith("/")) {
                        string parameter = string.Empty;
                        string command = chat.Text.Substring(1);
                        if (chat.Text.Contains(" ")) {
                            int spaceIndex = command.IndexOf(" ");
                            parameter = command.Substring(spaceIndex + 1);
                            command = command.Substring(0, spaceIndex);
                        }
                        Command.UDP(command, parameter, source, this); //wip
                    } else {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(players[chat.Sender].entityData.name + ": ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(chat.Text);
                        BroadcastUDP(datagram, null, true); //pass to all players
                    }
                    break;
                #endregion
                case DatagramID.interaction:
                    #region interaction
                    var interaction = new Interaction(datagram);
                    BroadcastUDP(datagram, source); //pass to all players except source
                    break;
                #endregion
                case DatagramID.connect:
                    #region connect
                    var connect = new Connect(datagram) {
                        Guid = (ushort)source.entityData.guid,
                        Mapseed = Database.mapseed
                    };
                    SendUDP(connect.data, source);

                    foreach(Player p in players.Values) {
                        if(p.playing) {
                            SendUDP(p.entityData.Data, source);
                        }
                    }
                    source.playing = true;
                    Task.Delay(100).ContinueWith(t => Load_world_delayed(source)); //WIP, causes crash when player disconnects before executed
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(connect.Guid + " is now playing");
                    break;
                #endregion
                case DatagramID.disconnect:
                    #region disconnect
                    var disconnect = new Disconnect(datagram);
                    players[disconnect.Guid].playing = false;
                    BroadcastUDP(datagram, source, true);
                    players[disconnect.Guid].entityData = new EntityUpdate();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(disconnect.Guid + " is now lurking");
                    break;
                #endregion
                case DatagramID.specialMove:
                    #region specialMove
                    var specialMove = new SpecialMove(datagram);
                    switch (specialMove.Id) {
                        case SpecialMoveID.taunt:
                            var targetGuid = specialMove.Guid;
                            specialMove.Guid = (ushort)source.entityData.guid;
                            SendUDP(specialMove.data, players[targetGuid]);
                            break;
                        case SpecialMoveID.cursedArrow:
                        case SpecialMoveID.arrowRain:
                        case SpecialMoveID.shrapnel:
                        case SpecialMoveID.smokeBomb:
                        case SpecialMoveID.iceWave:
                        case SpecialMoveID.confusion:
                        case SpecialMoveID.shadowStep:
                            BroadcastUDP(specialMove.data, source);
                            break;
                        default:
                            break;
                    }
                    break;
                #endregion
                case DatagramID.dummy:
                    break;
                default:
                    Console.WriteLine("unknown DatagramID: " + datagram[0]);
                    break;
            }
        }

        public void Load_world_delayed(Player player) {
            try {
                worldUpdate.Write(player.writer, true);
            } catch { }

        }
        public void Poison(Player target, Attack attack, byte iteration = 0) {
            if (iteration < 7 && players.ContainsValue(target) && target.playing) {
                SendUDP(attack.data, target);
                iteration++;
                Task.Delay(500).ContinueWith(t => Poison(target, attack, iteration));
            }
        }

        public void Ban(ushort guid) {
            var player = players[guid];
            var banEntry = new string[4];
            banEntry[(byte)BanEntry.name] = player.entityData.name;
            banEntry[(byte)BanEntry.ip] = player.entityData.name;
            banEntry[(byte)BanEntry.mac] = player.MAC;
            banEntry[(byte)BanEntry.reason] = "ban_message_default";
            bans.Add(banEntry);
            player.tcp.Close();
            File.WriteAllText(bansFilePath, JsonConvert.SerializeObject(bans));
        }
    }
}
