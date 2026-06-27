using UnityEngine;
//using TuxModLoader.Builders;
using System.Collections.Generic;
using UnityEngine.Events;
//using TuxModLoader;
using UnityEngine.UI;
using System.Collections;
using NCMS.Utils;
using NCMS;
using ReflectionUtility;
using TuxModLoader.Reflection;
using System.Reflection;

namespace ModernBox
{
	public class Bombs
	{

		public static void Init()
		{

        new BombBuilder("moab")
            .SetTexturePath("ui/icons/MOAB")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_MOABClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_MOABClickOTHER))
            .Build();

        new BombBuilder("clusternuke")
            .SetTexturePath("ui/icons/ClusterNuke")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_ClusterClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .Build();

        new BombBuilder("icebomb")
            .SetTexturePath("ui/icons/I hate this stupid bomb it messed me up")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_IceClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_IceClickOTHER))
            .Build();

        new BombBuilder("firebomb")
            .SetTexturePath("ui/icons/FIRE!")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_FireBombClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .Build(); 
            
        new BombBuilder("tuxium")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_TuxiumClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_TuxiumClickOTHER))
            .Build();   
            
        new BombBuilder("homo")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_HomoClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_HomoClickOTHER))
            .Build();

        new BombBuilder("fury")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_FuryClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_FuryClickOTHER))
            .Build();

        new BombBuilder("overload")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_OverloadClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_OverloadClickOTHER))
            .Build();

        new BombBuilder("zombie")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_ZombieClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_ZombieClickOTHER))
            .Build();
       
        new BombBuilder("xeno")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_XenoClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_XenoClickOTHER))
            .Build();

        new BombBuilder("jupiter")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_JupiterClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_JupiterClickOTHER))
            .Build();

        new BombBuilder("mini")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_MiniClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_MiniClickOTHER))
            .Build();

        new BombBuilder("death")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_DeathClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_DeathClickOTHER))
            .Build();

        new BombBuilder("cobalt")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_CobaltClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_CobaltClickOTHER))
            .Build();

        new BombBuilder("nodmg")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_NoDmgClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_NoDmgClickOTHER))
            .Build();

        new BombBuilder("agrenade")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_AGrenadeClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_AGrenadeClickOTHER))
            .Build();

        new BombBuilder("colorg")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_ColorGClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_ColorGClickOTHER))
            .Build();

        new BombBuilder("eraser")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_EraserClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_EraserClickOTHER))
            .Build();

        new BombBuilder("agony")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_AgonyClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_AgonyClickOTHER))
            .Build();

        new BombBuilder("ultron")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_UltronClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_UltronClickOTHER))
            .Build();

        new BombBuilder("nsa")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_NSAClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_NSAClickOTHER))
            .Build();

        new BombBuilder("tuuds")
            .SetTexturePath("drops/drop_czarbomba")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_DickClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_DickClickOTHER))
            .Build();

            new BombBuilder("test")
            .SetTexturePath("ui/icons/Wut")
            .SetDropLandAction(new DropsAction(BombUtilities.Instance.action_TestClick))
            .SetClickPowerAction(new PowerAction(Buttonz.Stuff_Drop))
            .SetBombEffectAction(new WorldAction(BombUtilities.Instance.action_TestClickOTHER))
            .Build();

        }
    }
}