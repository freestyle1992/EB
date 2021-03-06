﻿namespace XinZhao
{
    using System;
    using System.Drawing;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Events;

    internal class Mainframe
    {
        #region Static Fields

        public static readonly Random RDelay = new Random();

        #endregion

        #region Public Methods and Operators

        public static void Init()
        {
            Game.OnTick += OnGameUpdate;
            Orbwalker.OnPostAttack += Computed.JungleOnPostAttack;
            Orbwalker.OnPostAttack += Computed.ComboOnPostAttack;
            Orbwalker.OnPostAttack += Computed.LaneOnPostAttack;
            Orbwalker.OnPostAttack += Computed.HarassOnPostAttack;
            Interrupter.OnInterruptableSpell += OtherUtils.OnInterruptableSpell;
            Obj_AI_Base.OnProcessSpellCast += Computed.OnProcessSpellCast;
            Obj_AI_Base.OnSpellCast += Computed.OnSpellCast;
            Drawing.OnDraw += OnDraw;

            // Orbwalker.OnUnkillableMinion += Computed.OnUnkillableMinion;
            // Interrupter.OnInterruptableSpell += OtherUtils.OnInterruptableSpell;
        }

        #endregion

        #region Methods

        private static void OnDraw(EventArgs args)
        {
            if (!Config.IsChecked(Config.Draw, "drawXinsec") || Player.Instance.IsDead)
            {
                return;
            }

            AIHeroClient xinsecTarget = null;
            switch (Config.GetComboBoxValue(Config.Misc, "xinsecTargetting"))
            {
                case 0:
                    xinsecTarget = TargetSelector.SelectedTarget;
                    break;
                case 1:
                    xinsecTarget = TargetSelector.GetTarget(2000, DamageType.Mixed);
                    break;
                case 2:
                    xinsecTarget =
                        EntityManager.Heroes.Enemies.Where(en => en.Distance(Player.Instance.Position) <= 2000)
                            .OrderBy(en => en.MaxHealth)
                            .FirstOrDefault();
                    break;
            }

            if (xinsecTarget != null && Spells.E.CanCast() && Spells.R.CanCast()
                && !xinsecTarget.HasBuff("XinZhaoIntimidate") && !xinsecTarget.IsInvulnerable)
            {
                Drawing.DrawText(
                    Drawing.WorldToScreen(xinsecTarget.Position), 
                    Color.AntiqueWhite, 
                    "Xinsec:" + Environment.NewLine + xinsecTarget.ChampionName, 
                    10);
                if (Config.IsChecked(Config.Draw, "drawXinsecpred"))
                {
                    var extendToPos = Player.Instance.Position.GetBestAllyPlace(1750);
                    var xinsecTargetExtend = xinsecTarget.Position.Extend(extendToPos, -200).To3D();
                    Drawing.DrawCircle(xinsecTargetExtend, 100, Color.AliceBlue);
                }
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Player.Instance.IsDead)
            {
                return;
            }

            if (Config.IsKeyPressed(Config.Misc, "xinsecKey"))
            {
                Orbwalker.MoveTo(Game.CursorPos);
                Computed.Xinsec();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Modes.Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)
                && Player.Instance.ManaPercent >= Config.GetSliderValue(Config.JungleClear, "jcMana"))
            {
                Modes.JungleClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)
                && Player.Instance.ManaPercent >= Config.GetSliderValue(Config.Harass, "harassMana"))
            {
                Modes.Harass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)
                && Player.Instance.ManaPercent >= Config.GetSliderValue(Config.LaneClear, "lcMana"))
            {
                Modes.LaneClear();
            }
        }

        #endregion
    }
}