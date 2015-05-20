using ReBot.API;
using Newtonsoft.Json;

namespace ReBot
{
	[Rotation ("SC Paladin Protection", "Serb", WoWClass.Paladin, Specialization.PaladinHoly, 40, 25)]

	public class SerbPaladinHolySC : SerbPaladin
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		public SerbPaladinHolySC ()
		{
			GroupBuffs = new[] { "Blessing of Kings" };
			PullSpells = new[] { "Judgment" };
		}

		public override bool OutOfCombat ()
		{
			if (Buff (Me))
				return true;

			if (CleanAll ())
				return true;

			return false;
		}

		public override void Combat ()
		{

		}
	}
}

