
using UnityEngine;

namespace Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance {get; private set;}

        [Header("UI AudioSource")]
        [SerializeField] private AudioSource _uiSource;
        [Header("Message AudiooSource")]
        [SerializeField] private AudioSource _messageSource;

        [Header("UI Sounds")]
        [SerializeField] private AudioClip _buttonSound;

        [SerializeField] private AudioClip _surfaceSound;
        [SerializeField] private AudioClip _removeSound;

        [SerializeField] private AudioClip _menuSound;

        [Header("Message Sounds")]
        [SerializeField] private AudioClip _correctSound;
        [SerializeField] private AudioClip _finishSound;
        [SerializeField] private AudioClip _addLayerSound;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            
            if (_uiSource == null)
                _uiSource = gameObject.AddComponent<AudioSource>();
            if (_messageSource == null)
                _messageSource = gameObject.AddComponent<AudioSource>();
        }

        
        public void PlayButtonClick() => _uiSource.PlayOneShot(_buttonSound);

        public void PlaySurfaceSelect() => _uiSource.PlayOneShot(_surfaceSound);
        public void PlayRemoveSelect() => _uiSource.PlayOneShot(_removeSound);

        public void PlayMenuClick() => _uiSource.PlayOneShot(_menuSound);



        public void PlayCorrect() => _messageSource.PlayOneShot(_correctSound);

        public void PlayFinish() => _messageSource.PlayOneShot(_finishSound);
        public void PlayAddLayer() => _messageSource.PlayOneShot(_addLayerSound);

        public static void Click()          => Instance?.PlayButtonClick();
        public static void SelectSurface()  => Instance?.PlaySurfaceSelect();
        public static void RemoveSurface()  => Instance?.PlayRemoveSelect();
        public static void MenuClick()      => Instance?.PlayMenuClick();

        public static void Correct()        => Instance?.PlayCorrect();
        public static void Finish()         => Instance?.PlayFinish();
        public static void AddLayer()       => Instance?.PlayAddLayer();
    }
}