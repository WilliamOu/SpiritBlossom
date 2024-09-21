using SpiritBlossom.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SpiritBlossom.Items;
using System;
using Terraria.Audio;

namespace SpiritBlossom.Buffs
{
    public class SoulUnboundRecast : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();

            int buffTime = player.buffTime[buffIndex];

            if (SoundEngine.TryGetActiveSound(sbPlayer.ETetherSlot, out sbPlayer.ETetherSound))
            {
                sbPlayer.ETetherSound.Position = player.Center;

                if (buffTime < 4) { sbPlayer.ETetherSound.Volume = 0; }
                else { sbPlayer.ETetherSound.Volume -= SBUtils.GlobalSFXVolume / 4f; }
            }

            sbPlayer.SoulUnboundDash.DashToPosition(sbPlayer.SoulUnboundClone.position, ref sbPlayer.SoulUnboundRecastFrame);

            if (buffTime == 1)
            {
                sbPlayer.EndSoulUnbound(player);
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}