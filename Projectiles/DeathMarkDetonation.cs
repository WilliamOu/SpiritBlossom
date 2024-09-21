using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SpiritBlossom.Items;
using ReLogic.Content;

namespace SpiritBlossom.Projectiles
{
    public class DeathMarkDetonation : ModProjectile
    {
        private static Texture2D primaryDetonation;
        private static Texture2D centralDetonation;

        private int frameCount = 13;
        private int ticksPerFrame = 1;
        private int currentFrame = 0;
        private Vector2 initialPosition;

        private const float centralDetonationScale = 0.3f;

        private static void LoadTextures()
        {
            if (centralDetonation == null)
            {
                centralDetonation = ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/SoulUnbound/DeathMarkCentralDetonation", AssetRequestMode.ImmediateLoad).Value;
            }
            if (primaryDetonation == null)
            {
                primaryDetonation = ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/SoulUnbound/DeathMarkPrimaryDetonation", AssetRequestMode.ImmediateLoad).Value;
            }
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = frameCount;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = 0;
            Projectile.scale = SpiritBlossomPlayer.SoulUnboundDeathMarkSpriteScale;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = frameCount * ticksPerFrame;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void Load()
        {
            LoadTextures();
        }

        public override bool PreAI()
        {
            currentFrame++;

            /*if (currentFrame == 1) { initialPosition = Projectile.position; }
            Projectile.position = initialPosition;*/
            Projectile.position -= Projectile.velocity;

            if (++Projectile.frameCounter % ticksPerFrame == 0)
            {
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }

            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);

            SBUtils.PrintCurrentFrame(currentFrame);
            return true;
        }

        public override void AI()
        {
            return;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            LoadTextures();
            SBUtils.DrawFrame(Projectile.position, 0, centralDetonationScale, centralDetonation, currentFrame - 1, ticksPerFrame, Color.White, false, 4, 3);
            SBUtils.DrawFrame(Projectile.position + new Vector2(0, SpiritBlossomPlayer.SoulUnboundDeathMarkVerticalDrawOffset), 0, SpiritBlossomPlayer.SoulUnboundDeathMarkSpriteScale, primaryDetonation, currentFrame - 1, ticksPerFrame, Color.White, false, 4, 3);
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
    }
}