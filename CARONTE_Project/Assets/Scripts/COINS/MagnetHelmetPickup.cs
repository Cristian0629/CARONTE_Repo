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
                auraGO = Instantiate(auraPrefab); // ya NO como hijo

                var follow = auraGO.GetComponent<AuraFollowPlayer>();
                if (follow == null)
                    follow = auraGO.AddComponent<AuraFollowPlayer>();

                // Ajusta aquí la altura del aura
                Vector3 offset = new Vector3(0f, 2.2f, 0f);

                follow.Init(player, offset);

            }

            var state = auraGO.GetComponent<AuraState>();
            if (state == null) state = auraGO.AddComponent<AuraState>();
            state.token++;
            int myToken = state.token;

            auraGO.SetActive(true);

            // ✅ FIX: asegurar que el aura se dibuja POR ENCIMA del rig (PSB tiene muchas capas)
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

            // buscamos el sprite más "alto" del player (rig)
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

            // aplicamos ese layer/order a TODOS los renderers del aura (sprite y/o partículas)
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        // ✅ 1) ACTIVAR EL IMÁN (tu PlayerWaveRide lo tiene)
        var wave = other.GetComponent<PlayerWaveRide>();
        if (wave != null)
            wave.ActivateMagnet(magnetDuration);

        // ✅ 2) Fallback por si el método está en otro script / otro nombre
        other.SendMessage("ActivateMagnet", magnetDuration, SendMessageOptions.DontRequireReceiver);

        // ✅ Aura visual
        var runner = other.GetComponent<AuraRunner>();
        if (runner == null) runner = other.gameObject.AddComponent<AuraRunner>();
        runner.Play(auraPrefab, auraChildName, magnetDuration, auraOrderOffset);

        // desaparecer al instante
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // ✅ IMPORTANTÍSIMO: destruir YA para que el spawner no se bloquee
        Destroy(gameObject);
    }
}
