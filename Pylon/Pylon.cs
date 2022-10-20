using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
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
            TeleportPylonType pyType;
            switch (kw)
            {
                case "1": case "森林": case "f": case "forest": pyType = TeleportPylonType.SurfacePurity; break;
                case "2": case "雪原": case "s": case "snow": pyType = TeleportPylonType.Snow; break;
                case "3": case "沙漠": case "d": case "desert": pyType = TeleportPylonType.Desert; break;
                case "4": case "洞穴": case "c": case "cavern": pyType = TeleportPylonType.Underground; break;
                case "5": case "海洋": case "o": case "ocean": pyType = TeleportPylonType.Beach; break;
                case "6": case "丛林": case "j": case "jungle": pyType = TeleportPylonType.Jungle; break;
                case "7": case "神圣": case "h": case "hallow": pyType = TeleportPylonType.Hallow; break;
                case "8": case "蘑菇": case "m": case "mushroom": pyType = TeleportPylonType.GlowingMushroom; break;
                case "9": case "万能": case "u": case "universal": pyType = TeleportPylonType.Victory; break;

                case "help": Help(); return;
                default: op.SendErrorMessage("无效的晶塔名称！"); Help(); return;
            }

            Dictionary<TeleportPylonType, string> map = new Dictionary<TeleportPylonType, string>() {
                {TeleportPylonType.SurfacePurity, "[i:4876]森林" },
                {TeleportPylonType.Jungle, "[i:4875]丛林" },
                {TeleportPylonType.Hallow, "[i:4916]神圣" },
                {TeleportPylonType.Underground, "[i:4917]洞穴" },
                {TeleportPylonType.Beach, "[i:4918]海洋" },
                {TeleportPylonType.Desert, "[i:4919]沙漠" },
                {TeleportPylonType.Snow, "[i:4920]雪原" },
                {TeleportPylonType.GlowingMushroom, "[i:4921]蘑菇" },
                {TeleportPylonType.Victory, "[i:4951]万能" },
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
                if (info.TypeOfPylon == pyType)
                {
                    op.Teleport(info.PositionInTiles.X * 16, info.PositionInTiles.Y * 16);
                    op.SendInfoMessage($"已将你传送至 {map[pyType]}晶塔");
                    return;
                }
            }
            op.SendErrorMessage($"未找到 {map[pyType]}晶塔");
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
