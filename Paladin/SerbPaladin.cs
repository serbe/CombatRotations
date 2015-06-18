using System.Linq;
using ReBot.API;
using Newtonsoft.Json;

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

		[JsonProperty ("Mili Range")]
		public int MRange = 5;

		Menu _choice;

		double LayonHandsHealth = 0.2;
		double HolyShockHealth = 0.95;
		double EternalFlameHealth = 0.95;
		double HandofSacrificeHealth = 0.55;
		double HandofProtectionHealth = 0.2;

		public UnitObject LastJudgmentTarget;

		public void SetChoice (Menu c = Menu.Normal)
		{
			_choice = c;
		}
			
		// Targets

		public UnitObject CleanseTarget {
			get {
				return PartyMembers.Where (u => Range (40, u) && u.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType))).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		public UnitObject ExecutionSentenceTarget {
			get {
				return Usable ("Execution Sentence") ? FocusTankorMe (0.4) : null;
			}
		}

		public UnitObject SacredShieldTarget {
			get {
				if (Usable ("Sacred Shield")) {
					Unit = FocusTankorLowestNoAura (1, "Sacred Shield");
					if (Unit != null && (Health (Unit) < 1 || (Unit == Me.Focus || Unit == Tank)))
						return Unit;
					if (!Me.HasAura ("Sacred Shield"))
						return Me;
				}
				return null;
			}
		}

		public UnitObject EternalFlameTarget {
			get {
				return HolyPower > 0 && Usable ("Eternal Flame") ? FocusTankorLowestNoAura (EternalFlameHealth, "Eternal Flame") : null;
			}
		}

		public bool UseDivineProtection {
			get {
				if (Usable ("Divine Protection")) {
					if (Enemy.Where (u => u.IsCasting && u.Target == Me && !Me.HasAura ("Divine Protection") && u.CastingTime > 0).ToList ().Count > 0)
						return true;
				}
				return false;
			}
		}

		public UnitObject BeaconofLightTarget {
			get {
				if (Usable ("Beacon of Light")) {
					Unit = FocusTankorLowestNoAura (1, "Beacon of Light");
					if (Unit != null && (Health (Unit) < 1 || Unit == Tank || Unit == Me.Focus))
						return Unit;
				}
				return null;
			}
		}

		public UnitObject LightofDawnTarget {
			get {
				return Usable ("Light of Dawn") && HolyPower >= 3 ? Lowest (0.95, 30) : null;
			}
		}

		// Вспышка Света
		public UnitObject FlashofLightTarget {
			get {
				return Usable ("Flash of Light") ? Lowest (FlashofLightHealth) : null;
			}
		}

		public UnitObject HolyRadianceTarget {
			get {
				return Usable ("Holy Radiance") ? Lowest (HolyRadianceHealth) : null;
			}
		}

		// Свет небес
		public UnitObject HolyLightTarget {
			get {
				return Usable ("Holy Light") ? Lowest (HolyLightHealth) : null;
			}
		}

		public UnitObject LayonHandsTarget {
			get {
				return Usable ("Layon Hands") ? FocusTankorMe (LayonHandsHealth) : null;
			}
		}

		// Шок небес
		public UnitObject HolyShockTarget {
			get {
				if (HolyPower < MaxHolyPower) {
					Unit = Lowest (HolyShockHealth);
					if (Unit != null)
						return Unit;
				}
				if (!Me.InCombat) {
					Unit = FocusTankorMe (1);
					if (Unit != null && Unit != Me && Unit.InCombat)
						return Unit;
				}
				return null;
			}
		}

		public UnitObject HandOfProtectionTarget {
			get {
				return PartyMembers.Where (p => !p.IsDead && Health (p) <= HandofProtectionHealth && Range (40, p) && (!IsTank (p) || p == Me || IsHealer (p))).DefaultIfEmpty (null).FirstOrDefault ();
			}
		}

		// Длань жертвенности
		public UnitObject HandofSacrificeTarget {
			get {
				return HasGlyph (146957) && Usable ("Hand of Sacrifice") ? LowestNoAura (HandofSacrificeHealth, "Hand of Sacrifice") : null;
			}
		}

		// Get

		public int MaxHolyPower {
			get {
				return Me.Level >= 85 ? 5 : 3;
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

		public double FlashofLightHealth {
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

		public double HolyLightHealth {
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

		public double HolyRadianceHealth {
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

		public double WordofGloryHealth {
			get {
				switch (Playstyle) {
				case Menu.EFBlanket:
					return 0.99;
				case Menu.Ultimate:
					return 0.97;
				case Menu.Aggressive:
					return 0.97;			
				case Menu.Normal:
				default:
					return 0.97;
				case Menu.Conservative:
					return 0.97;
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


		public double HSHeal {
			get {
				switch (Playstyle) {
				case Menu.EFBlanket:
					return 0;
				case Menu.Ultimate:
					return 0.98;
				case Menu.Aggressive:
					return 0.98;			
				case Menu.Normal:
				default:
					return 0.98;
				case Menu.Conservative:
					return 0.98;
				}
			}
		}


		public double HPHeal {
			get {
				switch (Playstyle) {
				case Menu.EFBlanket:
					return 0;
				case Menu.Ultimate:
					return 0.99;
				case Menu.Aggressive:
					return 0.89;			
				case Menu.Normal:
				default:
					return 0.99;
				case Menu.Conservative:
					return 0.99;
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

		// Check

		public bool HasBuff (UnitObject u = null)
		{
			u = u ?? Me;
			return u.HasAura ("Blessing of Kings") || u.HasAura ("Legacy of the Emperor") || u.HasAura ("Mark of the Wild");
		}

		public bool NeedDivineProtection {
			get {
				Unit = Enemy.Where (u => u.IsCasting && u.CastingTime > 0 && u.CastingTime < 2).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && (Health (Me) < 0.7 || IsBoss (Unit)))
					return true;
				return false;
			}
		}

		// Combo

		public bool PaladinFreedom ()
		{
			if (HandofFreedom (Me) || Freedom ())
				return true;
		
			if (!Target.IsInCombatRange && Me.MovementSpeed > 0 && Me.MovementSpeed < MovementSpeed.NormalRunning) {
				if (Emancipate ())
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
			if (Health (Me) <= 0.7 && GetAuraStack ("Selfless Healer", Me) >= 3) {
				if (FlashofLight (Me))
					return true;
			}
			if (Health (Me) <= 0.9 && Me.HasAura ("Hand of Protection")) {
				if (FlashofLight (Me))
					return true;
			}
			if (Health (Me) <= 0.2 && !Me.HasAura ("Immunity") && Cooldown ("Lay on Hands") > 1 && Cooldown ("Divine Shield") > 1) {
				if (HandofProtection (Me))
					return true;
			}
			if (!InArena && Health (Me) <= 0.15 && !Me.HasAura ("Divine Shield") && !Me.HasAura ("Immunity")) {
				if (LayonHands (Me))
					return true;
			}
			if (InArena && Health (Me) <= 0.3 && HolyPower >= 1) {
				if (WordofGlory (Me))
					return true;
			}
			if (Health (Me) <= 0.4) {
				if (ExecutionSentence (Me))
					return true;
			}
			if (Health (Me) <= 0.4) {
				if (Healthstone ())
					return true;
			}
			if (Health (Me) <= 0.5 && !Me.HasAura ("Immunity")) {
				if (DivineShield ())
					return true;
			}
			if (InArena && Health (Me) <= 0.45 && HolyPower >= 2) {
				if (WordofGlory (Me))
					return true;
			}
			if (InArena && Health (Me) <= 0.6 && HolyPower >= 3) {
				if (WordofGlory (Me))
					return true;
			}
			if (Health (Me) <= 0.6 && Target.IsCasting && !Me.HasAura ("Divine Shield")) {
				if (DivineProtection ())
					return true;
			}
//			if (CastSelf ("Flash of Light", () => Health <= 0.6 && Me.HasAura ("Divine Shield") && TargetHealth >= 0.15))
//				return;
//			if (Me.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType))) {
//				if (Cleanse (Me))
//					return true;
//			}
			return false;
		}

		public bool ArenaHeal (UnitObject u)
		{
			if (Health (u) <= 0.7 && GetAuraStack ("Selfless Healer", Me) >= 3) {
				if (FlashofLight (u))
					return true;
			}
			if (Health (u) <= 0.2 && !u.HasAura ("Immunity")) {
				if (HandofProtection (u))
					return true;
			}
			if (Health (u) <= 0.3 && HolyPower >= 1) {
				if (WordofGlory (u))
					return true;
			}
			if (Health (u) <= 0.4) {
				if (ExecutionSentence (u))
					return true;
			}
			if (Health (u) <= 0.5 && HolyPower >= 2) {
				if (WordofGlory (u))
					return true;
			}
			if (Health (u) <= 0.7 && HolyPower >= 3) {
				if (WordofGlory (u))
					return true;
			}
			return false;
		}

		public bool Interrupt ()
		{
			if (InArena || InBg) {
				if (Usable ("Rebuke")) {
					Unit = API.Players.Where (x => x.IsPlayer && x.IsEnemy && x.IsHealer && Range (5, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Rebuke (Unit))
						return true; 
					Unit = API.Players.Where (x => x.IsPlayer && x.IsEnemy && Range (5, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Rebuke (Unit))
						return true; 
				}
				if (Cooldown ("Fist of Justice") == 0) {
					Unit = API.Players.Where (x => x.IsPlayer && x.IsEnemy && x.IsHealer && Range (20, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && FistofJustice (Unit))
						return true;
					Unit = API.Players.Where (x => x.IsPlayer && x.IsEnemy && Range (20, x) && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && FistofJustice (Unit))
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


		public bool UseWarningHeal ()
		{
			if (Usable ("Flash of Light")) {
				if (Me.Focus != null) {
					if (Health (Me.Focus) <= 0.5) {
						if (FlashofLight (Me.Focus))
							return true;
					}
				} else if (Tank != null) {
					if (Health (Tank) <= 0.5) {
						if (FlashofLight (Tank))
							return true;
					}
				} else {
					if (Health (Me) <= 0.5) {
						if (FlashofLight (Me))
							return true;
					}
				}
			}
			return false;
		}



		public bool  UseHealTarget ()
		{
			if (Target.IsFriendly && Range (40) && !Target.IsDead) {
				if (HolyPower < MaxHolyPower && Health () < 1) {
					if (HolyShock ())
						return true;
				}
				if (HolyPower >= 3 && Health () <= 0.8) {
					if (WordofGlory ())
						return true;
				}
				if (Health () <= HolyLightHealth) {
					if (HolyLight ())
						return true;
				}
				if (Health () <= FlashofLightHealth) {
					if (FlashofLight ())
						return true;
				}
			}
			return false;
		}

		public bool GetHolyPower ()
		{
			if (Me.HasAura ("Daybreak")) {
				Unit = BestAOEPlayer (40, 10, AOECount, 0.9);
				if (Unit != null && HolyShock (Unit))
					return true;
			}
			if (HolyPower <= MaxHolyPower && LowestPlayer != null) {
				if (LowestPlayerCount (0.8) >= AOECount) {
					if (Health (LowestPlayer) <= HolyRadianceHealth && HolyRadiance (LowestPlayer))
						return true;
				} else {
					if (HolyShock (LowestPlayer))
						return true;
				}
			}
			return false;
		}

		public bool RessurectAll ()
		{
			if (InGroup && Usable ("Redemption")) {
				Unit = MyGroup.Where (p => p.IsDead).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Redemption (Unit))
					return true;
			}
			return false;
		}

		// Spell

		// Длань защиты
		public bool HandofProtection (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hand of Protection") && Range (40, u) && !u.HasAura ("Forbearance") && C ("Hand of Protection", u);
		}

		// Божественный щит
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

		// Святой каратель
		public bool HolyAvenger ()
		{
			return Usable ("Holy Avenger") && CS ("Holy Avenger");
		}

		// Гнев карателя
		public bool AvengingWrath ()
		{
			return Usable ("Avenging Wrath") && CS ("Avenging Wrath");
		}

		public bool Seraphim ()
		{
			return Usable ("Seraphim") && Range (8) && HolyPower == 5 && CS ("Seraphim");
		}

		// Божественная защита
		public bool DivineProtection ()
		{
			return Usable ("Divine Protection") && CS ("Divine Protection");
		}

		// Защитник древних королей
		public bool GuardianofAncientKings (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Guardian of Ancient Kings") && DangerBoss (u, 30) && C ("Guardian of Ancient Kings", u);
		}

		// Ревностный защитник
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

		// Вечное пламя
		public bool EternalFlame (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Eternal Flame") && HolyPower >= 1 && Range (40, u) && C ("Eternal Flame", u);
		}

		// Торжество
		public bool WordofGlory (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Word of Glory") && HolyPower >= 1 && Range (40, u) && C ("Word of Glory", u);
		}

		public bool ShieldoftheRighteous (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shield of the Righteous") && (HolyPower >= 3 || Me.HasAura ("Divine Purpose")) && Range (MRange, u) && C ("Shield of the Righteous", u);
		}

		public bool TemplarsVerdict (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Templar's Verdict") && (HolyPower >= 3 || Me.HasAura ("Divine Purpose")) && Range (MRange, u) && C ("Templar's Verdict", u);
		}

		public bool LightofDawn ()
		{
			return Usable ("Light of Dawn") && (HolyPower >= 1 || Me.HasAura ("Divine Purpose")) && CS ("Light of Dawn");
		}

		public bool FinalVerdict (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Final Verdict") && (HolyPower >= 3 || Me.HasAura ("Divine Purpose")) && Range (MRange, u) && C ("Final Verdict", u);
		}

		public bool DivineStorm ()
		{
			return Usable ("Divine Storm") && (HolyPower >= 3 || Me.HasAura ("Divine Purpose")) && Range (MRange) && CS ("Divine Storm");
		}

		public bool SealofInsight ()
		{
			return Usable ("Seal of Insight") && CS ("Seal of Insight");
		}

		public bool Consecration ()
		{
			return Usable ("Consecration") && Range (MRange) && CS ("Consecration");
		}

		public bool SealofRighteousness ()
		{
			return Usable ("Seal of Righteousness") && CS ("Seal of Righteousness");
		}

		// Щит мстителя
		public bool AvengersShield (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Avenger's Shield") && Range (30, u) && C ("Avenger's Shield", u);
		}

		// Священный щит
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

		// Смертный приговор
		public bool ExecutionSentence (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Execution Sentence") && Range (40, u) && C ("Execution Sentence", u);
		}

		public bool HammeroftheRighteous (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hammer of the Righteous") && Range (MRange, u) && C ("Hammer of the Righteous", u);
		}

		public bool CrusaderStrike (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Crusader Strike") && Range (MRange, u) && C ("Crusader Strike", u);
		}

		public bool Exorcism (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Exorcism") && ((HasGlyph (122028) && Range (MRange, u)) || Range (30, u)) && C ("Exorcism", u);
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
			return Usable ("Rebuke") && Range (MRange, u) && C ("Rebuke", u);
		}

		public bool FistofJustice (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Fist of Justice") && Range (20, u) && C ("Fist of Justice", u);
		}

		// Возложение рук
		public bool LayonHands (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Lay on Hands") && Range (40, u) && !u.HasAura ("Forbearance") && C ("Lay on Hands", u);
		}

		public bool Redemption (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Redemption") && Range (40, u) && u.IsDead && CPD ("Redemption", u, 2000);
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

		public bool Emancipate ()
		{
			return Usable ("Emancipate") && CS ("Emancipate");
		}

		// Items


	}
}

