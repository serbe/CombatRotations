using ReBot.API;
using System.Linq;
using Newtonsoft.Json;

namespace ReBot
{
	[Rotation ("Serb Priest Shadow IV", "ReBot", WoWClass.Priest, Specialization.PriestShadow, 40, 25)]

	public class SerbPriestShadowIv : SerbPriest
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Auto target")]
		public bool UseAutoTarget = true;
		[JsonProperty ("Fight in instance")]
		public bool FightInInstance = true;
		[JsonProperty ("Heal %/100")]
		public double HealPr = 0.8;
		[JsonProperty ("Heal tank %/100")]
		public double TankPr = 0.9;

		public SerbPriestShadowIv ()
		{
			GroupBuffs = new [] {
				"Power Word: Fortitude"
			};
			PullSpells = new [] {
				"Shadow Word: Pain",
			};

		}

		public override bool OutOfCombat ()
		{
		}

		public override void Combat ()
		{

			if (!Me.HasAura ("Shadowform")) {
				if (Shadowform ())
					return;
			}

			if (EnemyInRange (40) == 1) {
				if (SingleTarget ())
					return;
			}

			if (EnemyInRange (40) > 1 && EnemyInRange (40) < 4) {
				if (SmallAOETarget ())
					return;
			}

			if (EnemyInRange (40) > 4) {
				if (HugeAOETarget ())
					return;
			}
				
		}

		public bool SingleTarget ()
		{
			if (Orb >= 3) {
				if (DevouringPlague ())
					return true;
			}

			if (Orb <= 4) {
				if (MindBlast ())
					return true;
				if (Health (Target) < 0.2) {
					if (ShadowWordDeath ())
						return true;
				}
			}

			if (Me.HasAura ("Shadow Word: Insanity")) {
				if (Insanity ())
					return true;
			}

			if (Me.HasAura ("Surge of Darkness")) {
				if (MindSpike ())
					return true;
			}

			if (!Target.HasAura ("Shadow Word: Pain", true) || (Target.HasAura ("Shadow Word: Pain", true) && Target.AuraTimeRemaining ("Shadow Word: Pain") <= 5.6)) {
				if (ShadowWordPain ())
					return true;
			}

			if (!Target.HasAura ("Vampiric Touch", true) || (Target.HasAura ("Vampiric Touch", true) && Target.AuraTimeRemaining ("Vampiric Touch") <= 4.5)) {
				if (VampiricTouch ())
					return true;
			}

			if (MindFlay ())
				return true;

			return false;
		}

		bool SmallAOETarget ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (Orb >= 3) {
				if (DevouringPlague ())
					return true;
			}

			if (Orb <= 4) {
				if (MindBlast ())
					return true;
				if (Health (Target) < 0.2) {
					if (ShadowWordDeath ())
						return true;
				}
			}

			if (Usable ("Shadow Word: Pain")) {
				CycleTarget = targets.Where (u => !u.HasAura ("Shadow Word: Pain", true) || (u.HasAura ("Shadow Word: Pain", true) && u.AuraTimeRemaining ("Shadow Word: Pain") <= 5.6)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (ShadowWordPain (CycleTarget))
						return true;
				}
			}

			if (Usable ("Vampiric Touch")) {
				CycleTarget = targets.Where (u => !u.HasAura ("Vampiric Touch", true) || (u.HasAura ("Vampiric Touch", true) && u.AuraTimeRemaining ("Vampiric Touch") <= 4.5)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (VampiricTouch (CycleTarget))
						return true;
				}
			}

			if (MindFlay ())
				return true;

			return false;
		}

		bool HugeAOETarget ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (Orb >= 3) {
				if (DevouringPlague ())
					return true;
			}

			if (Orb <= 4) {
				if (MindBlast ())
					return true;
				if (Health (Target) < 0.2) {
					if (ShadowWordDeath ())
						return true;
				}
			}

			if (Cascade ())
				return true;

			if (DivineStar ())
				return true;
			
			if (Halo ())
				return true;
			
			if (Usable ("Shadow Word: Pain")) {
				CycleTarget = targets.Where (u => !u.HasAura ("Shadow Word: Pain", true) || (u.HasAura ("Shadow Word: Pain", true) && u.AuraTimeRemaining ("Shadow Word: Pain") <= 5.6)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (ShadowWordPain (CycleTarget))
						return true;
				}
			}

			if (Usable ("Vampiric Touch")) {
				CycleTarget = targets.Where (u => !u.HasAura ("Vampiric Touch", true) || (u.HasAura ("Vampiric Touch", true) && u.AuraTimeRemaining ("Vampiric Touch") <= 4.5)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (VampiricTouch (CycleTarget))
						return true;
				}
			}

			if (MindSear ())
				return true;
			
			return false;
		}
	}
}

