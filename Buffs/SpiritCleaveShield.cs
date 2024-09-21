using SpiritBlossom.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SpiritBlossom.Items;
using System;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace SpiritBlossom.Buffs
{
    public class SpiritCleaveShield : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();

            if (sbPlayer.CurrentShieldHealth <= 0) { player.buffTime[buffIndex] = 1; }

            int buffTime = player.buffTime[buffIndex];

            if (buffTime == 1)
            {
                sbPlayer.ClearSpiritCleaveShield(player);
                sbPlayer.ShieldFrame = 91;
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}