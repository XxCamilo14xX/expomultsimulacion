using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public static VideoController instance;
    
    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    
    [Header("Configuración")]
    public bool persistBetweenScenes = false; // Marcar si quieres que persista entre escenas

    void Awake()
    {
        // Singleton para acceder desde otros scripts
        if (instance == null)
        {
            instance = this;
            
            // Hacer que persista entre escenas si está marcado
            if (persistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Obtener el componente VideoPlayer
        videoPlayer = GetComponent<VideoPlayer>();
        
        if (videoPlayer == null)
        {
            Debug.LogError("No se encontró VideoPlayer en este GameObject!");
            return;
        }
        
        // Obtener el AudioSource del Video Player (está asignado en Track 0)
        if (videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
        {
            audioSource = videoPlayer.GetTargetAudioSource(0);
            
            if (audioSource != null)
            {
                Debug.Log("AudioSource encontrado: " + audioSource.gameObject.name);
            }
            else
            {
                Debug.LogWarning("No se encontró AudioSource asignado al Video Player!");
            }
        }
    }

    // Pausar el video
    public void PauseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("Video pausado");
        }
        
        // Mutear el audio en lugar de pausarlo
        if (audioSource != null)
        {
            audioSource.mute = true;
            Debug.Log("Audio muteado");
        }
    }

    // Reproducir el video
    public void PlayVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            Debug.Log("Video reproduciendo");
        }
        
        // Desmutear el audio
        if (audioSource != null)
        {
            audioSource.mute = false;
            Debug.Log("Audio desmuteado");
        }
    }

    // Detener el video completamente
    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            Debug.Log("Video detenido");
        }
        
        if (audioSource != null)
        {
            audioSource.Stop();
            Debug.Log("Audio del video detenido");
        }
    }

    // Reiniciar el video desde el inicio
    public void RestartVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.time = 0;
            videoPlayer.Play();
            Debug.Log("Video reiniciado");
        }
        
        if (audioSource != null)
        {
            audioSource.time = 0;
            audioSource.Play();
            Debug.Log("Audio del video reiniciado");
        }
    }
}