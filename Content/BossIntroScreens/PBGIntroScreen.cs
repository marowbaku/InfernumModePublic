using CalamityMod.NPCs.PlaguebringerGoliath;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace InfernumMode.Content.BossIntroScreens
{
    public class PBGIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => new(completionRatio =>
        {
            float colorFadeInterpolant = Sin(AnimationCompletion * Pi * 4f + completionRatio * TwoPi) * 0.5f + 0.5f;
            return Color.Lerp(Color.Lime, Color.DarkOliveGreen, colorFadeInterpolant);
        });

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;

        public override bool ShouldBeActive() => NPC.AnyNPCs(ModContent.NPCType<PlaguebringerGoliath>());

        public override SoundStyle? SoundToPlayWithTextCreation => new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/PlagueReaperAbility");
    }
}