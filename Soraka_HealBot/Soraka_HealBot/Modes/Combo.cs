﻿namespace Soraka_HealBot.Modes
{
    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;

    using Soraka_HealBot.Extensions;

    internal static class Combo
    {
        #region Methods

        internal static void Execute()
        {
            var target = TargetSelector.GetTarget(Spells.E.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (Config.IsChecked(Config.Combo, "useQInCombo") && Spells.Q.CanCast())
            {
                Spells.Q.Cast(target);
            }

            if (Config.IsChecked(Config.Combo, "useEInCombo") && Spells.E.CanCast())
            {
                if (Config.IsChecked(Config.Combo, "eOnlyCC"))
                {
                    var ePred = Spells.E.GetPrediction(target);
                    if (ePred.HitChance == HitChance.Immobile)
                    {
                        Spells.E.Cast(target);
                    }

                    return;
                }

                Spells.E.Cast(target);
            }
        }

        #endregion
    }
}