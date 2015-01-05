﻿#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using Stack_Overflow.Utilitarios;

#endregion

namespace Stack_Overflow.Champions
{
    internal class Talon : Plugin
    {

        public Spell E;
        public Spell Q;
        public Spell W;
        public Spell R;

        private bool rCasted = false;

        public Talon()
        {
            Q = new Spell(SpellSlot.Q, 125);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 650);

            Q.SetSkillshot(0.25f, 60, 1450, false, SkillshotType.SkillshotLine);

            Game.OnGameUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;

            PrintChat("Talon Carregado.");

            Orbwalking.AfterAttack += AfterAttack;
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("drawQ");
            var drawW = GetBool("drawW");
            var drawE = GetBool("drawE");
            var drawR = GetBool("drawR");
            var p = Player.Position;

            if (drawQ)
                Utility.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);

            if (drawW)
                Utility.DrawCircle(p, W.Range, W.IsReady() ? Color.Aqua : Color.Red);

            if (drawE)
                Utility.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);

            if (drawR)
                Utility.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
            switch (OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;

                case Orbwalking.OrbwalkingMode.Combo:
                    Combar();
                    break;
            }

            if (!R.IsReady())
            {
                rCasted = false;
            }
        }

        private void Combar()
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);

            if (target == null)
                return;

            if (GetBool("comboR") && R.IsReady() && E.IsReady() && !rCasted)
            {
                R.Cast(Packets);
                rCasted = true;
            }

            if (GetBool("comboW") && W.IsReady())
            {
                W.CastIfHitchanceEquals(target, HitChance.Medium, Packets);
            }

            if (GetBool("comboE") && E.IsReady())
            {
                E.Cast(target, Packets);
                if (rCasted && GetBool("comboR"))
                {
                    R.Cast(Packets);
                    rCasted = false;
                }
            }

            var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
            var dmg = GetComboDamage(itemTarget);
            if (itemTarget != null)
            {
                ItemManager.Target = itemTarget;

                //see if killable
                if (dmg > itemTarget.Health - 50)
                    ItemManager.KillableTarget = true;

                ItemManager.UseTargetted = true;
            }

        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
            if (target == null)
                return;

            if (GetBool("harassW") && W.IsReady())
            {
                W.CastIfHitchanceEquals(target, HitChance.Medium, Packets);
            }

            if (GetBool("harassE") && E.IsReady())
            {
                E.Cast(target, Packets);
            }

        }

        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;

            if (Q.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1);

            if (W.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.W);

            if (E.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
                dmg += dmg * 0.2;
            }

            if (R.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.R);
                dmg += dmg * 0.2;
            }

            return (float)dmg;
        }

        public void AfterAttack(AttackableUnit unit, AttackableUnit mytarget)
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);

            if (OrbwalkerMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if(GetBool("harassQ") && Q.IsReady())
                {
                    Q.Cast(Packets);
                }
            }
            if (OrbwalkerMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (GetBool("comboQ") && Q.IsReady())
                {
                    Q.Cast(Packets);
                }
            }
        }

        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("comboQ", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("comboW", "Use W").SetValue(true));
            config.AddItem(new MenuItem("comboE", "Use E").SetValue(true));
            config.AddItem(new MenuItem("comboR", "Use R").SetValue(true));
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("harassQ", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("harassW", "Use W").SetValue(false));
            config.AddItem(new MenuItem("harassE", "Use E").SetValue(false));
        }

        public override void ItemMenu(Menu config)
        {
        }

        public override void Misc(Menu config)
        {
        }

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            config.AddItem(new MenuItem("drawW", "Draw W").SetValue(true));
            config.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            config.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));
        }
    }
}
