using UnityEngine;

public class noise : MonoBehaviour
{
    public static float Perlin3D(float x, float y,float z){
        float AB = Mathf.PerlinNoise(x,y);
        float BC = Mathf.PerlinNoise(y,z);
        float AC = Mathf.PerlinNoise(x,z);

        float BA = Mathf.PerlinNoise(y,x);
        float CB = Mathf.PerlinNoise(z,y);
        float CA = Mathf.PerlinNoise(z,x);

        float ABC = AB+BC+AC+BA+CB+CA;
        return ABC/6f;
    }
}
