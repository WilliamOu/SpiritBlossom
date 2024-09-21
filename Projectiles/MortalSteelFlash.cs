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
    public class MortalSteelFlash : ModProjectile
    {
        private static Texture2D flash;
        private static Texture2D spark;

        private float holdoutRange = 320f;
        private int frameCount = 40;
        private int ticksPerFrame = 1;
        private int currentFrame = 0;

        private int sparkFrameCount = 9;
        private int blankFrameCount = 0;

        private static void LoadTextures()
        {
            if (spark == null)
            {
                spark = ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/MortalSteelSpark", AssetRequestMode.ImmediateLoad).Value;
            }
            if (flash == null)
            {
                flash = ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/MortalSteelFlash", AssetRequestMode.ImmediateLoad).Value;
            }
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = frameCount;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = 0;
            Projectile.scale = 0.4f;
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

            holdoutRange *= Projectile.scale;
        }

        public override void Load()
        {
            LoadTextures();
        }

        public override bool PreAI()
        {
            currentFrame++;

            Player player = Main.player[Projectile.owner];

            Vector2 holdout = Projectile.velocity * holdoutRange;
            Projectile.position = player.MountedCenter + holdout;

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
            Player player = Main.player[Projectile.owner];
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();

            LoadTextures();
            bool flipIfFacingLeft = (Projectile.spriteDirection == -1) ? true : false;
            int flashFrameStart = sparkFrameCount + blankFrameCount;
            if (currentFrame > flashFrameStart)
            {
                SBUtils.DrawFrame(Projectile.position, Projectile.rotation, 1.3f * Projectile.scale, flash, currentFrame - flashFrameStart - 1, ticksPerFrame, Color.White, flipIfFacingLeft, 1, 30);
            }
            else if (currentFrame <= sparkFrameCount)
            {
                SBUtils.DrawFrame(Projectile.position, Projectile.rotation, Projectile.scale, spark, currentFrame - 1, ticksPerFrame, Color.White, flipIfFacingLeft, 1, 9);
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
    }
}