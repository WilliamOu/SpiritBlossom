using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SpiritBlossom.Items;

namespace SpiritBlossom.Projectiles
{
    public class SpiritCleaveWave : ModProjectile
    {
        private float holdoutRange = 135f;
        private int frameCount = 21;
        private int ticksPerFrame = 2;
        private int currentFrame = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
            Main.projFrames[Projectile.type] = frameCount;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = 0;
            Projectile.scale = 1.25f;
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

        public override bool PreDraw(ref Color lightColor)
        {
            return SBUtils.DrawFrame(this.Projectile, frameCount, ticksPerFrame);
        }

        public override bool PreAI()
        {
            currentFrame++;
            Player player = Main.player[Projectile.owner];

            if (++Projectile.frameCounter % ticksPerFrame == 0)
            {
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }

            Projectile.velocity = Vector2.Normalize(Projectile.velocity);
            Projectile.Center = player.MountedCenter + Projectile.velocity * holdoutRange;
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);

            SBUtils.PrintCurrentFrame(currentFrame);
            return true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override void AI()
        {
            // Makes the projectile glow
            Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);
        }
    }
}