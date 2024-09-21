using SpiritBlossom.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SpiritBlossom.Items;
using System;
using Terraria.Audio;

namespace SpiritBlossom.Buffs
{
    public class GatheringStormReady : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();
            if (SoundEngine.TryGetActiveSound(sbPlayer.Q3ReadySlot, out sbPlayer.Q3ReadySound)) { sbPlayer.Q3ReadySound.Position = player.Center; }

            RenderChargeDust(player);
        }

        private void RenderChargeDust(Player player)
        {
            // Charge Dust Code, credit to ThePaperLuigi
            Vector2 vector = new Vector2(
                Main.rand.Next(-10, 10) * (0.003f * 40 - 10),
                Main.rand.Next(-10, 10) * (0.003f * 40 - 10));
            Dust d = Main.dust[Dust.NewDust(
                player.MountedCenter + vector, 1, 1,
                43, 0, 0, 255,
                new Color(0.8f, 0.4f, 1f), 1f)];
            d.velocity = -vector / 12;
            d.velocity -= player.velocity / 8;
            d.noLight = true;
            d.noGravity = true;
        }
    }
}