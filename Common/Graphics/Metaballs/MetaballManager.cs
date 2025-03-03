﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Common.Graphics.Metaballs
{
    public class MetaballManager : ModSystem
    {
        private static List<BaseMetaballCollection> MetaballCollections;

        public override void Load()
        {
            if (Main.netMode is NetmodeID.Server)
                return;

            MetaballCollections = new();

            Type baseType = typeof(BaseMetaballCollection);

            foreach (Type type in Mod.Code.GetTypes())
            {
                if (!type.IsAbstract && type.IsSubclassOf(baseType))
                {
                    BaseMetaballCollection collection = Activator.CreateInstance(type) as BaseMetaballCollection;
                    collection.Load();
                    MetaballCollections.Add(collection);
                }
            }

            On_Main.SortDrawCacheWorms += DrawToMetaballTargets;
            On_Main.DrawInfernoRings += DrawParticles;
        }

        public override void Unload()
        {
            if (Main.netMode is NetmodeID.Server)
                return;

            On_Main.SortDrawCacheWorms -= DrawToMetaballTargets;
            On_Main.DrawInfernoRings -= DrawParticles;
        }

        private void DrawToMetaballTargets(On_Main.orig_SortDrawCacheWorms orig, Main self)
        {
            orig(self);
            DrawTargets(Main.spriteBatch);
        }

        private void DrawParticles(On_Main.orig_DrawInfernoRings orig, Main self)
        {
            orig(self);
            Main.spriteBatch.End();
            PrepareTargets(Main.spriteBatch);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        public override void PostUpdateDusts()
        {
            if (Main.netMode is NetmodeID.Server)
                return;

            foreach (BaseMetaballCollection collection in MetaballCollections)
            {
                collection.UpdateMetaballs();
                collection.Metaballs.RemoveAll(m => m.Size <= 1f);
            }
        }

        public static void PrepareTargets(SpriteBatch spriteBatch)
        {
            RenderTargetBinding[] bindings = Main.instance.GraphicsDevice.GetRenderTargets();
            foreach (BaseMetaballCollection collection in MetaballCollections)
                collection.DrawToTarget(spriteBatch);

            Main.instance.GraphicsDevice.SetRenderTargets(bindings);
        }

        public static void DrawTargets(SpriteBatch spriteBatch)
        {
            foreach (BaseMetaballCollection collection in MetaballCollections)
                collection.DrawTarget(spriteBatch);
        }

        public static BaseMetaballCollection GetCollectionByType<T>() where T : BaseMetaballCollection
        {
            if (Main.netMode is NetmodeID.Server || !MetaballCollections.Any())
                return null;

            return MetaballCollections.First(mc => mc.GetType() == typeof(T));
        }
    }
}
