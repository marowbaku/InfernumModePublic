using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ApolloAcceleratingPlasmaSpark : ModProjectile
    {
        // public override void SetStaticDefaults() => DisplayName.SetDefault("Plasma Spark");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 360;
            Projectile.Opacity = 0f;
            Projectile.hide = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.Opacity = Clamp(Projectile.Opacity + 0.1f, 0f, 1f);
            Projectile.rotation += Projectile.velocity.X * 0.025f;
            if (Projectile.velocity.Length() < 35f)
                Projectile.velocity *= 1.024f;

            // Emit dust.
            for (int i = 0; i < 2; i++)
            {
                Dust plasma = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.TerraBlade);
                plasma.scale *= 0.7f;
                plasma.velocity = plasma.velocity * 0.4f + Main.rand.NextVector2Circular(0.4f, 0.4f);
                plasma.fadeIn = 0.4f;
                plasma.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 48) * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = 0; i < 4; i++)
            {
                Vector2 drawOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * i * 12f;
                Vector2 afterimageDrawPosition = Projectile.Center + drawOffset - Main.screenPosition;
                Color backAfterimageColor = Projectile.GetAlpha(lightColor) * ((4f - i) / 4f);
                Main.spriteBatch.Draw(texture, afterimageDrawPosition, null, backAfterimageColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            Color frontAfterimageColor = Projectile.GetAlpha(lightColor) * 0.2f;
            for (int i = 0; i < 9; i++)
            {
                Vector2 drawOffset = (TwoPi * i / 9f + Projectile.rotation - PiOver2).ToRotationVector2() * 2f;
                Vector2 afterimageDrawPosition = Projectile.Center + drawOffset - Main.screenPosition;
                Main.spriteBatch.Draw(texture, afterimageDrawPosition, null, frontAfterimageColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }
    }
}
