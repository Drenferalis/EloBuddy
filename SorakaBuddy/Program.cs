﻿namespace SorakaBuddy
{
    using System;
    using System.Drawing;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Events;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK.Rendering;

    internal class Program
    {
        /// <summary>
        /// Soraka's Name
        /// </summary>
        public const string ChampionName = "Soraka";

        /// <summary>
        /// Starcall
        /// </summary>
        public static Spell.Skillshot Q;

        /// <summary>
        /// Astral Infusion
        /// </summary>
        public static Spell.Targeted W;

        /// <summary>
        /// Equinox
        /// </summary>
        public static Spell.Skillshot E;

        /// <summary>
        /// Wish
        /// </summary>
        public static Spell.Active R;

        /// <summary>
        /// Initializes the Menu
        /// </summary>
        public static Menu SorakaBuddy, ComboMenu, HarassMenu, HealMenu, InterruptMenu, GapcloserMenu, DrawingMenu, MiscMenu;

        /// <summary>
        /// Gets the Player
        /// </summary>
        public static AIHeroClient PlayerInstance
        {
            get { return Player.Instance; }
        }

        /// <summary>
        /// Runs when the Program Starts
        /// </summary>
        private static void Main()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        /// <summary>
        /// Called when Loading is Completed
        /// </summary>
        /// <param name="args">The loading arguments</param>
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            try
            {
                if (ChampionName != PlayerInstance.BaseSkinName)
                {
                    return;
                }

                Bootstrap.Init(null);

                Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Circular, 283, 1100, 210);
                W = new Spell.Targeted(SpellSlot.W, 550);
                E = new Spell.Skillshot(SpellSlot.E, 925, SkillShotType.Circular, 500, 1750, 70);
                R = new Spell.Active(SpellSlot.R, int.MaxValue);

                SorakaBuddy = MainMenu.AddMenu("SorakaBuddy", "SorakaBuddy");

                // Combo Menu
                ComboMenu = SorakaBuddy.AddSubMenu("Combo", "Combo");
                ComboMenu.AddGroupLabel("Combo Setting");
                ComboMenu.Add("useQ", new CheckBox("Use Q"));
                ComboMenu.Add("useE", new CheckBox("Use E"));
                ComboMenu.AddSeparator();
                ComboMenu.AddGroupLabel("ManaManager");
                ComboMenu.Add("manaQ", new Slider("Min Mana % before Q", 25));
                ComboMenu.Add("manaE", new Slider("Min Mana % before E", 25));

                // Harass Menu
                HarassMenu = SorakaBuddy.AddSubMenu("Harass", "Harass");
                HarassMenu.AddGroupLabel("Harass Setting");
                HarassMenu.Add("useQ", new CheckBox("Use Q"));
                HarassMenu.Add("useE", new CheckBox("Use E"));
                HarassMenu.AddSeparator();
                HarassMenu.AddGroupLabel("ManaManager");
                HarassMenu.Add("manaQ", new Slider("Min Mana % before Q", 25));
                HarassMenu.Add("manaE", new Slider("Min Mana % before E", 25));

                // Heal Menu
                var allies = EntityManager.Heroes.Allies.Where(a => !a.IsMe);
                HealMenu = SorakaBuddy.AddSubMenu("Auto Heal", "Heal");
                HealMenu.AddGroupLabel("Auto W Setting");
                HealMenu.Add("autoW", new CheckBox("Auto W Allies and Me"));
                HealMenu.Add("autoWHP_self", new Slider("Own HP % before using W", 50));
                HealMenu.Add("autoWHP_other", new Slider("Ally HP % before W", 50));
                HealMenu.AddSeparator();
                HealMenu.AddGroupLabel("Auto R Setting");
                HealMenu.Add("useR", new CheckBox("Auto R on HP %"));
                HealMenu.AddSeparator();
                HealMenu.Add("hpR", new Slider("HP % before using R", 25));
                HealMenu.AddSeparator();
                HealMenu.AddLabel("Which Champion to Heal? Using W?");
                foreach (var a in allies)
                {
                    HealMenu.Add("autoHeal_" + a.BaseSkinName, new CheckBox("Auto Heal with W " + a.BaseSkinName));
                }
                HealMenu.AddSeparator();
                HealMenu.AddLabel("Which Champion to Heal? Using R?");
                foreach (var a in allies)
                {
                    HealMenu.Add("autoHealR_" + a.BaseSkinName, new CheckBox("Auto Heal with R " + a.BaseSkinName));
                }
                HealMenu.Add("autoHealR_" + PlayerInstance.BaseSkinName, new CheckBox("Auto Heal Self with R"));
                HealMenu.AddSeparator();
                HealMenu.AddGroupLabel("Heal Priority");
                var healPrioritySlider = HealMenu.Add("Slider", new Slider("mode", 0, 0, 2));
                var healPriorityArray = new[] { "Most AD", "Most AP", "Lowest Health" };
                healPrioritySlider.DisplayName = healPriorityArray[healPrioritySlider.CurrentValue];
                healPrioritySlider.OnValueChange += delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = healPriorityArray[changeArgs.NewValue];
                };

                // Interrupt Menu
                InterruptMenu = SorakaBuddy.AddSubMenu("Interrupter", "Interrupter");
                InterruptMenu.AddGroupLabel("Interrupter Setting");
                InterruptMenu.Add("useE", new CheckBox("Use E on Interrupt"));

                // Gapcloser Menu
                GapcloserMenu = SorakaBuddy.AddSubMenu("Gapcloser", "Gapcloser");
                GapcloserMenu.AddGroupLabel("Gapcloser Setting");
                GapcloserMenu.Add("useQ", new CheckBox("Use Q on Gapcloser"));
                GapcloserMenu.Add("useE", new CheckBox("Use E on Gapcloser"));

                // Drawing Menu
                DrawingMenu = SorakaBuddy.AddSubMenu("Drawing", "Drawing");
                DrawingMenu.AddGroupLabel("Drawing Setting");
                DrawingMenu.Add("drawQ", new CheckBox("Draw Q Range"));
                DrawingMenu.Add("drawW", new CheckBox("Draw W Range"));
                DrawingMenu.Add("drawE", new CheckBox("Draw E Range"));

                // Misc Menu
                MiscMenu = SorakaBuddy.AddSubMenu("Misc", "Misc");
                MiscMenu.AddGroupLabel("Miscellaneous Setting");
                MiscMenu.Add("disableMAA", new CheckBox("Disable Minion AA"));
                MiscMenu.Add("disableCAA", new CheckBox("Disable Champion AA"));

                Chat.Print("SorakaBuddy: Initialized", Color.LightGreen);

                Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
                Game.OnTick += Game_OnTick;
                Drawing.OnDraw += Drawing_OnDraw;

                Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
                Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            }
            catch (Exception e)
            {
                Chat.Print("SorakaBuddy: Exception occured while Initializing Addon. Error: " + e.Message);
            }
        }

        /// <summary>
        /// Does Combo
        /// </summary>
        private static void Combo()
        {
            if (ComboMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, PlayerInstance.ServerPosition);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= ComboMenu["manaQ"].Cast<Slider>().CurrentValue
                        && (target.IsValidTarget(Q.Range) && Q.IsReady()))
                    {
                        var pred = Q.GetPrediction(target);
                            //Prediction.Position.PredictCircularMissile(target, Q.Range, Q.Radius, Q.CastDelay, Q.Speed, PlayerInstance.ServerPosition); 

                        if (pred.HitChance == HitChance.High)
                        {
                            Q.Cast(pred.CastPosition);
                        }
                    }
                }
            }
            if (ComboMenu["useE"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical, PlayerInstance.ServerPosition);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= ComboMenu["manaE"].Cast<Slider>().CurrentValue
                        && (target.IsValidTarget(E.Range) && E.IsReady()))
                    {
                        var pred = E.GetPrediction(target);
                            //Prediction.Position.PredictCircularMissile(target, E.Range, E.Radius, E.CastDelay, E.Speed, PlayerInstance.Position);

                        if (pred.HitChance == HitChance.High)
                        {
                            E.Cast(pred.CastPosition);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Does Harass
        /// </summary>
        private static void Harass()
        {
            if (HarassMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, PlayerInstance.ServerPosition);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= HarassMenu["manaQ"].Cast<Slider>().CurrentValue)
                    {
                        if (target.IsValidTarget(Q.Range) && Q.IsReady())
                        {
                            var pred = Q.GetPrediction(target); // Prediction.Position.PredictCircularMissile(target, Q.Range, Q.Radius, Q.CastDelay, Q.Speed, PlayerInstance.Position);

                            if (pred.HitChance == HitChance.High)
                            {
                                Q.Cast(pred.CastPosition);
                            }
                        }
                    }
                }
            }
            if (HarassMenu["useE"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical, PlayerInstance.ServerPosition);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= HarassMenu["manaE"].Cast<Slider>().CurrentValue)
                    {
                        if (target.IsValidTarget(E.Range) && E.IsReady())
                        {
                            var pred = E.GetPrediction(target); //Prediction.Position.PredictCircularMissile(target, E.Range, E.Radius, E.CastDelay, E.Speed, PlayerInstance.Position);

                            if (pred.HitChance == HitChance.High)
                            {
                                E.Cast(pred.CastPosition);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Does Auto Heal
        /// </summary>
        private static void AutoHeal()
        {
            var healPrioritySlider = HealMenu["Slider"].Cast<Slider>().DisplayName;
            var autoWhpOther = HealMenu["autoWHP_other"].Cast<Slider>().CurrentValue;
            var autoWhpSelf = HealMenu["autoWHP_self"].Cast<Slider>().CurrentValue;
            var Recall = PlayerInstance.HasBuff("Recall");

            switch (healPrioritySlider)
            {
                case "Most AD":
                    if (HealMenu["autoW"].Cast<CheckBox>().CurrentValue && W.IsReady())
                    {
                        var mostAdAlly = EntityManager.Heroes.Allies.Where(a => W.IsInRange(a) && !a.IsMe).OrderBy(a => a.TotalAttackDamage).ThenBy(a => a.Health).FirstOrDefault();

                        if (mostAdAlly != null)
                        {
                            if (mostAdAlly.HealthPercent <= autoWhpOther
                                && PlayerInstance.HealthPercent >= autoWhpSelf)
                            {
                                if (HealMenu["autoHeal_" + mostAdAlly.BaseSkinName].Cast<CheckBox>().CurrentValue && !Recall)
                                {
                                    W.Cast(mostAdAlly);
                                }
                            }
                        }
                    }
                    if (HealMenu["useR"].Cast<CheckBox>().CurrentValue && R.IsReady())
                    {
                        var mostAdAllyOor = EntityManager.Heroes.Allies.OrderByDescending(a => a.TotalAttackDamage).ThenBy(a => a.Health).FirstOrDefault();

                        if (mostAdAllyOor != null)
                        {
                            if (mostAdAllyOor.HealthPercent <= HealMenu["hpR"].Cast<Slider>().CurrentValue)
                            {
                                if (HealMenu["autoHealR_" + mostAdAllyOor.BaseSkinName].Cast<CheckBox>().CurrentValue && !Recall)
                                {
                                    R.Cast();
                                }
                            }
                        }
                    }
                    break;
                case "Most AP":
                    if (HealMenu["autoW"].Cast<CheckBox>().CurrentValue && W.IsReady())
                    {
                        var mostApAlly = EntityManager.Heroes.Allies.Where(a => W.IsInRange(a) && !a.IsMe).OrderBy(a => a.TotalMagicalDamage).ThenBy(a => a.Health).FirstOrDefault();

                        if (mostApAlly != null)
                        {
                            if (mostApAlly.HealthPercent <= autoWhpOther
                                && PlayerInstance.HealthPercent >= autoWhpSelf)
                            {
                                if (HealMenu["autoHeal_" + mostApAlly.BaseSkinName].Cast<CheckBox>().CurrentValue && !Recall)
                                {
                                    W.Cast(mostApAlly);
                                }
                            }
                        }
                    }
                    if (HealMenu["useR"].Cast<CheckBox>().CurrentValue && R.IsReady())
                    {
                        var mostApAllyOor = EntityManager.Heroes.Allies.OrderByDescending(a => a.TotalMagicalDamage).ThenBy(a => a.Health).FirstOrDefault();

                        if (mostApAllyOor != null)
                        {
                            if (mostApAllyOor.HealthPercent <= HealMenu["hpR"].Cast<Slider>().CurrentValue)
                            {
                                if (HealMenu["autoHealR_" + mostApAllyOor.BaseSkinName].Cast<CheckBox>().CurrentValue && !Recall)
                                {
                                    R.Cast();
                                }
                            }
                        }
                    }
                    break;
                case "Lowest Health":
                    if (HealMenu["autoW"].Cast<CheckBox>().CurrentValue && W.IsReady())
                    {
                        var lowestHealthAlly = EntityManager.Heroes.Allies.Where(a => W.IsInRange(a) && !a.IsMe).OrderBy(a => a.Health).FirstOrDefault();

                        if (lowestHealthAlly != null)
                        {
                            if (lowestHealthAlly.HealthPercent <= autoWhpOther
                                && PlayerInstance.HealthPercent >= autoWhpSelf)
                            {
                                if (HealMenu["autoHeal_" + lowestHealthAlly.BaseSkinName].Cast<CheckBox>().CurrentValue && !Recall)
                                {
                                    W.Cast(lowestHealthAlly);
                                }
                            }
                        }
                    }
                    if (HealMenu["useR"].Cast<CheckBox>().CurrentValue && R.IsReady())
                    {
                        var lowestHealthAllyOor = EntityManager.Heroes.Allies.OrderByDescending(a => a.Health).FirstOrDefault();

                        if (lowestHealthAllyOor != null)
                        {
                            if (lowestHealthAllyOor.HealthPercent <= HealMenu["hpR"].Cast<Slider>().CurrentValue)
                            {
                                if (HealMenu["autoHealR_" + lowestHealthAllyOor.BaseSkinName].Cast<CheckBox>().CurrentValue && !Recall)
                                {
                                    R.Cast();
                                }
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Called when Sender is Interruptable
        /// </summary>
        /// <param name="sender">The Attacker</param>
        /// <param name="e">The Spell Information</param>
        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (InterruptMenu["useE"].Cast<CheckBox>().CurrentValue || e.DangerLevel == DangerLevel.High)
            {
                if (sender.IsValidTarget(E.Range) && E.IsReady())
                {
                    var pred = E.GetPrediction(sender);

                    if (pred.HitChance == HitChance.High)
                    {
                        E.Cast(pred.CastPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Called when Sender can be Gapclosed
        /// </summary>
        /// <param name="sender">The Victim</param>
        /// <param name="e">The Spell Information</param>
        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (GapcloserMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                if (sender.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    var pred = Q.GetPrediction(sender);

                    if (pred.HitChance == HitChance.High)
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }

            if (GapcloserMenu["useE"].Cast<CheckBox>().CurrentValue)
            {
                if (sender.IsValidTarget(E.Range) && E.IsReady())
                {
                    var pred = E.GetPrediction(sender);

                    if (pred.HitChance == HitChance.High)
                    {
                        E.Cast(pred.CastPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Before Attack
        /// </summary>
        /// <param name="target">The Target that will be attacked</param>
        /// <param name="args">The Args</param>
        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            var t = target as AIHeroClient;
            var m = target as Obj_AI_Base;

            if (t != null)
            {
                var alliesNearPlayer = EntityManager.Heroes.Allies.Count(a => PlayerInstance.Distance(a) <= PlayerInstance.AttackRange);

                if (alliesNearPlayer <= 1)
                {
                    return;
                }
                if (MiscMenu["disableCAA"].Cast<CheckBox>().CurrentValue)
                {
                    args.Process = false;
                }
            }

            if (m != null)
            {
                var alliesNearPlayer = EntityManager.Heroes.Allies.Count(a => PlayerInstance.Distance(a) <= PlayerInstance.AttackRange);
                if (alliesNearPlayer <= 1)
                {
                    return;
                }
                if (MiscMenu["disableMAA"].Cast<CheckBox>().CurrentValue)
                {
                    args.Process = false;
                }
            }
        }

        /// <summary>
        /// Called when the Game Updates
        /// </summary>
        /// <param name="args">The Args</param>
        private static void Game_OnTick(EventArgs args)
        {
            if (HealMenu["autoW"].Cast<CheckBox>().CurrentValue
                || HealMenu["autoR"].Cast<CheckBox>().CurrentValue)
            {
                AutoHeal();
            }
            
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
        }
        
        /// <summary>
        /// Called when the Game Draws
        /// </summary>
        /// <param name="args">The Args</param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            var PlayerPosition = PlayerInstance.Position;

            if (DrawingMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Q.IsReady() ? SharpDX.Color.Green : SharpDX.Color.Red, Q.Range, PlayerPosition);
            }
            if (DrawingMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? SharpDX.Color.Green : SharpDX.Color.Red, W.Range, PlayerPosition);
            }
            if (DrawingMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(E.IsReady() ? SharpDX.Color.Green : SharpDX.Color.Red, E.Range, PlayerPosition);
            }
        }
    }
}
