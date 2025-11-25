using UnityEngine;

public class CameraViewToSprite : MonoBehaviour
{
    

    public Camera targetCamera;                  // The camera you want to capture
    public int width = 512;
    public int height = 512;
    public SpriteRenderer targetSpriteRenderer;  // Where to display the sprite

    public void CaptureCameraView()
    {
        // 1. Set up a temporary RenderTexture
        RenderTexture rt = new RenderTexture(width, height, 24);
        targetCamera.targetTexture = rt;
        targetCamera.Render();

        // 2. Activate the RenderTexture and read it into a Texture2D
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // 3. Clean up
        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // 4. Convert Texture2D to Sprite
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));

        // 5. Apply to your target SpriteRenderer (or UI Image)
        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = sprite;
        }
    }

    public static Sprite GetCameraViewAsSprite(Camera cam, int w, int h)
    {
        // 1. Set up a temporary RenderTexture
        RenderTexture rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.Render();

        // 2. Activate the RenderTexture and read it into a Texture2D
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        // 3. Clean up
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // 4. Convert Texture2D to Sprite
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
        return sprite;
    }
}
