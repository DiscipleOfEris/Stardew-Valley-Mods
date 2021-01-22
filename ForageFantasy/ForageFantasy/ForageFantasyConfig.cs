﻿namespace ForageFantasy
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using System;

    public interface GenericModConfigMenuAPI
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

        void RegisterLabel(IManifest mod, string labelName, string labelDesc);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);

        void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);

        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc, Func<Vector2, object, object> widgetUpdate, Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave);
    }

    /// <summary>
    /// Config file for the mod
    /// </summary>
    public class ForageFantasyConfig
    {
        public bool MushroomCaveQuality { get; set; } = true;

        public bool CommonFiddleheadFern { get; set; } = true;

        public bool ForageSurvivalBurger { get; set; } = true;

        public bool CompatibilityMode { get; set; } = false;

        public int TapperQualityOptions { get; set; } = 1;

        public bool TapperQualityRequiresTapperPerk { get; set; } = false;

        public bool BerryBushQuality { get; set; } = true;

        public int BerryBushChanceToGetXP { get; set; } = 100;

        public int BerryBushXPAmount { get; set; } = 1;

        private static string[] TQChoices { get; set; } = new string[] { "Disabled", "Forage Level Based", "Forage Level Based (No Botanist)", "Tree Age Based (Months)", "Tree Age Based (Years)" };

        public static void VerifyConfigValues(ForageFantasyConfig config, ForageFantasy mod)
        {
            bool invalidConfig = false;

            if (config.TapperQualityOptions < 0 || config.TapperQualityOptions > 4)
            {
                invalidConfig = true;
                config.TapperQualityOptions = 0;
            }

            if (config.BerryBushChanceToGetXP < 0)
            {
                invalidConfig = true;
                config.BerryBushChanceToGetXP = 0;
            }

            if (config.BerryBushChanceToGetXP > 100)
            {
                invalidConfig = true;
                config.BerryBushChanceToGetXP = 100;
            }

            if (config.BerryBushXPAmount < 0)
            {
                invalidConfig = true;
                config.BerryBushXPAmount = 0;
            }

            if (invalidConfig)
            {
                mod.DebugLog("At least one config value was out of range and was reset.");
                mod.Helper.WriteConfig(config);
            }
        }

        public static void SetUpModConfigMenu(ForageFantasyConfig config, ForageFantasy mod)
        {
            GenericModConfigMenuAPI api = mod.Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");

            if (api == null)
            {
                return;
            }

            var manifest = mod.ModManifest;

            api.RegisterModConfig(manifest, () => config = new ForageFantasyConfig(), delegate { mod.Helper.WriteConfig(config); VerifyConfigValues(config, mod); });

            api.RegisterLabel(manifest, "General Tweaks", null);

            api.RegisterSimpleOption(manifest, "Mushroom Cave Quality", "Mushrooms have quality based on forage level and botanist perk", () => config.MushroomCaveQuality, (bool val) => config.MushroomCaveQuality = val);
            api.RegisterSimpleOption(manifest, "Common Fiddlehead Fern¹", "Fiddlehead fern is available outside of the secret forest\nand added to the wild seeds pack and summer foraging bundle", () => config.CommonFiddleheadFern, (bool val) => config.CommonFiddleheadFern = val);
            api.RegisterSimpleOption(manifest, "Forage Survival Burger¹", "Forage based early game crafting recipes and even more efficient cooking recipes", () => config.ForageSurvivalBurger, (bool val) => config.ForageSurvivalBurger = val);
            api.RegisterSimpleOption(manifest, "Auto Pickup Compatibility", "Ensures compatibility with automatic pickup mods.\nSets the quality of mushrooms and tapper products based\non the player that would have the best result.\nIn multiplayer it only works if the host has the mod and\neveryone who has the mod has enabled this.", () => config.CompatibilityMode, (bool val) => config.CompatibilityMode = val);

            api.RegisterLabel(manifest, "Tapper Quality", null);

            api.RegisterChoiceOption(manifest, "Tapper Quality Options", null, () => GetElementFromConfig(TQChoices, config.TapperQualityOptions), (string val) => config.TapperQualityOptions = GetIndexFromArrayElement(TQChoices, val), TQChoices);
            api.RegisterSimpleOption(manifest, "Tapper Perk Is Required", null, () => config.TapperQualityRequiresTapperPerk, (bool val) => config.TapperQualityRequiresTapperPerk = val);

            api.RegisterLabel(manifest, "Berry Bushes", null);

            api.RegisterSimpleOption(manifest, "Berry Bush Quality", "Salmonberries and blackberries have quality based\non forage level even without botanist perk.", () => config.BerryBushQuality, (bool val) => config.BerryBushQuality = val);
            api.RegisterClampedOption(manifest, "Berry Bush Chance To Get XP", "Chance to get foraging experience when harvesting bushes.\nSet to 0 to disable feature.", () => config.BerryBushChanceToGetXP, (int val) => config.BerryBushChanceToGetXP = val, 0, 100);
            api.RegisterSimpleOption(manifest, "Berry Bush XP Amount", "Amount of XP gained per bush. For reference:\nChopping down a tree is 12XP, a foraging good is 7XP", () => config.BerryBushXPAmount, (int val) => config.BerryBushXPAmount = val);

            api.RegisterLabel(manifest, "", null);
            api.RegisterLabel(manifest, "1: Restart Needed For Changes To Take Effect", null);
        }

        private static string GetElementFromConfig(string[] options, int config)
        {
            if (config >= 0 && config < options.Length)
            {
                return options[config];
            }
            else
            {
                return options[0];
            }
        }

        private static int GetIndexFromArrayElement(string[] options, string element)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == element)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}