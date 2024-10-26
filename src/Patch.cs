using HarmonyLib;
using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace PowerMeter;

[HarmonyPatch]
class Patch
{
    public static bool showInGUI = true;
    public static bool showInOverlay = true;

    //static int zeroDrainTimer = 0;
    static double deltaAvg = 0f;
    static double lastCharge = 0f;
    static double lastSec = 0f;
    static string lastString = string.Empty;
    static StringBuilder sb = new StringBuilder();

    readonly static Color clrGreen = new Color(74f / 256f, 1f, 188f / 256f, 1f);
    enum Seconds
    {
        Minute = 60,
        Hour = Minute * 60,
        Day = Hour * 24,
        Week = Day * 7,
        Month = Day * 30,
        Year = Month * 12
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GUIOrbitDraw), "UpdateUIs")]
    static void GUIOrbitDraw_UpdateUIs__Postfix(GUIOrbitDraw __instance, ref TextMeshProUGUI ___txtSide)
    {
        if (!showInGUI) {
            return;
        }
        double sec = StarSystem.fEpoch;
        double dt = sec - lastSec;
        if (dt > 1.01f) {
            lastSec = sec;
            lastString = GeneratePowerString(ReadPowerConnected(__instance), dt);
        }
        ___txtSide.text += lastString;
    }

    /*
    if (CrewSim.bDebugShow)
    {
      this.txtID = UnityEngine.Object.Instantiate<TMP_Text>(UnityEngine.Resources.Load<TMP_Text>("prefabTextFloatUI"), this.coRoom.transform);
      this.txtID.text = this.coRoom.strNameFriendly + " " + this.coRoom.strID.Substring(0, 6);
      this.txtID.fontSize = 11f;
      this.txtID.gameObject.name = this.txtID.text;
    }
    */

    static double ReadPowerConnected(GUIOrbitDraw __instance)
    {
        if (__instance.COSelf == null) {
            return 0f;
        }
        Powered component = __instance.COSelf.GetComponent<Powered>();
        return (component == null) ? 0f : component.PowerConnected;
    }

    static string GeneratePowerString(double battCharge, double dt)
    {
        double deltaCharge = (lastCharge > 0) ? (battCharge - lastCharge) / dt : 0;
        lastCharge = battCharge;
        /*zeroDrainTimer = (deltaCharge > 0) ? 0 : Math.Min(3, zeroDrainTimer + 1);
        if (deltaAvg > 0 && zeroDrainTimer < 3 && Math.Abs(deltaAvg-deltaCharge) < 1) {
            deltaAvg = (deltaAvg + deltaCharge) / 2;
        } else {*/
        deltaAvg = deltaCharge;
        //}
        //Main.ModEntry.Logger.Log("lastdrain " + deltaAvg + " charge " + battCharge + " dt " + dt);
        sb.Length = 0;
        sb.AppendLine();
        sb.Append(MathUtils.ColorToColorTag(deltaAvg < 0 ? GUIOrbitDraw.clrOrange01 : clrGreen));
        sbAppendPowerStats(1e3 * Math.Abs(deltaAvg) * (double)Seconds.Hour, ref sb); // kWh/s
        if (deltaAvg < 0) {
            double drainTime = Math.Ceiling(battCharge / deltaAvg); // kWh/kW
            sbAppendDrainTime(Math.Abs(drainTime), ref sb);
        } else {
            sb.Append(" CHRG");
        }
        sb.Append("</color>");
        return sb.ToString();
    }

    static void sbAppendPowerStats(double power, ref StringBuilder sb)
    {
        if (power > 1e9) {
            sb.AppendFormat("{0:#.0} GW", power / 1e9);

        } else if (power > 100e6) {
            sb.AppendFormat("{0:0} MW", power / 1e6);

        } else if (power > 1e6) {
            sb.AppendFormat("{0:#.0} MW", power / 1e6);

        } else if (power > 100e3) {
            sb.AppendFormat("{0:0} kW", power / 1e3);

        } else if (power > 1e3) {
            sb.AppendFormat("{0:#.0} kW", power / 1e3);

        } else if (power > 0) {
            sb.AppendFormat("{0:0} W", power);
        }
    }

    static void sbAppendDrainTime(double drainTime, ref StringBuilder sb)
    {
        if (drainTime > (double)Seconds.Year * 42) {
            sb.Append(" (+42Y)");

        } else if (drainTime > (double)Seconds.Year) {
            sb.AppendFormat(" ({0}Y {1}M)",
                Math.Floor(drainTime / (double)Seconds.Year),
                Math.Floor(drainTime / (double)Seconds.Month) % 12
            );
        } else if (drainTime > (double)Seconds.Month) {
            sb.AppendFormat(" ({0}M {1}D)",
                Math.Floor(drainTime / (double)Seconds.Month),
                Math.Floor(drainTime / (double)Seconds.Day) % 30
            );
        } else if (drainTime > (double)Seconds.Week) {
            sb.AppendFormat(" ({0}W {1}D)", 
                Math.Floor(drainTime / (double)Seconds.Week), 
                Math.Floor(drainTime / (double)Seconds.Day) % 7
            );
        } else if (drainTime > (double)Seconds.Day) {
            sb.AppendFormat(" ({0}W {1}h)",
                Math.Floor(drainTime / (double)Seconds.Day),
                Math.Floor(drainTime / (double)Seconds.Hour) % 24
            );
        } else if (drainTime > (double)Seconds.Hour) {
            sb.AppendFormat(" ({0}h {1}m)",
                Math.Floor(drainTime / (double)Seconds.Hour),
                Math.Floor(drainTime / (double)Seconds.Minute) % 60
            );
        } else if (drainTime > 0) {
            sb.AppendFormat(" ({0}m {1}s)",
                Math.Floor(drainTime / (double)Seconds.Minute),
                drainTime % 60
            );
        }
    }
}
