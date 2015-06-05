using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using ReBot.API;

namespace ReBot
{
	public abstract class SerbPaladin : SerbUtils
	{
		// Consts && Vars

		public enum Menu
		{
			EFBlanket,
			Ultimate,
			Aggressive,
			Normal,
			Conservative,
			Auto,
		}

		Menu _choice;

		public UnitObject LastJudgmentTarget;

		public void SetChoice (Menu c = Menu.Normal)
		{
			_choice = c;
		}

		// Get

		public int MaxHolyPower {
			get {
				if (HasSpell ("Boundless Conviction"))
					return 5;
				return 3;
			}
		}

		public Menu Playstyle {
			get {
				if (_choice == Menu.Auto) {
					if (Me.ManaFraction >= 0.85)
						return Menu.Aggressive;
					if (Me.ManaFraction >= 0.45)
						return Menu.Normal;
					else
						return Menu.Conservative;
				}
				return _choice;
			}
		}

		public double TimeToHpg {
			get {
				if (HasGlobalCooldown ())
					return 1.5;
				return 0.35 + 1.5;
			}
		}

		public string SealSpell {
			get {
				if (HasSpell ("Seal of Insight"))
					return "Seal of Insight";
				else
					return "Seal of Command";
			}
		}

		public double FL {
			get {
				switch (Playstyle) {
				case Menu.EFBlanket:
					return 0;
				case Menu.Ultimate:
					return 0.65;
				case Menu.Aggressive:
					return 0.55;			 
				case Menu.Normal:
				default:
					return 0.50;
				case Menu.Conservative:
					return 0.45;
				}
			}
		}

		public double HL {
			get {
				switch (Playstyle) {
				case Menu.EFBlanket:
					return 0;
				case Menu.Ultimate:
					return 0.95;
				case Menu.Aggressive:
					return 0.85;			
				case Menu.Normal:
				default:
					return 0.80;
				case Menu.Conservative:
					return 0.75;
				}
			}
		}

		public double HoP {
			get {
				switch (Playstyle) {
				case Menu.EFBlanket:
					return 0;
				case Menu.Ultimate:
					return 0.35;
				case Menu.Aggressive:
					return 0.25;			
				case Menu.Normal:
				default:
					return 0.20;
				case Menu.Conservative:
					return 0.15;
				}
			}
		}

		public double HoS {
			get {
				switch (Playstyle) {
				case Menu.EFBlanket:
					return 0;
				case Menu.Ultimate:
					return 0.60;
				case Menu.Aggressive:
					return 0.50;			
				case Menu.Normal:
				default:
					return 0.45;
				case Menu.Conservative:
					return 0.40;
				}
			}
		}

		public double HR {
			get {
				switch (Playstyle) {
				case Menu.EFBlanket:
					return 0;
				case Menu.Ultimate:
					return 0.80;
				case Menu.Aggressive:
					return 0.70;		
				case Menu.Normal:
				default:
					return 0.65;
				case Menu.Conservative:
					return 0.60;
				}
			}
		}

		// Check

		public bool HasBuff (UnitObject u = null)
		{
			u = u ?? Me;
			return u.HasAura ("Blessing of Kings") || u.HasAura ("Legacy of the Emperor") || u.HasAura ("Mark of the Wild");
		}

		// Combo

		public bool Clean (UnitObject u = null)
		{
			u = u ?? Me;
			if (Range (40, u) && u.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType))) {
				if (Cleanse (u))
					return true;
			}
			return false;
		}

		public bool Buff (UnitObject u = null)
		{
			u = u ?? Me;
			if (HasBuff (u)) {
				if (!u.HasAura ("Blessing of Kings", true) && !u.HasAura ("Blessing of Might")) {
					if (BlessingofMight ())
						return true;
				}
			} else if (u.HasAura ("Blessing of Might")) {
				if (!u.HasAura ("Blessing of Might", true) && !u.HasAura ("Blessing of Kings")) {
					if (BlessingofKings ())
						return true;
				}
			} else {
				if (BlessingofKings ())
					return true;
			}
			return false;
		}

		public bool Heal ()
		{
			if (Health (Me) <= 0.5 && AuraStackCount ("Selfless Healer") >= 3) {
				if (FlashofLight (Me))
					return true;
			}
			if (Health (Me) <= 0.9 && Me.HasAura ("Hand of Protection")) {
				if (FlashofLight (Me))
					return true;
			}
			if (Health (Me) <= 0.8 && HolyPower >= 3) {
				if (WordofGlory (Me))
					return true;
			}
			if (Health (Me) <= 0.55 && HolyPower >= 2) {
				if (WordofGlory (Me))
					return true;
			}
			if (Health (Me) <= 0.3 && HolyPower >= 1) {
				if (WordofGlory (Me))
					return true;
			}
//			if (CastSelf ("Flash of Light", () => Health <= 0.6 && Me.HasAura ("Divine Shield") && TargetHealth >= 0.15))
//				return;
//			if (Me.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType))) {
//				if (Cleanse (Me))
//					return true;
//			}
			if (Health (Me) <= 0.3 && !Me.HasAura ("Immunity")) {
				if (DivineShield ())
					return true;
			}
			if (Health (Me) <= 0.2 && !Me.HasAura ("Divine Shield") && !Me.HasAura ("Immunity")) {
				if (LayonHands (Me))
					return true;
			}
			if (Health (Me) <= 0.6 && Target.IsCasting && !Me.HasAura ("Divine Shield")) {
				if (DivineProtection ())
					return true;
			}
			if (Health (Me) <= 0.15 && !Me.HasAura ("Immunity") && Cooldown ("Lay on Hands") > 1 && Cooldown ("Divine Shield") > 1) {
				if (HandofProtection (Me))
					return true;
			}

			return false;
		}

		public bool Interrupt ()
		{
			if (InArena || InBg) {
				if (Usable ("Rebuke")) {
					Player = API.Players.Where (x => x.IsPlayer && x.IsEnemy && x.IsHealer && Range (5, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Player != null && Rebuke (Player))
						return true; 
					Player = API.Players.Where (x => x.IsPlayer && x.IsEnemy && Range (5, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Player != null && Rebuke (Player))
						return true; 
				}
				if (Cooldown ("Fist of Justice") == 0) {
					Player = API.Players.Where (x => x.IsPlayer && x.IsEnemy && x.IsHealer && Range (20, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Player != null && FistofJustice (Player))
						return true;
					Player = API.Players.Where (x => x.IsPlayer && x.IsEnemy && Range (20, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Player != null && FistofJustice (Player))
						return true;
				}
			} else {
				Unit = Enemy.Where (x => Range (5, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Rebuke (Unit))
					return true; 
				if (Cooldown ("Fist of Justice") == 0) {
					Unit = Enemy.Where (x => !IsBoss (x) && Range (20, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && FistofJustice (Unit))
						return true;
				}
			}

			return false;
		}

		// Healer

		public bool UseBeaconofLight ()
		{
			if (Usable ("Beacon of Light")) {
				if (Me.Focus != null) {
					if (Me.Focus.IsFriendly && Range (60, Me.Focus) && !Me.Focus.HasAura ("Beacon of Light", true)) {
						if (BeaconofLight (Me.Focus))
							return true;
					}
				} else if (Tank != null) {
					if (!Tank.HasAura ("Beacon of Light", true)) {
						if (BeaconofLight (Tank))
							return true;
					}
				} else if (LowestPlayer != null) {
					if (!LowestPlayer.HasAura ("Beacon of Light", true) && !LowestPlayer.HasAura ("Beacon of Faith", true)) {
						if (BeaconofLight (LowestPlayer))
							return true;
					}
				}
			}
			return false;
		}

		public bool UseSacredShield ()
		{
			if (Usable ("Sacred Shield")) {
				if (Me.Focus != null) {
					if (Me.Focus.IsFriendly && Range (40, Me.Focus) && !Me.Focus.HasAura ("Sacred Shield", true)) {
						if (SacredShield (Me.Focus))
							return true;
					}
				} else if (Tank != null) {
					if (!Tank.HasAura ("Sacred Shield", true)) {
						if (SacredShield (Tank))
							return true;
					}
				} else {
					if (!Me.HasAura ("Sacred Shield", true)) {
						if (SacredShield (Me))
							return true;
					}
				}

				if (LowestPlayer != null) {
					if (!LowestPlayer.HasAura ("Sacred Shield", true)) {
						if (SacredShield (LowestPlayer))
							return true;
					}
				}
			}
			return false;
		}

		public bool UseEternalFlame ()
		{
			if (HolyPower > 0 && Usable ("Eternal Flame")) {
				if (Me.Focus != null) {
					if (Me.Focus.IsFriendly && Range (40, Me.Focus) && Health (Me.Focus) < 0.95 && !Me.Focus.HasAura ("Eternal Flame", true)) {
						if (EternalFlame (Me.Focus))
							return true;
					}
				} else if (Tank != null) {
					if (!Tank.HasAura ("Eternal Flame", true) && Health (Tank) < 0.95) {
						if (EternalFlame (Tank))
							return true;
					}
				} else if (LowestPlayer != null) {
					if (!LowestPlayer.HasAura ("Eternal Flame", true) && Health (LowestPlayer) < 0.95) {
						if (EternalFlame (LowestPlayer))
							return true;
					}
				} else {
					if (!Me.HasAura ("Eternal Flame", true) && Health (Me) < 0.95) {
						if (EternalFlame (Me))
							return true;
					}
				}
			}
			return false;
		}

		public bool UseLayonHands ()
		{
			if (Usable ("Lay on Hands")) {
				if (Me.Focus != null) {
					if (Health (Me.Focus) <= 0.25) {
						if (LayonHands (Me.Focus))
							return true;
					}
				} else if (Tank != null) {
					if (Health (Tank) <= 0.25) {
						if (LayonHands (Tank))
							return true;
					}
				} else {
					if (Health (Me) <= 0.25) {
						if (LayonHands (Me))
							return true;
					}
				}
			}
			return false;
		}

		public bool UseHolyShock ()
		{
			if (HolyPower < MaxHolyPower && Usable ("Holy Shock")) {
				Player = MyGroupAndMe.Where (p => Health (p) <= 0.95).OrderBy (p => Health (p)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Player != null && HolyShock (Player))
					return true;
				if (Tank != null && Tank.InCombat && HolyShock (Tank))
					return true;
			}
			return false;
		}

		public bool UseHandOfProtection ()
		{
			if (Usable ("Hand of Protection")) {
				Player = MyGroupAndMe.Where (p => Health (p) <= HoP && Range (40, p) && (!p.IsTank || p == Me || p.IsHealer)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Player != null && HandofProtection (Player))
					return true;
			}
			return false;
		}

		public bool UseHandofSacrifice ()
		{
			if (Me.Focus != null && Usable ("Hand of Sacrifice")) {
				if (Me.Focus.IsFriendly && Range (40, Me.Focus) && Health (Me.Focus) <= HoS && !Me.Focus.HasAura ("Hand of Sacrifice", true)) {
					if (HandofSacrifice (Me.Focus))
						return true;
				}
			}
			return false;
		}

		public bool UseHolyLight ()
		{
			if (Usable ("Holy Light")) {
				Player = MyGroupAndMe.Where (p => Health (p) <= HL).OrderBy (p => Health (p)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Player != null && HolyLight (Player))
					return true;
			}
			return false;
		}

		public bool UseFlashLight ()
		{
			if (Usable ("Holy Light") && Me.HasAura ("Infusion of Light")) {
				Player = MyGroupAndMe.Where (p => Health (p) < HL).OrderBy (p => p.HealthFraction).DefaultIfEmpty (null).FirstOrDefault ();
				if (Player != null && HolyLight (Player))
					return true;
			} else if (Usable ("Flash of Light")) {
				Player = MyGroupAndMe.Where (p => Health (p) < FL).OrderBy (p => p.HealthFraction).DefaultIfEmpty (null).FirstOrDefault ();
				if (Player != null && FlashofLight (Player))
					return true;
			}
			return false;
		}

		public bool UseLightofDawn ()
		{
			if (HolyPower >= 3) {
				Player = MyGroupAndMe.Where (p => Range (30, p) && Health (p) < 1).DefaultIfEmpty (null).FirstOrDefault ();
				if (Player != null && LightofDawn ())
					return true;
			}
			return false;
		}

		public bool UseHolyRadiance ()
		{
			if (Usable ("Holy Radiance")) {
				Player = MyGroupAndMe.Where (p => Health (p) < HR).OrderBy (p => p.HealthFraction).DefaultIfEmpty (null).FirstOrDefault ();
				if (Player != null && HolyRadiance (Player))
					return true;
			}
			return false;
		}

		public bool  UseHealTarget ()
		{
			if (Target.IsFriendly && Range (40) && !Target.IsDead) {
				if (HolyPower < MaxHolyPower && Health () <= 0.9) {
					if (HolyShock ())
						return true;
				}
				if (HolyPower >= 3 && Health () <= 0.8) {
					if (WordofGlory ())
						return true;
				}
				if (Health () <= HL) {
					if (HolyLight ())
						return true;
				}
				if (Health () <= FL) {
					if (FlashofLight ())
						return true;
				}
			}
			return false;
		}

		public bool GetHolyPower ()
		{
			if (HolyPower <= MaxHolyPower && LowestPlayer != null) {
				if (LowestPlayerCount (0.8) >= AOECount) {
					if (Me.HasAura ("Daybreak")) {
						if (HolyShock (LowestPlayer))
							return true;
					} else {
						if (Health (LowestPlayer) <= HR && HolyRadiance (LowestPlayer))
							return true;
					}
				} else {
					if (HolyShock (LowestPlayer))
						return true;
				}
			}
			return false;
		}

		public bool CleanAll ()
		{
			if (Usable ("Cleanse")) {
				Player = MyGroupAndMe.Where (u => u.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType))).DefaultIfEmpty (null).FirstOrDefault ();
				return Player != null && Cleanse (Player);
			}
			return false;
		}

		public bool RessurectAll ()
		{
			if (InGroup && Usable ("Redemption")) {
				Player = MyGroup.Where (p => p.IsDead).DefaultIfEmpty (null).FirstOrDefault ();
				if (Player != null && Redemption (Player))
					return true;
			}
			return false;
		}

		// Spell

		public bool HandofProtection (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hand of Protection") && Range (40, u) && !u.HasAura ("Forbearance") && C ("Hand of Protection", u);
		}

		public bool DivineShield ()
		{
			return Usable ("Divine Shield") && !Me.HasAura ("Forbearance") && CS ("Divine Shield");
		}

		public bool BlessingofMight ()
		{
			return Usable ("Blessing of Might") && CS ("Blessing of Might");
		}

		public bool BlessingofKings ()
		{
			return Usable ("Blessing of Kings") && CS ("Blessing of Kings");
		}

		public bool SpeedofLight ()
		{
			return Usable ("Speed of Light") && CS ("Speed of Light");
		}

		public bool HolyAvenger ()
		{
			return Usable ("Holy Avenger") && Danger () && CS ("Holy Avenger");
		}

		public bool AvengingWrath ()
		{
			return Usable ("Avenging Wrath") && Danger () && CS ("Avenging Wrath");
		}

		public bool Seraphim ()
		{
			return Usable ("Seraphim") && Range (8) && HolyPower == 5 && CS ("Seraphim");
		}

		public bool DivineProtection ()
		{
			return Usable ("Divine Protection") && CS ("Divine Protection");
		}

		public bool GuardianofAncientKings (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Guardian of Ancient Kings") && Range (30, u) && DangerBoss (u) && C ("Guardian of Ancient Kings", u);
		}

		public bool ArdentDefender ()
		{
			return Usable ("Ardent Defender") && DangerBoss () && CS ("Ardent Defender");
		}

		public bool FlashofLight (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Flash of Light") && Range (40, u) && C ("Flash of Light", u);
		}

		public bool HolyShock (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Holy Shock") && Range (40, u) && C ("Holy Shock", u);
		}

		public bool EternalFlame (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Eternal Flame") && HolyPower >= 1 && Range (40, u) && C ("Eternal Flame", u);
		}

		public bool WordofGlory (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Word of Glory") && HolyPower >= 1 && Range (40, u) && C ("Word of Glory", u);
		}

		public bool ShieldoftheRighteous (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shield of the Righteous") && (HolyPower >= 3 || Me.HasAura ("Divine Purpose")) && Range (5, u) && C ("Shield of the Righteous", u);
		}

		public bool TemplarsVerdict (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Templar's Verdict") && (HolyPower >= 3 || Me.HasAura ("Divine Purpose")) && Range (5, u) && C ("Templar's Verdict", u);
		}

		public bool LightofDawn ()
		{
			return Usable ("Light of Dawn") && (HolyPower >= 1 || Me.HasAura ("Divine Purpose")) && CS ("Light of Dawn");
		}

		public bool FinalVerdict (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Final Verdict") && (HolyPower >= 3 || Me.HasAura ("Divine Purpose")) && Range (5, u) && C ("Final Verdict", u);
		}

		public bool DivineStorm ()
		{
			return Usable ("Divine Storm") && (HolyPower >= 3 || Me.HasAura ("Divine Purpose")) && Range (5) && CS ("Divine Storm");
		}

		public bool SealofInsight ()
		{
			return Usable ("Seal of Insight") && CS ("Seal of Insight");
		}

		public bool Consecration ()
		{
			return Usable ("Consecration") && Range (5) && CS ("Consecration");
		}

		public bool SealofRighteousness ()
		{
			return Usable ("Seal of Righteousness") && CS ("Seal of Righteousness");
		}

		public bool AvengersShield (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Avenger's Shield") && Range (30, u) && C ("Avenger's Shield", u);
		}

		public bool SacredShield (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Sacred Shield") && Range (40, u) && C ("Sacred Shield", u);
		}

		public bool HolyPrism (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Holy Prism") && Range (40, u) && C ("Holy Prism", u);
		}

		public bool ExecutionSentence (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Execution Sentence") && Range (40, u) && C ("Execution Sentence", u);
		}

		public bool HammeroftheRighteous (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hammer of the Righteous") && Range (5, u) && C ("Hammer of the Righteous", u);
		}

		public bool CrusaderStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Crusader Strike") && Range (5, u) && C ("Crusader Strike", u);
		}

		public bool Exorcism (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Exorcism") && ((HasGlyph (122028) && Range (5, u)) || Range (30, u)) && C ("Exorcism", u);
		}

		public bool LightsHammer (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Light's Hammer") && Range (30, u) && C ("Light's Hammer", u);
		}

		public bool HammerofWrath (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hammer of Wrath") && (Health (u) < 0.2 || (HasSpell (157496) && Health (u) < 0.35) || Me.HasAura ("Crusader's Fury")) && Range (30, u) && C ("Hammer of Wrath", u);
		}

		public bool Judgment (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Judgment") && Range (30, u) && C ("Judgment", u);
		}

		public bool HolyWrath ()
		{
			return Usable ("Holy Wrath") && Range (8) && CS ("Holy Wrath");
		}

		public bool SealofTruth ()
		{
			return Usable ("Seal of Truth") && CS ("Seal of Truth");
		}

		public bool Cleanse (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Cleanse") && Range (40, u) && CPD ("Cleanse", u, 2000);
		}

		public bool RighteousFury ()
		{
			return Usable ("Righteous Fury") && CS ("Righteous Fury");
		}

		public bool Rebuke (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Rebuke") && Range (5, u) && C ("Rebuke", u);
		}

		public bool FistofJustice (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Fist of Justice") && Range (20, u) && C ("Fist of Justice", u);
		}

		public bool LayonHands (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Lay on Hands") && Range (40, u) && !u.HasAura ("Forbearance") && C ("Lay on Hands", u);
		}

		public bool Redemption (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Redemption") && Range (40, u) && u.IsDead && C ("Redemption", u);
		}

		public bool BeaconofLight (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Beacon of Light") && Range (60, u) && C ("Beacon of Light", u);
		}

		public bool HandofFreedom (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hand of Freedom") && Range (40, u) && C ("Hand of Freedom", u);
		}

		public bool HolyLight (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Holy Light") && Range (40, u) && C ("Holy Light", u);
		}

		public bool HandofSacrifice (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hand of Sacrifice") && Range (40, u) && C ("Hand of Sacrifice", u);
		}

		public bool HolyRadiance (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Holy Radiance") && Range (40, u) && C ("Holy Radiance", u);
		}

		public bool Denounce (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Denounce") && Range (40, u) && C ("Denounce", u);
		}

		// Items


	}
}

