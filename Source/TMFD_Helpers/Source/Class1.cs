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

		// ----------
		// Variable parsers
		// ----------
		private string SASStringShort() {
			if (!vessel.Autopilot.Enabled)
				return "<OFF>"; // SAS off

			string tmp = COLGreen;
			// If it's not on, check mode
			switch (vessel.Autopilot.Mode) {
				#region Cases
				case (VesselAutopilot.AutopilotMode.StabilityAssist):
					tmp += "STABL";
					break;
				case (VesselAutopilot.AutopilotMode.Maneuver):
					tmp += "MANUV";
					break;
				case (VesselAutopilot.AutopilotMode.Prograde):
					tmp += "PROGR";
					break;
				case (VesselAutopilot.AutopilotMode.Retrograde):
					tmp += "RETRO";
					break;
				case (VesselAutopilot.AutopilotMode.Normal):
					tmp += "NORML";
					break;
				case (VesselAutopilot.AutopilotMode.Antinormal):
					tmp += "ANORM";
					break;
				case (VesselAutopilot.AutopilotMode.RadialIn):
					tmp += "RAD -";
					break;
				case (VesselAutopilot.AutopilotMode.RadialOut):
					tmp += "RAD +";
					break;
				case (VesselAutopilot.AutopilotMode.Target):
					tmp += "TARGT";
					break;
				case (VesselAutopilot.AutopilotMode.AntiTarget):
					tmp += "ATARG";
					break;
				#endregion
				default:
					tmp = COLRed + "<???>";
					break;
			}

			tmp += COLNone;
			// End result
			// Off: <OFF>
			// Known mode: COLGreen + 5-letter mode string + COLNone
			// Unknown mode: COLRed + <???> + COLNone
			return tmp; 
		}

		/// <summary> Get TMFD helper variables
		/// </summary>
		/// <param name="varname">Variable name</param>
		/// <returns>Variable value</returns>
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

				case "SASSTRING5":
					return SASStringShort();

				// Pretty much anything not implmented.
				default:
					return "NotCoded";
			}
		}

		/// <summary> Loads settings from configNode
		/// </summary>
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

		// ----------
		// Helper functions
		// ----------
		private static void log(object o) {
			print("[TMFD] - " + o.ToString());
		}
		private static void tryLoadKey(ConfigNode node, string key, ref string variable) {
			if (node.HasValue(key)) {
				variable = node.GetValue(key);
			}
		}
	}
}
