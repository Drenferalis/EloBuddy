﻿namespace SorakaBuddy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Drawing;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Events;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;
    using SharpDX;

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
        /// <param name="args">The run arguments</param>
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        /// <summary>
        /// Called when Loading is Completed
        /// </summary>
        /// <param name="args">The loading arguments</param>
        static void Loading_OnLoadingComplete(EventArgs args)
        {
            try
            {
                if (ChampionName != PlayerInstance.BaseSkinName)
                {
                    return;
                }

                Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Circular, 283, 1100, 210);
                W = new Spell.Targeted(SpellSlot.W, 550);
                E = new Spell.Skillshot(SpellSlot.E, 925, SkillShotType.Circular, 500, 1750, 70);
                R = new Spell.Active(SpellSlot.R, int.MaxValue);

                SorakaBuddy = MainMenu.AddMenu("SorakaBuddy", "SorakaBuddy");

                // Combo Menu
                ComboMenu = SorakaBuddy.AddSubMenu("Combo", "Combo");
                ComboMenu.AddGroupLabel("Combo Setting");
                ComboMenu.Add("useQ", new CheckBox("Use Q", true));
                ComboMenu.Add("useE", new CheckBox("Use E", true));
                ComboMenu.AddSeparator();
                ComboMenu.AddGroupLabel("ManaManager");
                ComboMenu.Add("manaQ", new Slider("Min Mana % before Q", 25, 0, 100));
                ComboMenu.Add("manaE", new Slider("Min Mana % before E", 25, 0, 100));

                // Lane Clear Menu
                /*LaneClearMenu = SorakaBuddy.AddSubMenu("Lane Clear", "LaneClear");
                LaneClearMenu.Add("useQ", new CheckBox("Use Q", true));*/

                // Harass Menu
                HarassMenu = SorakaBuddy.AddSubMenu("Harass", "Harass");
                HarassMenu.AddGroupLabel("Harass Setting");
                HarassMenu.Add("useQ", new CheckBox("Use Q", true));
                HarassMenu.Add("useE", new CheckBox("Use E", true));
                HarassMenu.AddSeparator();
                HarassMenu.AddGroupLabel("ManaManager");
                HarassMenu.Add("manaQ", new Slider("Min Mana % before Q", 25, 0, 100));
                HarassMenu.Add("manaE", new Slider("Min Mana % before E", 25, 0, 100));

                // Heal Menu
                var Allies = HeroManager.Allies.Where(a => !a.IsMe);
                HealMenu = SorakaBuddy.AddSubMenu("Auto Heal", "Heal");
                HealMenu.AddGroupLabel("Auto W Setting");
                HealMenu.Add("autoW", new CheckBox("Auto W Allies and Me", true));
                HealMenu.Add("autoWHP_self", new Slider("Own HP % before using W", 50, 0, 100));
                HealMenu.Add("autoWHP_other", new Slider("Ally HP % before W", 50, 0, 100));
                HealMenu.AddSeparator();
                HealMenu.AddGroupLabel("Auto R Setting");
                HealMenu.Add("useR", new CheckBox("Auto R on HP %", true));
                HealMenu.AddSeparator();
                HealMenu.Add("hpR", new Slider("HP % before using R", 25, 0, 100));
                HealMenu.AddSeparator();
                HealMenu.AddLabel("Which Champion to Heal?");
                if (Allies != null)
                {
                    foreach (var a in Allies)
                    {
                        HealMenu.Add("autoHeal_" + a.BaseSkinName, new CheckBox("Auto Heal " + a.BaseSkinName, true));
                    }
                }
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
                InterruptMenu.Add("useE", new CheckBox("Use E on Interrupt", true));

                // Gapcloser Menu
                GapcloserMenu = SorakaBuddy.AddSubMenu("Gapcloser", "Gapcloser");
                GapcloserMenu.AddGroupLabel("Gapcloser Setting");
                GapcloserMenu.Add("useQ", new CheckBox("Use Q on Gapcloser", true));
                GapcloserMenu.Add("useE", new CheckBox("Use E on Gapcloser", true));

                // Drawing Menu
                DrawingMenu = SorakaBuddy.AddSubMenu("Drawing", "Drawing");
                DrawingMenu.AddGroupLabel("Drawing Setting");
                DrawingMenu.Add("drawQ", new CheckBox("Draw Q Range", true));
                DrawingMenu.Add("drawW", new CheckBox("Draw W Range", true));
                DrawingMenu.Add("drawE", new CheckBox("Draw E Range", true));

                // Misc Menu
                MiscMenu = SorakaBuddy.AddSubMenu("Misc", "Misc");
                MiscMenu.AddGroupLabel("Miscellaneous Setting");
                MiscMenu.Add("disableMAA", new CheckBox("Disable Minion AA", true));
                MiscMenu.Add("disableCAA", new CheckBox("Disable Champion AA", true));

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
        static void Combo()
        {
            if (ComboMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= ComboMenu["manaQ"].Cast<Slider>().CurrentValue)
                    {
                        if (target.IsValidTarget(Q.Range) && Q.IsReady())
                        {
                            var pred = Q.GetPrediction(target);

                            if (pred.HitChance == HitChance.High)
                            {
                                Q.Cast(pred.CastPosition);
                            }
                        }
                    }
                }
            }
            if (ComboMenu["useE"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= ComboMenu["manaE"].Cast<Slider>().CurrentValue)
                    {
                        if (target.IsValidTarget(E.Range) && E.IsReady())
                        {
                            var pred = E.GetPrediction(target);

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
        /// Does Harass
        /// </summary>
        static void Harass()
        {
            if (HarassMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= HarassMenu["manaQ"].Cast<Slider>().CurrentValue)
                    {
                        if (target.IsValidTarget(Q.Range) && Q.IsReady())
                        {
                            var pred = Q.GetPrediction(target);

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
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= HarassMenu["manaE"].Cast<Slider>().CurrentValue)
                    {
                        if (target.IsValidTarget(E.Range) && E.IsReady())
                        {
                            var pred = E.GetPrediction(target);

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
        /// Does LaneClear (Disabled ATM)
        /// </summary>
        static void LaneClear()
        {
            return;
        }

        /// <summary>
        /// Does Auto Heal
        /// </summary>
        static void AutoHeal()
        {
            var MostADAlly = HeroManager.Allies.Where(a => W.IsInRange(a) && !a.IsMe).OrderBy(a => a.TotalAttackDamage).FirstOrDefault();
            var MostAPAlly = HeroManager.Allies.Where(a => W.IsInRange(a) && !a.IsMe).OrderBy(a => a.TotalMagicalDamage).FirstOrDefault();
            var LowestHealthAlly = HeroManager.Allies.Where(a => W.IsInRange(a) && !a.IsMe).OrderBy(a => a.Health).FirstOrDefault();
            var healPrioritySlider = HealMenu["healPrioritySlider"].Cast<Slider>().DisplayName;
            
            var autoWHP_other = HealMenu["autoWHP_other"].Cast<Slider>().CurrentValue;
            var autoWHP_self = HealMenu["autoWHP_self"].Cast<Slider>().CurrentValue;
            var Recall = Player.HasBuff("Recall");

            switch (healPrioritySlider)
            {
                case "Most AD":
                    if (MostADAlly != null)
                    {
                        if (MostADAlly.Health <= autoWHP_other
                            && PlayerInstance.Health >= autoWHP_self)
                        {
                            if (HealMenu["autoHeal_" + MostADAlly.BaseSkinName].Cast<CheckBox>().CurrentValue && !Recall)
                            {
                                W.Cast(MostADAlly);
                            }
                        }
                    }
                    else if (HealMenu["useR"].Cast<CheckBox>().CurrentValue)
                    {
                        var MostADAllyOOR = HeroManager.Allies.Where(a => !a.IsMe).OrderBy(a => a.TotalAttackDamage).FirstOrDefault();

                        if (MostADAllyOOR.Health <= HealMenu["hpR"].Cast<Slider>().CurrentValue)
                        {
                            if (HealMenu["autoHeal_" + MostADAllyOOR.BaseSkinName].Cast<CheckBox>().CurrentValue && !Player.HasBuff("Recall"))
                            {
                                R.Cast();
                            }
                        }
                    }
                    break;
                case "Most AP":
                    if (MostAPAlly != null)
                    {
                        if (MostAPAlly.Health <= autoWHP_other
                            && PlayerInstance.Health >= autoWHP_self)
                        {
                            if (HealMenu["autoHeal_" + MostAPAlly.BaseSkinName].Cast<CheckBox>().CurrentValue && !Recall)
                            {
                                W.Cast(MostAPAlly);
                            }
                        }
                    }
                    else if (HealMenu["useR"].Cast<CheckBox>().CurrentValue)
                    {
                        var MostAPAllyOOR = HeroManager.Allies.Where(a => !a.IsMe).OrderBy(a => a.TotalMagicalDamage).FirstOrDefault();

                        if (MostAPAllyOOR.Health <= HealMenu["hpR"].Cast<Slider>().CurrentValue)
                        {
                            if (HealMenu["autoHeal_" + MostAPAllyOOR.BaseSkinName].Cast<CheckBox>().CurrentValue && !Player.HasBuff("Recall"))
                            {
                                R.Cast();
                            }
                        }
                    }
                    break;
                case "Lowest Health":
                    if (LowestHealthAlly != null)
                    {
                        if (LowestHealthAlly.Health <= autoWHP_other
                            && PlayerInstance.Health >= autoWHP_self)
                        {
                            if (HealMenu["autoHeal_" + LowestHealthAlly.BaseSkinName].Cast<CheckBox>().CurrentValue && !Recall)
                            {
                                W.Cast(LowestHealthAlly);
                            }
                        }
                    }
                    else if (HealMenu["useR"].Cast<CheckBox>().CurrentValue)
                    {
                        var LowestHealthAllyOOR = HeroManager.Allies.Where(a => !a.IsMe).OrderBy(a => a.Health).FirstOrDefault();

                        if (LowestHealthAllyOOR.Health <= HealMenu["hpR"].Cast<Slider>().CurrentValue)
                        {
                            if (HealMenu["autoHeal_" + LowestHealthAllyOOR.BaseSkinName].Cast<CheckBox>().CurrentValue && !Player.HasBuff("Recall"))
                            {
                                R.Cast();
                            }
                        }
                    }
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Called when Sender is Interruptable
        /// </summary>
        /// <param name="sender">The Attacker</param>
        /// <param name="e">The Spell Information</param>
        static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (InterruptMenu["useE"].Cast<CheckBox>().CurrentValue || e.DangerLevel == DangerLevel.High)
            {
                if (sender.IsValidTarget(E.Range))
                {
                    if (E.IsReady())
                    {
                        var pred = E.GetPrediction(sender);

                        if (pred.HitChance == HitChance.High)
                        {
                            E.Cast(pred.CastPosition);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when Sender can be Gapclosed
        /// </summary>
        /// <param name="sender">The Victim</param>
        /// <param name="e">The Spell Information</param>
        static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
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
        static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            var t = target as AIHeroClient;
            var m = target as Obj_AI_Base;

            if (t != null)
            {
                if (MiscMenu["disableCAA"].Cast<CheckBox>().CurrentValue)
                {
                    args.Process = false;
                }
            }
            else if (m != null)
            {
                if (MiscMenu["disableMAA"].Cast<CheckBox>().CurrentValue)
                {
                    args.Process = false;
                }
            }
            else
            {
                return;
            }
        }

        static void Game_OnTick(EventArgs args)
        {
            switch(Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    Harass();
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    LaneClear();
                    break;
                case Orbwalker.ActiveModes.None:
                    AutoHeal();
                    break;
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var PlayerPosition = PlayerInstance.Position;

            if (DrawingMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(PlayerPosition, Q.Range, Q.IsReady() ? System.Drawing.Color.Green : System.Drawing.Color.Red);
            }
            if (DrawingMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(PlayerPosition, W.Range, W.IsReady() ? System.Drawing.Color.Green : System.Drawing.Color.Red);
            }
            if (DrawingMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(PlayerPosition, E.Range, E.IsReady() ? System.Drawing.Color.Green : System.Drawing.Color.Red);
            }
        }
    }
}
