using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicSwitcher : MonoBehaviour
{
    public static SceneMusicSwitcher Instance { get; private set; }

    [Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip music;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Audio Source (en este mismo GameObject)")]
    [SerializeField] private AudioSource audioSource;

    [Header("Scenes que NO debe tocar (ya las llevas hechas)")]
    [SerializeField] private string[] ignoreScenes = { "Main Menu", "IntroDialogue" };

    [Header("Lista de músicas por escena")]
    [SerializeField] private SceneMusic[] sceneMusics;

    [Header("Fallback (si una escena no está en la lista)")]
    [SerializeField] private AudioClip defaultMusic;
    [Range(0f, 1f)][SerializeField] private float defaultVolume = 1f;

    [Header("Resume ramp (Pause -> Resume)")]
    [SerializeField] private float resumeRampSeconds = 2f;
    [SerializeField] private float resumeStartPitch = 0.65f;

    private Coroutine pitchRoutine;
    private int pitchToken;

    // ✅ recuerda el volumen real de la escena (para restaurarlo al reanudar)
    private float lastSetVolume = 1f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // ✅ Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.pitch = 1f;

        // Solo valor inicial por si aún no has llamado a Play()
        lastSetVolume = Mathf.Clamp01(audioSource.volume);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplyMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicForScene(scene.name);
    }

    private void ApplyMusicForScene(string sceneName)
    {
        // Ignorar escenas
        for (int i = 0; i < ignoreScenes.Length; i++)
        {
            if (string.Equals(ignoreScenes[i], sceneName, StringComparison.OrdinalIgnoreCase))
                return;
        }

        // Buscar música asignada para esa escena
        for (int i = 0; i < sceneMusics.Length; i++)
        {
            if (string.Equals(sceneMusics[i].sceneName, sceneName, StringComparison.OrdinalIgnoreCase))
            {
                Play(sceneMusics[i].music, sceneMusics[i].volume);
                return;
            }
        }

        // Fallback
        Play(defaultMusic, defaultVolume);
    }

    private void Play(AudioClip clip, float vol)
    {
        if (clip == null) return;

        // ✅ si venías de pausa con pitch lento, resetea
        ResetPitchImmediate();

        // ✅ guarda volumen “real” de esta escena
        lastSetVolume = Mathf.Clamp01(vol);

        // Si ya está sonando ese clip, solo ajusta volumen
        if (audioSource.clip == clip)
        {
            audioSource.volume = lastSetVolume;

            // ✅ si estaba pausada/parada, asegúrate de que suena
            audioSource.UnPause();
            if (!audioSource.isPlaying) audioSource.Play();

            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = lastSetVolume;
        audioSource.Play();
    }

    // =========================================================
    // ✅ Control desde Pause/Resume/Death (con corte inmediato)
    // =========================================================

    public void PauseMusic()
    {
        if (audioSource == null) return;

        pitchToken++;
        if (pitchRoutine != null) StopCoroutine(pitchRoutine);

        // ✅ CORTE INMEDIATO
        audioSource.volume = 0f;

        if (audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.Pause(); // por si está en un estado raro, lo dejamos pausado igualmente
    }

    public void ResumeMusicWithRamp()
    {
        ResumeMusicWithRamp(resumeRampSeconds, resumeStartPitch);
    }

    public void ResumeMusicWithRamp(float rampSeconds, float startPitch)
    {
        if (audioSource == null) return;
        if (audioSource.clip == null) return;

        pitchToken++;
        if (pitchRoutine != null) StopCoroutine(pitchRoutine);

        // ✅ restaura volumen real de la escena
        audioSource.volume = Mathf.Clamp01(lastSetVolume);

        // ✅ CLAVE: si estaba STOP (GameOver), UnPause no hace nada -> forzamos Play si no suena
        audioSource.UnPause();
        if (!audioSource.isPlaying)
            audioSource.Play();

        // empieza lento y sube a normal
        audioSource.pitch = Mathf.Max(0.05f, startPitch);
        pitchRoutine = StartCoroutine(PitchRampRoutine(pitchToken, rampSeconds));
    }

    private IEnumerator PitchRampRoutine(int token, float seconds)
    {
        float start = audioSource.pitch;
        float target = 1f;

        if (seconds <= 0f)
        {
            audioSource.pitch = target;
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            if (token != pitchToken) yield break;

            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / seconds);
            audioSource.pitch = Mathf.Lerp(start, target, a);
            yield return null;
        }

        audioSource.pitch = target;
    }

    public void StopMusic()
    {
        if (audioSource == null) return;

        pitchToken++;
        if (pitchRoutine != null) StopCoroutine(pitchRoutine);

        // ✅ CORTE INMEDIATO
        audioSource.volume = 0f;

        audioSource.pitch = 1f;
        audioSource.Stop();
    }

    private void ResetPitchImmediate()
    {
        pitchToken++;
        if (pitchRoutine != null) StopCoroutine(pitchRoutine);
        if (audioSource != null) audioSource.pitch = 1f;
    }
}
