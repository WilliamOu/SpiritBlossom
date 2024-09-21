using static Terraria.ModLoader.ModContent;
using SpiritBlossom.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SpiritBlossom.Items;
using System;
using Terraria.Audio;
using CalamityMod;

namespace SpiritBlossom.Buffs
{
    public class SoulUnboundBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();

            /*player.wingTime = player.wingTimeMax;
            if (ModLoader.HasMod("CalamityMod"))
            {
                CalamityCompatibleFlight(player);
            }*/

            int buffTime = player.buffTime[buffIndex];

            if (buffTime == 120)
            {
                SoundEngine.PlaySound(sbPlayer.EWarning with { Volume = SBUtils.GlobalSFXVolume });
            }
            if (SoundEngine.TryGetActiveSound(sbPlayer.ETetherSlot, out sbPlayer.ETetherSound))
            {
                sbPlayer.ETetherSound.Position = player.Center;
            }

            float movementSpeedBonus = MathHelper.Lerp(SpiritBlossomPlayer.SoulUnboundMinMovementSpeedBonus, SpiritBlossomPlayer.SoulUnboundMaxMovementSpeedBonus, sbPlayer.SoulUnboundFrame / SpiritBlossomPlayer.SoulUnboundDuration);

            player.moveSpeed += movementSpeedBonus;

            sbPlayer.SoulUnboundDash.Dash(ref sbPlayer.SoulUnboundFrame);

            if (buffTime == 1)
            {
                if (player.HasBuff(BuffType<FateSealedBuff>()) || player.HasBuff(BuffType<MortalSteelBuff>()))
                {
                    player.buffTime[buffIndex]++;
                    // player.AddBuff(BuffType<SoulUnboundBuff>(), 2);
                    return;
                }

                sbPlayer.RecastSoulUnbound(player);
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }

        [JITWhenModsEnabled("CalamityMod")]
        private void CalamityCompatibleFlight(Player player)
        {
            player.Calamity().infiniteFlight = true;
        }
    }
}