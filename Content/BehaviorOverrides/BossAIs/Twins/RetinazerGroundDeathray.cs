using CalamityMod;
using CalamityMod.Projectiles.BaseProjectiles;
using InfernumMode.Assets.Effects;
using InfernumMode.Common.Graphics.Interfaces;
using InfernumMode.Common.Graphics.Primitives;
using InfernumMode.Core.GlobalInstances.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Twins
{
    public class RetinazerGroundDeathray : BaseLaserbeamProjectile, IPixelPrimitiveDrawer
    {
        public PrimitiveTrailCopy LaserDrawer
        {
            get;
            set;
        }

        public NPC Owner => Main.npc[(int)Projectile.ai[1]];

        public const int LifetimeConst = 35;

        public const float LaserLengthConst = 2820f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override float MaxScale => Utils.Remap(Time, 0f, 8f, 0.2f, 1f);

        public override float MaxLaserLength => Utils.Remap(Time, 0f, 8f, 50f, LaserLengthConst);

        public override float Lifetime => LifetimeConst;

        public override Color LaserOverlayColor => Color.Lerp(Color.IndianRed, Color.Red, 0.6f) * 1.2f;

        public override Color LightCastColor => LaserOverlayColor;

        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/UltimaRayStart", AssetRequestMode.ImmediateLoad).Value;

        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/UltimaRayMid", AssetRequestMode.ImmediateLoad).Value;

        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/UltimaRayEnd", AssetRequestMode.ImmediateLoad).Value;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Deathray");
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.Calamity().DealsDefenseDamage = true;
        }

        public override void AttachToSomething()
        {
            if (!Main.npc.IndexInRange((int)Projectile.ai[1]) || !Owner.active)
                Projectile.Kill();

            Owner.rotation = 0f;
            Projectile.velocity = (Owner.rotation + PiOver2).ToRotationVector2();
            Projectile.Center = Owner.Center + Projectile.velocity * 88f;

            if (Main.netMode != NetmodeID.MultiplayerClient && Time == 8f)
            {
                Vector2 endOfLaser = Projectile.Center + Projectile.velocity * (LaserLength - 32f);
                for (int i = 0; i < 24; i++)
                {
                    Vector2 laserVelocity = (TwoPi * i / 24f).ToRotationVector2() * 6f;
                    ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(laser =>
                    {
                        laser.tileCollide = false;
                    });
                    Utilities.NewProjectileBetter(endOfLaser, laserVelocity, ProjectileID.DeathLaser, TwinsAttackSynchronizer.SmallLaserDamage, 0f);
                }
                Utilities.NewProjectileBetter(endOfLaser, Vector2.Zero, ModContent.ProjectileType<LaserGroundShock>(), 0, 0f);
            }
        }

        public override float DetermineLaserLength() => DetermineLaserLength_CollideWithTiles(10);

        public float LaserWidthFunction(float _) => Projectile.scale * Projectile.width * Projectile.localAI[1] * 0.5f;

        public Color LaserColorFunction(float completionRatio)
        {
            float colorInterpolant = CalamityUtils.Convert01To010(Time / Lifetime) * 0.45f + 0.15f;
            colorInterpolant = Lerp(colorInterpolant, 1f, 1f - 1f / Projectile.localAI[1]);

            return Color.Lerp(Color.Red, Color.White, colorInterpolant * 0.5f) * (1f / Projectile.localAI[1]);
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            // This should never happen, but just in case.
            if (Projectile.velocity == Vector2.Zero)
                return;

            // Initialize the laser drawer.
            LaserDrawer ??= new(LaserWidthFunction, LaserColorFunction, null, true, InfernumEffectsRegistry.ArtemisLaserVertexShader);

            Vector2 laserEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * LaserLength;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(Projectile.Center, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // Select textures to pass to the shader, along with the electricity color.
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseColor(Color.White);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseImage1("Images/Extra_197");
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseImage2("Images/Misc/Perlin");

            float oldLocalAI = Projectile.localAI[1];
            for (float scaleFactor = 3f; scaleFactor >= 1f; scaleFactor -= 0.6f)
            {
                Projectile.localAI[1] = scaleFactor;
                LaserDrawer.DrawPixelated(baseDrawPoints, -Main.screenPosition, 54);
            }
            Projectile.localAI[1] = oldLocalAI;
        }

        public override void DetermineScale() => Projectile.scale = CalamityUtils.Convert01To010(Time / Lifetime);

        public override bool ShouldUpdatePosition() => false;
    }
}
