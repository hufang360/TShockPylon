# 指令晶塔

当因 NPC不在、boss战中、事件入侵中，无法使用使用晶塔时，可以使用指令晶塔进行传送。

> 泰拉 1.4.5.x 已允许在 boss战 和 事件入侵中 使用晶塔


## 基础使用
```
/pylon <晶塔名称>, 将你传送至对应晶塔
/pylon 可以简写成 /py, 或者写成 /晶塔

/py place <晶塔名称>, 放置晶塔（高级权限）

/py find <物品名称>, 在所有晶塔附近查找箱子里的物品
```

**有效的晶塔名称：**
```
1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11
森林, 雪原, 沙漠, 洞穴, 海洋, 丛林, 神圣, 蘑菇, 地狱, 以太, 万能
f, s, d, c, o, j, h, m, u, a, uni
forest, snow, desert, cavern, ocean, jungle, hallow, mushroom, underworld, aether, universal
```

## 传送要求

1、要传送的晶塔必须存在；

2、玩家附近需要有任意一种晶塔；


## 权限

授权示意：
```bash
/group addperm default pylon
```

| 权限 | 说明 |
| --- | --- |
| pylon | 主权限 |
| pylon.place | 高级权限，允许使用 `/py place` 指令 |


<br/>