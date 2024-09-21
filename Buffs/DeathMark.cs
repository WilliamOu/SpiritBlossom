using static Terraria.ModLoader.ModContent;
using SpiritBlossom.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SpiritBlossom.Items;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using SpiritBlossom.Projectiles;
using System;
using Terraria.ID;

namespace SpiritBlossom.Buffs
{
    public class DeathMark : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            DeathMarkGlobalNPCs globalNPC = npc.GetGlobalNPC<DeathMarkGlobalNPCs>();
            SpiritBlossomPlayer sbPlayer = globalNPC.MarkApplier.GetModPlayer<SpiritBlossomPlayer>();
            if (globalNPC.Detonate)
            {
                npc.buffTime[buffIndex] = 1;
            }

            if (npc.buffTime[buffIndex] == 1)
            {
                npc.DelBuff(buffIndex);
                buffIndex--;
                DetonateMark(npc, globalNPC);
                return;
            }
        }

        private void DetonateMark(NPC npc, DeathMarkGlobalNPCs globalNPC)
        {
            Projectile.NewProjectile(globalNPC.MarkApplier.GetSource_ItemUse(globalNPC.MarkApplier.HeldItem), npc.Center, new Vector2(1f, 0f), ProjectileType<DeathMarkDetonation>(), 0, 0, globalNPC.MarkApplier.whoAmI);

            int damage = (int)Math.Ceiling(globalNPC.StoredDamage);

            NPC.HitInfo hitInfo = new NPC.HitInfo
            {
                Damage = damage,
                Knockback = 0f,
                HitDirection = 0,
                Crit = false
            };

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.StrikeNPC(hitInfo, noPlayerInteraction: false, fromNet: false);

                NetMessage.SendStrikeNPC(npc, hitInfo);
            }
        }
    }
}