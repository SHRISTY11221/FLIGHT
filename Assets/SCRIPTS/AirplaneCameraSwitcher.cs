using UnityEngine;

namespace HeneGames.Airplane
{
    public class AirplaneCameraSwitcher : MonoBehaviour
    {
        [Header("Cameras")]
        [SerializeField] private Camera thirdPersonCam;
        [SerializeField] private Camera noseCam;

        [Header("Settings")]
        [SerializeField] private KeyCode switchKey = KeyCode.C;

        private bool noseView;

        private void Start()
        {
            SetThirdPerson();
        }

        private void Update()
        {
            if (Input.GetKeyDown(switchKey))
            {
                if (noseView)
                    SetThirdPerson();
                else
                    SetNoseView();
            }
        }

        private void SetNoseView()
        {
            noseView = true;
            noseCam.enabled = true;
            thirdPersonCam.enabled = false;
        }

        private void SetThirdPerson()
        {
            noseView = false;
            noseCam.enabled = false;
            thirdPersonCam.enabled = true;
        }
    }
}