using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReflectionUtility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NCMS.Utils;
using System.Text.RegularExpressions;
using System.Reflection;
namespace ModernBox
{
    class CreditsWindow
    {
		public static int MoveDown = -50;
        private static ScrollWindow window;
        private static GameObject content;

    public static void init()
    {
        window = Windows.CreateNewWindow("CreditsWindow", "ModernBox");

        var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
        scrollView.gameObject.SetActive(true);

        var viewport = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport");
        var viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.sizeDelta = new Vector2(0, 17);

        content = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");

			          string gold = "#FFD700";
					  string Dgold = "#ffae00";
					  var description =
			@"<color='" + gold + @"'>For the full list go to GameBanana.</color>
			";
					  var name = window.transform.Find("Background").Find("Name").gameObject;
					  var nameText = name.GetComponent<Text>();
					  nameText.text = description;
					  nameText.color = new Color(0.9f, 0.6f, 0, 1);
					  nameText.fontSize = 10;
					  nameText.alignment = TextAnchor.LowerCenter;
					  nameText.supportRichText = true;
					  name.transform.SetParent(window.transform.Find("Background").Find("Scroll View").Find("Viewport").Find("Content"));
					  name.SetActive(true);
					  var nameRect = name.GetComponent<RectTransform>();
					  nameRect.anchorMin = new Vector2(0.5f, 1);
					  nameRect.anchorMax = new Vector2(0.5f, 1);
					  nameRect.offsetMin = new Vector2(-90f, nameText.preferredHeight * -1);
					  nameRect.offsetMax = new Vector2(90f, -17);
					  nameRect.sizeDelta = new Vector2(180, nameText.preferredHeight + 50);
					  window.GetComponent<RectTransform>().sizeDelta = new Vector2(0, nameText.preferredHeight + 50);
					  name.transform.localPosition = new Vector2(name.transform.localPosition.x, ((nameText.preferredHeight / 2) + 60) * -1);
    
        new ButtonBuilder("tux")
            .SetSprite(Resources.Load<Sprite>("ui/icons/authors/sss"))
            .SetTitle("Tuxxego")
            .SetDescription("Creator and main developer")
            .SetPosition(0, 1)
            .SetType(ButtonType.Click)
            .SetTransform(content.transform)
            .Build();

        new ButtonBuilder("dank")
            .SetSprite(Resources.Load<Sprite>("ui/icons/authors/Dank"))
            .SetTitle("Dank")
            .SetDescription("Just Dank")
            .SetPosition(1, 1)
            .SetType(ButtonType.Click)
            .SetTransform(content.transform)
            .Build();

        new ButtonBuilder("morfos")
            .SetSprite(Resources.Load<Sprite>("ui/icons/authors/morfos"))
            .SetTitle("MORFOS")
            .SetDescription("Code/Sprites/Caretaker")
            .SetPosition(2, 1)
            .SetType(ButtonType.Click)
            .SetTransform(content.transform)
            .Build();           

        new ButtonBuilder("sherman")
            .SetSprite(Resources.Load<Sprite>("ui/icons/authors/sherman"))
            .SetTitle("Full Auto Sherman")
            .SetDescription("Main sprite creator")
            .SetPosition(3, 1)
            .SetType(ButtonType.Click)
            .SetTransform(content.transform)
            .Build();    

        new ButtonBuilder("schrott")
            .SetSprite(Resources.Load<Sprite>("ui/icons/authors/schrott"))
            .SetTitle("MrSchrott")
            .SetDescription("Various code stuff")
            .SetPosition(4, 1)
            .SetType(ButtonType.Click)
            .SetTransform(content.transform)
            .Build();   

        new ButtonBuilder("bluenight")
            .SetSprite(Resources.Load<Sprite>("ui/icons/authors/bluenight"))
            .SetTitle("BlueNight")
            .SetDescription("Various code & sprite stuff")
            .SetPosition(1, 2)
            .SetType(ButtonType.Click)
            .SetTransform(content.transform)
            .Build();   

        new ButtonBuilder("ariel")
            .SetSprite(Resources.Load<Sprite>("ui/icons/authors/ariel"))
            .SetTitle("Arielp2")
            .SetDescription("Various sprite stuff")
            .SetPosition(2, 2)
            .SetType(ButtonType.Click)
            .SetTransform(content.transform)
            .Build();

        new ButtonBuilder("trike")
            .SetSprite(Resources.Load<Sprite>("ui/icons/authors/trike"))
            .SetTitle("Trike")
            .SetDescription("Various sprite stuff")
            .SetPosition(3, 2)
            .SetType(ButtonType.Click)
            .SetTransform(content.transform)
            .Build();
    }
  }
}