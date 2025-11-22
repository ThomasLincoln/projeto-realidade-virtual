using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public int pontuacaoAtual = 0;

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

    public void AdicionarPontos(int pontos)
    {
        pontuacaoAtual += pontos;
        Debug.Log("Pontuação Atual: " + pontuacaoAtual);
    }
}

