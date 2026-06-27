using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using NCMS;
using NCMS.Utils;
using ModernBox;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    public float timePlayed;
    public int bombsDropped;
    public int zomboos;
    public int unitsSpawned;
    public int planetsVisited;
    public string currentPlanet;
    public string currentPlanetType;
    public int currentVehicles;
    public string currentEra = "none";
    public string currentEraDescription = "There is no description for this era as this isn't an era! Spawn some units to make a kingdom and this should turn into Medieval.";

    private Text statLabel;
    private Text statLabel2;
    private Text statLabel3;
    private Image glowingImage;
    private float pulseTime = 0f;
    private Image flashingAdImage;
    private float flashTime = 0f;

    public string eraoverride;
    public bool enableMedieval;
    public bool enableRenaissance;
    public bool enableModern;
    public bool enableHyperfuture;

    private Vector3 glowStartPos;
    private bool glowPosInitialized = false;

    private bool typedBombs = false;
    private bool typedVehicles = false;
    private bool typedAI = false;
    private bool typedZombies = false;

    private bool isTyping = false; 

    private string cursorChar = "●";
    private float cursorBlinkSpeed = 6f;
    private bool cursorVisible = true;
    private float cursorTimer = 0f;
    private string activeTypingLine = "";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        currentPlanet = PlanetManager.instance.GetCurrentPlanet();
        currentPlanetType = PlanetManager.instance.GetCurrentPlanetType();
    }

        public void toggleMedieval()
                {
                    Main.modifyBoolOption("MedievalOption", PowerButtons.GetToggleValue("era_mediaval_toggle"));
                    if (PowerButtons.GetToggleValue("era_mediaval_toggle"))
                    {
                        turnOnMedieval();
                        return;
                    }
                    turnOffMedieval();
                }

        public void turnOnMedieval()
        {
            enableMedieval = true;
        }

        public void turnOffMedieval()
        {
            enableMedieval = false;
        }

        public void toggleRenaissance()
                {
                    Main.modifyBoolOption("RenaissanceOption", PowerButtons.GetToggleValue("era_renaissance_toggle"));
                    if (PowerButtons.GetToggleValue("era_renaissance_toggle"))
                    {
                        turnOnRenaissance();
                        return;
                    }
                    turnOffRenaissance();
                }

        public void turnOnRenaissance()
        {
            enableRenaissance = true;
        }

        public void turnOffRenaissance()
        {
            enableRenaissance = false;
        }

        public void toggleModern()
                {
                    Main.modifyBoolOption("ModernOption", PowerButtons.GetToggleValue("era_modern_toggle"));
                    if (PowerButtons.GetToggleValue("era_modern_toggle"))
                    {
                        turnOnModern();
                        return;
                    }
                    Debug.Log(enableMedieval);
                    Debug.Log(enableRenaissance);
                    Debug.Log(enableModern);
                    Debug.Log(enableHyperfuture);
                    turnOffModern();
                }

        public void turnOnModern()
        {
            enableModern = true;
        }

        public void turnOffModern()
        {
            enableModern = false;
        }

        public void toggleHyperfuture()
                {
                    Main.modifyBoolOption("HyperfutureOption", PowerButtons.GetToggleValue("era_hyperfuture_toggle"));
                    if (PowerButtons.GetToggleValue("era_hyperfuture_toggle"))
                    {
                        turnOnHyperfuture();
                        return;
                    }
                    turnOffHyperfuture();
                }

        public void turnOnHyperfuture()
        {
            enableHyperfuture = true;
        }

        public void turnOffHyperfuture()
        {
            enableHyperfuture = false;
        }

    public void RegisterStatLabel(Text label)
    {
        statLabel = label;
    }

    public void RegisterStatLabel2(Text label)
    {
        statLabel2 = label;
    }

    public void RegisterStatLabel3(Text label)
    {
        statLabel3 = label;
    }

    public void RegisterImage(Image image)
    {
        glowingImage = image;
    }

    public void RegisterFlashingImage(Image image)
    {
        flashingAdImage = image;
    }

    public void DropBomb() => bombsDropped++;
    public void VisitPlanet() => planetsVisited++;
    public void SpawnUnit() => unitsSpawned++;

    public void SetEra(string era)
    {
        eraoverride = era;
    }

    void Update()
    {
        timePlayed += Time.deltaTime;

        int potentialUnits = 0;
        foreach (Actor actor in MapBox.instance.units)
        {
            if (actor != null && actor.hasTrait("Unitpotential"))
            {
                potentialUnits++;
            }
        }
        currentVehicles = potentialUnits;

        int zomboos = 0;
        foreach (Actor actor in MapBox.instance.units)
        {
            if (actor != null && actor.hasTrait("zombie"))
            {
                zomboos++;
            }
        }

        if (statLabel != null)
        {

            if (!isTyping)
            {

                statLabel.text = GoofyShit();
            }

            if (!typedBombs && bombsDropped > 0 && !isTyping)
            {
                typedBombs = true;
                StartCoroutine(TypeLine($"<b>Bombs Dropped:</b> {bombsDropped}"));
            }

            if (!typedVehicles && currentVehicles > 0 && !isTyping)
            {
                typedVehicles = true;
                StartCoroutine(TypeLine($"<b>Current Vehicles:</b> {currentVehicles}"));
            }

            if (!typedAI && unitsSpawned > 0 && !isTyping)
            {
                typedAI = true;
                StartCoroutine(TypeLine($"<b>AI Nukes Dropped:</b> {unitsSpawned}"));
            }

            if (!typedZombies && zomboos > 0 && !isTyping)
            {
                typedZombies = true;
                StartCoroutine(TypeLine($"<b>Zombies:</b> {zomboos}"));
            }
        }

        if (statLabel2 != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>Current Era:</b> {currentEra}");
            sb.AppendLine($"<b>Current Era Description:</b> {currentEraDescription}");
            statLabel2.text = sb.ToString();
        }

        if (statLabel3 != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<color=#70D4FC><b>Planet</b></color>  <color=#F0F0E0>{currentPlanet}</color>");
            sb.AppendLine($"<color=#70D4FC><b>Planet Type</b></color>  <color=#A8E08A>{currentPlanetType}</color>");
            sb.AppendLine($"<color=#70D4FC><b>Planets Visited</b></color>  <color=#F0F0E0>{PlanetManager.instance.getplanettotalcount()}</color>");
            statLabel3.text = sb.ToString();
        }

        if (glowingImage != null)
        {

            if (!glowPosInitialized)
            {
                glowStartPos = glowingImage.transform.localPosition;
                glowPosInitialized = true;
            }

            pulseTime += Time.deltaTime;

            float glowA = Mathf.Sin(pulseTime * 2f) * 0.25f + 0.75f;            
            float glowB = Mathf.Sin(pulseTime * 5f + 1.2f) * 0.08f + 1f;        
            float finalGlow = glowA * glowB;

            float colorShift = Mathf.Sin(pulseTime * 0.8f) * 0.05f;
            float r = Mathf.Clamp01(finalGlow + colorShift);
            float g = Mathf.Clamp01(finalGlow);
            float b = Mathf.Clamp01(finalGlow - colorShift * 0.75f);

            glowingImage.color = new Color(r, g, b, 1f);

            float baseScale   = 1f + Mathf.Sin(pulseTime * 1.4f) * 0.06f;
            float rippleScale = 1f + Mathf.Sin(pulseTime * 7f) * 0.015f;
            float finalScale  = baseScale * rippleScale;

            glowingImage.transform.localScale = Vector3.one * finalScale;

            float rot = Mathf.Sin(pulseTime * 1.8f) * 3f;
            glowingImage.transform.localRotation = Quaternion.Euler(0f, 0f, rot);

            float driftX = Mathf.Sin(pulseTime * 0.6f) * 0.75f;
            float driftY = Mathf.Cos(pulseTime * 0.9f) * 0.75f;

            glowingImage.transform.localPosition = glowStartPos + new Vector3(driftX, driftY, 0f);
        }

        if (flashingAdImage != null)
        {
            var hover = flashingAdImage.GetComponent<DiscordAdHover>();
            if (hover != null && hover.isHovered)
            {
                flashingAdImage.color = Color.yellow;
                flashingAdImage.transform.localScale = Vector3.one * 1.15f;
            }
            else
            {
                flashTime += Time.deltaTime * 4f;
                float scale = 1f + Mathf.Abs(Mathf.Sin(flashTime)) * 0.15f;
                flashingAdImage.color = new Color(1f, 1f, 1f, 0.9f + Mathf.Sin(flashTime * 2f) * 0.1f);
                flashingAdImage.transform.localScale = Vector3.one * scale;
            }
        }
    }

    private string FormatTime(float time)
    {
        int totalSeconds = Mathf.FloorToInt(time);

        int days = totalSeconds / 86400;                   
        int hours = (totalSeconds % 86400) / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        if (days > 0)
        {
            if (hours > 0 && minutes > 0)
                return $"{days}d {hours:D2}h {minutes:D2}m {seconds:D2}s";

            if (hours > 0)
                return $"{days}d {hours:D2}h {seconds:D2}s";

            return $"{days}d {seconds:D2}s";
        }

        if (hours > 0)
        {
            if (minutes > 0)
                return $"{hours:D2}h {minutes:D2}m {seconds:D2}s";

            return $"{hours:D2}h {seconds:D2}s";
        }

        if (minutes > 0)
            return $"{minutes:D2}m {seconds:D2}s";

        return $"{seconds:D2}s";
    }

    private IEnumerator TypeLine(string lineToType)
    {
        isTyping = true;
        activeTypingLine = "";

        statLabel.text += "\n";

        StringBuilder visibleText = new StringBuilder();
        int i = 0;

        while (i < lineToType.Length)
        {

            if (lineToType[i] == '<')
            {
                int closing = lineToType.IndexOf('>', i);
                if (closing != -1)
                {

                    string fullTag = lineToType.Substring(i, closing - i + 1);
                    visibleText.Append(fullTag);
                    i = closing + 1;

                    statLabel.text = GoofyShit() + "\n" +
                                    visibleText.ToString() +
                                    (cursorVisible ? cursorChar : "");

                    yield return null;
                    continue;
                }
            }

            visibleText.Append(lineToType[i]);
            i++;

            statLabel.text = GoofyShit() + "\n" +
                            visibleText.ToString() +
                            (cursorVisible ? cursorChar : "");

            yield return new WaitForSeconds(0.03f);
        }

        statLabel.text = GoofyShit() + "\n" + visibleText.ToString();

        isTyping = false;
        activeTypingLine = "";
    }

    private string GoofyShit()
    {
        StringBuilder sb = new StringBuilder();

        if (ModernBoxPrefs.Balance == BalanceMode.Carnage)
        {
            sb.AppendLine("<color=red><b>CARNAGE</b></color>");
        }

        sb.AppendLine($"<b>Time Playing:</b> {FormatTime(timePlayed)}");
        sb.AppendLine($"<b>Current Era:</b> {currentEra}");
        sb.AppendLine($"<b>Version: 5.01</b>");

        if (typedBombs)
            sb.AppendLine($"<b>Bombs Dropped:</b> {bombsDropped}");

        if (typedVehicles)
            sb.AppendLine($"<b>Current Vehicles:</b> {currentVehicles}");

        if (typedAI)
            sb.AppendLine($"<b>AI Nukes Dropped:</b> {unitsSpawned}");

        if (typedZombies)
            sb.AppendLine($"<b>Zombies:</b> {zomboos}");

        return sb.ToString().TrimEnd();
    }
}