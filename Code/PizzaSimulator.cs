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
using UnityEngine.Purchasing.MiniJSON;
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
    // Manu-Fix 025: NCMS reuses windows by ID. Build their contents once, keep
    // the template header in place, and allow only one answer per event.
    private readonly Dictionary<string, ScrollWindow> eventWindows = new Dictionary<string, ScrollWindow>();
    private readonly Dictionary<string, ScrollWindow> employeeWindows = new Dictionary<string, ScrollWindow>();
    private RandomEvent activeEvent;
    private bool eventOpening;
    private int nextEmployeeId = 1000;
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
        AddEmployee(EmployeeLibrary.CreateEmployee("bluenight"));
        AddEmployee(EmployeeLibrary.CreateEmployee("dank"));
        AddEmployee(EmployeeLibrary.CreateEmployee("morfos"));

        ScheduleNextEvent();
    }

    void Update()
    {
        foreach (var emp in employees)
        {
            pizzaCount += emp.pizzasPerSecond * Time.deltaTime;
        }

        if (activeEvent != null)
        {
            // Closing an event without an answer dismisses it; it must not
            // keep the next event blocked or permit a later duplicate answer.
            if (!eventOpening && !ScrollWindow.isAnimationActive() &&
                !ScrollWindow.isCurrentWindow("PizzaEvent_" + activeEvent.id))
            {
                activeEvent = null;
                ScheduleNextEvent();
            }
        }
        else
        {
            eventTimer += Time.deltaTime;
            if (eventTimer >= nextEventTime && !ScrollWindow.isWindowActive() && !ScrollWindow.isAnimationActive())
            {
                TriggerRandomEvent();
            }
        }

        worldTipTimer += Time.deltaTime;
        if (worldTipTimer >= worldTipInterval && !ScrollWindow.isWindowActive() && !eventOpening)
        {
            worldTipTimer = 0f;
            // Manu-Fix 023: fester Hinweistext, kein Textschluessel (pTranslate false) - Begruendung in UnitTracker.SpawnVehicle.
            WorldTip.showNow($"🍕 Total pizzas made: {Mathf.FloorToInt(pizzaCount)}", false, "top", 1.5f);
        }
        
    }

    void ScheduleNextEvent()
    {
        eventTimer = 0f;
        nextEventTime = Random.Range(30f, 60f);
    }

    void TriggerRandomEvent()
    {
        if (activeEvent != null) return;
        ScheduleNextEvent();
        RandomEvent randomEvent = EventLibrary.GetRandomEvent();
        if (!eventWindows.ContainsKey(randomEvent.id))
        {
            ScrollWindow window = ModernBoxLocale.Window("PizzaEvent_" + randomEvent.id, "ModernBox");
            Transform content = PrepareContent(window, randomEvent.description);
            new ButtonBuilder($"{randomEvent.id}_option1")
                .SetSprite(Resources.Load<Sprite>("ui/icons/authors/sss"))
                .SetTitle(randomEvent.option1Title)
                .SetDescription(randomEvent.option1Description)
                .SetPositionWindow(2, 3)
                .SetType(ButtonType.Click)
                .SetTransform(content)
                .SetFunction(() => AnswerEvent(randomEvent, randomEvent.option1Function))
                .Build();
            new ButtonBuilder($"{randomEvent.id}_option2")
                .SetSprite(Resources.Load<Sprite>("ui/icons/authors/sss"))
                .SetTitle(randomEvent.option2Title)
                .SetDescription(randomEvent.option2Description)
                .SetPositionWindow(4, 3)
                .SetType(ButtonType.Click)
                .SetTransform(content)
                .SetFunction(() => AnswerEvent(randomEvent, randomEvent.option2Function))
                .Build();
            eventWindows.Add(randomEvent.id, window);
        }
        activeEvent = randomEvent;
        eventOpening = true;
        StartCoroutine(OpenEvent(randomEvent));
    }

    private IEnumerator OpenEvent(RandomEvent randomEvent)
    {
        // NCMS initializes a newly created window with a hide tween. Let that
        // complete before requesting the first show, otherwise WorldBox drops it.
        yield return null;
        while (ScrollWindow.isAnimationActive()) yield return null;
        if (activeEvent == randomEvent) Windows.ShowWindow("PizzaEvent_" + randomEvent.id);
        eventOpening = false;
    }

    private void AnswerEvent(RandomEvent randomEvent, Action answer)
    {
        if (activeEvent != randomEvent) return;
        activeEvent = null;
        eventOpening = false;
        worldTipTimer = -3f; // Preserve the answer's three-second feedback.
        ScheduleNextEvent();
        if (ScrollWindow.isCurrentWindow("PizzaEvent_" + randomEvent.id))
            ScrollWindow.hideAllEvent();
        answer?.Invoke();
    }

    public void HireEmployee()
    {
        // Manu-Fix 025: the old hire action only created a window. Add the
        // employee to production and avoid reusing an existing employee ID.
        string id;
        do { id = "friend_" + nextEmployeeId++; }
        while (employees.Any(employee => employee.id == id) || Windows.GetWindow("PizzaEmployee_" + id) != null);
        AddEmployee(EmployeeLibrary.CreateEmployee(id));
    }

    private void AddEmployee(Employee emp)
    {
        if (emp == null || string.IsNullOrEmpty(emp.id) || employees.Any(employee => employee.id == emp.id)) return;
        CreateEmployeeWindow(emp);
        employees.Add(emp);
    }

    public void CreateEmployeeWindow(Employee emp)
    {
        if (emp == null || string.IsNullOrEmpty(emp.id) || employeeWindows.ContainsKey(emp.id)) return;
        ScrollWindow window = ModernBoxLocale.Window("PizzaEmployee_" + emp.id, "ModernBox");
        PrepareContent(window, $"{emp.name}\nPizzas/sec: {emp.pizzasPerSecond:F2}");
        employeeWindows.Add(emp.id, window);
    }

    private static Transform PrepareContent(ScrollWindow window, string description)
    {
        Transform scrollView = window.transform.Find("Background/Scroll View");
        scrollView.gameObject.SetActive(true);
        Transform content = scrollView.Find("Viewport/Content");
        var rect = content.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 170);
        Text template = window.transform.Find("Background/Name").GetComponent<Text>();
        var descriptionObject = new GameObject("PizzaDescription", typeof(RectTransform), typeof(Text));
        descriptionObject.transform.SetParent(content, false);
        Text text = descriptionObject.GetComponent<Text>();
        text.font = template.font;
        text.text = description;
        text.color = new Color(0.9f, 0.6f, 0, 1);
        text.fontSize = 10;
        text.alignment = TextAnchor.UpperCenter;
        text.raycastTarget = false;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 1);
        textRect.anchorMax = new Vector2(0.5f, 1);
        textRect.pivot = new Vector2(0.5f, 1);
        textRect.anchoredPosition = new Vector2(0, -15);
        textRect.sizeDelta = new Vector2(180, 60);
        template.gameObject.SetActive(false);
        return content;
    }
}
