using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Commands.ChatCommands.Add(new Command("pylon", Manage, "pylon", "py", "晶塔") { HelpText = "指令晶塔" });
        }

        public override void Initialize()
        {
        }

        void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;

            void Help()
            {
                op.SendInfoMessage("/py <晶塔名称>, 将你传送到指定晶塔\n" +
                    "有效的晶塔名称：\n" +
                    "1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11\n" +
                    "森林, 雪原, 沙漠, 洞穴, 海洋, 丛林, 神圣, 蘑菇, 地狱, 以太, 万能\n" +
                    "f, s, d, c, o, j, h, m, u, a, uni\n" +
                    "forest, snow, desert, cavern, ocean, jungle, hallow, mushroom, underworld, aether, universal"
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
                // 放置晶塔
                case "place":
                case "p":
                    Place(args);
                    return;

                case "fix":
                    FixPylonError(args);
                    return;

                case "help": Help(); return;

                // 找箱子
                case "find":
                    args.Parameters.RemoveAt(0);
                    ShowMe(args);
                    return;

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

            List<string> map = [
                "[i:4876]森林",
                "[i:4875]丛林",
                "[i:4916]神圣",
                "[i:4917]洞穴",
                "[i:4918]海洋",
                "[i:4919]沙漠",
                "[i:4920]雪原",
                "[i:4921]蘑菇",
                "[i:4951]万能",
                "[i:5652]地狱",
                "[i:5653]以太",
            ];

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

        static int GetPylonType(string text)
        {
            return text switch
            {
                "1" or "森林" or "f" or "forest" => (int)TeleportPylonType.SurfacePurity,
                "2" or "雪原" or "s" or "snow" => (int)TeleportPylonType.Snow,
                "3" or "沙漠" or "d" or "desert" => (int)TeleportPylonType.Desert,
                "4" or "洞穴" or "c" or "cavern" => (int)TeleportPylonType.Underground,
                "5" or "海洋" or "o" or "ocean" => (int)TeleportPylonType.Beach,
                "6" or "丛林" or "j" or "jungle" => (int)TeleportPylonType.Jungle,
                "7" or "神圣" or "h" or "hallow" => (int)TeleportPylonType.Hallow,
                "8" or "蘑菇" or "m" or "mushroom" => (int)TeleportPylonType.GlowingMushroom,
                "9" or "地狱" or "u" or "underworld" => (int)TeleportPylonType.Underworld,
                "10" or "以太" or "a" or "aether" => (int)TeleportPylonType.Shimmer,
                "11" or "万能" or "uni" or "universal" => (int)TeleportPylonType.Victory,
                _ => -1,
            };
        }

        /// <summary>
        /// 附近是否有晶塔
        /// </summary>
        static bool NearHasPylon(TSPlayer op)
        {
            Rectangle rect = GetScreen(op);
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

        #region 放置晶塔
        static void Place(CommandArgs args)
        {
            if (!args.Player.HasPermission("pylon.place"))
            {
                args.Player.SendErrorMessage("你没有放置晶塔的权限！");
                return;
            }
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
        #endregion

        #region 修复晶塔错误
        static void FixPylonError(CommandArgs args)
        {
            int count = 0;
            foreach (TileEntity value in TileEntity.ByPosition.Values)
            {
                if (value is TETeleportationPylon obj)
                {
                    var rx = obj.Position.X;
                    var ry = obj.Position.Y;
                    ITile tile = Main.tile[rx, ry];
                    if (!tile.active() || tile.type != 597)
                    {
                        TileEntity.ByPosition.Remove(obj.Position);
                        TileEntity.ByID.Remove(obj.ID);
                        NetMessage.SendData(86, -1, -1, null, obj.ID, rx, ry);
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
        #endregion

        #region 找箱子
        // 物品名
        private static void ShowMe(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (!NearHasPylon(op))
            {
                op.SendErrorMessage("附近需要有任意一种晶塔，才能进行查找！");
                return;
            }

            if (args.Parameters.Count == 0)
            {
                op.SendInfoMessage("语法不正确，输入 /py find <物品名称/id> [箱子编号] 查找箱子里的物品！");
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                default:
                    int selectNum = 0;
                    if (args.Parameters.Count >= 2) _ = int.TryParse(args.Parameters[1], out selectNum);
                    FindNearChest(op, args.Parameters[0], selectNum);
                    return;
            }
        }

        private static void FindNearChest(TSPlayer op, string itemNameOrID, int selectNum = 0)
        {
            Item item;
            if (int.TryParse(itemNameOrID, out int id))
            {
                if (id == 0)
                {
                    op.SendInfoMessage("物品名输入有误！");
                    return;
                }
                item = new Item();
                item.netDefaults(id);
            }
            else
            {
                List<Item> found = TShock.Utils.GetItemByIdOrName(itemNameOrID);
                if (found.Count == 0)
                {
                    op.SendInfoMessage("物品名输入有误！");
                    return;
                }
                else if (found.Count > 1)
                {
                    op.SendMultipleMatchError(found.Select(i => $"{i.Name}({i.type})"));
                    return;
                }
                item = found[0];
            }

            // 所有晶塔附近一屏区域
            List<Rectangle> areas = [];
            foreach (TileEntity value in TileEntity.ByPosition.Values)
            {
                if (value is TETeleportationPylon obj)
                {
                    var rx = obj.Position.X;
                    var ry = obj.Position.Y;
                    Rectangle area = new(rx - 61, ry - 34 + 3, 122, 68);
                    areas.Add(area);
                }
            }

            int total = 0;
            List<Chest> chests = [];
            foreach (Chest ch in Main.chest.Where(ch => ch != null))
            {
                if (!InMultiArea(areas, ch.x, ch.y))
                    continue;

                int stack = 0;
                foreach (Item item2 in ch.item.Where(item2 => item2 != null && item2.active && item2.type == item.type))
                {
                    stack += item2.stack;
                }
                if (stack == 0) continue;
                chests.Add(ch);
                total += stack;
            }
            if (total == 0)
            {
                op.SendInfoMessage($"所有晶塔附近的箱子里都没有 [i:{item.type}]{item.Name}");
                return;
            }

            if (selectNum <= 0)
                selectNum = 1;
            else if (selectNum > chests.Count)
                selectNum = chests.Count;

            Chest ch2 = chests[selectNum - 1];
            if (op.RealPlayer) op.Teleport(ch2.x * 16, (ch2.y - 2) * 16);
            op.SendInfoMessage($"在所有晶塔附近找到 {chests.Count}个箱子 存放了 [i:{item.type}]{item.Name}，共计{total}件");
        }

        private static bool InMultiArea(List<Rectangle> rects, int x, int y)
            => rects.Any(rect => InArea(rect, x, y));

        private static bool InArea(Rectangle rect, int x, int y)
        {
            return x >= rect.X && x <= rect.X + rect.Width && y >= rect.Y && y <= rect.Y + rect.Height;
        }
        #endregion

        #region 通用方法
        /// <summary>
        /// 日志
        /// </summary>
        public static void Log(string msg) { TShock.Log.ConsoleInfo($"[pylon]：{msg}"); }
        public static Rectangle GetScreen(TSPlayer op) { return GetScreen(op.TileX, op.TileY); }
        public static Rectangle GetScreen(int playerX, int playerY) { return new Rectangle(playerX - 61, playerY - 34 + 3, 122, 68); }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

    }
}
