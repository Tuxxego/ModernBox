//========= MODERNBOX 2.2.0.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;
using ai;
using ai.behaviours;

namespace ModernBox
{
    class Name : MonoBehaviour
	{

        public static void init()
		{

            NameGeneratorAsset ModernNames = new NameGeneratorAsset();
            ModernNames.part_groups = new List<string>();
            ModernNames.id = "Modern_Names";
		    ModernNames.part_groups.Add("Donald,Benedict,Marshall,Samuel,Samantha,Amanda,Arthur,Kim,William,Will,Hannibal,Rhodes,Estelle,Senior,Michael,Norman,Nancy,Wilmot,Robert,Collins,Myrtle,Willow,Cedric,Muriel,Christian,Tasha,Natasha,Iris,Perry,Sherlock,Dr.,Kate,Foster,Hadden,Leopold,Beata,Beatrice,Trace,Russell,Baz,Bart,Sadie,Grace,Pena,Presley,Altair,Halsey,Waldo,Basil,Viktor,Victor,Herman,Salvage,Oscard,Phyllis,Marc,Matt,Jose,Jerome,Bradley,Cirino,Francesco,Domenico,Affonso,Orfeo,Cristoforo,Magnusson,Bitch,Kleiner,Alyx,Wyfinn,Glitchy,Clyde,Bitch,Ass,Dick,Sonic,FUCK,Brian,Brayon,Bryan,Bryant,Braylon,Evan,Bobby,Brandon,Fodrell,Bell,Keywan,Willium,Williums,Angel,Garcia,Perez,Andrews,Doctor,Assdick,Whore,Hoe,Tucker,Leywin,Dexter,Nguyen,Le,Tran,Lee,Ford,Mitchell,Russell,Odinson,Ernest,Bond,Lannister,Reynard,Royal,Junior,Langstaff,Mendez,Jenning,Hayes,Uchiha,Senju,Cooper,Harmon,Fuller,Walker,George,Peaky,Blinders,Archer,Walton,Subaru,Khaleesi,Johnston,Luscardio,Doyle,Caldwell,Connor,Kenaway,Harper,Pearce,Strong,Stephenson,Erickson,Dunn,Slater,Chelsea,Green,Brown,Cunnings,Douglas,Shillingford,Kinsman,Yates,Barrett,Castro,Herrera,King,Vere,Gilbert,Larson,Goodwin,Jenson,Garner,Bakr,al-Malik,Walid,I,II,III,IV,V,VI,Walid,al-Aziz,Mansur,Saffah,Hadi,Rashid,Ma'mum,Amin,Mu'tasim,Wathiq,Mutawakkil,Muntasir,Musta'in,Mu'tazz,Muhtadi,Mu'tamid,Muktafi,Qahir,Radi,Muttaqi,Qa'aim,Zahir,Ghaffari,Noor,Azam,Tawil,Ghazi,Jamal,Sahli,Qadir,Aydin,Hoda,Shah,Hamed,Pasha,Pahlavi,Shehadeh,Akbari,Amir,Azad,Loutfi,Kamel,Farah,Shakir,Saleem,Latif,Ghattas,Abdul,Bonaparte,Perrault,Lavigne,Branchett,Charpentier,Favreu,Thibault,Jacques,Parris,Hardy,Deniau,Boucher,Dubois,Duval,Dumont,Augustin,Bellamy,Reyer,Marie,Sauvageon,Morel,Lyon,Richard,Lachapelle,Traver,Rattan,Naidu,Purhit,Chaudhuri,Mittal,Nagy,Rama,Pathak,Kashyap,Dhar,Khatri,Dugal,Patil,Sarna,Talwar,Kohli,Bogdak,Devi,Zulu,Raju,Sura,Basu,Kalla,Mane,Singh,Sindh,Kallah,Romani,Udinesi,Loggia,Marino,Davide,Mazzeo,Angelo,Cociarelli,Pugliesi,Mazzi,Alfonsi,Pisani,Siciliani,Mussolini,De,Cavour,De,Capet,Rua,Padovesi,Palerma,Gucci,Schmitler,Rommel,Hess,Student,Arndt,Endres,Hauffe,Herr,Herold,Hoppe,Hoth,Hube,Hüsnersdorff,Hühner,Hauck,Fahrmbacher,Falley,Felber,Weber,Frieber,Block,Berthold,Bader,Gimmler,Geyer,Kamecke,Karst,Both,Burgdorf,Dietl,Hermann,Koch,Knieß,Kissel,Krause,Müller,Ruoff,Praun,Braun,Loch,Siebert,Schwalbe,Rauch,Ziegler,Warnecke,Versock,Utz,Stapf,Wedel,Wagner,Kiku,Takehiko,Shinji,Showa,Dai,Cho,Takako,Kumiko,Hotaka,Kaede,Setsuko,Momoko,Bunko,Akemi,Sakiko,Ren,Ai,Tsuneo,Noboru,Hiroko,Yoshito,Masa,Kumiko,Jong-Un,Ae-Cha,Il-Sung,Jong-Il,Sun-Hi,Hyuk,Yoon,Jin-Kyong,Min,Hoon,So,Dong,Cha,Iseul,Yun,Soo-Yun,Hyun-Jung,Zedong,Jinping,Xioping,Xiaohui,Qin,Tao,Zhu,Chen,Tian,Xia,Lei,Zhang,Mao,Zeng,Chang,Qian,Zhou,Yin,Wei,Tang,Fan,Tan,Gao,Liang,Jiang,Gong,Hao,Wei,Vladimir,Yakov,Zhukov,Trotsky,Leonid,Iosif,Dimitri,Nikita,Nazariy,Aleksander,Desya,Ivan,Natalya,Sasha,Alyona,Anastasiya,Anoushka,Anya,Dinara,Dominika,Doroteya,Eva,Faina,Galina,Inessa,Katya,Karina,Kira,Luda,Marina,Sonya,Ulyana,Alexei,Boris,Lev,Lukashenko,Oleg,Rurik,Stanislav,Valentin,Vasily,Viktor,Yury,Vasco,Eduardo,Domingo,Claudio,Oscar,Pablo,Agapito,Gerardo,Sebastian,Solomon,Victor,Felipe,Basilio,Leandro,Ana,Chita,Elmira,Piedad,Delfina,Ximena,Francisca,Adelita,Costanza,Shaka,Jabali,Shani,Njowga,Jimoh,Hashaan,Hamisi,Tabari,Mhina,Badru,Rashid,Kibwe,Mbwana,Kani,Shany,Kesi,Imani,Goma,Kesi,Mwanahamisi,Adiah,Nigesa,Najuma,Kapuki,Erdogan,Ilker,Ersin,Murat,Burak,Erdinç,Nijaz,Hakan,Ümit,Taylan,Serhat,Onur,Demir,Bora,Tayyib,Öztürk,Mustafa,Mansur,Adnan,Kerem,Emira,Başak,Yasmina,Sefa,Binnaz,Sanem,Basak,Dila,Aylin,Sira,Ifor,Owen,Meirion,Lolyn,Brynmor,Yale,Caron,Gwen,Ffraid,Deryn,Tucker,Josh,John,Pinky,Ness,Luke,Milligan,Leo,Jack,Aaron,Jeromy,Clark,Andrew,Craig,Penny,Wyatt,Finn,Lucas,Dale,Dave,Rovert,Robert,Hayes,Ronald,Reagan,Donald,Trump,Joe,Mama,Biden,Washington,Corse,George,Jorge,Darek,Darren,Red,Blue,Green,Yellow,Tuxxego,Charles,King,Martin,Luther,Kennedy,Roger,Fred,Ian,Anthony,Padilla,Hecox,Magnusson(Bitch),Kleiner,Alyx,Wyfinn,Glitchy,Clyde,Bitch,Ass,Dick,Sonic,FUCK,Brian,Brayon,Bryan,Bryant,Braylon,Evan,Bobby,Brandon,Fodrell,Bell,Keywan,Willium,Williums,Angel,Garcia,Perez,Andrews,Doctor,Assdick,Whore,Hoe,Tucker,Leywin,Dexter,Nguyen,Le,Tran,Lee,Ford,Mitchell,Russell,Odinson,Ernest,Bond,Lannister,Reynard,Royal,Junior,Langstaff,Mendez,Jenning,Hayes,Uchiha,Senju,Cooper,Harmon,Fuller,Walker,George,Peaky,Blinders,Archer,Walton,Subaru,Khaleesi,Johnston,Luscardio,Doyle,Caldwell,Connor,Kenaway,Harper,Pearce,Strong,Stephenson,Erickson,Dunn,Slater,Chelsea,Green,Brown,Cunnings,Douglas,Shillingford,Kinsman,Yates,Barrett,Buttfucker,Castro,Herrera,King,Vere,Gilbert,Larson,Goodwin,Jenson,Garner,Bakr,al-Malik,Walid,I,II,III,IV,V,VI,Walid,al-Aziz,Mansur,Saffah,Hadi,Rashid,Ma'mum,Amin,Mu'tasim,Wathiq,Mutawakkil,Muntasir,Musta'in,Mu'tazz,Muhtadi,Mu'tamid,Muktafi,Qahir,Radi,Muttaqi,Qa'aim,Zahir,Ghaffari,Noor,Azam,Tawil,Ghazi,Jamal,Sahli,Qadir,Aydin,Hoda,Shah,Hamed,Pasha,Pahlavi,Shehadeh,Akbari,Amir,Azad,Loutfi,Kamel,Farah,Shakir,Saleem,Latif,Ghattas,Abdul,Bonaparte,Perrault,Lavigne,Branchett,Charpentier,Favreu,Thibault,Jacques,Parris,Hardy,Deniau,Boucher,Dubois,Duval,Dumont,Augustin,Bellamy,Reyer,Marie,Sauvageon,Morel,Lyon,Richard,Lachapelle,Traver,Rattan,Naidu,Purhit,Chaudhuri,Mittal,Nagy,Rama,Pathak,Kashyap,Dhar,Khatri,Dugal,Patil,Sarna,Talwar,Kohli,Bogdak,Devi,Zulu,Raju,Sura,Basu,Kalla,Mane,Singh,Sindh,Kallah,Romani,Udinesi,Loggia,Marino,Davide,Mazzeo,Angelo,Cociarelli,Pugliesi,Mazzi,Alfonsi,Pisani,Siciliani,Mussolini,De,Cavour,De,Capet,Rua,Padovesi,Palerma,Gucci,Schmitler,Rommel,Hess,Student,Arndt,Endres,Hauffe,Herr,Herold,Hoppe,Hoth,Hube,Hüsnersdorff,Hühner,Hauck,Fahrmbacher,Falley,Felber,Weber,Frieber,Block,Berthold,Bader,Gimmler,Geyer,Kamecke,Karst,Both,Burgdorf,Dietl,Hermann,Koch,Knieß,Kissel,Krause,Müller,Ruoff,Praun,Braun,Loch,Siebert,Schwalbe,Rauch,Ziegler,Warnecke,Versock,Utz,Stapf,Wedel,Wagner,Kiku,Takehiko,Shinji,Showa,Dai,Cho,Takako,Kumiko,Hotaka,Kaede,Setsuko,Momoko,Bunko,Akemi,Sakiko,Ren,Ai,Tsuneo,Noboru,Hiroko,Yoshito,Masa,Kumiko,Jong-Un,Ae-Cha,Il-Sung,Jong-Il,Sun-Hi,Hyuk,Yoon,Jin-Kyong,Min,Hoon,So,Dong,Cha,Iseul,Yun,Soo-Yun,Hyun-Jung,Zedong,Jinping,Xioping,Xiaohui,Qin,Tao,Zhu,Chen,Tian,Xia,Lei,Zhang,Mao,Zeng,Chang,Qian,Zhou,Yin,Wei,Tang,Fan,Tan,Gao,Liang,Jiang,Gong,Hao,Wei,Vladimir,Yakov,Zhukov,Trotsky,Leonid,Iosif,Dimitri,Nikita,Nazariy,Aleksander,Desya,Ivan,Natalya,Sasha,Alyona,Anastasiya,Anoushka,Anya,Dinara,Dominika,Doroteya,Eva,Faina,Galina,Inessa,Katya,Karina,Kira,Luda,Marina,Sonya,Ulyana,Alexei,Boris,Lev,Lukashenko,Oleg,Rurik,Stanislav,Valentin,Vasily,Viktor,Yury,Vasco,Eduardo,Domingo,Claudio,Oscar,Pablo,Agapito,Gerardo,Sebastian,Solomon,Victor,Felipe,Basilio,Leandro,Ana,Chita,Elmira,Piedad,Delfina,Ximena,Francisca,Adelita,Costanza,Shaka,Jabali,Shani,Njowga,Jimoh,Hashaan,Hamisi,Tabari,Mhina,Badru,Rashid,Kibwe,Mbwana,Kani,Shany,Kesi,Imani,Goma,Kesi,Mwanahamisi,Adiah,Nigesa,Najuma,Kapuki,Erdogan,Ilker,Ersin,Murat,Burak,Erdinç,Nijaz,Hakan,Ümit,Taylan,Serhat,Onur,Demir,Bora,Tayyib,Öztürk,Mustafa,Mansur,Adnan,Kerem,Emira,Başak,Yasmina,Sefa,Binnaz,Sanem,Basak,Dila,Aylin,Sira,Ifor,Owen,Meirion,Lolyn,Brynmor,Yale,Caron,Gwen,Ffraid,Deryn,Tucker,Josh,John,Pinky,Ness,Luke,Milligan,Leo,Jack,Aaron,Jeromy,Clark,Andrew,Craig,Penny,Wyatt,Finn,Lucas,Dale,Dave,Rovert,Robert,Hayes,Ronald,Reagan,Donald,Trump,Joe,Mama,Biden,Washington,Corse,George,Jorge,Darek,Darren,Red,Blue,Green,Yellow,Tuxxego,Charles,King,Martin,Luther,Kennedy,Roger,Fred,Ian,Anthony,Padilla,Hecox,Magnusson(Bitch),Kleiner,Alyx,Wyfinn,Glitchy,Clyde,Bitch,Ass,Dick,Sonic,FUCK,Brian,Brayon,Bryan,Bryant,Braylon,Evan,Bobby,Brandon,Fodrell,Bell,Keywan");
		    ModernNames.part_groups.Add(" ");
            ModernNames.part_groups.Add("Tucker,Leywin,Dexter,Nguyen,Le,Tran,Lee,Ford,Mitchell,Russell,Odinson,Ernest,Bond,Lannister,Reynard,Royal,Junior,Langstaff,Mendez,Jenning,Hayes,Uchiha,Senju,Cooper,Harmon,Fuller,Walker,George,Peaky,Blinders,Archer,Walton,Subaru,Khaleesi,Johnston,Luscardio,Doyle,Caldwell,Connor,Kenaway,Harper,Pearce,Strong,Stephenson,Erickson,Dunn,Slater,Chelsea,Green,Brown,Cunnings,Douglas,Shillingford,Kinsman,Yates,Barrett,Castro,Herrera,King,Vere,Gilbert,Larson,Goodwin,Jenson,Garner,Bakr,al-Malik,Walid,I,II,III,IV,V,VI,Walid,al-Aziz,Mansur,Saffah,Hadi,Rashid,Ma'mum,Amin,Mu'tasim,Wathiq,Mutawakkil,Muntasir,Musta'in,Mu'tazz,Muhtadi,Mu'tamid,Mu'tadid,Muktafi,Qahir,Radi,Muttaqi,Qa'aim,Zahir,Ghaffari,Noor,Azam,Tawil,Ghazi,Jamal,Sahli,Qadir,Aydin,Hoda,Shah,Hamed,Pasha,Pahlavi,Shehadeh,Akbari,Amir,Azad,Loutfi,Kamel,Farah,Shakir,Saleem,Latif,Ghattas,Abdul,Bonaparte,Perrault,Lavigne,Branchett,Charpentier,Favreu,Thibault,Jacques,Parris,Hardy,Deniau,Boucher,Dubois,Duval,Dumont,Augustin,Bellamy,Reyer,Marie,Sauvageon,Morel,Lyon,Richard,Lachapelle,Traver,Rattan,Naidu,Purhit,Chaudhuri,Mittal,Nagy,Rama,Pathak,Kashyap,Dhar,Khatri,Dugal,Patil,Sarna,Talwar,Kohli,Bogdak,Devi,Zulu,Raju,Sura,Basu,Kalla,Mane,Singh,Sindh,Kallah,Romani,Udinesi,Loggia,Marino,Davide,Mazzeo,Angelo,Cociarelli,Pugliesi,Mazzi,Alfonsi,Pisani,Siciliani,Mussolini,De,Cavour,De,Capet,Rua,Padovesi,Palerma,Gucci,Schmitler,Rommel,Hess,Student,Arndt,Endres,Hauffe,Herr,Herold,Hoppe,Hoth,Hube,Hüsnersdorff,Hühner,Hauck,Fahrmbacher,Falley,Felber,Weber,Frieber,Block,Berthold,Bader,Gimmler,Geyer,Kamecke,Karst,Both,Burgdorf,Dietl,Hermann,Koch,Knieß,Kissel,Krause,Müller,Ruoff,Praun,Braun,Loch,Siebert,Schwalbe,Rauch,Ziegler,Warnecke,Versock,Utz,Stapf,Wedel,Wagner,Kiku,Takehiko,Shinji,Showa,Dai,Cho,Takako,Kumiko,Hotaka,Kaede,Setsuko,Momoko,Bunko,Akemi,Sakiko,Ren,Ai,Tsuneo,Noboru,Hiroko,Yoshito,Masa,Kumiko,Jong-Un,Ae-Cha,Il-Sung,Jong-Il,Sun-Hi,Hyuk,Yoon,Jin-Kyong,Min,Hoon,So,Dong,Cha,Iseul,Yun,Soo-Yun,Hyun-Jung,Zedong,Jinping,Xioping,Xiaohui,Qiaolian,Kang,Bai,Shihong,Meixiu,Puyi,Zhong,Shan,Longwei,Huwei,Ma,Duyi,Lihua,Qingsheng,Qiaolian,Qingsheng,Yuan,Xue,Delun,Yun,Renshu,Jun,Tin,Shan,Xilai,Sog,Jie,Yuanjun,Weimin,Liu,Hu,Jintao,Heng,Meihui,Xiaso,Xhosa,Huifang,Huan,Shun,Juan,Stalin,Lenin,Gorbachev,Krushev,Yarapolk,Putin,Nikolaev,Orlova,Stepanov,Zuhukov,Kozlov,Fedoro,Belova,Morozov,Kulikova,Petrograd,Pavlov,Sokolayev,Aleksandrov,Sergeev,Nikitin,Novikov,Gusev,Zaytsev,Borisova,Smirnov,Semenov,Ivanov,Bogdanov,Kuzmin,Qunming,Sorokin,Mikhailov,Baranov,Barnakhov,Ilienov,Vinogradov,Kiselev,Vinoliev,Morozova,Karpov,Dmitrieva,Zakharov,Yarapolk,Batuta,Vladov,Egorov,Marapolk,Tarsov,Borisova,Grigoreva,Fedorov,Sorokina,Smirnoff,Sidrov,Semenova,Vasiliev,Morozov,Kozlov,Nikitin,Golubev,Vinogradov,Agapov,Ayad,Amer,Ashraf,Awad,Abdul,Bakir,Bilal,Dawoud,Essa,Faraj,Ghazzawi,Hussain,Hajjar,Jameel,Karim,Khoury,Qadir,Rafiq,Shehab,Tawil,Al,Tajr,Zaman,Bilbao,Franco,Avena,Leyva,Teran,Olguin,Salto,Blasco,Armas,Arnulfo,Arnandez,Torre,Navvaro,Gomez,Martin,Serrano,Blanco,Rubio,Ortega,Moreno,Alvarez,Garcia,Fernandez,Morales,Mbutu,Adeoyo,Bankole,Solarin,Mbadinuju,Attah,Baka,Okeke,Buari,Magoro,Okoye,Oyo,Dahomey,Hausa,Okilo,Azikiwe,Gbadamosi,Kalejaiye,Olanrenwaju,Dimka,Akpabia,Nzeogwu,Olanrewaju,Madaki,Onwudiwe,Soyinka,Yar'Adua,Mbanefo,Boro,Orji,Okar,Amaechi,Dafur,Kanem,Dakar,Bankole,Okonjo,Onyejekwe,Ataseven,Dervis,Sanli,Yakin,Akbas,Köse,Kucuk,Kılıç,Yilmaz,Ulusoy,Yildrim,Yilmaz,Özdemir,Balut,Barak,Tilki,Penry,Comey,Hughes,Sealy,Williams,Gittings,Geddings,Kerry");
		    ModernNames.addTemplate("part_group");
            AssetManager.name_generator.add(ModernNames);
			
			NameGeneratorAsset ModernOrcNames = new NameGeneratorAsset();
            ModernOrcNames.id = "Modern_Orc_Names";
            ModernOrcNames.part_groups = new List<string>();
		    ModernOrcNames.part_groups.Add("Grommash, Thrakka, Grulok, Durgar, Morgash, Drakka, Krusk, Gornak, Thokk, Gruul, Roktar, Azog, Skarok, Guldan, Karghul, Zogar, Drekthar, Garrosh, Nazgul, Kharaz, Loktar, Grimgor, Uruk, Gorthok, Zulmog, Skarok, Thurg, Rakthar, Morgok, Hargul, Bolg, Drekthuul, Krolmog, Narzug, Nazrak, Thokgul, Skragg, Zarkun, Brulg, Murgok, Krulg, Gornar, Grishnakh, Drokthar, Lugmog, Gorlak, Durgok, Brokthar, Thargor, Narthok, Azrak, Gorgash, Murgoth, Throkash, Kargor, Zarkoth, Hurgok, Skulmog, Lokrak, Drekthok, Morgash, Gorzak, Krorgar, Brugmog, Gornash, Nazrok, Thorgor, Drakthul, Skorash, Zulrag, Grulnar, Thrugor, Grishnak");
		    ModernOrcNames.part_groups.Add(" ");
            ModernOrcNames.part_groups.Add("Bloodaxe, Ironhide, Skullcrusher, Blackfang, Grimtusk, Stonefist, Darkblade, Bonecrusher, Doomhammer, Ironskull, Warblade, Blackrock, Steelheart, Bloodrage, Thunderfist, Ironjaw, Blackthorn, Rockjaw, Deathbringer, Goreblade, Ironclaw, Bonegnasher, Darkstorm, Ironfang, Skullsplitter, Rageshadow, Doomclaw, Steelgore, Bloodthorn, Blackmaw, Grimskull, Thunderstrike, Ironrage, Deathfang, Goregrinder, Skullsmasher, Bonecrusher, Darkrider, Ironskin, Blackclaw, Warfist, Bloodskull, Grimshank, Thunderjaw, Ironbane, Deathstrike, Steelhand, Bonebreaker, Darktide, Gorefang, Skullrender, Ironheart, Blackthunder, Warshadow, Bloodfury, Grimsnarl, Thunderheart, Ironsoul, Deathgrip, Steelclaw, Bonetaker, Darkblade, Gorefist, Skullcrusher, Ironhammer, Blackfang, Warbringer, Bloodiron, Grimtusk, Thunderblade, Ironhide, Deathfang, Skullsplitter");
		    ModernOrcNames.addTemplate("part_group");
            AssetManager.name_generator.add(ModernOrcNames);
			
			NameGeneratorAsset ModernElfNames = new NameGeneratorAsset();
            ModernElfNames.part_groups = new List<string>();
            ModernElfNames.id = "Modern_Elf_Names";
		    ModernElfNames.part_groups.Add("Lirael, Eledrin, Thalindra, Elowen, Galadriel, Aricen, Lorandor, Ilyndor, Thandoril, Larethian, Caelitha, Aerendir, Faelarion, Elowyn, Silvaris, Thalindria, Arandur, Lirindel, Elanor, Thranduil, Lorien, Ilyndrial, Lariel, Aranduil, Liraelith, Caladrel, Elandrial, Thalindor, Thalion, Eloweth, Galanor, Aricendil, Lorandriel, Ilyndaria, Thandorith, Larethiel, Caelindra, Aelarion, Elowethil, Thalindar, Larieth, Silvandor, Thranduilith, Lorithil, Ilyndoril, Elanelor, Thalindriel, Aricenith, Lirithor, Galathil, Caladrial, Elanorith, Thalionil, Thalowyn, Elowendir, Faelarian, Aralitha, Lirindrial, Caelandor, Lorienith, Ilyndorith, Thandorithil, Larethianil, Silvandrial, Elanoril");
		    ModernElfNames.part_groups.Add(" ");
            ModernElfNames.part_groups.Add("Silverleaf, Moonshadow, Starwhisper, Windrunner, Nightbloom, Frostfall, Oakenshade, Sunblade, Swiftarrow, Shadowdancer, Elmsong, Emberheart, Raincaller, Stormfeather, Thundersong, Lightweaver, Brightwood, Goldengrove, Skywatcher, Songsteel, Windgazer, Moonfire, Dewalker, Starshower, Emberlight, Frostgale, Leafwalker, Stormcaller, Lightblossom, Sunshower, Glitterthorn, Swiftbreeze, Snowfall, Daydreamer, Shadeleaf, Silverbrook, Thunderstrike, Nightfrost, Dreamshadow, Goldensun, Sunwhisper, Swiftgale, Shadowveil, Windheart, Moonblossom, Rainwhisper, Starflower, Lightfoot, Snowsong, Frostblade, Dewshine, Thundershadow, Lightdancer, Emberfrost, Elmbreeze, Windchaser, Stormleaf, Glittershade, Goldengale, Sunstar, Rainshimmer, Leafshroud, Starcaller, Moonblade, Swiftshade, Skyblossom, Snowwhisper, Nightgazer, Lightwatcher, Frostwind, Thunderfall, Shadowdance");
		    ModernElfNames.addTemplate("part_group");
            AssetManager.name_generator.add(ModernElfNames);
			
			NameGeneratorAsset ModernDwarfNames = new NameGeneratorAsset();
            ModernDwarfNames.part_groups = new List<string>();
            ModernDwarfNames.id = "Modern_Dwarf_Names";
		    ModernDwarfNames.part_groups.Add("Balin, Thorin, Dwalin, Gimli, Fili, Kili, Gloin, Dori, Nori, Ori, Bifur, Bofur, Bombur, Oin, Thrain, Thror, Durin, Brokk, Hogni, Hreidmar, Eitri, Bruni, Hrothgar, Thrainn, Orin, Frerin, Torhild, Grundi, Snorri, Farin, Thrainor, Skirfir, Grimir, Hjor, Thrainar, Alaric, Bardin, Dvalin, Orin, Frar, Vindin, Gardrin, Thrainar, Regin, Lodin, Kvorin, Thrainar, Norin, Grorn, Rorin, Fargrim, Brolin, Thrainar, Jorund, Mjord, Thrainar, Ulfgar, Thrainar, Grimur, Borin, Brannin, Thrainar, Karin, Hreinar, Thrainar, Dagnal, Thorin, Thorin, Thrainar, Dufin, Andin, Thrainar, Helgi, Dori, Thrainar.");
		    ModernDwarfNames.part_groups.Add(" ");
            ModernDwarfNames.part_groups.Add("Ironbeard, Stoneforge, Graniteheart, Hammerstrike, Steelhelm, Oakenshield, Fireforge, Anvilcrusher, Goldhammer, Thunderbeard, Bronzeaxe, Blackanvil, Stonefoot, Ironpick, Flintgrinder, Steelbeard, Rockdelver, Silveranvil, Stoneheart, Thunderfist, Goldenshovel, Copperforge, Emberpick, Stonebrow, Steelhand, Ironshaper, Granitehelm, Firebeard, Anvilbreaker, Goldensmith, Hammerhelm, Rockseeker, Stonewarden, Ironforge, Stonecutter, Bronzefist, Silverbrow, Emberforge, Thunderhammer, Steelcutter, Goldenshield, Ironshaper, Stoneshield, Graniteaxe, Fireanvil, Anvilbeard, Hammerheart, Steelbrow, Stonedelver, Ironhand, Graniteforge, Goldendelver, Stoneanvil, Thunderpick, Steelgrinder, Ironhelm, Bronzeheart, Stoneshaper, Goldenshaper, Stonehammer, Thunderhelm, Firecutter, Anvilshaper, Bronzebeard, Hammercutter, Steelforge, Silverforge, Emberanvil, Ironcutter, Stoneshaper, Goldengrinder, Stoneforge.");
		    ModernDwarfNames.addTemplate("part_group");
            AssetManager.name_generator.add(ModernDwarfNames);
			
			NameGeneratorAsset JetNames = new NameGeneratorAsset();
            JetNames.part_groups = new List<string>();
            JetNames.id = "Jet_Names";
		    JetNames.part_groups.Add("F-,V-,X-,J-,S-");
		    JetNames.part_groups.Add("10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40");
            JetNames.part_groups.Add("A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z");
		    JetNames.addTemplate("part_group");
            AssetManager.name_generator.add(JetNames);
			
			NameGeneratorAsset HumveeNames = new NameGeneratorAsset();
            HumveeNames.part_groups = new List<string>();
            HumveeNames.id = "Humvee_Names";
		    HumveeNames.part_groups.Add("H-");
		    HumveeNames.part_groups.Add("10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40");
            HumveeNames.part_groups.Add("A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z");
		    HumveeNames.addTemplate("part_group");
            AssetManager.name_generator.add(HumveeNames);
			
			NameGeneratorAsset MIRVNames = new NameGeneratorAsset();
            MIRVNames.part_groups = new List<string>();
            MIRVNames.id = "MIRV_Names";
		    MIRVNames.part_groups.Add("F-,V-,X-,J-,S-");
		    MIRVNames.part_groups.Add("10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40");
            MIRVNames.part_groups.Add("A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z");
		    MIRVNames.addTemplate("part_group");
            AssetManager.name_generator.add(MIRVNames);


		}	
		public static void toggleNames()
        {
            Main.modifyBoolOption("namesOption", PowerButtons.GetToggleValue("names_toggle"));
            if (PowerButtons.GetToggleValue("names_toggle"))
            {
                turnOnNames();
                return;
            }
            turnOffNames();
        }
		
		public static void toggleOtherNames()
        {
            Main.modifyBoolOption("othernamesOption", PowerButtons.GetToggleValue("other_names_toggle"));
            if (PowerButtons.GetToggleValue("other_names_toggle"))
            {
                turnOnOtherNames();
                return;
            }
            turnOffOtherNames();
        }


        public static void turnOnNames()
        {
            ActorAsset human = AssetManager.actor_library.get("human");
			human.name_template_unit = "Modern_Names";
        }
		
		public static void turnOnOtherNames()
        {

            ActorAsset orc = AssetManager.actor_library.get("orc");
			orc.name_template_unit = "Modern_Orc_Names";

			ActorAsset elf = AssetManager.actor_library.get("elf");
			elf.name_template_unit = "Modern_Elf_Names";
			
			ActorAsset dwarf = AssetManager.actor_library.get("dwarf");
			dwarf.name_template_unit = "Modern_Dwarf_Names";
        }
		
		public static void turnOffOtherNames()
        {

            ActorAsset orc = AssetManager.actor_library.get("orc");
			orc.name_template_unit = "orc_unit";
			
			ActorAsset elf = AssetManager.actor_library.get("elf");
			elf.name_template_unit = "elf_unit";

			ActorAsset dwarf = AssetManager.actor_library.get("dwarf");
			dwarf.name_template_unit = "dwarf_unit";
        }

        public static void turnOffNames()
        {

            ActorAsset human = AssetManager.actor_library.get("human");
			human.name_template_unit = "human_unit"; 
        }        
    }
}