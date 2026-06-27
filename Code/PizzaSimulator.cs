using System;
using System.IO;
using System.Linq;
using NCMS;
using tools;
using NCMS.Utils;
using UnityEngine;
using ReflectionUtility;
using HarmonyLib;
using System.Reflection;
using Newtonsoft.Json;
using ModernBox;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Threading.Tasks;
using static Config;
using System.Reflection.Emit;
using UnityEngine.Tilemaps;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using UnityEngine.CrashReportHandler;
using System.IO.Compression;
using System.Threading;
using System.Text;
using Beebyte.Obfuscator;
using ai;
using ai.behaviours;
using life.taxi;
using SleekRender;
using tools.debug;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using WorldBoxConsole;
using UnityEngine.UI;
using static TopTileLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions.Must;
using Random=UnityEngine.Random;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.General;

public class PizzaSimulator : MonoBehaviour
{
    public static PizzaSimulator instance;

    private List<Employee> employees = new List<Employee>();
    private float pizzaCount = 0f;

    private float eventTimer = 0f;
    private float nextEventTime;

    private float worldTipTimer = 0f;
    private float worldTipInterval = 1f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        employees.Add(EmployeeLibrary.CreateEmployee("bluenight"));
        employees.Add(EmployeeLibrary.CreateEmployee("dank"));
        employees.Add(EmployeeLibrary.CreateEmployee("morfos"));

        foreach (var emp in employees)
        {
            CreateEmployeeWindow(emp);
        }

        ScheduleNextEvent();
    }

    void Update()
    {
        foreach (var emp in employees)
        {
            pizzaCount += emp.pizzasPerSecond * Time.deltaTime;
        }

        eventTimer += Time.deltaTime;
        if (eventTimer >= nextEventTime)
        {
            TriggerRandomEvent();
            ScheduleNextEvent();
        }

        worldTipTimer += Time.deltaTime;
        if (worldTipTimer >= worldTipInterval)
        {
            worldTipTimer = 0f;
            WorldTip.showNow($"🍕 Total pizzas made: {Mathf.FloorToInt(pizzaCount)}", true, "top", 1.5f);
        }
        
    }

    void ScheduleNextEvent()
    {
        eventTimer = 0f;
        nextEventTime = Random.Range(30f, 60f);
    }

    void TriggerRandomEvent()
    {
        RandomEvent randomEvent = EventLibrary.GetRandomEvent();

        ScrollWindow window = Windows.CreateNewWindow("PizzaEvent_" + randomEvent.id, "ModernBox");

        var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
        scrollView.gameObject.SetActive(true);

        var viewport = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport");
        var viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.sizeDelta = new Vector2(0, 17);

        GameObject content = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");

        GameObject name = window.transform.Find("Background").Find("Name").gameObject;
        Text nameText = name.GetComponent<Text>();
        nameText.text = $"<color=#FFD700>{randomEvent.description}</color>";
        nameText.color = new Color(0.9f, 0.6f, 0, 1);
        nameText.fontSize = 10;
        nameText.alignment = TextAnchor.UpperCenter;
        nameText.supportRichText = true;
        name.transform.SetParent(content.transform);
        name.SetActive(true);

        RectTransform nameRect = name.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1);
        nameRect.anchorMax = new Vector2(0.5f, 1);
        nameRect.offsetMin = new Vector2(-90f, nameText.preferredHeight * -1);
        nameRect.offsetMax = new Vector2(90f, -17);
        nameRect.sizeDelta = new Vector2(180, nameText.preferredHeight + 50);
        window.GetComponent<RectTransform>().sizeDelta = new Vector2(0, nameText.preferredHeight + 50);
        name.transform.localPosition = new Vector2(name.transform.localPosition.x, ((nameText.preferredHeight / 2) + 30) * -1);

        new ButtonBuilder($"{randomEvent.id}_option1")
            .SetSprite(Resources.Load<Sprite>("ui/icons/authors/sss"))
            .SetTitle(randomEvent.option1Title)
            .SetDescription(randomEvent.option1Description)
            .SetPosition(0, 4)
            .SetType(ButtonType.Click)
            .SetTransform(content.transform)
            .SetFunction(() => randomEvent.option1Function?.Invoke())
            .Build();

        new ButtonBuilder($"{randomEvent.id}_option2")
            .SetSprite(Resources.Load<Sprite>("ui/icons/authors/sss"))
            .SetTitle(randomEvent.option2Title)
            .SetDescription(randomEvent.option2Description)
            .SetPosition(0, 4)
            .SetType(ButtonType.Click)
            .SetTransform(content.transform)
            .SetFunction(() => randomEvent.option2Function?.Invoke())
            .Build();

        Windows.ShowWindow("PizzaEvent_" + randomEvent.id);
    }

    public void CreateEmployeeWindow(Employee emp)
    {
        ScrollWindow window = Windows.CreateNewWindow("PizzaEmployee_" + emp.id, "ModernBox");

        var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View");
        scrollView.gameObject.SetActive(true);

        var viewport = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport");
        var viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.sizeDelta = new Vector2(0, 17);

        GameObject content = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/{window.name}/Background/Scroll View/Viewport/Content");

        GameObject name = window.transform.Find("Background").Find("Name").gameObject;
        Text nameText = name.GetComponent<Text>();
        nameText.text = $"<color=#FFD700>{emp.name}</color>\n<color=#ffae00>Pizzas/sec: {emp.pizzasPerSecond}</color>";
        nameText.color = new Color(0.9f, 0.6f, 0, 1);
        nameText.fontSize = 10;
        nameText.alignment = TextAnchor.UpperCenter;
        nameText.supportRichText = true;
        name.transform.SetParent(content.transform);
        name.SetActive(true);

        RectTransform nameRect = name.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1);
        nameRect.anchorMax = new Vector2(0.5f, 1);
        nameRect.offsetMin = new Vector2(-90f, nameText.preferredHeight * -1);
        nameRect.offsetMax = new Vector2(90f, -17);
        nameRect.sizeDelta = new Vector2(180, nameText.preferredHeight + 50);
        window.GetComponent<RectTransform>().sizeDelta = new Vector2(0, nameText.preferredHeight + 50);
        name.transform.localPosition = new Vector2(name.transform.localPosition.x, ((nameText.preferredHeight / 2) + 30) * -1);
    }
}
