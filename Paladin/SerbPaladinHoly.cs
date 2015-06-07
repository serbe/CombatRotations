using ReBot.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace ReBot
{
	[Rotation ("SC Paladin Holy", "Serb", WoWClass.Paladin, Specialization.PaladinHoly, 40, 25)]

	public class SerbPaladinHolySC : SerbPaladin
	{

		[JsonProperty ("Select Mana Playstyle"), JsonConverter (typeof(StringEnumConverter))]
		public Menu ManaPlaystyle = Menu.Normal;
		[JsonProperty ("Ressurect all no combat")]
		public bool RessAll = true;
		[JsonProperty ("Heal Target")]
		public bool HealTarget;
		[JsonProperty ("Use Hand of Sactifice to focus")]
		public bool UseHoS;

		public SerbPaladinHolySC ()
		{
			BeerTimersInit ();
			GroupBuffs = new[] { "Blessing of Kings" };
			PullSpells = new[] { "Judgment" };

			SetChoice (ManaPlaystyle);
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

//			if (UseSacredShield ())
//				return true;
//
//			if (UseEternalFlame ())
//				return true;

			if (Mana (Me) > 0.5) {
				if (UseHolyLight ())
					return true;
				if (UseFlashLight ())
					return true;
			}

			if (Tank != null && Tank.InCombat && HolyShock (Tank))
				return true;

			return false;
		}

		public override void Combat ()
		{
//			if (CurrentBotName == "Quest") {
//				OverrideCombatModus = CombatModus.Healer;
//				OverrideCombatRole = CombatRole.Healer;
//			} else {
//				OverrideCombatModus = CombatModus.Healer;
//				OverrideCombatRole = CombatRole.Healer;
//			}
				

			if (MeIsBusy)
				return;

			if (Me.CanNotParticipateInCombat ()) {
				if (HandofFreedom (Me) || Freedom ())
					return;
				return;
			}

			if (Target != null) {
				if (Target.IsEnemy && Target.IsInCombatRange) {
					if (HolyPrism ())
						return;
				}
			}

			if (UseLayonHands ())
				return;
			
			if (UseSacredShield ())
				return;

			if (Health (Me) <= 0.25) {
				if (DivineShield ())
					return;
			}

			if (LowestPlayerCount (0.5) >= AOECount) {
				if (AvengingWrath ())
					return;
			}

			if (UseHandOfProtection ())
				return;

			if (UseHoS && UseHandofSacrifice ())
				return;

			if (Me.HasAura ("Divine Purpose")) {
				if (UseLightofDawn ())
					return;
			}

			if (UseBeaconofLight ())
				return;

			if (HealTarget && Target != null) {
				if (UseHealTarget ())
					return;
			}


			if (LowestPlayerCount (0.83) >= AOECount) {

				if (UseLightofDawn ())
					return;
				if (GetHolyPower ())
					return;
				if (UseHolyRadiance ())
					return;
			}
			if (Health (LowestPlayer) > FL) {
				if (GetHolyPower ())
					return;
			}

			if (UseFlashLight ())
				return;
			if (UseEternalFlame ())
				return;
			if (UseHolyLight ())
				return;
			if (UseLayonHands ())
				return;

			if (Usable ("Hammer of Wrath")) {
				Unit = Enemy.Where (u => Range (30, u) && Health (u) < 0.2).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && HammerofWrath (Unit))
					return;
			}

			if (Target != null && !Target.IsDead && Target.IsEnemy) {
				if (Health () < 0.2 && HammerofWrath ())
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
		}
	}
}

