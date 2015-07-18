using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RPMC = JSI.RasterPropMonitorComputer;

namespace TAHV_MFD
{

	public class TMFD : PartModule
	{
		// ----------
		// Variables
		// ----------
		private bool instantiated = false;
		private static ConfigNode TMFDSettings = null;
		private static ConfigNode DeepSettings = null;
		private static string COLHighlight	= "";
		private static string COLNone	= "";
		private static string COLRed	= "";
		private static string COLGreen	= "";
		private static string COLYellow	= "";

		private static string version = "";

		private static RPMC RPMComputer = null;
		
		// ----------
		// Startup
		// ----------
		public override void OnAwake() {

			loadSettings();
			RPMComputer = RPMC.Instantiate(this.part);

			instantiated = true;
		}

		/// <summary> Get TMFD helper variables
		/// </summary>
		/// <param name="varname"></param>
		/// <returns></returns>
		public Object getCustomVariable(string varname) {

			if (varname.Equals("TMFD_AVAILABLE")) {
				if (instantiated) {	return 1;} 
				else { return -1; }
			}
			if (varname.StartsWith("TMFD_")) {
				varname = varname.Remove(0, 5);
			} else {
				return "NoPre_" + varname;
			}
			switch (varname) {
				case "VERSION":
					return version;
				// Debug dump
				case "DSDUMP":
					string tmp = "", DSDUMP = DeepSettings.ToString();
					for (int i = 0; i < DSDUMP.Length; i += 40) { tmp += DSDUMP.Substring(i, Math.Min(40, DSDUMP.Length - i)) + Environment.NewLine; }
					tmp = tmp.Replace("[#", "[[]#");
					return tmp;
				// Various colors
				case "HIGHLIGHT":
					return COLHighlight;
				case "NOCOLOR":
					return COLNone;
				case "RED":
					return COLRed;
				case "GREEN":
					return COLGreen;
				case "YELLOW":
					return COLYellow;

				// Returns either transparent or the color defined in colorTagRed
				// wrapping this in brackets to prevent readability problems.
				case "WARNBLINK": {
					bool blink = (Int32)RPMComputer.ProcessVariable("PERIOD_1HZ", -1) == 1;
					if (blink) return COLRed;
					else return "[#00000000]";
				}

				// Pretty much anything not implmented.
				default:
					return "NotCoded";
			}
		}



		private static void log(object o){
			print("[TMFD] - " + o.ToString());
		}
		private static void tryLoadKey(ConfigNode node, string key, ref string variable) {
			if (node.HasValue(key)) {
				variable = node.GetValue(key);
			}
		}

		private static void loadSettings() {
			//TMFDSettings = GameDatabase.Instance.GetConfigNode("TahvMFDConfig");
			log("TahvMFDConfig exists? " + GameDatabase.Instance.ExistsConfigNode("TahvMFDConfig").ToString());
			TMFDSettings = GameDatabase.Instance.GetConfigNodes("TahvMFDConfig")[0];
			if (null != TMFDSettings) {
				DeepSettings = TMFDSettings.GetNode("DeepSettings");
				if (null != DeepSettings) {

					tryLoadKey(DeepSettings, "colorTagHighlight", ref COLHighlight);
					tryLoadKey(DeepSettings, "colorTagDefault", ref COLNone);
					tryLoadKey(DeepSettings, "colorTagRed", ref COLRed);
					tryLoadKey(DeepSettings, "colorTagGreen", ref COLGreen);
					tryLoadKey(DeepSettings, "colorTagYellow", ref COLYellow);
					tryLoadKey(DeepSettings, "version", ref version);

					log(DeepSettings);
				} else { log("DeepSettings is null"); }
			} else { log("TMFDSettings is null");}
		}
	}
}
