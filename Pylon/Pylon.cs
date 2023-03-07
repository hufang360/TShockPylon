using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace Pylon
{
    [ApiVersion(2, 1)]
    public class Pylon : TerrariaPlugin
    {

        public override string Description => "指令晶塔";
        public override string Name => "指令晶塔";
        public override string Author => "hufang360";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;


        public Pylon(Main game) : base(game)
        {
            Commands.ChatCommands.Add(new Command("pylon", PYCommand, "pylon", "py", "晶塔") { HelpText = "指令晶塔" });
        }

        public override void Initialize()
        {
        }

        void PYCommand(CommandArgs args)
        {
            TSPlayer op = args.Player;

            void Help()
            {
                op.SendInfoMessage("/py <晶塔名称>, 将你传送到指定晶塔\n" +
                    "有效的晶塔名称：\n" +
                    "1, 2, 3, 4, 5, 6, 7, 8, 9\n" +
                    "森林, 雪原, 沙漠, 洞穴, 海洋, 丛林, 神圣, 蘑菇, 万能\n" +
                    "f, s, d, c, o, j, h, m, u\n" +
                    "forest, snow, desert, cavern, ocean, jungle, hallow, mushroom, universal"
                    );
            }

            if (args.Parameters.Count == 0)
            {
                Help();
                return;
            }

            string kw = args.Parameters[0].ToLowerInvariant().Replace("晶塔", "");
            int pyType;
            switch (kw)
            {
                case "place":
                case "p":
                    Place(args);
                    return;

                case "fix":
                    FixPylonError(args);
                    return;

                case "help": Help(); return;

                default:
                    pyType = GetPylonType(kw);
                    if (pyType == -1)
                    {
                        op.SendErrorMessage("无效的晶塔名称！");
                        Help();
                        return;
                    }
                    break;
            }

            List<string> map = new() {
                "[i:4876]森林",
                "[i:4875]丛林",
                "[i:4916]神圣",
                "[i:4917]洞穴",
                "[i:4918]海洋",
                "[i:4919]沙漠",
                "[i:4920]雪原",
                "[i:4921]蘑菇",
                "[i:4951]万能",
            };

            if (!NearHasPylon(op))
            {
                op.SendErrorMessage($"附近需要有任意一种晶塔，才能传送到 {map[pyType]}晶塔！");
                return;
            }

            List<TeleportPylonInfo> pylons = Main.PylonSystem.Pylons;
            for (int i = 0; i < pylons.Count; i++)
            {
                TeleportPylonInfo info = pylons[i];
                if ((int)info.TypeOfPylon == pyType)
                {
                    op.Teleport(info.PositionInTiles.X * 16, info.PositionInTiles.Y * 16);
                    op.SendInfoMessage($"已将你传送至 {map[pyType]}晶塔");
                    return;
                }
            }
            op.SendErrorMessage($"未找到 {map[pyType]}晶塔");
        }

        int GetPylonType(string text)
        {
            switch (text)
            {
                case "1": case "森林": case "f": case "forest": return (int)TeleportPylonType.SurfacePurity;
                case "2": case "雪原": case "s": case "snow": return (int)TeleportPylonType.Snow;
                case "3": case "沙漠": case "d": case "desert": return (int)TeleportPylonType.Desert;
                case "4": case "洞穴": case "c": case "cavern": return (int)TeleportPylonType.Underground;
                case "5": case "海洋": case "o": case "ocean": return (int)TeleportPylonType.Beach;
                case "6": case "丛林": case "j": case "jungle": return (int)TeleportPylonType.Jungle;
                case "7": case "神圣": case "h": case "hallow": return (int)TeleportPylonType.Hallow;
                case "8": case "蘑菇": case "m": case "mushroom": return (int)TeleportPylonType.GlowingMushroom;
                case "9": case "万能": case "u": case "universal": return (int)TeleportPylonType.Victory;
                default: return -1;
            }
        }

        /// <summary>
        /// 附近是否有晶塔
        /// </summary>
        bool NearHasPylon(TSPlayer op)
        {
            Rectangle rect = utils.GetScreen(op);
            for (int x = rect.X; x < rect.Right; x++)
            {
                for (int y = rect.Y; y < rect.Bottom; y++)
                {
                    if (Main.tile[x, y].type == TileID.TeleportationPylon)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void Place(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("请输入要放置的晶塔名称，例如：/py place 丛林晶塔，名称还可用1~9代替。");
                return;
            }

            int pyType = GetPylonType(args.Parameters[0].ToLowerInvariant());
            if (pyType == -1)
            {
                args.Player.SendErrorMessage("无效的晶塔名称！");
                return;
            }

            var curX = args.Player.TileX;
            var curY = args.Player.TileY + 3;
            var rect = new Rectangle(curX - 1, curY - 4, 3, 4);
            var emptyCount = 0;
            var underCount = 0;
            for (int rx = rect.Left; rx < rect.Right; rx++)
            {
                for (int ry = rect.Top; ry < rect.Bottom; ry++)
                {
                    ITile tile = Main.tile[rx, ry];
                    if (!tile.active())
                    {
                        emptyCount++;
                    }

                    // 脚下一格
                    if (ry == rect.Bottom - 1)
                    {
                        tile = Main.tile[rx, ry + 1];
                        if (tile.active())
                        {
                            underCount++;
                        }
                    }
                }
            }
            if (emptyCount == 12 && underCount == 3)
            {
                var cx = args.Player.TileX;
                var cy = args.Player.TileY + 2;
                WorldGen.Place3x4(cx, cy, 597, pyType);
                NetMessage.SendTileSquare(-1, cx, cy, 4);
                var index = TETeleportationPylon.Place(cx, cy);
                if (index != -1)
                {
                    NetMessage.SendData(86, -1, -1, null, index, cx, cy);
                }
                else
                {
                    args.Player.SendErrorMessage("放置晶塔失败！");
                }
            }
            else
            {
                args.Player.SendErrorMessage("你附近3x4的区域有物体，请前往空地！");
            }
            Netplay.ResetSections();
        }


        void FixPylonError(CommandArgs args)
        {
            int count = 0;
            foreach (TileEntity value in TileEntity.ByPosition.Values)
            {
                TETeleportationPylon tETeleportationPylon = value as TETeleportationPylon;
                if (tETeleportationPylon != null)
                {
                    var rx = tETeleportationPylon.Position.X;
                    var ry = tETeleportationPylon.Position.Y;
                    ITile tile = Main.tile[rx, ry];
                    if (!tile.active() || tile.type != 597)
                    {
                        TileEntity.ByPosition.Remove(tETeleportationPylon.Position);
                        TileEntity.ByID.Remove(tETeleportationPylon.ID);
                        NetMessage.SendData(86, -1, -1, null, tETeleportationPylon.ID, rx, ry);
                        count++;
                    }
                }
            }
            if (count > 0)
            {
                Main.PylonSystem.RequestImmediateUpdate();
            }
            args.Player.SendSuccessMessage($"已清除{count}个错误的晶塔信息");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

    }

    public class utils
    {
        /// <summary>
        /// 日志
        /// </summary>
        public static void Log(string msg) { TShock.Log.ConsoleInfo($"[pylon]：{msg}"); }
        public static Rectangle GetScreen(TSPlayer op) { return GetScreen(op.TileX, op.TileY); }
        public static Rectangle GetScreen(int playerX, int playerY) { return new Rectangle(playerX - 61, playerY - 34 + 3, 122, 68); }
    }
}
