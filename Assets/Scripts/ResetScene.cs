using CustomAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{
#if UNITY_EDITOR
    [SceneSelector]
#endif
    [SerializeField]
    private string scene;

    [SerializeField] private InputActionReference reference;

    private void OnEnable()
    {
        reference.action.performed += Pressed;
    }

    private void OnDisable()
    {
        reference.action.performed -= Pressed;
    }
    
    private void Pressed(InputAction.CallbackContext obj)
    {
        SceneManager.LoadScene(scene);
    }
}