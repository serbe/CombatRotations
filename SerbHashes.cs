using System.Collections.Generic;
using System.IO.Compression;

namespace ReBot
{
	public class SerbHashes
	{
		public HashSet<string> PgUnits = new HashSet<string> {
			"Oto the Protector",
			"Sooli the Survivalist",
			"Kavan the Arcanist",
			"Ki the Assassin"
		};

		public HashSet<string> BurstAura = new HashSet<string> {
			"Adrenaline Rush",
			"Vendetta",
			"Breath of Sindragosa",
			"Celestial Alignment",
			"Berserk",
			"Incarnation: Chosen of Elune",
			"Incarnation: King of the Jungle",
			"Incarnation: Son of Ursoc",
			"Bestial Wrath",
			"Frenzy",
			"Barrage",
			"Rapid Fire",
			"Serenity",
			"Avenging Wrath",
			"Hand of Protection"
		};

		public HashSet<string> DefAura = new HashSet<string> {
			
		};

		public HashSet<string> RangeDefAura = new HashSet<string> {
			"Zen Meditation"
		};

		public HashSet<string> MagicDefAura = new HashSet<string> {
			"Diffuse Magic",
			"Devotion Aura"
		};

		public HashSet<string> AoeSkill = new HashSet<string> {
			"Blizzard"
		};
	}
}

