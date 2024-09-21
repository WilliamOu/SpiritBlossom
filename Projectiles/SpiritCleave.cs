using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SpiritBlossom.Items;
using StarsAbove.Items.Armor.SkyStriker;

namespace SpiritBlossom.Projectiles
{
    public class SpiritCleave : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];
        private ref float Progress => ref Projectile.ai[1];
        private ref float Size => ref Projectile.ai[2];

        private float handRotationProgress = 0;
        private const float initialHandRotationOffset = MathHelper.Pi / 3f;

        private const float postSwingRange = MathHelper.Pi / 20f;
        private const float finalHandRotationOffset = MathHelper.PiOver2 - postSwingRange;

        private const float swingRange = MathHelper.Pi * 7/8;
        private const float windup = 0.15f;

        private int frameCount = 21;
        private int ticksPerFrame = 2;
        private int framesUntilVFXWave = 11;
        private int currentFrame = 0;
        private int postSwingTimer = 0;

        private bool firstEnemyHit = false;
        private bool finishedSwing = false;
        private bool flipSword = false;
        private int shieldAmount = 0;

        private float swingTime => framesUntilVFXWave * ticksPerFrame - 1;
        private float postSwingTime => (frameCount - framesUntilVFXWave) * ticksPerFrame;

        private Player Owner => Main.player[Projectile.owner];

        private float projectileHorizontalOffset = 50;
        private float projectileVerticalOffset = -10;
        private float fixLeftSideVerticalOffset = -6;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = frameCount * ticksPerFrame;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(projectileHorizontalOffset, texture.Height + projectileVerticalOffset);

            int flipSpriteBasedOnSpriteDirectionAndWhetherTheSwordIsFlipped = (flipSword == true) ? Projectile.spriteDirection * -1 : Projectile.spriteDirection;
            origin += (flipSpriteBasedOnSpriteDirectionAndWhetherTheSwordIsFlipped == -1) ? new Vector2(0, fixLeftSideVerticalOffset) : Vector2.Zero; 
            SpriteEffects flipVerticalIfFacingLeft = (flipSpriteBasedOnSpriteDirectionAndWhetherTheSwordIsFlipped == -1) ? SpriteEffects.FlipVertically: SpriteEffects.None;

            float drawRotation = Projectile.rotation + handRotationProgress * Projectile.spriteDirection;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor * Projectile.Opacity, drawRotation, origin, Projectile.scale, flipVerticalIfFacingLeft, 0);

            SBUtils.RectVisualizer(Projectile.Hitbox);
            SBUtils.ConeVisualizer(Main.player[Projectile.owner].MountedCenter, 375, Projectile.velocity.ToRotation(), 1.1f * MathHelper.PiOver4);
            return false;
        }

        public override bool PreAI()
        {
            Timer++;
            currentFrame++;

            // Calculate progress using a function of choice
            float normalizedTime = Timer / swingTime;
            if (normalizedTime <= 1)
            {
                Progress = swingRange * CalculateProgress(normalizedTime);
                handRotationProgress = initialHandRotationOffset * (1 - CalculateProgress(normalizedTime));
            }
            else
            {
                SetSwordPositionPostSwing();
            }

            Size = MathHelper.SmoothStep(0, 1, Math.Min(1f, Timer / (swingTime * windup)));

            // Update frame
            if (++Projectile.frameCounter % ticksPerFrame == 0)
            {
                Projectile.frame = ++Projectile.frame % frameCount;
            }

            SetSwordPosition();

            SBUtils.PrintCurrentFrame(currentFrame);
            return false;
        }

        public override void PostAI()
        {
            if (shieldAmount <= 0) { return; }

            Player player = Main.player[Projectile.owner];
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();
            sbPlayer.OnSpiritCleaveHit(player, shieldAmount);
            shieldAmount = 0;
        }

        private float CalculateProgress(float t)
        {
            return t * t * t * t * t * t;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();
            if (!firstEnemyHit) {
                SoundEngine.PlaySound(sbPlayer.WHit with { Volume = SBUtils.GlobalSFXVolume });
                SoundEngine.PlaySound(sbPlayer.WShieldActivate with { Volume = SBUtils.GlobalSFXVolume });
                shieldAmount += SpiritBlossomPlayer.SpiritCleavePrimaryHitShieldAmount;
                firstEnemyHit = true;
            }
            else
            {
                shieldAmount += SpiritBlossomPlayer.SpiritCleaveSecondaryHitShieldAmount;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (currentFrame / ticksPerFrame < framesUntilVFXWave || currentFrame / ticksPerFrame > framesUntilVFXWave + 3)
            {
                return false;
            }
            // The cleave uses two hitboxes: a cone for the wave, and a box around the player, giving some leniency since this ability is so hard to hit
            return targetHitbox.IntersectsConeSlowMoreAccurate(Main.player[Projectile.owner].MountedCenter, 375, Projectile.velocity.ToRotation(), 1.1f * MathHelper.PiOver4) || projHitbox.Intersects(targetHitbox);
        }

        private void SetSwordPosition()
        {
            Player player = Main.player[Projectile.owner];

            // Calculate the angle to the mouse position
            float targetAngle = Projectile.velocity.ToRotation();

            // Determine the direction based on the angle
            Projectile.spriteDirection = Projectile.direction = (targetAngle > -MathHelper.PiOver2 && targetAngle < MathHelper.PiOver2) ? 1 : -1;
            player.ChangeDir(Projectile.spriteDirection);

            // Calculate the base angle (135 degrees below the target angle)
            // The VFX for Spirit Cleave is at a 90 degree angle, so we swing to the starting position of the VFX, then 'teleport' the sword an additional 90 degrees in order to give the illusion of an incredibly fast sword swing
            float baseAngle = targetAngle + (MathHelper.Pi/8 + swingRange) * Projectile.spriteDirection;

            // Calculate the current rotation based on the progress of the swing
            Projectile.rotation = baseAngle - Progress * Projectile.spriteDirection;

            Vector2 armPosition;
            if (!finishedSwing)
            {
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
                armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            }
            else
            {
                float rotation = (Projectile.spriteDirection == -1) ? 3f * MathHelper.PiOver4 - MathHelper.Pi / 10f : 5f * MathHelper.PiOver4 + MathHelper.Pi / 10f;
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
                armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, rotation);
            }

            // Set the projectile position
            armPosition.Y += Owner.gfxOffY;
            Projectile.Center = armPosition;
            Projectile.scale = Size * Owner.GetAdjustedItemScale(Owner.HeldItem);

            // When the sword is flipped, its not set as the player's held projectile so it renders behind the player's head
            // This creates the effect of the sword resting on the opposite shoulder
            if (!flipSword)
            {
                Owner.heldProj = Projectile.whoAmI;
            }
        }

        private void SetSwordPositionPostSwing()
        {
            float maxProgress = swingRange * CalculateProgress(1) + MathHelper.PiOver2;;
            if (finishedSwing == false)
            {
                Progress = maxProgress;
                finishedSwing = true;
            }
            else
            {
                float normalizedTime = postSwingTimer / postSwingTime;
                Progress = maxProgress + postSwingRange * CalculatePostSwingProgress(normalizedTime);
                handRotationProgress = -1 * finalHandRotationOffset * CalculatePostSwingProgress(normalizedTime);
                if (normalizedTime > 0.2) { flipSword = true; }
                postSwingTimer++;
            }
        }

        private float CalculatePostSwingProgress(float t)
        {
            return 1 - (1 - t) * (1 - t) * (1 - t);
        }
    }
}