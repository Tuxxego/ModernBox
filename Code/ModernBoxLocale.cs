using System;
using System.Collections.Generic;
using HarmonyLib;
using NCMS.Utils;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.Events;

namespace ModernBox
{
    // Manu-Fix 024 + CN Localization: Explicitly retain ModernBox strings in English, German, and Chinese (cz/ch/zh/tc).
    internal static class ModernBoxLocale
    {
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> German = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> Chinese = new Dictionary<string, string>();

        // Exact key -> Chinese translation
        private static readonly Dictionary<string, string> CnByKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Tabs
            { "tab_modernbox", "现代盒子 (ModernBox)" },
            { "ModernBoxTab", "现代盒子" },
            { "ModernBoxTabFuckYou", "现代盒子 (ModernBox)" },
            { "ModernBoxTabFuckYou Description", "时代演进、太空探索、意识形态与更多内容。" },
            { "ModernBoxUnits", "现代盒子：兵种与载具" },
            { "ModernBoxUnitsFuckYou", "现代盒子：兵种与载具" },
            { "ModernBoxUnitsFuckYou Description", "投放各式现代步兵、装甲载具、战机与战舰！" },
            { "ModernBoxBombs", "现代盒子：炸弹与核武" },
            { "ModernBoxBombsFuckYou", "现代盒子：炸弹与核武" },
            { "ModernBoxBombsFuckYou Description", "投放各类战术炸弹、核武器与终极毁灭武器！" },
            { "ModernBoxEras", "现代盒子：时代与科技" },
            { "ModernBoxErasFuckYou", "现代盒子：时代与科技" },
            { "ModernBoxErasFuckYou Description", "管理时代演进、自动研发与各王国科技进度！" },
            { "ModernBoxSpace", "现代盒子：太空与星图" },
            { "ModernBoxSpaceFuckYou", "现代盒子：太空与星图" },
            { "ModernBoxSpaceFuckYou Description", "探索浩瀚星海、访问其他行星与自定义星系！" },
            { "ModernBoxItems", "现代盒子：军工与开关" },
            { "ModernBoxItemsFuckYou", "现代盒子：军工与开关" },
            { "ModernBoxItemsFuckYou Description", "管理武器制造、载具生产与特殊装备开关。" },
            { "PizzaTab", "披萨模式" },
            { "PizzaTabFuckYou", "披萨模式" },
            { "PizzaTabFuckYou Description", "欢迎来到 Tux 的披萨店经营小游戏！" },
            { "Tuxxego_mod_creator", "本模组由 Tuxxego 制作（已加载性能优化与中文汉化补丁）。" },

            // Windows
            { "AboutWindow", "关于 ModernBox" },
            { "AchievementsWindow", "ModernBox 成就" },
            { "CreditsWindow", "制作人员名单" },
            { "CustomGalaxiesWindow", "自定义星系管理" },
            { "InfiniteBoxWindow", "关于 InfiniteBox" },
            { "PizzaWindow", "Tux 的披萨店" },
            { "SpaceWindow", "太空与星际航行" },
            { "manu_mb_forschung", "研发与建设" },
            { "manu_mb_forschung_oeffnen", "研发与建设" },
            { "manu_mb_forschung_oeffnen_description", "查看所有王国的科技研发进度、前置条件、城市与军队规模。" },
            { "manu_mb_forschung_oeffnen Description", "查看所有王国的科技研发进度、前置条件、城市与军队规模。" },

            // Main Tab Buttons
            { "credits", "关于 ModernBox" },
            { "credits_description", "查看 ModernBox 的幕后开发团队与更多信息！" },
            { "opencredits", "制作人员名单" },
            { "opencredits_description", "查看参与制作本模组的开发者与画师。" },
            { "resettodefaults", "恢复默认设置" },
            { "resettodefaults_description", "将所有已保存的模组开关恢复为默认状态。" },
            { "infinitebox", "InfiniteBox" },
            { "infinitebox_description", "了解作者的下一款独立沙盒游戏项目！" },
            { "resetbawls", "重新运行初始设置向导" },
            { "resetbawls_description", "警告：点击此按钮前请先保存当前世界存档！" },
            { "m3achievements", "模组成就" },
            { "m3achievements_description", "查看已解锁的 ModernBox 专属成就。" },
            { "ResetButtonM2", "重置成就进度" },
            { "ResetButtonM2_description", "清空所有已解锁的模组成就数据。" },
            { "pizza", "披萨" },
            { "pizza_description", "来一块披萨吧（连点 100 次有彩蛋）。" },

            // Space Tab & Window Buttons
            { "OpenStarmap", "打开星图" },
            { "OpenStarmap_description", "造访其他行星、恒星系与遥远星系。" },
            { "CustomGalaxies", "自定义星系" },
            { "CustomGalaxies_description", "启用或禁用你已安装的自定义星系。" },
            { "GalaxyHub", "星系中心 (GalaxyHub)" },
            { "GalaxyHub_description", "下载并安装社区创作的星系！" },
            { "galaxy", "星图 (Starmap)" },
            { "galaxy_description", "造访其他行星与恒星系统！" },
            { "beginningtoggle", "星球持久化存档" },
            { "beginningtoggle_description", "开启后重启游戏将继续留在上次所在的星球，而不是重新生成新世界。" },
            { "colonize", "星际殖民" },
            { "colonize_description", "将单位送往其他星球！（即将推出）" },
            { "colonizewhat", "未知领域" },
            { "colonizewhat_description", "敬请期待……" },

            // Era Tab Buttons
            { "era_no_set", "自动科技研发" },
            { "era_no_set_description", "解除上帝手动锁定，让所有王国通过科技研发、城市建设和军队发展自然演进时代。" },
            { "era_no_set Description", "解除上帝手动锁定，让所有王国通过科技研发、城市建设和军队发展自然演进时代。" },
            { "era_mediaval_set", "上帝锁定：中世纪" },
            { "era_mediaval_set_description", "无论研发进度如何，世界时代将始终锁定在中世纪。" },
            { "era_renaissance_set", "上帝锁定：文艺复兴" },
            { "era_renaissance_set_description", "无论研发进度如何，世界时代将始终锁定在文艺复兴时期。" },
            { "era_modern_set", "上帝锁定：现代" },
            { "era_modern_set_description", "无论研发进度如何，世界时代将始终锁定在现代。" },
            { "era_hyperfuture_set", "上帝锁定：超未来" },
            { "era_hyperfuture_set_description", "无论研发进度如何，世界时代将始终锁定在超未来时代。" },
            { "era_mediaval_toggle", "开关：中世纪" },
            { "era_mediaval_toggle_description", "控制中世纪单位与建筑是否允许启用。" },
            { "era_renaissance_toggle", "开关：文艺复兴时代" },
            { "era_renaissance_toggle_description", "控制文艺复兴时代内容是否允许启用。" },
            { "era_modern_toggle", "开关：现代" },
            { "era_modern_toggle_description", "控制现代科技与军备是否允许启用。" },
            { "era_hyperfuture_toggle", "开关：超未来时代" },
            { "era_hyperfuture_toggle_description", "控制超未来时代科技与机甲是否允许启用。" },

            // Items / Production Toggles
            { "nukes_toggle", "开关：核战争" },
            { "nukes_toggle_description", "允许敌对王国之间互相发射核武器。" },
            { "vehicle_toggle", "开关：载具生产" },
            { "vehicle_toggle_description", "控制各王国是否能够自行建造陆空载具。" },
            { "nuketexttoggle", "开关：核打击通告" },
            { "nuketexttoggle_description", "当有国家遭到核武器打击时发送世界历史通告。" },
            { "gun_toggle", "开关：枪械制造" },
            { "gun_toggle_description", "控制城市是否能够制造火器与现代枪械。" },
            { "mirv_toggle", "开关：分导核弹 (MIRV)" },
            { "mirv_toggle_description", "控制城市是否能够制造手持分导式多弹头核武器。" },
            { "drugs_toggle", "开关：兴奋剂与药物" },
            { "drugs_toggle_description", "控制单位是否能够制造并使用强效药物。" },
            { "mgltoggle", "开关：化学与疯狂瓦斯武器" },
            { "mgltoggle_description", "控制是否允许生产疯狂瓦斯榴弹发射器 (MGL)。" },
            { "atat", "开关：AT-AT 装甲步行机生产" },
            { "atat_description", "控制王国是否能生产 AT-ST / AT9000 系列装甲步行机。" },
            { "dread", "开关：无畏机甲生产" },
            { "dread_description", "控制王国是否能生产无畏机甲 (Dreadnaught) 级重型单位。" },
            { "gunship", "开关：武装直升机与炮艇生产" },
            { "gunship_description", "控制王国是否能生产武装直升机、轰炸机与空中炮艇。" },
            { "tiefighter", "开关：TIE 战机生产" },
            { "tiefighter_description", "控制王国是否能生产 TIE 星际战机。" },
            { "goliath", "开关：歌利亚巨型机甲生产" },
            { "goliath_description", "控制王国是否能生产歌利亚 (Goliath) 级重型载具。" },
            { "mechagodzilla_goliath", "开关：机械哥斯拉生产" },
            { "mechagodzilla_goliath_description", "控制王国是否能生产机械哥斯拉级巨兽单位。" },
            { "earthquaker", "开关：震地者机甲生产" },
            { "earthquaker_description", "控制王国是否能生产震地者 (Earthquaker) 级重型载具。" },
            { "spacemarines", "开关：星际战士改造" },
            { "spacemarines_description", "控制超未来时代的王国是否能将单位改造为星际战士 / 太空兽人。" },

            // Bombs
            { "moab_button", "超级核弹 (Super-Nuke)" },
            { "moab_button_description", "初代经典巨型核弹，威力堪比沙皇炸弹。" },
            { "icebomb_button", "极寒冰封弹" },
            { "icebomb_button_description", "让爆炸范围内的所有生物体验真正的“核冬天”。" },
            { "tuxium_button", "Tuxium 终极巨弹" },
            { "tuxium_button_description", "ModernBox 传统艺能：一颗威力荒谬、纯粹为了壮观而存在的超级巨弹。" },
            { "firebomb_button", "烈阳焚世弹" },
            { "firebomb_button_description", "将烈日投向大地，焚尽一切！" },
            { "clusternuke_button", "集束核弹" },
            { "clusternuke_button_description", "一次投下 5 枚连续引爆的核弹头！" },
            { "homo_button", "同心炸弹" },
            { "homo_button_description", "散播特殊特质的恶搞炸弹。" },
            { "fury_button", "图克西亚之怒 (慎用)" },
            { "fury_button_description", "警告：此炸弹极易导致游戏崩溃，除非你想重启游戏否则请勿使用！" },
            { "overload_button", "过载核弹" },
            { "overload_button_description", "威力惊人的超负荷聚变核弹。" },
            { "zombie_button", "丧尸生化核弹" },
            { "zombie_button_description", "将杀伤范围内的生物感染转化为丧尸。" },
            { "xeno_button", "异形感染核弹 (Xenium)" },
            { "xeno_button_description", "释放外星异形能量与感染的特殊核弹。" },
            { "jupiter_button", "木星聚变核弹" },
            { "jupiter_button_description", "释放木星风暴级别的毁灭性冲击。" },
            { "mini_button", "微型战术核弹" },
            { "mini_button_description", "小巧玲珑但依然致命的战术核弹。" },
            { "death_button", "死神核弹" },
            { "death_button_description", "名字听起来很吓人，但其实没有那么夸张。" },
            { "cobalt_button", "钴弹 (脏弹)" },
            { "cobalt_button_description", "范围紧凑但杀伤力极强的放射性能量弹。" },
            { "nodmg_button", "无伤烟花核弹" },
            { "nodmg_button_description", "不造成实质地形破坏，但拥有华丽的视觉特效。" },
            { "agrenade_button", "原子手雷" },
            { "agrenade_button_description", "掌握在手心里的浓缩核爆威力！" },
            { "colorg_button", "炫彩炸弹" },
            { "colorg_button_description", "绽放绚丽多彩爆炸特效的染色炸弹。" },
            { "eraser_button", "地形抹除弹" },
            { "eraser_button_description", "一键抹除爆炸范围内的地块！" },
            { "agony_button", "苦痛核弹" },
            { "agony_button_description", "愿神怜悯那些被卷入爆炸半径的生灵。" },
            { "ultron_button", "奥创纪元核弹" },
            { "ultron_button_description", "通往和平的唯一道路……就是你们的灭绝。" },
            { "nsa_button", "半吊子原子弹" },
            { "nsa_button_description", "从核弹学校辍学去转行的屑核弹（而且转行也失败了）。" },
            { "tuuds_button", "不洁宇宙毁灭系统 (TUUDS)" },
            { "tuuds_button_description", "彻底摧毁一切并抹除整个宇宙时间线！此操作不可逆，请谨慎使用。" },
            { "test_button", "测试炸弹" },
            { "test_button_description", "恭喜你发现了隐藏的测试炸弹。" },

            // Traits
            { "trait_group_Ideology", "意识形态" },
            { "trait_Dynastic", "王朝正统主义" },
            { "trait_Dynastic_info", "吾王万岁！！！！" },
            { "trait_Dynastic_info_2", "皇室贵胄与世袭正统" },
            { "trait_Martial", "军国尚武主义" },
            { "trait_Martial_info", "所有人都会领教我们和平的方式——靠武力！！！" },
            { "trait_Martial_info_2", "《钢铁雄心4》重度爱好者" },
            { "trait_Peoplewoven", "人民公社主义" },
            { "trait_Peoplewoven_info", "为了劳苦大众！！" },
            { "trait_Peoplewoven_info_2", "同样是《钢铁雄心4》爱好者" },
            { "trait_Mercantile", "重商主义" },
            { "trait_Mercantile_info", "没有为了金币把家人卖掉，就算是我们一路上最大的情谊了" },
            { "trait_Mercantile_info_2", "兄弟，要来点马克西姆币吗？" },
            { "trait_Chaosvolt", "混沌革命主义" },
            { "trait_Chaosvolt_info", "起义！反抗！！大革命！！！" },
            { "trait_Chaosvolt_info_2", "“大体上和平”的核内战" },
            { "trait_Unitpotential", "载具/战争单位" },
            { "trait_Unitpotential_info", "赋予现代化战争载具属性 :3" },
            { "trait_Unitpotential_info_2", "直升机～直升机～" },
            { "trait_NavalUnit", "现代海军单位" },
            { "trait_NavalUnit_info", "赋予现代海军舰艇属性 :3" },
            { "trait_NavalUnit_info_2", "巨舰大炮，乐趣无穷 :D" },
            { "trait_Gay", "同性恋" },
            { "trait_Gay_info", "这位角色喜欢同性（同心炸弹联动特质）。" },
            { "trait_Gay_info_2", "彩蛋特质" },
            { "trait_zombie_spawner", "丧尸信标" },
            { "trait_zombie_spawner_info", "尸群正向他集结……" },
            { "trait_zombie_spawner_info_2", "丧尸突变母体" },

            // Tasks & Decisions
            { "task_unit_warBoatAttackDecision", "战舰炮击" },
            { "warBoatAttackDecision", "海军炮击" },
            { "missileArtilleryDecision", "导弹火炮齐射" },
            { "HORDEmissileArtilleryDecision", "部落导弹火炮齐射" },
            { "HARDENmissileArtilleryDecision", "矮人导弹火炮齐射" },
            { "GAIAmissileArtilleryDecision", "自然导弹火炮齐射" },
            { "nuclearmissileDecision", "战略核导弹打击" },
            { "AntiBossNukeDecision", "反巨兽战术核打击" },
            { "bomber_force_reload_rtb", "返航补给弹药" },
            { "bomber_land_and_reload", "降落并装填弹药" },
            { "bomber_takeoff_for_war", "紧急升空作战" },
            { "bomber_engage_enemy_targets", "交战并轰炸敌方目标" },
            { "bomber_peace_station", "和平时期驻航待命" },

            // Achievements
            { "played_m2", "传奇老玩家" },
            { "played_m2_description", "在开启 ModernBox 的情况下游玩世界盒子" },
            { "moab_drop", "也没那么超级" },
            { "moab_drop_description", "想当年这玩意儿曾是模组里最大的炸弹……" },
            { "discord", "图克西亚公民" },
            { "discord_description", "加入了官方 Discord 社区！" },
            { "bomb_overload", "炸弹过载！！！" },
            { "bomb_overload_description", "将每种炸弹至少各投下一遍。" },
            { "deleted", "万物归零……" },
            { "deleted_description", "老哥直接把整个宇宙给删了 ☠️。" },
            { "danksshittybomb", "彻底抹除" },
            { "danksshittybomb_description", "投下一枚地形抹除弹。" },
            { "planetlander", "跨星球文明" },
            { "planetlander_description", "通过星图造访另一颗星球。" },
        };

        // Unit ID / English Name -> Chinese display name
        internal static readonly Dictionary<string, string> CnUnitNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "baseWarUnit", "基础军工底盘" },
            { "Light Vehicle", "轻型战车" },
            { "light_vehicle", "轻型战车" },
            { "Artillery", "火炮" },
            { "Support Unit", "支援单位" },
            { "support_unit", "支援单位" },
            { "Tank", "主战坦克" },
            { "Helicopter", "武装直升机" },
            { "Helicopter thing", "重型武装直升机" },
            { "helicopter_thing", "重型武装直升机" },
            { "Armored Car", "装甲战车" },
            { "armored_car", "装甲战车" },
            { "Missile System", "自行导弹系统" },
            { "missile_system", "自行导弹系统" },
            { "Bomber", "战略轰炸机" },
            { "Fighter Jet", "喷气式战斗机" },
            { "fighter_jet", "喷气式战斗机" },
            { "F55FighterJet", "F-55 隐身战机" },
            { "Demon Scorpion", "恶魔机械蝎" },
            { "demon_scorpion", "恶魔机械蝎" },
            { "Demon Crocodile", "恶魔装甲鳄" },
            { "demon_crocodile", "恶魔装甲鳄" },
            { "Demon Golem", "恶魔魔像" },
            { "demon_golem", "恶魔魔像" },
            { "Wyvern", "恶魔飞龙" },
            { "Dragon", "恶魔巨龙轰炸机" },
            { "Demon Reaver", "恶魔掠夺者机甲" },
            { "demon_reaver", "恶魔掠夺者机甲" },
            { "Cargo Ship", "现代运输舰/特种船" },
            { "cargo_ship", "现代运输舰/特种船" },
            { "Destroyer Ship", "现代驱逐舰/战列舰" },
            { "destroyer_ship", "现代驱逐舰/战列舰" },
            { "Elite Soldier", "精锐星际战士" },
            { "elite_soldier", "精锐星际战士" },
            { "Light Support Vehicle", "轻型支援步行机" },
            { "light_support_vehicle", "轻型支援步行机" },
            { "Gunship", "未来空中炮艇" },
            { "Railgun", "磁轨炮坦克" },
            { "Heavy Land Support", "重型陆战支援机甲" },
            { "heavy_land_support", "重型陆战支援机甲" },
            { "Heavy Artillery", "重型自行火炮" },
            { "heavy_artillery", "重型自行火炮" },
            { "Armored Walker", "重型装甲泰坦机甲" },
            { "armored_walker", "重型装甲泰坦机甲" },

            // Specific spawn unit IDs
            { "EliteP9000", "精锐 P9000 主战坦克" },
            { "OmegaRailgun", "欧米茄磁轨炮坦克" },
            { "eliteAT9000", "精锐 AT9000 重型机甲" },
            { "eliteMA9000", "精锐 MA9000 重型火炮" },
            { "dreadnaught_brrt", "无畏机甲 (火神炮型)" },
            { "HumanTitanElite", "精锐人类泰坦机甲" },
            { "SpaceMarine", "星际战士 (Space Marine)" },
            { "Terran", "人族陆战机甲 (Terran)" },
            { "teslatruckgun", "磁暴电弧战车" },
            { "atst", "AT-ST 侦察步行机" },
            { "artilleryatst", "AT-ST 火炮步行机" },
            { "atstsniper", "AT-ST 狙击步行机" },
            { "modernhumvee_Human", "人类悍马战车" },
            { "howitzer_Human", "人类榴弹炮" },
            { "humancavalry", "人类铁骑兵" },
            { "humancannon", "人类火炮" },
            { "spaceork", "太空兽人战士" },
            { "modernhumvee_Ork", "兽人战车" },
            { "howitzer_Ork", "兽人榴弹炮" },
            { "ogreunit", "食人魔战兵" },
            { "orccannon", "兽人火炮" },
            { "armoredwolf", "披甲战狼骑兵" },
            { "modernhumvee_Dwarf", "矮人战车" },
            { "howitzer_Dwarf", "矮人榴弹炮" },
            { "dwarfcannon", "矮人火炮" },
            { "golemgem", "宝石魔像" },
            { "modernhumvee_Gaia", "精灵战车" },
            { "howitzer_Gaia", "精灵榴弹炮" },
            { "treant", "树人卫士" },
            { "elfcannon", "精灵火炮" },
            { "demonscorpion", "恶魔机械蝎" },
            { "demonwyvern", "恶魔飞龙" },
            { "xenolevitank", "异形悬浮坦克" },
            { "xenoUFO", "异形飞碟 (UFO)" },
            { "P9000", "P9000 未来坦克" },
            { "dreadnaught", "无畏机甲" },
            { "Tank_Human", "人类主战坦克" },
            { "MissileSystem_Human", "人类导弹发射车" },
            { "wheeledtank_Human", "人类轮式突击炮" },
            { "davincitank", "达芬奇蒸汽坦克" },
            { "catapulta", "人类投石车" },
            { "batteringram", "攻城冲车" },
            { "Tank_Ork", "兽人主战坦克" },
            { "MissileSystem_Ork", "兽人导弹发射车" },
            { "wheeledtank_Ork", "兽人轮式突击炮" },
            { "orcatapulta", "兽人投石车" },
            { "Tank_Dwarf", "矮人主战坦克" },
            { "MissileSystem_Dwarf", "矮人导弹发射车" },
            { "wheeledtank_Dwarf", "矮人轮式突击炮" },
            { "santaguin", "企鹅攻城炮" },
            { "Tank_Gaia", "精灵主战坦克" },
            { "MissileSystem_Gaia", "精灵导弹发射车" },
            { "wheeledtank_Gaia", "精灵轮式突击炮" },
            { "woolyrhino", "披甲长毛犀" },
            { "demoncroc", "恶魔装甲鳄" },
            { "demongolem", "恶魔魔像" },
            { "demonreaver", "恶魔掠夺者机甲" },
            { "xenorailgun", "异形磁轨炮" },
            { "xenotripod", "异形三脚毁灭机甲" },
            { "AT9000", "AT9000 重型步行机" },
            { "supportatst", "AT-ST 支援步行机" },
            { "supporttruck_Human", "人类后勤支援车" },
            { "humanpaladin", "人类圣骑士" },
            { "supporttruck_Ork", "兽人后勤支援车" },
            { "orcwarlock", "兽人术士" },
            { "supporttruck_Dwarf", "矮人后勤支援车" },
            { "dwarfdoctor", "矮人军医" },
            { "supporttruck_Gaia", "精灵后勤支援车" },
            { "fairydragon", "仙女龙" },
            { "HeliELite", "精锐未来武装直升机" },
            { "FutureGunship", "未来重型空中炮艇" },
            { "TIEfighter", "TIE 星际战斗机" },
            { "EliteBomber", "精锐战略核轰炸机" },
            { "Heli_Human", "人类武装直升机" },
            { "Bomber_Human", "人类战略轰炸机" },
            { "FighterJet_Human", "人类喷气式战斗机" },
            { "balloonunit", "热气球轰炸艇" },
            { "Heli_Ork", "兽人武装直升机" },
            { "Bomber_Ork", "兽人战略轰炸机" },
            { "FighterJet_Ork", "兽人喷气式战斗机" },
            { "Heli_Dwarf", "矮人武装直升机" },
            { "Bomber_Dwarf", "矮人战略轰炸机" },
            { "FighterJet_Dwarf", "矮人喷气式战斗机" },
            { "Heli_Gaia", "精灵武装直升机" },
            { "Bomber_Gaia", "精灵战略轰炸机" },
            { "FighterJet_Gaia", "精灵喷气式战斗机" },
            { "bigfaerydragon", "巨型仙女龙" },
            { "Bomber_Demon", "恶魔巨龙轰炸机" },
            { "xenoUFObomber", "异形重型轰炸母舰" },
            { "HumanTitan", "人类泰坦机甲" },
            { "MA9000", "MA9000 未来自行火炮" },

            // Zombies
            { "Speed Zombie", "极速丧尸" },
            { "speed_zombie", "极速丧尸" },
            { "Spiked Zombie", "尖刺丧尸" },
            { "spiked_zombie", "尖刺丧尸" },
            { "Poison Zombie", "剧毒丧尸" },
            { "poison_zombie", "剧毒丧尸" },
            { "Acid Zombie", "强酸丧尸" },
            { "acid_zombie", "强酸丧尸" },
            { "Tentacle Zombie", "触手丧尸" },
            { "tentacle_zombie", "触手丧尸" },
            { "Stalker Zombie", "潜行者巨型丧尸" },
            { "stalker_zombie", "潜行者巨型丧尸" },
            { "Mother Zombie", "丧尸母体" },
            { "mother_zombie", "丧尸母体" },
            { "Fire Maniac Zombie", "狂焰丧尸" },
            { "fire_maniac_zombie", "狂焰丧尸" },
            { "Hulk Zombie", "浩克巨型丧尸" },
            { "hulk_zombie", "浩克巨型丧尸" },
            { "Abomination Zombie", "憎恶缝合丧尸" },
            { "abomination_zombie", "憎恶缝合丧尸" },
            { "Clawed Zombie", "利爪巨型丧尸" },
            { "clawed_zombie", "利爪巨型丧尸" },
            { "Balloon Zombie", "气球漂浮丧尸" },
            { "balloon_zombie", "气球漂浮丧尸" },
            { "Acid Man Zombie", "酸液狂人丧尸" },
            { "acid_man_zombie", "酸液狂人丧尸" },
            { "Demon Zombie", "恶魔丧尸" },
            { "demon_zombie", "恶魔丧尸" },
            { "Doctor Zombie", "瘟疫医生丧尸" },
            { "doctor_zombie", "瘟疫医生丧尸" },
            { "Druid Zombie", "德鲁伊丧尸" },
            { "druid_zombie", "德鲁伊丧尸" },
            { "Evil Horseman Zombie", "邪恶骑士丧尸" },
            { "evil_horseman_zombie", "邪恶骑士丧尸" },
            { "Ice Lich Zombie", "寒冰巫妖丧尸" },
            { "ice_lich_zombie", "寒冰巫妖丧尸" },
            { "Fairy Zombie", "妖精丧尸" },
            { "fairy_zombie", "妖精丧尸" },
            { "Tarantula Zombie", "狼蛛丧尸" },
            { "tarantula_zombie", "狼蛛丧尸" },

            // Weapons & Items
            { "Glock17, the bestest gun", "格洛克 17 手枪 (Glock 17)" },
            { "Uzi SMG", "乌兹冲锋枪 (Uzi SMG)" },
            { "Malorian Arms 3516", "马洛里安 3516 手枪" },
            { "Desert Eagle Mark XIX", "沙漠之鹰手枪 (Desert Eagle)" },
            { "Bestest Shotgun", "美式战术霰弹枪" },
            { "Greatest SHotgun", "重型独头弹霰弹枪" },
            { "HK MP7", "HK MP7 冲锋枪" },
            { "Thompson M1A1", "汤姆逊 M1A1 冲锋枪" },
            { "Sturmgewehr 44", "StG 44 突击步枪" },
            { "Avtomat Kalashnikov 47", "AK-47 突击步枪" },
            { "AK-103", "AK-103 突击步枪" },
            { "M16A1", "M16A1 突击步枪" },
            { "FAMAS F1", "FAMAS F1 突击步枪" },
            { "M4A1 Carbine", "M4A1 卡宾枪" },
            { "Green Laserarm", "绿色激光步枪" },
            { "XM8 Prototype", "XM8 原型突击步枪" },
            { "HK416", "HK416 突击步枪" },
            { "FNV varmit rifle", "猎狐步枪 (Varmint Rifle)" },
            { "Gauss_cannon", "高斯电磁炮" },
            { "Gauss cannon", "高斯电磁炮" },
            { "Minigun", "加特林机枪 (Minigun)" },
            { "Sniper", "狙击步枪" },
            { "Musket", "滑膛火枪 (Musket)" },
            { "Crossbow", "重弩 (Crossbow)" },
            { "Flintlock", "燧发手枪 (Flintlock)" },
            { "flamethrower", "火焰喷射器" },
            { "Rocket-propelled grenade", "RPG 火箭筒" },
            { "M32, the friendly grenade launcher!", "M32 转轮榴弹发射器" },
            { "madness_gas_launcher", "疯狂瓦斯榴弹发射器 (MGL)" },
            { "Madness gas launcher", "疯狂瓦斯榴弹发射器 (MGL)" },
            { "Fatman", "胖子微型核弹发射器 (Fat Man)" },
            { "Budget MIRV", "廉价版分导核弹 (Budget MIRV)" },
            { "Decent MIRV", "标准版分导核弹 (Decent MIRV)" },
            { "MIRV", "分导式多弹头核弹 (MIRV)" },
            { "MIRVBomb", "重型分导核弹 (MIRV Bomb)" },
            { "STRONGMIRV", "终极强力分导核弹 (Strong MIRV)" },
            { "Bath Salts", "狂暴浴盐 (Bath Salts)" },
            { "Fentanyl", "芬太尼 (Fentanyl)" },
            { "Morphine", "军用吗啡 (Morphine)" },
            { "Oxycodone", "羟考酮止痛剂 (Oxycodone)" },
            { "Ritalin", "利他林兴奋剂 (Ritalin)" },
            { "Modernarmor", "现代防弹护甲" },
            { "item_modernarmor", "现代防弹护甲" },
            { "Item modernarmor", "现代防弹护甲" },
            { "Modernhelmet", "现代战术头盔" },
            { "item_modernhelmet", "现代战术头盔" },
            { "Item modernhelmet", "现代战术头盔" },
            { "Modernboots", "现代作战军靴" },
            { "item_modernboots", "现代作战军靴" },
            { "Item modernboots", "现代作战军靴" },
            { "Grenadebelt", "战术手雷腰带" },
            { "item_grenadebelt", "战术手雷腰带" },
            { "Item grenadebelt", "战术手雷腰带" },
            { "Medicpack", "战地医疗包" },
            { "item_medicpack", "战地医疗包" },
            { "Item medicpack", "战地医疗包" },
        };

        // General English UI phrases -> Chinese
        private static readonly Dictionary<string, string> CnGeneralStrings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Welcome to ModernBox 3", "欢迎来到 ModernBox 5.01 (中文优化版)" },
            { "Here's a list of achievements!", "以下是 ModernBox 模组成就列表！" },
            { "For the full list go to GameBanana.", "完整贡献者名单请前往 GameBanana 页面查看。" },
            { "InfiniteBox is a god simulator game similar to WorldBox but more built for eras, space, and more advanced AI. It will also have a built in modloader with full documentation. Learn More by Joining the Discord Server!", "InfiniteBox 是作者正在开发的独立上帝模拟游戏，专为时代演进、太空探索和高级 AI 打造，并将内置完整文档的模组加载器。加入 Discord 了解更多！" },
            { "With M3 you can travel to other planets and even galaxies!", "在这里你可以跨越星海，前往其他行星甚至遥远的星系！" },
            { "Toggle custom galaxies you have installed, to install some or create your own, join the discord server.", "启用或禁用你已安装的自定义星系。如需下载或自制星系，欢迎加入 Discord 社区。" },
            { "Welcome to Tux's Pizza shop! I have to work on the multiplayer mod so you will take over my duty as a pizza shop owner for a bit. Please treat my employees (bluenight, dank, and morfos) with respect! oh, and to leave pizza mode, just restart the game.", "欢迎来到 Tux 的披萨店！我得去忙多人联机模组了，所以这家披萨店暂时交给你打理。请善待我的员工（bluenight、dank 和 morfos）！另外，重启游戏即可退出披萨模式。" },
            { "You must spawn this vehicle within a kingdom.", "必须在王国领土范围内生成该载具。" },
            { "Coming soon. Maybe", "即将推出……大概吧。" },
            { "One oven is overheating!", "有一台烤炉过热了！" },
            { "Shut it down", "暂时关停" },
            { "Production slows, but no damage.", "产量暂时下降，但不会损坏设备。" },
            { "Oven cooled. Slowed down a bit.", "烤炉已冷却，生产稍微放缓。" },
            { "Push it harder", "全功率超频运转" },
            { "Increases output, but risk damage.", "大幅提高产出，但有设备受损风险。" },
            { "Oven's going crazy fast!", "烤炉正在疯狂全速运转！" },
            { "A friend wants to join your pizza team!", "一位朋友想要加入你的披萨店团队！" },
            { "Hire them", "录用入职" },
            { "Adds a new employee.", "增加一名制作披萨的新员工。" },
            { "Friend hired!", "新员工已入职！" },
            { "Maybe later", "下次一定" },
            { "No change.", "维持现状。" },
            { "Friend told to wait.", "已婉拒，让对方再等等。" },
            { "The first pizza click... the journey begins.", "第一下披萨点击……奇妙的旅程开始了。" },
            { "10 pizza clicks! You're just getting started.", "点击 10 次披萨！你才刚刚热身。" },
            { "20 clicks! That's a lot of finger grease.", "点击 20 次！手指上全是芝士油香。" },
            { "30 clicks! This pizza is feeling the pressure.", "点击 30 次！这块披萨感受到了压力。" },
            { "40 clicks! Are you... okay?", "点击 40 次！你……还好吗？" },
            { "Halfway to pizza insanity. 50 clicks!", "距离披萨狂热还有一半：已点击 50 次！" },
            { "60 clicks. This is beyond casual pizza behavior.", "点击 60 次。这已经超越普通的吃货行为了。" },
            { "70 clicks... pizza is starting to sweat.", "点击 70 次……披萨开始冒汗了。" },
            { "80 clicks! Your dedication to pizza is admirable.", "点击 80 次！你对披萨的执着令人敬佩。" },
            { "90 clicks! Final stretch to the ultimate pizza moment.", "点击 90 次！距离终极披萨时刻仅一步之遥。" },
            { "100 clicks! YOU HAVE ENTERED... PIZZA MODE!", "点击 100 次！你已正式进入……披萨经营模式！" },
            { "NEW M2 ACHIEVEMENT UNLOCKED", "已解锁新的 MODERNBOX 成就" },
            { "Creator and main developer", "模组创始人与主程序" },
            { "Just Dank", "就是 Dank 本人" },
            { "Code/Sprites/Caretaker", "程序 / 贴图绘制 / 项目维护" },
            { "Main sprite creator", "主力贴图画师" },
            { "Various code stuff", "多项代码功能开发" },
            { "Various code & sprite stuff", "多项代码与贴图制作" },
            { "Various sprite stuff", "多项贴图美术制作" },
            { "Performing first time setup...", "正在执行首次初始化配置……" },
            { "THANKS TO", "特别鸣谢" },
            { "Welcome to M5", "欢迎使用 ModernBox M5" },
            { "Configure your ModernBox experience", "配置你的 ModernBox 游戏体验" },
            { "Welcome to ModernBox, change these settings or you'll be executed by the state.", "欢迎来到 ModernBox！你可以在下方根据喜好调整时代发展速度与现代武器平衡性。" },
            { "BEGIN  →", "开始配置  →" },
            { "TECHNOLOGY PROGRESSION", "科技与时代演进速度" },
            { "How fast should kingdoms advance through eras?", "各王国推进科技时代的节奏应该有多快？" },
            { "RAPID", "极速模式" },
            { "STANDARD", "标准模式" },
            { "SLOW", "慢速模式" },
            { "REALISM", "写实漫长" },
            { "Modern tech arrives almost immediately.", "现代科技几乎会在短期内迅速解锁。" },
            { "50–200 years between eras.", "每个时代之间大约间隔 50～200 年。" },
            { "Things take longer than usual.", "时代演进比标准节奏更加漫长。" },
            { "Thousands of years may pass.", "可能需要历经上千年才能迈入现代。" },
            { "NEXT  →", "下一步  →" },
            { "COMBAT BALANCE", "现代武器战斗平衡" },
            { "How powerful should modern weapons be?", "现代武器与载具相对于原版单位的威力如何？" },
            { "WEAK", "削弱模式" },
            { "VANILLA", "原版平衡" },
            { "ASYM.", "非对称战争" },
            { "OG MODE", "经典狂暴" },
            { "CARNAGE", "纯粹屠戮" },
            { "Vanilla units still hold their own.", "原版冷兵器单位依然有一战之力。" },
            { "As if the devs shipped this.", "贴合原版数值体系的平滑平衡。" },
            { "Modern armies dominate older ones.", "现代化的军队将对落后时代形成碾压。" },
            { "Classic 2023 chaos.", "还原 2023 初代版本的混乱高伤体验。" },
            { "Pure destruction. Lag warning.", "毁天灭地的超高伤害（注意卡顿）。" },
            { "JOIN THE COMMUNITY", "加入模组社区" },
            { "Want to join the Discord server?", "想要加入官方 Discord 讨论群组吗？" },
            { "Get patch notes, report bugs, and chat with other people who play the mod.", "获取最新更新日志、反馈 Bug 并与其他模组玩家交流。" },
            { "JOIN", "加入社区" },
            { "Open Discord\nand join the\nserver now.", "打开 Discord\n立即加入\n官方服务器。" },
            { "SAVE & CONTINUE  →", "保存并继续  →" },
            { "RESTART REQUIRED", "配置已保存（需重启生效）" },
            { "Settings saved successfully.\n\nThis UI disables other game systems to render.\nPlease exit and relaunch to apply your configuration.", "设置已成功保存。\n\n初始化向导界面在显示时会暂停其他游戏系统。\n请退出并重新启动游戏以应用新配置。" },
            { "EXIT GAME", "退出游戏" },
        };

        internal static bool HasKey(string key) => !string.IsNullOrEmpty(key) && English.ContainsKey(key);

        internal static bool IsChinese()
        {
            string lang = LocalizedTextManager.instance?.language;
            return lang == "cz" || lang == "ch" || lang == "zh" || lang == "tc";
        }

        internal static string Tr(string englishText)
        {
            if (string.IsNullOrEmpty(englishText) || !IsChinese()) return englishText;
            if (CnGeneralStrings.TryGetValue(englishText, out string cn)) return cn;
            if (CnByKey.TryGetValue(englishText, out cn)) return cn;
            if (CnUnitNames.TryGetValue(englishText, out cn)) return cn;
            return englishText;
        }

        private static string ResolveChinese(string key, string englishFallback)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (CnByKey.TryGetValue(key, out string byKey)) return byKey;
                if (key.EndsWith(" Description", StringComparison.Ordinal))
                {
                    string baseKey = key.Substring(0, key.Length - 12);
                    if (CnByKey.TryGetValue(baseKey + "_description", out string descByKey)) return descByKey;
                }
                if (key.EndsWith("_description", StringComparison.Ordinal))
                {
                    string baseKey = key.Substring(0, key.Length - 12);
                    if (CnByKey.TryGetValue(baseKey + " Description", out string descByKey)) return descByKey;
                    if (baseKey.StartsWith("spawn_", StringComparison.OrdinalIgnoreCase))
                    {
                        string unitId = baseKey.Substring(6);
                        if (CnUnitNames.TryGetValue(unitId, out string unitCn))
                            return "生成单位：" + unitCn;
                    }
                }
                if (key.StartsWith("spawn_", StringComparison.OrdinalIgnoreCase) && !key.Contains("description"))
                {
                    string unitId = key.Substring(6);
                    if (CnUnitNames.TryGetValue(unitId, out string unitCn))
                        return "生成：" + unitCn;
                }
                if (CnUnitNames.TryGetValue(key, out string unitByKey)) return unitByKey;
            }

            if (!string.IsNullOrEmpty(englishFallback))
            {
                if (CnUnitNames.TryGetValue(englishFallback, out string unitByText)) return unitByText;
                if (CnGeneralStrings.TryGetValue(englishFallback, out string genByText)) return genByText;
                if (CnByKey.TryGetValue(englishFallback, out string keyByText)) return keyByText;

                if (englishFallback.StartsWith("Spawn (", StringComparison.Ordinal) && englishFallback.EndsWith(")", StringComparison.Ordinal))
                {
                    string inner = englishFallback.Substring(7, englishFallback.Length - 8);
                    if (CnUnitNames.TryGetValue(inner, out string innerCn)) return $"生成 ({innerCn})";
                    return $"生成 ({inner})";
                }
                if (englishFallback.StartsWith("Spawn the unit: ", StringComparison.Ordinal))
                {
                    string inner = englishFallback.Substring(16).Trim();
                    if (CnUnitNames.TryGetValue(inner, out string innerCn)) return $"生成单位：{innerCn}";
                    return $"生成单位：{inner}";
                }
                if (englishFallback.StartsWith("The atomic nuke that droped out of nuke school", StringComparison.Ordinal))
                {
                    return "从核弹学校辍学去转行的半吊子原子弹（结果转行也失败了）。";
                }
            }

            return englishFallback ?? "";
        }

        internal static void Register(string key, string text)
        {
            if (string.IsNullOrEmpty(key)) return;
            string en = text ?? "";
            string cn = ResolveChinese(key, en);
            English[key] = en;
            Chinese[key] = cn;
            LM.Add("en", key, en);
            LM.Add("cz", key, cn);
            LM.Add("ch", key, cn);
            LM.AddToCurrentLocale(key, Value(key));
        }

        internal static void Bilingual(string key, string de, string en, string cn = null)
        {
            string resolvedCn = !string.IsNullOrEmpty(cn) ? cn : ResolveChinese(key, en);
            English[key] = en;
            German[key] = de;
            Chinese[key] = resolvedCn;
            LM.Add("en", key, en);
            LM.Add("de", key, de);
            LM.Add("cz", key, resolvedCn);
            LM.Add("ch", key, resolvedCn);
            LM.AddToCurrentLocale(key, Value(key));
        }

        private static string Value(string key)
        {
            string lang = LocalizedTextManager.instance?.language;
            if ((lang == "cz" || lang == "ch" || lang == "zh" || lang == "tc") && Chinese.TryGetValue(key, out string cn) && !string.IsNullOrEmpty(cn))
                return cn;
            if (lang == "de" && German.TryGetValue(key, out string de))
                return de;
            return English.TryGetValue(key, out string en) ? en : key;
        }

        internal static void Apply()
        {
            if (LocalizedTextManager.instance == null) return;
            foreach (string key in English.Keys) LM.AddToCurrentLocale(key, Value(key));
        }

        internal static ScrollWindow Window(string id, string title)
        {
            if (!English.ContainsKey(id)) Register(id, title);
            ScrollWindow visible = ScrollWindow.getCurrentWindow();
            ScrollWindow window = Windows.CreateNewWindow(id, Value(id));
            if (visible != null && visible.gameObject.activeInHierarchy && ScrollWindow.getCurrentWindow() != visible)
                AccessTools.Method(typeof(ScrollWindow), "setCurrentWindow").Invoke(null, new object[] { visible });
            return window;
        }

        internal static PowerButton Button(string id, Sprite sprite, string title, string description,
            Vector2 position, ButtonType type = ButtonType.Click, Transform parent = null, UnityAction call = null)
        {
            ButtonText(id, title, description);
            if (type == ButtonType.GodPower)
            {
                GodPower power = AssetManager.powers.get(id);
                if (power != null)
                {
                    Register(power.getLocaleID(), title ?? id);
                    Register(power.getDescriptionID(), description ?? title ?? id);
                }
            }
            return PowerButtons.CreateButton(id, sprite, Value(id), Value(id + " Description"), position, type, parent, call);
        }

        internal static void ButtonText(string id, string title, string description)
        {
            Register(id, title ?? id);
            Register(id + " Description", description ?? title ?? id);
            Register(id + "_description", description ?? title ?? id);
            Register(id.Underscore(), title ?? id);
            Register(id.Underscore() + "_description", description ?? title ?? id);
        }
    }

    [HarmonyPatch(typeof(LocalizedTextManager), nameof(LocalizedTextManager.loadLocalizedText))]
    internal static class ModernBoxLocaleReload
    {
        private static void Postfix() { ModernBoxLocale.Apply(); }
    }
}
