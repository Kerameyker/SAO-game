using UnityEngine;

/// <summary>
/// Базовий контролер персонажа для гри в стилі Muck.
/// Рух WASD відносно напрямку камери, спринт, стрибок, гравітація.
/// Повісь на GameObject "Player" (Capsule) разом із компонентом CharacterController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Рух")]
    public float walkSpeed = 200f; // тимчасово x40 від початкового значення для швидкого тестування
    public float sprintMultiplier = 1.6f;
    public float jumpHeight = 1.4f;
    public float gravity = -20f;
    public float rotationSpeed = 12f; // швидкість повороту моделі персонажа до напрямку руху

    [Header("Плавання")]
    public float swimSpeed = 3f;
    public float swimGravityScale = 0.15f; // слабка гравітація у воді - природне повільне занурення без Space

    [Header("Посилання")]
    public Transform cameraTransform; // перетягни сюди Main Camera (або камеру-ріг) з інспектора
    public Animator animator;         // якщо лишити порожнім - скрипт сам знайде Animator у дочірній моделі (unitychan)

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isInWater;

    public void SetInWater(bool value) { isInWater = value; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f; // притискає до землі, щоб isGrounded працював стабільно

        float h = Input.GetAxisRaw("Horizontal"); // A/D або стрілки
        float v = Input.GetAxisRaw("Vertical");   // W/S або стрілки

        Vector3 inputDir = new Vector3(h, 0f, v).normalized;
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        float targetAnimSpeed = 0f; // 0 = стоїть, 0.5 = йде, 1 = біжить (для Unity-chan Locomotions)

        if (inputDir.magnitude >= 0.1f && cameraTransform != null)
        {
            // напрямок руху рахуємо відносно того, куди дивиться камера,
            // щоб W завжди означало "вперед від камери", як у Muck
            float camYaw = cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, camYaw, 0f) * inputDir;

            float speed = isInWater ? swimSpeed : walkSpeed * (sprinting ? sprintMultiplier : 1f);
            controller.Move(moveDir * speed * Time.deltaTime);

            // плавно повертаємо модель персонажа обличчям у напрямку руху
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            targetAnimSpeed = sprinting ? 1f : 0.5f;
        }

        if (animator != null)
        {
            // dampTime (0.15) плавно згладжує перехід між позами, щоб не було ривка
            animator.SetFloat("Speed", targetAnimSpeed, 0.15f, Time.deltaTime);
            animator.SetFloat("Direction", 0f);
        }

        if (isInWater)
        {
            float vertical = 0f;
            if (Input.GetKey(KeyCode.Space)) vertical = 1f;
            else if (Input.GetKey(KeyCode.LeftControl)) vertical = -1f;
            velocity.y = vertical * swimSpeed + gravity * swimGravityScale * Time.deltaTime;
            if (animator != null) animator.SetBool("Jump", false);
        }
        else
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (animator != null) animator.SetBool("Jump", true);
            }
            else if (isGrounded && animator != null)
            {
                animator.SetBool("Jump", false);
            }
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(velocity * Time.deltaTime);
    }
}
