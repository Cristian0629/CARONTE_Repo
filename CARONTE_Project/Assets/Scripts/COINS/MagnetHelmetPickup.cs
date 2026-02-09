using System.Collections;
using UnityEngine;

public class MagnetHelmetPickup : MonoBehaviour
{
    [Header("Magnet")]
    [SerializeField] private float magnetDuration = 6f;

    [Header("VFX Aura")]
    [SerializeField] private GameObject auraPrefab;          // arrastra aquí tu "Shield Round Magic Loop..."
    [SerializeField] private string auraChildName = "MagnetAura";

    private bool collected;

    // ✅ NUEVO: para que si pillas otro casco, el aura no se apague por una coroutine vieja
    private class AuraState : MonoBehaviour
    {
        public int token;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        // ✅ Si tu imán se activa desde otro script, no lo tocamos.
        // Si tienes un componente tipo "PlayerMagnet" y quieres activarlo aquí,
        // descomenta y ajusta el nombre:
        //
        // other.GetComponent<PlayerMagnet>()?.Activate(magnetDuration);

        // ✅ Desactiva collider + sprite para que "desaparezca" al instante
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // ✅ Aura visual (IMPORTANTE: NO destruimos el pickup aún o se corta la coroutine)
        StartCoroutine(PlayAuraRoutine(other.transform, magnetDuration));

        // (Opcional) Si quieres que no moleste en escena, lo puedes mover fuera
        transform.position = new Vector3(9999f, 9999f, 0f);
    }

    private IEnumerator PlayAuraRoutine(Transform player, float duration)
    {
        if (player == null || auraPrefab == null)
        {
            Destroy(gameObject);
            yield break;
        }

        // Busca/crea el aura en el player
        Transform existing = player.Find(auraChildName);
        GameObject auraGO;

        if (existing != null)
        {
            auraGO = existing.gameObject;
        }
        else
        {
            auraGO = Instantiate(auraPrefab, player);
            auraGO.name = auraChildName;
            auraGO.transform.localPosition = Vector3.zero;
            auraGO.transform.localRotation = Quaternion.identity;
            auraGO.transform.localScale = Vector3.one;
        }

        // ✅ Estado/token para evitar que una coroutine antigua apague el aura del nuevo powerup
        var state = auraGO.GetComponent<AuraState>();
        if (state == null) state = auraGO.AddComponent<AuraState>();
        state.token++;
        int myToken = state.token;

        // Activa y reinicia VFX si hace falta
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

        // ✅ Espera en tiempo real (por si Time.timeScale cambia)
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // ✅ Solo apagamos el aura si este token sigue siendo el actual
        if (auraGO != null)
        {
            var st = auraGO.GetComponent<AuraState>();
            if (st != null && st.token == myToken)
                auraGO.SetActive(false);
        }

        // ✅ Ahora sí, destruimos el pickup sin cortar la coroutine
        Destroy(gameObject);
    }
}
