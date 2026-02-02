using System;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [Header("Mixer")]
    public AudioMixer mainMixer;
    private const string MIXER_MASTER = "MasterVolume";
    private const string MIXER_MUSIC = "MusicVolume";
    private const string MIXER_SFX = "SFXVolume";
    
    [Header("Sounds")]
    [SerializeField] private AudioSource buttonSound;
    [SerializeField] private AudioSource letterAddedSound;
    [SerializeField] private AudioSource letterRemovedSound;
    [SerializeField] private AudioSource levelCompletedSound;
    [SerializeField] private AudioSource gameOverSound;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InputManager.onLetterAdded += PlayLetterAddedSound;
        InputManager.onLetterRemoved += PlayLetterRemovedSound;
        
        GameManager.OnGameStateChanged += GameStateChangedCallback;
    }

    private void OnDestroy()
    {
        InputManager.onLetterAdded -= PlayLetterAddedSound;
        InputManager.onLetterRemoved -= PlayLetterRemovedSound;
        
        GameManager.OnGameStateChanged -= GameStateChangedCallback;
    }

    private void GameStateChangedCallback(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.LevelComplete:
                levelCompletedSound.Play();
                break;
            
            case GameState.GameOver:
                gameOverSound.Play();
                break;
        }
    }

    public void PlayButtonSound()
    {
        buttonSound.Play();
    }

    private void PlayLetterAddedSound()
    {
        letterAddedSound.Play();
    }
    
    private void PlayLetterRemovedSound()
    {
        letterRemovedSound.Play();
    }

    public void SetMasterVolume(float volume)
    {
        mainMixer.SetFloat(MIXER_MASTER, ConvertToDecibel(volume));
    }
    
    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat(MIXER_MUSIC, ConvertToDecibel(volume));
    }
    
    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat(MIXER_SFX, ConvertToDecibel(volume));
    }
    
    private float ConvertToDecibel(float sliderValue)
    {
        // Log10(0) es infinito negativo, así que ponemos un tope mínimo (0.0001)
        // La fórmula estándar es: log10(valor) * 20
        sliderValue = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        return Mathf.Log10(sliderValue) * 20; 
    }
}
