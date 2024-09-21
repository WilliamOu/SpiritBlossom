using static Terraria.ModLoader.ModContent;
using SpiritBlossom.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SpiritBlossom.Items;
using System;
using Terraria.Audio;

namespace SpiritBlossom.Buffs
{
    public class FateSealedBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();

            int buffTime = player.buffTime[buffIndex];

            if (buffTime == 21)
            {
                sbPlayer.ThreePointCalculation(player);
                sbPlayer.FateSealedBlink.Set(null, player, sbPlayer.FateSealedBlinkDirection, sbPlayer.FateSealedCastPosition, 0, 1, buffTime - 1, sbPlayer.FateSealedAdjustedBlinkDistance, false, true, ref sbPlayer.FateSealedFrame);
            }

            sbPlayer.FateSealedBlink.Dash(ref sbPlayer.FateSealedFrame);

            if (buffTime == 1)
            {
                player.AddBuff(BuffType<Buffs.FateSealedCooldown>(), SpiritBlossomPlayer.FateSealedCooldown);
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}