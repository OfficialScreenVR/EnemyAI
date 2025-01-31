using GorillaLocomotion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


namespace ScreenVR
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Global Settings Callback Manager")]
        [SerializeField] private GlobalSettingsCallbackManager callbackManager;
        [Header("Editable Floats")]
        [SerializeField] private float SightDistance;
        [SerializeField] private float WanderingSpeed;
        [SerializeField] private float ChasingSpeed;
        [SerializeField] private float AnimationSpeedConversionRate = 1f;
        [SerializeField] private Transform[] WanderPoints;

        [Header("Animation To Control")]
        [SerializeField] private Animator EnemyAnimator;
        [SerializeField] private bool changeanimation;
        [Header("Roaring Audio Controller")]
        [SerializeField] private AudioSource roaringAudioSource;
        [SerializeField] private float roaringAudioConversionRate;
        [SerializeField] private float decimalAddToAudio;
        [Header("Jumpscare Options")]
        [SerializeField] private Transform RespawnPoint;
        [SerializeField] private GameObject JumpscareObject;
        [SerializeField] private AudioSource JumpscareAudioSource;
        [SerializeField] private GameObject Torch;
        [SerializeField] private GameObject MonsterMesh;
        [SerializeField] private List<GameObject> ObjectsToDisable;
        [SerializeField] private GameObject JumpscareBox;
        [SerializeField] private float Duration;
        [SerializeField] private bool jockcheck;
        [Header("Safe Zone Options")]
        [SerializeField] private Vector3 Offset;
        [SerializeField] private Vector3 Size;
        [SerializeField] private LayerMask ChasingMask;
        private GameObject[] Players;
        private LayerMask noLayer;
        private NavMeshAgent NavigationAgent;
        private Vector3 previousPosition;
        private Transform CurrentPoint;
        private Collider collider;
        private void Start()
        {
            collider = GetComponent<Collider>();
            NavigationAgent = GetComponent<NavMeshAgent>();
            CurrentPoint = WanderPoints[Random.Range(0, WanderPoints.Length)];
            NavigationAgent.SetDestination(CurrentPoint.position);
        }
        private void Update()
        {
            Players = GameObject.FindGameObjectsWithTag("PlayerLocation");
            Transform player = GetClosestEnemy(Players);
            float distance = Vector3.Distance(transform.position, player.position);
            Vector3 DirectionToPlayer = transform.position - player.position;
            if (distance < SightDistance)
            {
                Vector3 targetPostition = transform.position - DirectionToPlayer;
                NavigationAgent.SetDestination(targetPostition);
                NavigationAgent.speed = ChasingSpeed;
            }
            else
            {
                NavigationAgent.speed = WanderingSpeed;
                NavigationAgent.SetDestination(CurrentPoint.position);
                float distanceToPoint = Vector3.Distance(transform.position, CurrentPoint.position);
                if (distanceToPoint < 1)
                {
                    print("Changing Destination");
                    CurrentPoint = WanderPoints[Random.Range(0, WanderPoints.Length)];

                }
            }
            float RoarVolume = (transform.position - previousPosition).magnitude / Time.deltaTime / roaringAudioConversionRate + decimalAddToAudio;
            float speed = (transform.position - previousPosition).magnitude / Time.deltaTime / AnimationSpeedConversionRate;
            roaringAudioSource.volume = RoarVolume;
            if (changeanimation)
            {
                EnemyAnimator.speed = speed;
            }
            else
            {
                EnemyAnimator.speed = 1;
            }

            previousPosition = transform.position;
        }
        private Transform GetClosestEnemy(GameObject[] players)
        {
            Transform ClosestPlayer = null;
            float closestDistance = Mathf.Infinity;
            Collider[] cols = Physics.OverlapBox(Offset, Size / 2, Quaternion.identity, ChasingMask);
            foreach (GameObject player in players)
            {
                if (cols.Any(x => x.transform.position == player.transform.position)) continue;
                float distanceOfPlayer = Vector3.Distance(player.transform.position, transform.position);
                if (distanceOfPlayer < closestDistance)
                {
                    ClosestPlayer = player.transform;
                    closestDistance = distanceOfPlayer;
                }
            }
            return ClosestPlayer;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!jockcheck)
            {
                if (other.CompareTag("MonsterTrigger"))
                {
                    collider.enabled = false;
                    MonsterMesh.SetActive(false);
                    JumpscareObject.SetActive(true);
                    JumpscareAudioSource.Play();
                    Torch.SetActive(false);
                    JumpscareBox.SetActive(true);
                    callbackManager.Player.locomotionEnabledLayers = noLayer;
                    callbackManager.playerRB.isKinematic = true;
                    callbackManager.playerTransform.position = RespawnPoint.position;
                    foreach (GameObject obj in ObjectsToDisable)
                    {
                        obj.SetActive(false);
                    }
                    StartCoroutine(JumpscareReset());

                }
            }

        }
        private IEnumerator JumpscareReset()
        {

            yield return new WaitForSeconds(Duration);


            foreach (GameObject obj in ObjectsToDisable)
            {
                obj.SetActive(true);
            }
            JumpscareBox.SetActive(false);
            collider.enabled = true;
            MonsterMesh.SetActive(true);
            Torch.SetActive(true);
            JumpscareObject.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            callbackManager.playerRB.isKinematic = false;
            callbackManager.Player.locomotionEnabledLayers = callbackManager.LocomotiveLayers;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, SightDistance);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Offset, Size);
            if (CurrentPoint != null)
            {
                Gizmos.DrawSphere(CurrentPoint.position, 1);
            }
        }
    }
}