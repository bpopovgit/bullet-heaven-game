using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapTransitionDirector : MonoBehaviour
{
    private const string GameSceneName = "Game";
    private const float FadeDuration = 0.55f;
    private const float CompleteHoldSeconds = 1.6f;
    private const float IntroAnnouncementDuration = 2.2f;

    public static MapTransitionDirector Instance { get; private set; }

    private float _districtStartTime;
    private bool _transitioning;
    private bool _runEnded;
    private Image _fadeOverlay;
    private Camera _camera;

    public float DistrictElapsedSeconds => Mathf.Max(0f, Time.time - _districtStartTime);
    public float DistrictRemainingSeconds
    {
        get
        {
            MapDefinition d = RunSession.CurrentDistrict;
            return d == null ? 0f : Mathf.Max(0f, d.DurationSeconds - DistrictElapsedSeconds);
        }
    }

    private static bool _sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_sceneHookRegistered)
            return;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        _sceneHookRegistered = true;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.IsValid() || scene.name != GameSceneName)
            return;

        if (FindObjectOfType<MapTransitionDirector>() != null)
            return;

        if (!RunSession.IsActive)
            RunSession.StartNewRun();

        GameObject host = new GameObject("MapTransitionDirector");
        host.AddComponent<MapTransitionDirector>();
    }

    private void Awake()
    {
        Instance = this;
        _camera = Camera.main;
        BuildFadeOverlay();
        ApplyCurrentDistrict(initial: true);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (_transitioning || _runEnded)
            return;

        MapDefinition district = RunSession.CurrentDistrict;
        if (district == null)
            return;

        // District transition is disabled until distinct maps exist. Re-enable when each
        // district has its own scene/layout instead of just a tint + timer.
        // if (Time.time - _districtStartTime >= district.DurationSeconds)
        //     StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        _transitioning = true;

        MapDefinition cleared = RunSession.CurrentDistrict;
        Announce($"{cleared.DisplayName.ToUpperInvariant()} CLEARED", 1.6f);
        yield return new WaitForSecondsRealtime(CompleteHoldSeconds);

        yield return Fade(0f, 1f);

        RunSession.AdvanceDistrict();

        if (RunSession.IsRunComplete)
        {
            _runEnded = true;
            RunSession.EndRun();
            Announce("RUN COMPLETE", 5f);
            yield return Fade(1f, 0.85f);
            yield break;
        }

        ClearEnemies();
        ApplyCurrentDistrict(initial: false);

        yield return Fade(1f, 0f);
        _transitioning = false;
    }

    private void ApplyCurrentDistrict(bool initial)
    {
        MapDefinition district = RunSession.CurrentDistrict;
        if (district == null)
            return;

        _districtStartTime = Time.time;

        if (_camera == null)
            _camera = Camera.main;
        if (_camera != null)
            _camera.backgroundColor = district.BackgroundTint;

        string verb = initial ? "ENTERING" : "ADVANCING TO";
        Announce($"{verb} {district.DisplayName.ToUpperInvariant()}\n{district.Flavor}", IntroAnnouncementDuration);
        Debug.Log($"DISTRICT START: {district.DisplayName} (idx {RunSession.CurrentDistrictIndex}, hp×{district.EnemyHpMultiplier}, dmg×{district.EnemyDamageMultiplier}, boss={district.IsBossDistrict})");
    }

    private void ClearEnemies()
    {
        EnemyHealth[] all = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        int destroyed = 0;
        for (int i = 0; i < all.Length; i++)
        {
            EnemyHealth eh = all[i];
            if (eh == null) continue;

            FactionMember fm = eh.GetComponent<FactionMember>();
            if (fm != null && fm.Faction == FactionType.Human)
                continue;

            Destroy(eh.gameObject);
            destroyed++;
        }
        Debug.Log($"DISTRICT CLEAR: removed {destroyed} non-Human entities for the next district.");
    }

    private void BuildFadeOverlay()
    {
        GameObject canvasGO = new GameObject("MapTransitionCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject overlayGO = new GameObject("FadeOverlay");
        overlayGO.transform.SetParent(canvasGO.transform, false);
        _fadeOverlay = overlayGO.AddComponent<Image>();
        _fadeOverlay.color = new Color(0f, 0f, 0f, 0f);
        _fadeOverlay.raycastTarget = false;

        RectTransform rt = _fadeOverlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (_fadeOverlay == null)
            yield break;

        float elapsed = 0f;
        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / FadeDuration));
            _fadeOverlay.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        _fadeOverlay.color = new Color(0f, 0f, 0f, to);
    }

    private static void Announce(string message, float duration)
    {
        if (RunAnnouncementUI.Instance == null || string.IsNullOrWhiteSpace(message))
            return;
        RunAnnouncementUI.Instance.ShowMessage(message, duration);
    }
}
