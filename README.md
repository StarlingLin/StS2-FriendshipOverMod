# 杀戮尖塔2友尽对决模组

卡图与创意来自[BiliBili@一只冰茄子](https://www.bilibili.com/video/BV1SqX4B4Eiq/)。

需安装前置模组BaseLib。

目前完成卡牌如下，欢迎提出改进建议等：

1. 自爆：失去{SelfHpLoss:diff()}点生命。\n对所有敌人造成{EnemyDamage:diff()}点伤害。\n对所有队友造成{AllyDamage:diff()}点伤害。
2. 混沌汲取：随机一名队友失去{LoseStrength:diff()}点[gold]力量[/gold]和{LoseDexterity:diff()}点[gold]敏捷[/gold]。\n你获得{GainStrength:diff()}点[gold]力量[/gold]和{GainDexterity:diff()}点[gold]敏捷[/gold]。
3. 王车易位：交换你和选定队友的[gold]格挡[/gold]。{IfUpgraded:show:\n返还队友一半的[gold]格挡[/gold]。|}

# StS2-FriendshipOverMod

A **Slay the Spire 2** mod themed around chaotic multiplayer card interactions.

Card art and original ideas are based on content by [BiliBili @ 一只冰茄子](https://www.bilibili.com/video/BV1SqX4B4Eiq/).

## Requirement

This mod requires **BaseLib** to be installed.

## Current Cards

The following cards have been implemented so far. Feedback and suggestions are welcome.

1. **Self-Destruct**  
   Lose {SelfHpLoss:diff()} HP.  
   Deal {EnemyDamage:diff()} damage to all enemies.  
   Deal {AllyDamage:diff()} damage to all allies.

2. **Chaos Drain**  
   A random ally loses {LoseStrength:diff()} [gold]Strength[/gold] and {LoseDexterity:diff()} [gold]Dexterity[/gold].  
   Gain {GainStrength:diff()} [gold]Strength[/gold] and {GainDexterity:diff()} [gold]Dexterity[/gold].

3. **Castling**  
   Swap your [gold]Block[/gold] with the chosen ally's [gold]Block[/gold].  
   {IfUpgraded:show:Refund half of that ally's [gold]Block[/gold].|}
