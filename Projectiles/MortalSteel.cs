using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SpiritBlossom.Items;

namespace SpiritBlossom.Projectiles
{
    public class MortalSteel : ModProjectile
    {
        private float offsetMin = 170f;
        private float offsetMax = 230f;
        private float projectileLength = 375f;
        private int frameCount = 1;
        private int ticksPerFrame = 1;
        private int currentFrame = 0;
        private bool firstEnemyHit = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
            Main.projFrames[Projectile.type] = frameCount;
        }

        public override void SetDefaults() {
            Projectile.aiStyle = 19;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.scale = 0.8f;
            Projectile.timeLeft = 30;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;

            offsetMin *= Projectile.scale;
            offsetMax *= Projectile.scale;
            projectileLength *= Projectile.scale;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            SBUtils.LineVisualizer(player.MountedCenter, player.MountedCenter + new Vector2(projectileLength * MathF.Cos(Projectile.rotation) * Projectile.spriteDirection, projectileLength * MathF.Sin(Projectile.rotation) * Projectile.spriteDirection), 30f);

            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = tex.Frame(1, frameCount, 0, Projectile.frameCounter / ticksPerFrame % frameCount);

            SpriteEffects flipIfFacingLeft = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(frame.Width / 2, frame.Height / 2), Projectile.scale, flipIfFacingLeft);
            return false;
        }

        public override bool PreAI()
        {
            currentFrame++;

            Player player = Main.player[Projectile.owner];

            player.heldProj = Projectile.whoAmI;

            if (++Projectile.frameCounter % ticksPerFrame == 0)
            {
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }

            float progress = 1f - MathF.Exp(-Projectile.timeLeft / 12f);
            Projectile.velocity = Vector2.Normalize(Projectile.velocity);
            Projectile.Center = player.MountedCenter + Vector2.Lerp(Projectile.velocity * offsetMin, Projectile.velocity * offsetMax, progress);

            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
            player.ChangeDir(Projectile.spriteDirection);

            // Set the player's arm position
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - MathHelper.PiOver2);

            SBUtils.PrintCurrentFrame(currentFrame);

            // Makes the projectile glow
            Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Main.player[Projectile.owner];
            float unusedFloat = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), player.MountedCenter, player.MountedCenter + new Vector2(projectileLength * MathF.Cos(Projectile.rotation) * Projectile.spriteDirection, projectileLength * MathF.Sin(Projectile.rotation) * Projectile.spriteDirection), 30f, ref unusedFloat);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            SpiritBlossomPlayer sbPlayer = player.GetModPlayer<SpiritBlossomPlayer>();
            if (!firstEnemyHit)
            {
                SoundEngine.PlaySound(sbPlayer.QHit[SBUtils.SoundIndex] with { Volume = SBUtils.GlobalSFXVolume });
                firstEnemyHit = true;
                sbPlayer.OnMortalSteelHit(player);
            }
        }
    }
}