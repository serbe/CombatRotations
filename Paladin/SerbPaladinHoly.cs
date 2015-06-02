using ReBot.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace ReBot
{
	[Rotation ("SC Paladin Holy", "Serb", WoWClass.Paladin, Specialization.PaladinHoly, 40, 25)]

	public class SerbPaladinHolySC : SerbPaladin
	{
		public enum Menu
		{
			EFBlanket,
			Ultimate,
			Aggressive,
			Normal,
			Conservative,
			Auto,
		}

		[JsonProperty ("Select Mana Playstyle"), JsonConverter (typeof(StringEnumConverter))]
		public Menu Choice = Menu.Normal;
		[JsonProperty ("Ressurect all no combat")]
		public bool RessAll = true;
		[JsonProperty ("Heal Target")]
		public bool HealTarget;

		public Menu Playstyle {
			get {
				if (Choice == Menu.Auto) {
					if (Me.ManaFraction >= 0.85)
						return Menu.Aggressive;
					if (Me.ManaFraction >= 0.45)
						return Menu.Normal;
					else
						return Menu.Conservative;
				}
				return Choice;
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

		public SerbPaladinHolySC ()
		{
			BeerTimersInit ();
			GroupBuffs = new[] { "Blessing of Kings" };
			PullSpells = new[] { "Judgment" };
		}

		public override bool OutOfCombat ()
		{
			if (HasGlobalCooldown ())
				return true;

			if (!IsInShapeshiftForm ("Seal of Insight")) {
				if (SealofInsight ())
					return true;
			}

			if (Buff (Me))
				return true;

			if (CleanAll ())
				return true;

			if (RessAll && RessurectAll ())
				return true;

			if (UseBeaconofLight ())
				return true;

			if (UseSacredShield ())
				return true;

			if (UseEternalFlame ())
				return true;
			

			return false;
		}

		public override void Combat ()
		{
			if (MeIsBusy)
				return;

			if (Me.CanNotParticipateInCombat ()) {
				if (HandofFreedom (Me) || Freedom ())
					return;
				return;
			}

			if (UseSacredShield ())
				return;
			
			if (Health (Me) <= 0.25) {
				if (DivineShield ())
					return;
			}

			if (UseLayonHands ())
				return;

			if (LowestPlayerCount (0.6) >= AOECount) {
				if (AvengingWrath ())
					return;
			}

			if (Usable ("Hand of Protection")) {
				Player = MyGroupAndMe.Where (p => Health (p) <= HoP && Range (40, p) && (!p.IsTank || p == Me || p.IsHealer)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Player != null && HandofProtection (Player))
					return;
			}

			if (Me.Focus != null && Usable ("Hand of Sacrifice")) {
				if (Me.Focus.IsFriendly && Range (40, Me.Focus) && Health (Me.Focus) <= HoS && !Me.Focus.HasAura ("Hand of Sacrifice", true)) {
					if (HandofSacrifice (Me.Focus))
						return;
				}
			}

			if (Me.HasAura ("Divine Purpose")) {
				if (UseLightofDawn ())
					return;
			}

			if (UseBeaconofLight ())
				return;

			if (Target != null) {
				if (Target.IsFriendly && Target.IsInCombatRange) {
					if (HealTarget) {
						if (Cast ("Holy Shock", () => HolyPower < 5 && Target.HealthFraction <= 0.9))
							return;
						if (Cast ("Word of Glory", () => HolyPower >= 3 && Target.HealthFraction <= 0.8))
							return;
						if (Cast ("Holy Light", () => Target.HealthFraction <= HL))
							return;
						if (Cast ("Flash of Light", () => Target.HealthFraction <= FL))
							return;
					}
				}
			}


			if (LowestPlayerCount (0.9) >= AOECount) {

				if (UseLightofDawn ())
					return;
				if (GetHolyPower (HR))
					return;
				if (UseHolyRadiance (HR))
					return;
			}
			if (Health (LowestPlayer) > FL) {
				if (GetHolyPower (HR))
					return;
			}

			//if (DoHP(grpAndMe)) return;
			if (UseEternalFlame ())
				return;
			if (UseLayonHands ())
				return;
			if (UseHolyLight (HL))
				return;
			if (UseFlashLight (HL, FL))
				return;


			if (Target != null && Target.IsEnemy) {
				if (HammerofWrath ())
					return;
				if (Judgment ())
					return;
				if (HolyShock ())
					return;
				if (CrusaderStrike ())
					return;
				if (Denounce ())
					return;
			}
			return;









			if (UseEternalFlame ())
				return;

			if (UseHolyShock ())
				return;
			
//			if (Me.Focus.Target != null) {
//				if (Me.Focus.Target.IsEnemy && Me.Focus.Target.IsInCombatRange) {
//					Cast ("Holy Prism");
//				}
//			}

			if (UseFlashLight (HL, FL))
				return;
			if (UseHolyLight (HL))
				return;


			// Let's create holy power or help the damage count

//			if (Target != null) {
//				if (Target.IsEnemy && Target.IsInCombatRange) {
//					if (Cast ("Hammer of Wrath", () => Target.HealthFraction <= 0.2))
//						return;
//					Cast ("Judgment");
//					Cast ("Denounce", () => denounce);
//					Cast ("Crusader Strike", () => strike);
//				}
//			}
//			return;		


		}
	}
}

