using UnityEngine;

public class boat : MonoBehaviour
{
    public Sprite[] sprites; // Array de sprites para a animação
    public float frameDuration = 0.1f; // Duração de cada quadro
    public float floatAmplitude = 0.5f; // Amplitude do movimento horizontal (quanto o barco se move para os lados)
    public float floatSpeed = 1f; // Velocidade do movimento horizontal (oscilações por segundo)
    
    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;

    private float initialX; // Posição inicial no eixo X

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialX = transform.position.x; // Armazena a posição inicial no eixo X
    }

    void Update()
    {
        // Atualiza a animação frame a frame
        timer += Time.deltaTime;

        if (timer >= frameDuration)
        {
            // Avança para o próximo sprite
            currentFrame = (currentFrame + 1) % sprites.Length;
            spriteRenderer.sprite = sprites[currentFrame];
            timer = 0f; // Reseta o timer
        }

        // Adiciona o movimento horizontal suave
        float offsetX = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(initialX + offsetX, transform.position.y, transform.position.z);
    }
}
