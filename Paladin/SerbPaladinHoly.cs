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
			return false;
		}

		public override void Combat ()
		{

		}
	}
}

