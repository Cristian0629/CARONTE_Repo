using System.Collections;
using UnityEngine;

public class MagnetHelmetPickup : MonoBehaviour
{
    [Header("Magnet")]
    [SerializeField] private float magnetDuration = 6f;

    [Header("VFX Aura")]
    [SerializeField] private GameObject auraPrefab;
    [SerializeField] private string auraChildName = "MagnetAura";

    private bool collected;

    private class AuraState : MonoBehaviour
    {
        public int token;
    }

    // Runner que vive en el Player (para que el aura se apague aunque destruyamos el pickup)
    private class AuraRunner : MonoBehaviour
    {
        public void Play(GameObject auraPrefab, string auraChildName, float duration)
        {
            StartCoroutine(PlayAuraRoutine(transform, auraPrefab, auraChildName, duration));
        }

        private IEnumerator PlayAuraRoutine(Transform player, GameObject auraPrefab, string auraChildName, float duration)
        {
            if (player == null || auraPrefab == null) yield break;

            Transform existing = player.Find(auraChildName);
            GameObject auraGO;

            if (existing != null) auraGO = existing.gameObject;
            else
            {
                auraGO = Instantiate(auraPrefab, player);
                auraGO.name = auraChildName;
                auraGO.transform.localPosition = Vector3.zero;
                auraGO.transform.localRotation = Quaternion.identity;
                auraGO.transform.localScale = Vector3.one;
            }

            var state = auraGO.GetComponent<AuraState>();
            if (state == null) state = auraGO.AddComponent<AuraState>();
            state.token++;
            int myToken = state.token;

            auraGO.SetActive(true);

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
        runner.Play(auraPrefab, auraChildName, magnetDuration);

        // desaparecer al instante
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // ✅ IMPORTANTÍSIMO: destruir YA para que el spawner no se bloquee
        Destroy(gameObject);
    }
}
