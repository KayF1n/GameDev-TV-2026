using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDialogSystem : MonoBehaviour {
    [SerializeField] private float dialogSearchingDistance = 5f;
    [SerializeField] private LayerMask npcMask;

    public DialogueNPC[] FindDialogueNPC() {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 direction = transform.forward;

        Ray ray = new Ray(origin, direction);

        RaycastHit[] hits = Physics.SphereCastAll(
            ray,
            1f,
            dialogSearchingDistance,
            npcMask,
            QueryTriggerInteraction.Collide
        );

        if (hits.Length == 0)
            return Array.Empty<DialogueNPC>();

        // Використовуємо HashSet щоб уникнути дублікатів
        System.Collections.Generic.HashSet<DialogueNPC> uniqueNPCs = new();

        foreach (var hit in hits) {
            // Шукаємо компонент на будь-якому батьківському об'єкті
            DialogueNPC npc = hit.collider.GetComponentInParent<DialogueNPC>();
            if (npc != null) {
                uniqueNPCs.Add(npc);
            }
        }

        // Конвертуємо в масив
        DialogueNPC[] npcs = new DialogueNPC[uniqueNPCs.Count];
        uniqueNPCs.CopyTo(npcs);

        npcs = npcs.Where(npc => npc.CanSpeak).ToArray();

        // Сортуємо по дистанції
        Array.Sort(npcs, (a, b) => {
            float da = Vector3.Distance(transform.position, a.transform.position);
            float db = Vector3.Distance(transform.position, b.transform.position);
            return da.CompareTo(db);
        });

        return npcs;
    }

    private void OnDrawGizmos() {
        Color grey = Color.grey;
        grey.a = 0.1f;
        Gizmos.color = grey;
        Gizmos.DrawSphere(transform.position, dialogSearchingDistance);
    }
}
