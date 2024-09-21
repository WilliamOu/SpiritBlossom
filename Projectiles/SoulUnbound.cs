using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SpiritBlossom.Items;
using Terraria.Graphics.Shaders;
using SpiritBlossom.System;
using StarsAbove;
using System.Threading;

namespace SpiritBlossom.Projectiles
{
    public class SoulUnbound : ModProjectile
    {
        private Vector2 initialBodyPosition;
        private Vector2 cloneVisualOffsetCorrection = new Vector2(4, 4);

        private float holdoutRange = 90f;
        private int frameCount = SpiritBlossomPlayer.SoulUnboundDuration;
        private int ticksPerFrame = 1;
        private int currentFrame = 0;
        private float cloudTrailScroll = 0;
        private float secondaryTrailScroll = 0;
        private float trailScrollSpeed = 0.001f;
        private float cloneScrollX = 0;
        private float cloneScrollY = 0;
        private float cloneScrollXSpeed = 0.003f;
        private float cloneScrollYSpeed = 0.003f;

        private Vector2 cloneDashDirection;

        private float scale = 0.5f;

        private static RenderTarget2D cloneRenderTarget;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
            Main.projFrames[Projectile.type] = frameCount;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = 0;
            Projectile.scale = 0.85f;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 15;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            initialBodyPosition = Projectile.position;
        }

        public override void Load()
        {
            if (!Main.dedServ)
            {
                using var eventSlim = new ManualResetEventSlim();

                Main.QueueMainThreadAction(() =>
                {
                    cloneRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);

                    eventSlim.Set();
                });

                eventSlim.Wait();

                Main.graphics.GraphicsDevice.DeviceReset += OnDeviceReset;
            }
        }

        private static void OnDeviceReset(object sender, EventArgs eventArgs)
        {
            var gd = (GraphicsDevice)sender;

            var parameters = gd.PresentationParameters;

            cloneRenderTarget?.Dispose();
            cloneRenderTarget = new RenderTarget2D(gd, parameters.BackBufferWidth, parameters.BackBufferHeight);
        }

        public override void Unload()
        {
            Main.graphics.GraphicsDevice.DeviceReset -= OnDeviceReset;

            if (cloneRenderTarget is not null)
            {
                using var eventSlim = new ManualResetEventSlim();

                Main.QueueMainThreadAction(() =>
                {
                    cloneRenderTarget.Dispose();
                    eventSlim.Set();
                });

                eventSlim.Wait();

                cloneRenderTarget = null;
            }
        }

        public override bool PreAI()
        {
            currentFrame++;

            Player player = Main.player[Projectile.owner];

            if (currentFrame == 1)
            {
                initialBodyPosition = Projectile.position;
                cloneDashDirection = -Projectile.velocity;

                player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone = (Player)player.Clone();
                Player clone = player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone;

                // Set the clone's position and reset visual states
                clone.position = initialBodyPosition + cloneVisualOffsetCorrection;
                clone.fullRotation = 0f;
                clone.itemAnimation = 0;
                clone.itemTime = 0;
                clone.legFrame.Y = 0;
                clone.bodyFrame.Y = 0;
                clone.headFrame.Y = 0;
                clone.wings = 0;
                clone.wingFrame = 0;

                clone.invis = false;
                clone.immuneAlpha = 0;
                clone.immuneTime = 0;
                clone.immune = false;
                clone.stealth = 0f;
                clone.setSolar = false;
                clone.shadowDodgeTimer = 0;
                clone.ghostFade = 0f;
            }

            if (Projectile.timeLeft == 1 && (player.HasBuff(BuffType<Buffs.SoulUnboundBuff>()) || player.HasBuff(BuffType<Buffs.SoulUnboundRecast>())))
            {
                Projectile.timeLeft++;
            }

            Vector2 positionOffset = currentFrame switch
            {
                <= 11 => cloneDashDirection * SpiritBlossomPlayer.SoulUnboundDashDistancePerTick,
                <= 12 => cloneDashDirection * SpiritBlossomPlayer.SoulUnboundDashDistancePerTick * 4f / 5f,
                <= 13 => cloneDashDirection * SpiritBlossomPlayer.SoulUnboundDashDistancePerTick * 3f / 5f,
                <= 14 => cloneDashDirection * SpiritBlossomPlayer.SoulUnboundDashDistancePerTick * 2f / 5f,
                <= 15 => cloneDashDirection * SpiritBlossomPlayer.SoulUnboundDashDistancePerTick * 1f / 5f,
                _ => Vector2.Zero
            };

            positionOffset = Collision.TileCollision(player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone.position, positionOffset, player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone.width, player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone.height);

            player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone.position += positionOffset;

            Projectile.position = player.position;

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            if (!(player.HasBuff(BuffType<Buffs.SoulUnboundBuff>()) || player.HasBuff(BuffType<Buffs.SoulUnboundRecast>()))) { return false; }

            Effect trailShaderEffect = ShaderSystem.SoulUnboundTrailShader.Value;
            Effect cloneShaderEffect = ShaderSystem.SoulUnboundCloneShader.Value;

            RenderClone(player, cloneShaderEffect);
            RenderTrails(player, trailShaderEffect);
            ResetSpriteBatch();

            return false;
        }

        private void RenderTrails(Player player, Effect shaderEffect)
        {
            Vector2 direction = player.Center - player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone.Center;
            float rotation = (float)Math.Atan2(direction.Y, direction.X);
            float distance = direction.Length();
            direction.Normalize();

            shaderEffect.Parameters["bgTexture"].SetValue(ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/SoulUnbound/CloudTrailColorPremultipliedAlphaNoSideAlpha").Value);
            shaderEffect.Parameters["sourceLength"].SetValue(distance);
            shaderEffect.Parameters["colorIntensity"].SetValue(1.2f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, shaderEffect, Main.GameViewMatrix.TransformationMatrix);

            shaderEffect.Parameters["useTransparencyForSecondaryCloudTrail"].SetValue(false);
            shaderEffect.Parameters["textureLength"].SetValue(256 * scale);
            shaderEffect.Parameters["sourceScrollX"].SetValue(cloudTrailScroll);
            DrawTrail(player, "SpiritBlossom/Projectiles/SoulUnbound/CloudTrailPremultipliedAlpha", distance, rotation, direction);

            shaderEffect.Parameters["useTransparencyForSecondaryCloudTrail"].SetValue(true);
            shaderEffect.Parameters["textureLength"].SetValue(512 * scale);
            shaderEffect.Parameters["sourceScrollX"].SetValue(secondaryTrailScroll);
            DrawTrail(player, "SpiritBlossom/Projectiles/SoulUnbound/SecondaryTrailPremultipliedAlpha", distance, rotation, direction);

            cloudTrailScroll += trailScrollSpeed;
            if (cloudTrailScroll > 1f) { cloudTrailScroll -= 1f; }

            secondaryTrailScroll -= trailScrollSpeed;
            if (secondaryTrailScroll < -1f) { cloudTrailScroll += 1f; }
        }

        private void RenderClone(Player player, Effect shaderEffect)
        {
            var clone = player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone;
            SBUtils.LineVisualizer(clone.Center, player.Center, 4f);

            shaderEffect.Parameters["bgTexture"].SetValue(ModContent.Request<Texture2D>("SpiritBlossom/Projectiles/SoulUnbound/PlayerShaderPremultipliedAlpha").Value);
            shaderEffect.Parameters["colorIntensity"].SetValue(1f);
            shaderEffect.Parameters["bgWidth"].SetValue(128);
            shaderEffect.Parameters["bgHeight"].SetValue(32);
            shaderEffect.Parameters["bgScrollX"].SetValue(cloneScrollX);
            shaderEffect.Parameters["bgScrollY"].SetValue(cloneScrollY);

            Main.graphics.GraphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTarget(cloneRenderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            Main.PlayerRenderer.DrawPlayer(Main.Camera, clone, clone.position, clone.fullRotation, clone.fullRotationOrigin, 0f);
            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTarget(Main.screenTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Main.spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            // Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shaderEffect, Main.Transform);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            Main.spriteBatch.Draw(cloneRenderTarget, Vector2.Zero, Color.White);

            cloneScrollX += cloneScrollXSpeed;
            if (cloneScrollX > 1f) { cloneScrollX -= 1f; }

            cloneScrollY += cloneScrollYSpeed;
            if (cloneScrollY > 1f) { cloneScrollY -= 1f; }
        }

        private void DrawTrail(Player player, string trailPath, float distance, float rotation, Vector2 direction)
        {
            Texture2D trailTexture = ModContent.Request<Texture2D>(trailPath).Value;

            SpriteEffects flipIfFacingLeft = (player.Center.X < player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone.Center.X) ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Rectangle sourceRect = new Rectangle(0, 0, trailTexture.Width, trailTexture.Height);
            Rectangle destinationRect = new Rectangle(
                (int)(player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone.Center.X - Main.screenPosition.X),
                (int)(player.GetModPlayer<SpiritBlossomPlayer>().SoulUnboundClone.Center.Y - Main.screenPosition.Y),
                (int)distance,
                (int)(trailTexture.Height * scale)
            );

            Main.spriteBatch.Draw(trailTexture, destinationRect, sourceRect, Color.White, rotation, new Vector2(0, trailTexture.Height / 2), flipIfFacingLeft, 0f);
        }

        private void ResetSpriteBatch()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}