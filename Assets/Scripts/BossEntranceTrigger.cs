using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEntranceTrigger : MonoBehaviour
{
    [SerializeField] Signal onEntrance;
    [SerializeField] Bosses boss;

    enum Bosses
    {
        horus,
        bennu,
        bastet,
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            switch (boss)
            {
                case Bosses.horus:
                    if (!BossStatuses.Instance.horusKilled[TitleLoadManager.SAVE_SLOT]) onEntrance.RaiseSignal();
                    break;
                case Bosses.bennu:
                    if (!BossStatuses.Instance.bennuKilled[TitleLoadManager.SAVE_SLOT]) onEntrance.RaiseSignal();
                    break;
                case Bosses.bastet:
                    if (!BossStatuses.Instance.bastKilled[TitleLoadManager.SAVE_SLOT]) onEntrance.RaiseSignal();
                    break;
            }
        }
    }
}
