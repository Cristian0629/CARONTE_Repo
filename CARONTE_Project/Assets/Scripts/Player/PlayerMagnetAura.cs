using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMagnetAura : MonoBehaviour
{
    [SerializeField] private GameObject auraPrefab;
    private GameObject currentAura;

    public void ShowAura(float duration)
    {
        if (currentAura == null)
        {
            currentAura = Instantiate(auraPrefab, transform);
            currentAura.transform.localPosition = Vector3.zero;

            
            var sg = currentAura.GetComponent<SortingGroup>();
            if (sg == null) sg = currentAura.AddComponent<SortingGroup>();

            var playerSG = GetComponentInParent<SortingGroup>();
            if (playerSG != null)
            {
                sg.sortingLayerID = playerSG.sortingLayerID;
                sg.sortingOrder = playerSG.sortingOrder + 5;
            }
            else
            {
                sg.sortingOrder = 500;
            }
        }

        currentAura.SetActive(true);
        CancelInvoke(nameof(HideAura));
        Invoke(nameof(HideAura), duration);
    }

    private void HideAura()
    {
        if (currentAura != null)
            currentAura.SetActive(false);
    }
}
