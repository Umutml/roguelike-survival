using System;
using System.Collections;
using MyBox;
using UnityEngine;

namespace _Scripts.GameCore.NPC
{
    public class BasePopulationNpcController : MonoBehaviour
    {
        [SerializeField] private PopulationType populationType = PopulationType.None;

        private Animator _animator;
        private bool _isArrowMoving = false;
        private float moveSpeed = 20f;
        private float moveDistance = 10f;
        private Vector3 startPosition;
        private Vector3 targetPosition;


        private static readonly int IsSitting = Animator.StringToHash("IsSitting");
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");
        private static readonly int IsPatrolling = Animator.StringToHash("IsPatrolling");
        private static readonly int IsStanding = Animator.StringToHash("IsStanding");
        private static readonly int IsArcher = Animator.StringToHash("IsArcher");
        private static readonly int IsTalkingOnPhone = Animator.StringToHash("IsTalkingOnPhone");
        private static readonly int IsLying = Animator.StringToHash("IsLying");
        private static readonly int IsGuarding = Animator.StringToHash("IsGuarding");
        private static readonly int IsMechanic = Animator.StringToHash("IsMechanic");
        private static readonly int IsRun = Animator.StringToHash("IsRun");

        [ConditionalField(nameof(populationType), false, PopulationType.Archer)]
        public GameObject ArrowObject;

        [ConditionalField(nameof(populationType), false, PopulationType.Archer)]
        public Transform ArrowDefaultParent;

        [ConditionalField(nameof(populationType), false, PopulationType.Archer)]
        public GameObject ArrowHandParent;

        [ConditionalField(nameof(populationType), false, PopulationType.Archer)]
        public GameObject StringObject;

        [ConditionalField(nameof(populationType), false, PopulationType.Archer)]
        public Transform StringDefaultParent;

        [ConditionalField(nameof(populationType), false, PopulationType.Archer)]
        public GameObject StringHandParent;

        public enum PopulationType
        {
            None,
            Sitting,
            Standing,
            Walking,
            Patrolling,
            Archer,
            TalkingOnPhone,
            Lying,
            Guarding,
            Mechanic,
            Run
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            SetAnimations();
        }

        private void OnEnable()
        {
            SetAnimations();
        }

        private void SetAnimations()
        {
            switch (populationType)
            {
                case PopulationType.Sitting:
                    SetForSitting();
                    break;
                case PopulationType.Standing:
                    SetForStanding();
                    break;
                case PopulationType.Walking:
                    SetForWalking();
                    break;
                case PopulationType.Patrolling:
                    SetForPatrolling();
                    break;
                case PopulationType.Archer:
                    SetForArcher();
                    break;
                case PopulationType.TalkingOnPhone:
                    SetForTalkingOnPhone();
                    break;
                case PopulationType.Lying:
                    SetForLying();
                    break;
                case PopulationType.Guarding:
                    SetForGuarding();
                    break;
                case PopulationType.Mechanic:
                    SetForMechanic();
                    break;
                case PopulationType.Run:
                    SetForRun();
                    break;
            }
        }

        private void Update()
        {
            if (populationType is not PopulationType.Archer) return;

            if (_isArrowMoving)
            {
                ArrowObject.transform.position = Vector3.MoveTowards(ArrowObject.transform.position, targetPosition,
                    moveSpeed * Time.deltaTime);

                if (Vector3.Distance(ArrowObject.transform.position, targetPosition) < 0.01f)
                {
                    _isArrowMoving = false;
                }
            }
        }

        private void SetForSitting()
        {
            _animator.SetBool(IsSitting, true);
        }

        private void SetForStanding()
        {
            _animator.SetBool(IsStanding, true);
        }

        private void SetForWalking()
        {
            _animator.SetBool(IsWalking, true);
        }

        private void SetForPatrolling()
        {
            _animator.SetBool(IsPatrolling, true);
        }

        private void SetForArcher()
        {
            _animator.SetBool(IsArcher, true);

            startPosition = ArrowObject.transform.position;

            targetPosition = startPosition + transform.forward * moveDistance;
        }

        private void SetForTalkingOnPhone()
        {
            _animator.SetBool(IsTalkingOnPhone, true);
        }

        private void SetForLying()
        {
            _animator.SetBool(IsLying, true);
        }

        private void SetForGuarding()
        {
            _animator.SetBool(IsGuarding, true);
        }

        private void SetForMechanic()
        {
            _animator.SetBool(IsMechanic, true);
        }

        private void SetForRun()
        {
            _animator.SetBool(IsRun, true);
        }

        public void SetArcherStingParentToHand()
        {
            StringObject.transform.SetParent(StringHandParent.transform);
            StringObject.transform.localPosition = Vector3.zero;
            StringObject.transform.localRotation = Quaternion.identity;
        }

        public void SetArcherStingParentToDefault()
        {
            StringObject.transform.SetParent(StringDefaultParent);
            StringObject.transform.localPosition = Vector3.zero;
            StringObject.transform.localRotation = Quaternion.identity;

            SetArcherArrowParentToNull();
        }

        public void SetArcherArrowParentToHand()
        {
            ArrowObject.SetActive(true);
            ArrowObject.transform.SetParent(ArrowHandParent.transform);
            ArrowObject.transform.localPosition = Vector3.zero;
            ArrowObject.transform.localRotation = Quaternion.identity;
        }

        private void SetArcherArrowParentToDefault()
        {
            ArrowObject.transform.SetParent(ArrowDefaultParent);
            ArrowObject.transform.localPosition = Vector3.zero;
            ArrowObject.transform.localRotation = Quaternion.identity;
        }

        private void SetArcherArrowParentToNull()
        {
            ArrowObject.transform.SetParent(null);
            _isArrowMoving = true;
            StartCoroutine(SetDisableArrowObject());
        }

        IEnumerator SetDisableArrowObject()
        {
            yield return new WaitForSeconds(.2f);
            _isArrowMoving = false;
            ArrowObject.SetActive(false);
            SetArcherArrowParentToDefault();
        }
    }
}