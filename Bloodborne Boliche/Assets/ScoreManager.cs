using UnityEngine;
using TMPro; // Necessário para usar TextMeshPro

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public int pontuacaoAtual = 0;

    [Header("Interface do Usuário")]
    public TextMeshProUGUI textoPontuacao; // Arraste o objeto de Texto aqui no Inspector

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AtualizarTextoUI(); // Garante que comece mostrando "Pontos: 0"
    }

    public void AdicionarPontos(int pontos)
    {
        pontuacaoAtual += pontos;
        Debug.Log("Pontuação Atual: " + pontuacaoAtual);

        AtualizarTextoUI();
    }

    void AtualizarTextoUI()
    {
        if (textoPontuacao != null)
        {
            textoPontuacao.text = "Pontos: " + pontuacaoAtual;
        }
    }
}