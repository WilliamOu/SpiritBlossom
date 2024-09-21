using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SpiritBlossom.Common.GlobalNPCs;
using SpiritBlossom.Items;
using static Terraria.ModLoader.ModContent;
using static tModPorter.ProgressUpdate;

namespace SpiritBlossom.Projectiles
{
    public class MortalSteelWave : ModProjectile
    {
        private float offsetMin = 0f;
        private float offsetMax = 0f;
        private float progress = 0;
        private float initialOffset = 210f;
        private float initialHitboxOffset = -80f;
        private float hitboxIncrementPerTick = 15.5f;
        private float projectileLength = 200f;
        // private float dashDistancePerTick = 23f;
        private int frameCount = 40;
        private int ticksPerFrame = 2;
        private int currentFrame = 0;
        private int activeFrameStart = 8;
        private int activeFrameEnd = 26;
        private int dashFrameStart = 8;

        private Vector2 startingPosition;
        private float hitboxOffset;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
            Main.projFrames[Projectile.type] = frameCount;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = 0;
            Projectile.scale = 1f;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = frameCount * ticksPerFrame;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;

            offsetMin *= Projectile.scale;
            offsetMax *= Projectile.scale;
            initialOffset *= Projectile.scale;
            projectileLength *= Projectile.scale;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Vector2 hitboxStartPosition = startingPosition + Projectile.velocity * hitboxOffset;
            Vector2 hitboxEndPosition = hitboxStartPosition + Projectile.velocity * projectileLength;

            SBUtils.LineVisualizer(hitboxStartPosition, hitboxEndPosition, 150f);

            return SBUtils.DrawFrame(Projectile, frameCount, ticksPerFrame);
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
            if (currentFrame == 1)
            {
                startingPosition = player.MountedCenter;
                Projectile.Center = startingPosition + Projectile.velocity * initialOffset;
                hitboxOffset = initialHitboxOffset;
            }

            if (currentFrame >= dashFrameStart * ticksPerFrame && currentFrame <= activeFrameEnd * ticksPerFrame)
            {
                Projectile.Center = startingPosition + Projectile.velocity * initialOffset + Vector2.SmoothStep(Projectile.velocity * offsetMin, Projectile.velocity * offsetMax, progress);
                hitboxOffset += hitboxIncrementPerTick;
            }

            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
            player.ChangeDir(Projectile.spriteDirection);

            SBUtils.PrintCurrentFrame(currentFrame);

            return true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            int currentFrameIndex = currentFrame / ticksPerFrame;

            if (currentFrameIndex < activeFrameStart || currentFrameIndex > activeFrameEnd)
            {
                return false;
            }

            float unusedFloat = 0f;
            Vector2 hitboxStartPosition = startingPosition + Projectile.velocity * hitboxOffset;
            Vector2 hitboxEndPosition = hitboxStartPosition + Projectile.velocity * projectileLength;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), hitboxStartPosition, hitboxEndPosition, 150f, ref unusedFloat);
        }


        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();

            if (!target.boss && target.type != NPCID.TargetDummy)
            {
                target.GetGlobalNPC<SpiritBlossomCrowdControlGlobalNPCs>().InitializeMortalSteelValues(target);
                target.AddBuff(BuffType<Buffs.SpiritBlossomCrowdControl>(), 300);
            }

            if (player.HeldItem.ModItem is SpiritSwords spiritBlossom)
            {
                SoundEngine.PlaySound(sbPlayer.Q3Hit with { Volume = SBUtils.GlobalSFXVolume });
            }
        }
    }
}