using UnityEngine;
using NavMeshPlus.Components;
using UnityEngine.AI;

public class S_VilajoisUtilityAI2D : MonoBehaviour
{
    [Header("Stats du villageois")]
    [Range(0f, 100f)]
    public float Faim = 50f;
    [Range(0f, 100f)]
    public float Fatigue = 20f;

    [Header("Courbes de priorité")]
    public AnimationCurve CourbeManger = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve CourbeDormir = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Cibles")]
    public Transform Nourriture;
    public Transform Lit;

    [Header("NavMeshPlus 2D")]
    public NavMeshAgent agent;

    private enum EtatVillageois { Idle, SeDeplacer, Manger, Dormir }
    private EtatVillageois etatActuel = EtatVillageois.Idle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        DecideAction();
        CheckArrival();
    }

    private float GetScoreManger()
    {
        float scoreNorm = Mathf.Clamp01(Faim / 100f); // Normalisé 0-1
        return CourbeManger.Evaluate(scoreNorm);
    }

    private float GetScoreDormir()
    {
        float scoreNorm = Mathf.Clamp01(Fatigue / 100f); // Normalisé 0-1
        return CourbeDormir.Evaluate(scoreNorm);
    }

    private void DecideAction()
    {
        if (etatActuel == EtatVillageois.SeDeplacer)
            return;

        float scoreManger = GetScoreManger();
        float scoreDormir = GetScoreDormir();

        if (scoreManger > scoreDormir)
            AllerManger();
        else
            AllerDormir();
    }

    private void AllerManger()
    {
        if (Nourriture == null || !agent.isOnNavMesh) return;
        agent.SetDestination(Nourriture.position);
        etatActuel = EtatVillageois.SeDeplacer;
        Debug.Log("🚶‍♂️ Le villageois va vers la nourriture (2D).");
    }

    private void AllerDormir()
    {
        if (Lit == null || !agent.isOnNavMesh) return;
        agent.SetDestination(Lit.position);
        etatActuel = EtatVillageois.SeDeplacer;
        Debug.Log("🚶‍♂️ Le villageois va vers le lit (2D).");
    }

    private void CheckArrival()
    {
        if (etatActuel != EtatVillageois.SeDeplacer) return;
        if (!agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (GetScoreManger() > GetScoreDormir())
                ActionManger();
            else
                ActionDormir();
        }
    }

    private void ActionManger()
    {
        etatActuel = EtatVillageois.Manger;
        Debug.Log("🍗 Le villageois mange (2D).");
        Faim = Mathf.Max(Faim - 50f, 0f);
        etatActuel = EtatVillageois.Idle;
    }

    private void ActionDormir()
    {
        etatActuel = EtatVillageois.Dormir;
        Debug.Log("😮‍💨 Le villageois dort (2D).");
        Fatigue = Mathf.Max(Fatigue - 50f, 0f);
        etatActuel = EtatVillageois.Idle;
    }
}
