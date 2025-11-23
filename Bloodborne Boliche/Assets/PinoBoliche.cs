using UnityEngine;

public class PinoBoliche : MonoBehaviour
{
    private bool foiContabilizado = false;
    private Vector3 posicaoInicial;
    private Quaternion rotacaoInicial;
    private Vector3 eixoParaCima;
    private float distanciaLimiteCalculada;
    private Rigidbody rb;

    [Header("Configurações")]
    [Range(1f, 100f)] public float porcentagemMovimento = 20f;
    public float anguloParaCair = 45f;
    public int pontosPorPino = 10;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        posicaoInicial = transform.position;
        rotacaoInicial = transform.rotation; // Guarda a rotação original ("em pé")

        // --- Detecção Automática do Eixo (Sua correção) ---
        Vector3[] eixosPossiveis = { Vector3.up, Vector3.forward, Vector3.right };
        float melhorAlinhamento = -1f;
        eixoParaCima = Vector3.up;

        foreach (Vector3 eixo in eixosPossiveis)
        {
            Vector3 direcaoGlobal = transform.TransformDirection(eixo);
            float alinhamento = Mathf.Abs(Vector3.Dot(direcaoGlobal, Vector3.up));
            if (alinhamento > melhorAlinhamento)
            {
                melhorAlinhamento = alinhamento;
                eixoParaCima = eixo;
            }
        }

        // --- Cálculo Limite de Movimento ---
        Collider colisor = GetComponent<Collider>();
        if (colisor != null)
            distanciaLimiteCalculada = colisor.bounds.size.y * (porcentagemMovimento / 100f);
        else
            distanciaLimiteCalculada = 0.5f;
    }

    void Update()
    {
        if (foiContabilizado) return;

        // Verifica inclinação e distância
        Vector3 direcaoDoTopoAtual = transform.TransformDirection(eixoParaCima);
        float anguloAtual = Vector3.Angle(direcaoDoTopoAtual, Vector3.up);
        float distancia = Vector3.Distance(transform.position, posicaoInicial);

        if (anguloAtual > anguloParaCair || distancia > distanciaLimiteCalculada)
        {
            ContarPonto();
        }
    }

    void ContarPonto()
    {
        foiContabilizado = true;

        // Avisa o GameManager em vez do ScoreManager antigo
        if (GameManager.instance != null)
        {
            GameManager.instance.RegistrarPinoCaido(pontosPorPino);
        }
    }

    // --- NOVAS FUNÇÕES PARA O SISTEMA DE JOGO ---

    // Chamado na 2ª tentativa: se caiu, desaparece
    public void OcultarSeCaiu()
    {
        if (foiContabilizado)
        {
            gameObject.SetActive(false);
        }
    }

    // Chamado no Reset Geral: volta pro lugar e fica em pé
    public void ResetarCompleto()
    {
        gameObject.SetActive(true);
        foiContabilizado = false;

        // Para a física
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Dica: Movemos o Rigidbody e o Transform para garantir
            rb.position = posicaoInicial;
            rb.rotation = rotacaoInicial;
            rb.Sleep(); // Coloca o pino para "dormir" para não cair sozinho ao spawnar
        }

        transform.position = posicaoInicial;
        transform.rotation = rotacaoInicial;
    }
}