using ReBot.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ReBot
{
	[Rotation ("SC Paladin Protection", "Serb", WoWClass.Paladin, Specialization.PaladinHoly, 40, 25)]

	public class SerbPaladinHolySC : SerbPaladin
	{
		public enum Menu
		{
			Ultimate,
			Aggressive,
			Normal,
			HolyPower,
			Auto,
		}

		[JsonProperty ("Select Mana Playstyle (Ultimate not usable under Auto)"), JsonConverter (typeof(StringEnumConverter))]
		public Menu Choice = Menu.Normal;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Ressurect all no combat")]
		public bool RessAll = true;

		public Menu Playstyle {
			get {
				if (Choice == Menu.Auto) {
					if (Me.ManaFraction >= 0.75)
						return Menu.Aggressive;
					if (Me.ManaFraction >= 0.45)
						return Menu.Normal;
					else
						return Menu.HolyPower;
				}
				return Choice;
			}
		}

		public double FL {
			get {
				switch (Playstyle) {
				case Menu.Ultimate:
					return 0.75;
				case Menu.Aggressive:
					return 0.40;			 
				case Menu.Normal:
					return 0.35;
				case Menu.HolyPower:
					return 0;
				}
				return 0.35;
			}
		}

		public double HL {
			get {
				switch (Playstyle) {
				case Menu.Ultimate:
					return 0.95;
				case Menu.Aggressive:
					return 0.85;			
				case Menu.Normal:
					return 0.75;
				case Menu.HolyPower:
					return 0;
				}
				return 0.75;
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

			if (Gcd && HasGlobalCooldown ())
				return;

			if (Me.CanNotParticipateInCombat ()) {
				if (HandofFreedom (Me) || Freedom ())
					return;
				return;
			}

			if (UseBeaconofLight ())
				return;
			
			if (UseSacredShield ())
				return;

			if (UseEternalFlame ())
				return;

//			if (Me.Focus.Target != null) {
//				if (Me.Focus.Target.IsEnemy && Me.Focus.Target.IsInCombatRange) {
//					Cast ("Holy Prism");
//				}
//			}

			//if (DoHP(grpAndMe)) return;
//			if (DoEF (grpAndMe))
//				return;
			if (UseLightofDawn ())
				return;
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

