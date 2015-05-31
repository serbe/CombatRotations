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

		public UnitObject LastJudgmentTarget;

		// Get

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

		public bool CleanAll ()
		{
			Player = Group.GetGroupMemberObjects ().Where (u => (Range (40, u) && u.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType)))).DefaultIfEmpty (null).FirstOrDefault ();
			return Player != null && Cleanse (Player);
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
			if (Me.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType))) {
				if (Cleanse (Me))
					return true;
			}
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

			// Party

			if (InInstance) {
				Player = Group.GetGroupMemberObjects ().Where (p => !p.IsDead && p.IsHealer && Health (p) < 0.2 && !p.HasAura ("Immunity") && Range (40, p)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Player != null && HandofProtection (Player))
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

		public bool BloodFury ()
		{
			return Usable ("Blood Fury") && Danger () && CS ("BloodFury");
		}

		public bool Berserking ()
		{
			return Usable ("Berserking") && Danger () && CS ("Berserking");
		}

		public bool ArcaneTorrent ()
		{
			return Usable ("Arcane Torrent") && Danger () && CS ("Arcane Torrent");
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
			return Usable ("Guardian of Ancient Kings") && Range (30, u) && C ("Guardian of Ancient Kings", u);
		}

		public bool ArdentDefender ()
		{
			return Usable ("Ardent Defender") && CS ("Ardent Defender");
		}

		public bool FlashofLight (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Flash of Light") && Range (40, u) && C ("Flash of Light", u);
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
			return Usable ("Cleanse") && Range (40, u) && C ("Cleanse", u);
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


		// Items


	}
}

