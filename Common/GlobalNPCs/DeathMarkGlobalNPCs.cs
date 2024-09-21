using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using SpiritBlossom.Items;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using SpiritBlossom.Projectiles;
using Terraria.ID;
using ReLogic.Content;
using System.Runtime.CompilerServices;

namespace SpiritBlossom.Common.GlobalNPCs
{
    internal class DeathMarkGlobalNPCs : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public static Texture2D StandardMarkFormation;
        public static Texture2D StandardMarkLoop;
        public static Texture2D ExecuteMarkFormation;
        public static Texture2D ExecuteMarkLoop;
        public Player MarkApplier;
        public float StoredDamage;
        public int StandardFrame;
        public int ExecuteFrame;
        public bool Detonate;

        private const int standardMarkFormationFrameCount = 39;
        private const int standardMarkLoopFrameCount = 80;
        private const int executeMarkFormationFrameCount = 13;
        private const int executeMarkLoopFrameCount = 286;

        private static void LoadStandardFormationTexture()
        {
            if (StandardMarkFormation == null)
            {
                StandardMarkFormation = ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/SoulUnbound/DeathMarkFormation", AssetRequestMode.ImmediateLoad).Value;
            }
        }

        private static void LoadStandardTexture()
        {
            if (StandardMarkLoop == null)
            {
                StandardMarkLoop = ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/SoulUnbound/DeathMarkLoop", AssetRequestMode.ImmediateLoad).Value;
            }
        }

        private static void LoadExecuteFormationTexture()
        {
            if (ExecuteMarkFormation == null)
            {
                ExecuteMarkFormation = ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/SoulUnbound/DeathMarkExecutionThresholdFormation", AssetRequestMode.ImmediateLoad).Value;
            }
        }

        private static void LoadExecuteTexture()
        {
            if (ExecuteMarkLoop == null)
            {
                ExecuteMarkLoop = ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/SoulUnbound/DeathMarkExecutionThreshold", AssetRequestMode.ImmediateLoad).Value;
            }
        }

        public override void Load()
        {
            LoadStandardFormationTexture();
            LoadStandardTexture();
            LoadExecuteTexture();
        }

        public void Initialize(Player player, NPC npc)
        {
            StoredDamage = 0;
            StandardFrame = 0;
            ExecuteFrame = 0;
            Detonate = false;
            MarkApplier = player;
            npc.netUpdate = true;
        }

        public void StackDamage(float damage, NPC npc)
        {
            StoredDamage += damage;
            npc.netUpdate = true;
        }

        public void DetonateMark(NPC npc)
        {
            Detonate = true;
            npc.netUpdate = true;
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!npc.HasBuff(BuffType<Buffs.DeathMark>())) { return; }

            Vector2 drawOffset = new Vector2(0, SpiritBlossomPlayer.SoulUnboundDeathMarkVerticalDrawOffset);

            if (npc.life > StoredDamage)
            {
                ExecuteFrame = 0;
                if (StandardFrame < standardMarkFormationFrameCount)
                {
                    LoadStandardFormationTexture();
                    SBUtils.DrawFrame(npc.Center + drawOffset, 0, SpiritBlossomPlayer.SoulUnboundDeathMarkSpriteScale, StandardMarkFormation, StandardFrame, 1, Color.White, false, 7, 6);
                }
                else
                {
                    LoadStandardTexture();
                    int frame = (StandardFrame - standardMarkFormationFrameCount) % standardMarkLoopFrameCount;
                    SBUtils.DrawFrame(npc.Center + drawOffset, 0, SpiritBlossomPlayer.SoulUnboundDeathMarkSpriteScale, StandardMarkLoop, frame, 1, Color.White, false, 9, 9);
                }
                StandardFrame++;
            }
            else
            {
                StandardFrame = standardMarkFormationFrameCount;
                if (ExecuteFrame < executeMarkFormationFrameCount)
                {
                    LoadExecuteFormationTexture();
                    SBUtils.DrawFrame(npc.Center + drawOffset, 0, SpiritBlossomPlayer.SoulUnboundDeathMarkSpriteScale, ExecuteMarkFormation, ExecuteFrame, 1, Color.White, false, 1, executeMarkFormationFrameCount);
                }
                else
                {
                    LoadExecuteTexture();
                    SBUtils.DrawFrame(npc.Center + drawOffset, 0, SpiritBlossomPlayer.SoulUnboundDeathMarkSpriteScale, ExecuteMarkLoop, ExecuteFrame, 1, Color.White, false, 17, 17);
                }
                ExecuteFrame++;
            }
        }
    }
}