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
		private static RPMC RPMComputer = null;
		private static ConfigNode TMFDSettings = null;
		private static ConfigNode DeepSettings = null;

		private static string COLHighlight			= "";
		private static string COLNone				= "";
		private static string COLRed				= "";
		private static string COLGreen				= "";
		private static string COLYellow				= "";
		private static string version				= "";
		private static string shortMECOString		= "";
		private static string longMECOString		= "";
		private static string clampedRadarString	= "";

		private static int maxRadar = 10000;


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
					tmp += "RAD +"; //This is flipped with Radial In in the code. Don't ask me, Squad set it up that way.
					break;
				case (VesselAutopilot.AutopilotMode.RadialOut):
					tmp += "RAD -"; //This is flipped with Radial Out in the code. Don't ask me, Squad set it up that way.
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
		private string SASStringLong() {
			if (!vessel.Autopilot.Enabled)
				return "<OFF>           "; // SAS off

			string tmp = COLGreen;
			// If it's not on, check mode
			switch (vessel.Autopilot.Mode) {
				#region Cases
				case (VesselAutopilot.AutopilotMode.StabilityAssist):
					tmp += "Stability assist";
					break;
				case (VesselAutopilot.AutopilotMode.Maneuver):
					tmp += "Hold Maneuver   ";
					break;
				case (VesselAutopilot.AutopilotMode.Prograde):
					tmp += "Hold Prograde   ";
					break;
				case (VesselAutopilot.AutopilotMode.Retrograde):
					tmp += "Hold Retrograde ";
					break;
				case (VesselAutopilot.AutopilotMode.Normal):
					tmp += "Hold To Normal  ";
					break;
				case (VesselAutopilot.AutopilotMode.Antinormal):
					tmp += "Hold Anti-Normal";
					break;
				case (VesselAutopilot.AutopilotMode.RadialIn):
					tmp += "Hold Radial Out "; //This is flipped with Radial In in the code. Don't ask me, Squad set it up that way.
					break;
				case (VesselAutopilot.AutopilotMode.RadialOut):
					tmp += "Hold Radial In  "; //This is flipped with Radial Out in the code. Don't ask me, Squad set it up that way.
					break;
				case (VesselAutopilot.AutopilotMode.Target):
					tmp += "Hold At Target  ";
					break;
				case (VesselAutopilot.AutopilotMode.AntiTarget):
					tmp += "Hold Anti-Target";
					break;
				#endregion
				default:
					tmp = COLRed + "<???>           ";
					break;
			}

			tmp += COLNone;
			// End result
			// Off: <OFF>
			// Known mode: COLGreen + 5-letter mode string + COLNone
			// Unknown mode: COLRed + <???> + COLNone
			return tmp;
		}

		/// <summary>Logic to return either a MECO string or the throttle level
		/// </summary>
		/// <param name="MecoString">The string to use if throttle is 0%</param>
		private object MECOStringOrThrottle(string MecoString) {
			if (vessel.ctrlState.mainThrottle == 0.0f) {
				return MecoString;
			}
			return vessel.ctrlState.mainThrottle;
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
					for (int i = 0; i < DSDUMP.Length; i += 60) { tmp += DSDUMP.Substring(i, Math.Min(60, DSDUMP.Length - i)) + Environment.NewLine; }
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
				case "WARNBLINK":
				{
					if ((Int32)RPMComputer.ProcessVariable("PERIOD_1HZ", -1) == 1)
						return COLRed;
					else
						return "[#00000000]";
				}

				case "SASSTRING5":
					return SASStringShort();
				case "SASSTRING16":
					return SASStringLong();

				case "MECO_OR_THROTTLE":
					return MECOStringOrThrottle(shortMECOString + COLNone);
				case "LONGMECO_OR_THROTTLE":
					return MECOStringOrThrottle(longMECOString + COLNone);

				case "CLAMPEDRADAR":
				{
					if (vessel.terrainAltitude <= maxRadar)
						return vessel.terrainAltitude;
					else
						return clampedRadarString;
				}

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
					string tmp, deadtmp;
					tmp = deadtmp = "HorseBatteryStaple";
		
					tryLoadKey(DeepSettings, "colorTagHighlight", ref COLHighlight);
					tryLoadKey(DeepSettings, "colorTagDefault", ref COLNone);
					tryLoadKey(DeepSettings, "colorTagRed", ref COLRed);
					tryLoadKey(DeepSettings, "colorTagGreen", ref COLGreen);
					tryLoadKey(DeepSettings, "colorTagYellow", ref COLYellow);
					tryLoadKey(DeepSettings, "version", ref version);
					tryLoadKey(DeepSettings, "shortMECOString", ref shortMECOString);
					tryLoadKey(DeepSettings, "longMECOString", ref longMECOString);
					tryLoadKey(DeepSettings, "clampedRadarString", ref clampedRadarString);

					tryLoadKey(DeepSettings, "clampRadarAt", ref tmp);
					if (!tmp.Equals(deadtmp)) {
						try {
							int.Parse(tmp);
						}
						catch (FormatException) {
							log("Parsing problem: [" + tmp + "] is not parsable to an integer (clampRadarAt).");
						}
						finally	{
							tmp = deadtmp;
						}
					}

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
