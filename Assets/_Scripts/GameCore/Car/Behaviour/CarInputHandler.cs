using UnityEngine;

namespace GameCore.Car
{
    public class CarInputHandler : MonoBehaviour
    {
        #region Fields

        private PlayerInputActions _playerInputActions;
        private CarStatusController _carStatusController;

        
        private float _moveInput;
        private float _steerInput;
        private bool _isBraking;

        #endregion


        #region Properties

        public PlayerInputActions PlayerInputActions
        {
            get => _playerInputActions;
            set => _playerInputActions = value;
        }

        public float MoveInput
        {
            get => _moveInput;
            private set => _moveInput = value;
        }

        public float SteerInput
        {
            get => _steerInput;
            private set => _steerInput = value;
        }

        #endregion


        #region Unity Methods

        private void Awake()
        {
            _carStatusController = GetComponent<CarStatusController>();
        }


        private void Update()
        {
            GetInputs();
        }

        #endregion


        #region Private Methods

        private void GetInputs()
        {
            if (_playerInputActions == null) return;
            
            var moveInput = _playerInputActions.Player.Move.ReadValue<Vector2>();


            if (!_carStatusController.IsDead)
            {
                var carDirection = transform.forward;
                var carDirectionRight = transform.right;
            
                var cardDirectionV2 = new Vector2(carDirection.x, carDirection.z);
                var cardDirectionRightV2 = new Vector2(carDirectionRight.x, carDirectionRight.z);

                var outputX = Vector2.Dot(cardDirectionV2, moveInput);
                var outputY = Vector2.Dot(cardDirectionRightV2, moveInput);
                
                _moveInput = outputX;
                _steerInput = outputY;
            }
            else
            {
                _moveInput = Mathf.Lerp(_moveInput, 0, 0.1f);
            }
        }

        #endregion
    }
}