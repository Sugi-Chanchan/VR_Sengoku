using UnityEngine;

public class CameraFilter : MonoBehaviour
{
    public Material filter;
    bool gameOver = false;
    float parameter=0;
    float damageEffect = 0;

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (gameOver)
        {
            parameter += Time.deltaTime;
            parameter = (parameter > 1) ? 1 : parameter;
            filter.SetFloat("_Parameter", parameter);
        }

        if (damageEffect >= 0)
        {
            filter.SetFloat("_Damage", damageEffect);
            damageEffect -= Time.deltaTime / 5;
        }
        Graphics.Blit(src, dest, filter);
    }

    public void GameOver()
    {
        gameOver = true;
    }
    public void GameStart()
    {
        gameOver = false;
        filter.SetFloat("_Parameter", 0);
        parameter = 0;
    }
    public void Damage()
    {
        damageEffect = 0.7f;
    }
}