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

namespace SpiritBlossom.Projectiles
{
    public class FateSealed : ModProjectile
    {
        private int frameCount = 63;
        private int ticksPerFrame = 2;
        private int currentFrame = 0;
        private float horizontalOffset = 0f;
        private float verticalOffset = 0f;

        private Vector2 startingPosition;
        private float projectileLength = 540f;
        private float initialOffset = 300f;
        private float hitboxOffset = 0f;

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
            Projectile.localNPCHitCooldown = 4;
            Projectile.usesLocalNPCImmunity = true;

            currentFrame = 1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            /*string texturePath = "SpiritBlossom/Projectiles/FateSealedFrames/SpiritBlink" + currentFrame / ticksPerFrame;
            Texture2D tex = ModContent.Request<Texture2D>(texturePath).Value;
            Rectangle frame = tex.Frame();

            SpriteEffects flipIfFacingLeft = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Color transparentWhite = new Color(200, 200, 200, 256);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, transparentWhite, Projectile.rotation, new Vector2(frame.Width / 2 + horizontalOffset, frame.Height / 2 + verticalOffset), Projectile.scale, flipIfFacingLeft);*/

            Vector2 HitboxStartingPosition = startingPosition + Projectile.velocity * hitboxOffset;
            SBUtils.LineVisualizer(HitboxStartingPosition, HitboxStartingPosition + new Vector2(projectileLength * MathF.Cos(Projectile.rotation) * Projectile.spriteDirection, projectileLength * MathF.Sin(Projectile.rotation) * Projectile.spriteDirection), 150f);
            // return false;

            Color transparentWhite = new Color(200, 200, 200, 256);
            return SBUtils.DrawFrame(this.Projectile, frameCount, ticksPerFrame, transparentWhite, 8, 8);
        }

        public override bool PreAI()
        {
            currentFrame++;
            Player player = Main.player[Projectile.owner];
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();

            if (++Projectile.frameCounter % ticksPerFrame == 0)
            {
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }

            Projectile.velocity = Vector2.Normalize(Projectile.velocity);
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);

            if (currentFrame / ticksPerFrame < 20)
            {
                player.ChangeDir(Projectile.spriteDirection);
                startingPosition = Main.player[Projectile.owner].MountedCenter;
                Projectile.Center = startingPosition + Projectile.velocity * initialOffset;
            }
            if (((float)currentFrame / ticksPerFrame) == 20f)
            {
                SoundEngine.PlaySound(sbPlayer.RBlink with { Volume = SBUtils.GlobalSFXVolume });
            }

            SBUtils.PrintCurrentFrame(currentFrame);

            // Makes the projectile glow
            Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (((float)currentFrame / ticksPerFrame) == 20f || ((float)currentFrame / ticksPerFrame) == 30f)
            {
                float unusedFloat = 0f;
                Vector2 HitboxStartingPosition = startingPosition + Projectile.velocity * hitboxOffset;
                return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), HitboxStartingPosition, HitboxStartingPosition + new Vector2(projectileLength * MathF.Cos(Projectile.rotation) * Projectile.spriteDirection, projectileLength * MathF.Sin(Projectile.rotation) * Projectile.spriteDirection), 150f, ref unusedFloat);
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();
            if (((float)currentFrame / ticksPerFrame) == 20f)
            {
                SoundEngine.PlaySound(sbPlayer.RInitialHit with { Volume = SBUtils.GlobalSFXVolume });

                if (target.type != NPCID.TargetDummy)
                {
                    target.GetGlobalNPC<SpiritBlossomCrowdControlGlobalNPCs>().InitializeFateSealedStunValues(target);
                    target.AddBuff(BuffType<Buffs.SpiritBlossomCrowdControl>(), 300);
                }

                sbPlayer.OnFateSealedHit(player, target);
            }
            else
            {
                SoundEngine.PlaySound(sbPlayer.RResidualHit with { Volume = SBUtils.GlobalSFXVolume });
                target.GetGlobalNPC<SpiritBlossomCrowdControlGlobalNPCs>().InitializeFateSealedPullValues(target, sbPlayer.FarthestEnemyFromPlayerDuringFateSealedCast.Item2, sbPlayer.PointBehindFarthestEnemyProjectionThatEnemiesArePulledTo, Vector2.Normalize(Projectile.velocity));
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
    }
}