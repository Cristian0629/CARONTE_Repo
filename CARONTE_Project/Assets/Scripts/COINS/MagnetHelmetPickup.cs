using System.Collections;
using UnityEngine;

public class MagnetHelmetPickup : MonoBehaviour
{
    [Header("Magnet")]
    [SerializeField] private float magnetDuration = 6f;

    [Header("VFX Aura")]
    [SerializeField] private GameObject auraPrefab;
    [SerializeField] private string auraChildName = "MagnetAura";

    [Header("Aura Render Order")]
    [Tooltip("Cuánto por encima del sprite más alto del jugador se dibuja el aura.")]
    [SerializeField] private int auraOrderOffset = 10;

    [Header("SFX (Loop durante el power-up)")]
    [SerializeField] private AudioClip magnetLoopSfx;
    [Range(0f, 1f)]
    [SerializeField] private float magnetLoopVolume = 0.35f;

    [Tooltip("Empieza el loop un poco dentro del clip para saltar el silencio/entrada lenta (ej: 0.03 - 0.10).")]
    [SerializeField] private float magnetLoopStartOffset = 0.05f;

    private bool collected;

    private class AuraState : MonoBehaviour
    {
        public int token;
    }

    // Runner que vive en el Player (para que el aura se apague aunque destruyamos el pickup)
    private class AuraRunner : MonoBehaviour
    {
        public void Play(GameObject auraPrefab, string auraChildName, float duration, int auraOrderOffset)
        {
            StartCoroutine(PlayAuraRoutine(transform, auraPrefab, auraChildName, duration, auraOrderOffset));
        }

        private IEnumerator PlayAuraRoutine(Transform player, GameObject auraPrefab, string auraChildName, float duration, int auraOrderOffset)
        {
            if (player == null || auraPrefab == null) yield break;

            Transform existing = player.Find(auraChildName);
            GameObject auraGO;

            if (existing != null) auraGO = existing.gameObject;
            else
            {
                auraGO = Instantiate(auraPrefab);

                var follow = auraGO.GetComponent<AuraFollowPlayer>();
                if (follow == null)
                    follow = auraGO.AddComponent<AuraFollowPlayer>();

                Vector3 offset = new Vector3(0f, 2.2f, 0f);
                follow.Init(player, offset);
            }

            var state = auraGO.GetComponent<AuraState>();
            if (state == null) state = auraGO.AddComponent<AuraState>();
            state.token++;
            int myToken = state.token;

            auraGO.SetActive(true);

            ForceAuraAbovePlayer(player, auraGO.transform, auraOrderOffset);

            var anim = auraGO.GetComponentInChildren<Animator>(true);
            if (anim != null)
            {
                anim.Rebind();
                anim.Update(0f);
            }

            var ps = auraGO.GetComponentInChildren<ParticleSystem>(true);
            if (ps != null)
            {
                ps.Clear(true);
                ps.Play(true);
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            if (auraGO != null)
            {
                var st = auraGO.GetComponent<AuraState>();
                if (st != null && st.token == myToken)
                    auraGO.SetActive(false);
            }
        }

        private static void ForceAuraAbovePlayer(Transform player, Transform auraRoot, int offset)
        {
            if (player == null || auraRoot == null) return;

            var playerRenderers = player.GetComponentsInChildren<SpriteRenderer>(true);
            int maxOrder = int.MinValue;
            int layerId = 0;
            bool found = false;

            for (int i = 0; i < playerRenderers.Length; i++)
            {
                var r = playerRenderers[i];
                if (r == null) continue;

                if (!found)
                {
                    found = true;
                    layerId = r.sortingLayerID;
                    maxOrder = r.sortingOrder;
                }
                else
                {
                    if (r.sortingOrder > maxOrder) maxOrder = r.sortingOrder;
                }
            }

            if (!found) return;

            var auraSprites = auraRoot.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < auraSprites.Length; i++)
            {
                auraSprites[i].sortingLayerID = layerId;
                auraSprites[i].sortingOrder = maxOrder + offset;
            }

            var auraParticles = auraRoot.GetComponentsInChildren<ParticleSystemRenderer>(true);
            for (int i = 0; i < auraParticles.Length; i++)
            {
                auraParticles[i].sortingLayerID = layerId;
                auraParticles[i].sortingOrder = maxOrder + offset;
            }
        }
    }

    // Runner de audio en el Player (loop mientras dura el imán)
    private class MagnetAudioRunner : MonoBehaviour
    {
        private AudioSource src;
        private Coroutine routine;
        private int token;

        public void PlayLoop(AudioClip clip, float volume, float duration, float startOffsetSeconds)
        {
            if (clip == null) return;

            if (src == null)
            {
                src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = true;
                src.spatialBlend = 0f; // 2D
            }

            token++;
            if (routine != null) StopCoroutine(routine);

            src.clip = clip;
            src.volume = Mathf.Clamp01(volume);
            src.loop = true;

            // ✅ NUEVO: empezar “dentro” del clip para que suene antes
            float offset = Mathf.Max(0f, startOffsetSeconds);
            if (clip.length > 0.02f && offset > 0f)
            {
                // deja un pequeño margen para que no caiga justo al final
                float maxSafe = Mathf.Max(0f, clip.length - 0.02f);
                src.time = Mathf.Min(offset, maxSafe);
            }
            else
            {
                src.time = 0f;
            }

            src.Play();

            routine = StartCoroutine(StopAfter(token, duration));
        }

        private IEnumerator StopAfter(int myToken, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                if (myToken != token) yield break;
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            if (myToken != token) yield break;

            if (src != null)
            {
                src.Stop();
                src.clip = null;
            }

            routine = null;
        }

        public void StopNow()
        {
            token++;
            if (routine != null) StopCoroutine(routine);
            routine = null;

            if (src != null)
            {
                src.Stop();
                src.clip = null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        // 1) Activar el imán
        var wave = other.GetComponent<PlayerWaveRide>();
        if (wave != null)
            wave.ActivateMagnet(magnetDuration);

        other.SendMessage("ActivateMagnet", magnetDuration, SendMessageOptions.DontRequireReceiver);

        // 2) Aura visual
        var runner = other.GetComponent<AuraRunner>();
        if (runner == null) runner = other.gameObject.AddComponent<AuraRunner>();
        runner.Play(auraPrefab, auraChildName, magnetDuration, auraOrderOffset);

        // 3) Audio loop mientras dura el imán (arranca antes con offset)
        if (magnetLoopSfx != null)
        {
            var audioRunner = other.GetComponent<MagnetAudioRunner>();
            if (audioRunner == null) audioRunner = other.gameObject.AddComponent<MagnetAudioRunner>();
            audioRunner.PlayLoop(magnetLoopSfx, magnetLoopVolume, magnetDuration, magnetLoopStartOffset);
        }

        // desaparecer al instante
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Destroy(gameObject);
    }
}
