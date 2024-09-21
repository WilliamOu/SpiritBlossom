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
    public class MortalSteelBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();

            int buffTime = player.buffTime[buffIndex];

            sbPlayer.MortalSteelDash.Dash(ref sbPlayer.MortalSteelFrame);

            FadeOutGatheringStormReadySound(sbPlayer);

            if (buffTime == 1)
            {
                player.AddBuff(BuffType<Buffs.MortalSteelCooldown>(), SpiritBlossomPlayer.MortalSteelCooldown);
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }

        private void FadeOutGatheringStormReadySound(SpiritBlossomPlayer sbPlayer)
        {
            if (!SoundEngine.TryGetActiveSound(sbPlayer.Q3ReadySlot, out sbPlayer.Q3ReadySound)) { return; }

            sbPlayer.Q3ReadySound.Volume -= SBUtils.GlobalSFXVolume / 8f;
        }
    }
}