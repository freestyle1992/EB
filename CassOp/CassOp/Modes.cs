﻿namespace CassOp
{
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;

    internal class Modes
    {
        #region Public Methods and Operators

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Spells.R.Range + 250, DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (Config.IsChecked(Config.Combo, "useRInCombo") && Spells.R.IsReady())
            {
                var enemiesAroundTarget =
                    EntityManager.Heroes.Enemies.Count(
                        en => en.Distance(target.Position) <= 1000 && en.Name != target.Name);
                if (Config.IsChecked(Config.Combo, "comboFlashR") && target.IsFacing(Player.Instance)
                    && (target.Distance(Player.Instance) > Spells.R.Range
                        && target.Distance(Player.Instance) <= Spells.R.Range + 400)
                    && (Spells.Flash != null && Spells.Flash.IsReady)
                    && Computed.ComboDmg(target) * Spells.ComboDmgMod > target.Health
                    && enemiesAroundTarget <= Config.GetSliderValue(Config.Combo, "maxEnFlash"))
                {
                    Spells.FlashR = true;
                    var relPos = target.Position.Shorten(Player.Instance.Position, -300);
                    Spells.R.Cast(relPos);
                    Core.DelayAction(
                        () => Player.CastSpell(Spells.Flash.Slot, target.Position), 
                        Mainframe.RDelay.Next(300, 400));
                }

                var countFace =
                    EntityManager.Heroes.Enemies.Count(
                        h => h.IsValidTarget(Spells.R.Range) && h.IsFacing(Player.Instance));
                if (countFace >= Config.GetSliderValue(Config.Combo, "comboMinR")
                    && target.IsValidTarget(Spells.R.Range))
                {
                    Spells.R.Cast(target);
                }
            }

            if (Config.IsChecked(Config.Combo, "useQInCombo") && Spells.Q.IsReady()
                && !target.HasBuffOfType(BuffType.Poison))
            {
                var qPred = Spells.Q.GetPrediction(target);
                if (qPred.HitChancePercent >= 85)
                {
                    Spells.Q.Cast(qPred.CastPosition);
                }
            }

            if (Config.IsChecked(Config.Combo, "useWInCombo") && Spells.W.IsReady()
                && target.IsValidTarget(Spells.W.Range))
            {
                if (Config.IsChecked(Config.Combo, "comboWonlyCD"))
                {
                    if (!Spells.Q.IsReady() && (Spells.QCasted - Game.Time) < -0.5f
                        && !target.HasBuffOfType(BuffType.Poison))
                    {
                        var wPred = Spells.W.GetPrediction(target);
                        if (wPred.CastPosition.Distance(Player.Instance.Position) >= Spells.WMinRange
                            && wPred.HitChancePercent >= 85)
                        {
                            Spells.W.Cast(wPred.CastPosition);
                        }
                    }
                }
                else
                {
                    var wPred = Spells.W.GetPrediction(target);
                    if (wPred.CastPosition.Distance(Player.Instance.Position) >= Spells.WMinRange
                        && !wPred.CastPosition.IsWall() && wPred.HitChancePercent >= 85)
                    {
                        Spells.W.Cast(wPred.CastPosition);
                    }
                }
            }

            if (Config.IsChecked(Config.Combo, "useEInCombo") && Spells.E.IsReady()
                && target.IsValidTarget(Spells.E.Range)
                && (!Config.IsChecked(Config.Combo, "comboEonP") || target.HasBuffOfType(BuffType.Poison)))
            {
                if (Config.IsChecked(Config.Combo, "humanEInCombo"))
                {
                    var delay = Computed.RandomDelay(Config.GetSliderValue(Config.Misc, "humanDelay"));
                    Core.DelayAction(() => Spells.E.Cast(target), delay);
                }
                else
                {
                    Spells.E.Cast(target);
                }
            }
        }

        public static void LastHit()
        {
            if (Orbwalker.IsAutoAttacking)
            {
                return;
            }

            if (Config.IsChecked(Config.LastHit, "useEInLH") && Spells.E.IsReady())
            {
                var minToE =
                    EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(
                        m => m.Health < Spells.GetEDamage(m) && m.IsValidTarget(Spells.E.Range));
                if (minToE != null)
                {
                    if (!Config.IsChecked(Config.LastHit, "lastEonP") || minToE.HasBuffOfType(BuffType.Poison))
                    {
                        Spells.E.Cast(minToE);
                    }
                }
            }
        }

        #endregion

        #region Methods

        internal static void Harass()
        {
            var target = TargetSelector.GetTarget(Spells.R.Range, DamageType.Magical);
            if (target == null || !target.IsValidTarget(Spells.Q.Range)
                || Player.Instance.ManaPercent < Config.GetSliderValue(Config.Harass, "manaToHarass"))
            {
                return;
            }

            if (Config.IsChecked(Config.Harass, "useQInHarass") && Spells.Q.IsReady()
                && !target.HasBuffOfType(BuffType.Poison)
                && (Orbwalker.LastHitMinion == null && !Orbwalker.IsAutoAttacking))
            {
                var qPred = Spells.Q.GetPrediction(target);
                if (qPred.HitChancePercent >= 90)
                {
                    Spells.Q.Cast(qPred.CastPosition);
                }
            }

            if (Config.IsChecked(Config.Harass, "useWInHarass") && Spells.W.IsReady()
                && target.IsValidTarget(Spells.W.Range)
                && (Orbwalker.LastHitMinion == null && !Orbwalker.IsAutoAttacking))
            {
                if (Config.IsChecked(Config.Harass, "harassWonlyCD"))
                {
                    if (!Spells.Q.IsReady() && (Spells.QCasted - Game.Time) < -0.5f
                        && !target.HasBuffOfType(BuffType.Poison))
                    {
                        var wPred = Spells.W.GetPrediction(target);
                        if (wPred.CastPosition.Distance(Player.Instance.Position) >= Spells.WMinRange
                            && wPred.HitChancePercent >= 85)
                        {
                            Spells.W.Cast(wPred.CastPosition);
                        }
                    }
                }
                else
                {
                    var wPred = Spells.W.GetPrediction(target);
                    if (wPred.HitChancePercent >= 85)
                    {
                        Spells.W.Cast(wPred.CastPosition);
                    }
                }
            }

            if (Config.IsChecked(Config.Harass, "useEInHarass") && Spells.E.IsReady()
                && target.IsValidTarget(Spells.E.Range)
                && (!Config.IsChecked(Config.Harass, "harassEonP") || target.HasBuffOfType(BuffType.Poison)))
            {
                if (Config.IsChecked(Config.Harass, "humanEInHarass"))
                {
                    var delay = Computed.RandomDelay(Config.GetSliderValue(Config.Misc, "humanDelay"));
                    Core.DelayAction(() => Spells.E.Cast(target), delay);
                }
                else
                {
                    Spells.E.Cast(target);
                }
            }
        }

        internal static void JungleClear()
        {
            var minions = EntityManager.MinionsAndMonsters.Monsters.OrderByDescending(x => x.MaxHealth);
            if (!minions.Any() || Player.Instance.ManaPercent < Config.GetSliderValue(Config.JungleClear, "manaToJC"))
            {
                return;
            }

            if (Config.IsChecked(Config.JungleClear, "useQInJC") && Spells.Q.IsReady())
            {
                var qFarmLoc =
                    Computed.GetBestCircularFarmLocation(
                        minions.Where(m => m.Distance(Player.Instance) <= Spells.Q.Range)
                            .Select(mx => mx.ServerPosition.To2D())
                            .ToList(), 
                        Spells.Q.Width, 
                        Spells.Q.Range);
                if (qFarmLoc.MinionsHit > 0)
                {
                    Spells.Q.Cast(qFarmLoc.Position.To3D());
                }
            }

            if (Config.IsChecked(Config.JungleClear, "useWInJC") && Spells.W.IsReady())
            {
                var wFarmLoc =
                    Computed.GetBestCircularFarmLocation(
                        minions.Where(m => m.Distance(Player.Instance) <= Spells.W.Range)
                            .Select(mx => mx.ServerPosition.To2D())
                            .ToList(), 
                        Spells.W.Width, 
                        Spells.W.Range);
                if (wFarmLoc.MinionsHit >= Config.GetSliderValue(Config.LaneClear, "minWInLC")
                    && wFarmLoc.Position.To3D().Distance(Player.Instance.Position) >= Spells.WMinRange)
                {
                    Spells.W.Cast(wFarmLoc.Position.To3D());
                }
            }

            if (Config.IsChecked(Config.JungleClear, "useEInJC") && Spells.E.IsReady())
            {
                var jngToE =
                    EntityManager.MinionsAndMonsters.Monsters.OrderByDescending(x => x.MaxHealth)
                        .FirstOrDefault(
                            m =>
                            m.IsValidTarget(Spells.E.Range)
                            && (!Config.IsChecked(Config.JungleClear, "jungEonP") || m.HasBuffOfType(BuffType.Poison)));
                if (jngToE != null)
                {
                    Spells.E.Cast(jngToE);
                }
            }
        }

        internal static void LaneClear()
        {
            var minions = EntityManager.MinionsAndMonsters.EnemyMinions;
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            if (!objAiMinions.Any() || minions == null
                || Player.Instance.ManaPercent < Config.GetSliderValue(Config.LaneClear, "manaToLC"))
            {
                return;
            }

            if (Config.IsChecked(Config.LaneClear, "useQInLC") && Spells.Q.IsReady()
                && (Orbwalker.LastHitMinion == null && !Orbwalker.IsAutoAttacking))
            {
                var qFarmLoc =
                    Computed.GetBestCircularFarmLocation(
                        objAiMinions.Where(m => m.Distance(Player.Instance) <= Spells.Q.Range)
                            .Select(mx => mx.ServerPosition.To2D())
                            .ToList(), 
                        Spells.Q.Width, 
                        Spells.Q.Range);
                if (qFarmLoc.MinionsHit >= Config.GetSliderValue(Config.LaneClear, "minQInLC"))
                {
                    Spells.Q.Cast(qFarmLoc.Position.To3D());
                }
            }

            if (Config.IsChecked(Config.LaneClear, "useWInLC") && Spells.W.IsReady()
                && (Orbwalker.LastHitMinion == null && !Orbwalker.IsAutoAttacking))
            {
                var wFarmLoc =
                    Computed.GetBestCircularFarmLocation(
                        objAiMinions.Where(m => m.Distance(Player.Instance) <= Spells.W.Range)
                            .Select(mx => mx.ServerPosition.To2D())
                            .ToList(), 
                        Spells.W.Width, 
                        Spells.W.Range);
                if (wFarmLoc.MinionsHit >= Config.GetSliderValue(Config.LaneClear, "minWInLC")
                    && wFarmLoc.Position.To3D().Distance(Player.Instance.Position) >= Spells.WMinRange)
                {
                    Spells.W.Cast(wFarmLoc.Position.To3D());
                }
            }

            if (Config.IsChecked(Config.LaneClear, "useEInLC") && Spells.E.IsReady())
            {
                var minToE =
                    EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(
                        m =>
                        m.IsValidTarget(Spells.E.Range) && Spells.GetEDamage(m) > m.Health
                        && ((!Config.IsChecked(Config.LaneClear, "laneEonP") || m.HasBuffOfType(BuffType.Poison))
                            || (Config.IsChecked(Config.LaneClear, "useManaEInLC")
                                && Player.Instance.ManaPercent <= Config.GetSliderValue(Config.LaneClear, "manaEInLC"))));
                if (minToE != null)
                {
                    Spells.E.Cast(minToE);
                }
            }
        }

        internal static void PermActive()
        {
            if (Config.IsKeyPressed(Config.Misc, "assistedR"))
            {
                Computed.AssistedR();
            }

            if (Config.IsChecked(Config.Misc, "eKillSteal"))
            {
                Computed.KillSteal("E");
            }

            if (Config.IsChecked(Config.Harass, "autoEHarass")
                && Player.Instance.ManaPercent >= Config.GetSliderValue(Config.Harass, "manaToAutoHarass"))
            {
                Computed.AutoE();
            }

            if (Config.IsChecked(Config.Harass, "autoQHarass")
                && Player.Instance.ManaPercent >= Config.GetSliderValue(Config.Harass, "manaToAutoHarass"))
            {
                Computed.AutoQ();
            }

            if (Config.IsChecked(Config.Harass, "autoWHarass")
                && Player.Instance.ManaPercent >= Config.GetSliderValue(Config.Harass, "manaToAutoHarass"))
            {
                Computed.AutoW();
            }
        }

        #endregion
    }
}