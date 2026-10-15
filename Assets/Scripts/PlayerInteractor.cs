using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 범위 안의 상호작용 대상 중 가장 가까운 하나에 [E] 안내를 띄우고 E키를 처리한다
public class PlayerInteractor : MonoBehaviour
{
    private readonly List<Interactable> inRange = new List<Interactable>();
    private Interactable current;

    public void Register(Interactable target)
    {
        if (!inRange.Contains(target)) inRange.Add(target);
    }

    public void Unregister(Interactable target)
    {
        inRange.Remove(target);
    }

    private void Update()
    {
        bool blocked = (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) || StatusMenu.IsOpen;

        current = blocked ? null : FindNearest();
        if (InteractPrompt.Instance != null) InteractPrompt.Instance.SetTarget(current);

        if (current == null) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            current.Interact();
        }
    }

    private Interactable FindNearest()
    {
        Interactable best = null;
        float bestDist = float.MaxValue;
        Vector2 me = transform.position;

        for (int i = inRange.Count - 1; i >= 0; i--)
        {
            Interactable target = inRange[i];
            if (target == null || !target.isActiveAndEnabled)
            {
                inRange.RemoveAt(i);
                continue;
            }
            if (!target.CanInteract) continue;

            float d = ((Vector2)target.transform.position - me).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = target;
            }
        }
        return best;
    }
}
