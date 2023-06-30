using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

namespace AirpassUnity.VRSports
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(AudioSource))]
    public class VRSportsButton : MonoBehaviour, IVRSportsInteractable
    {
        public static VRSportsButton interacting;
        public bool interactable = true;
        public ButtonType type;
        public Image img_holding;
        public AudioClip sfxHolding;
        public AudioClip sfxInteracted;
        public FillMethod fillMethod = FillMethod.Vertical;
        public bool fillClockWise = false;
        public bool holdFlip = false;
        public Color interactedColor = new Color(52.0f / 255.0f, 52.0f / 255.0f, 52.0f / 255.0f, 1);
        [Range(0.0f, 1.0f)] public float holdedMaxOffset = 1.0f;

        public UnityEvent OnHoldBegin;
        public UnityEvent OnHolding;
        public UnityEvent OnHolded;
        public UnityEvent OnHoldLost;

        public UnityEvent OnEntered;

        public UnityEvent<int> OnInteractEnter;
        public UnityEvent<int> OnInteractDown;
        public UnityEvent<int> OnInteractHolding;
        public UnityEvent<int> OnInteractUp;
        public UnityEvent<int> OnInteractExit;
        public UnityEvent<int> OnInteractClick;

        protected Sprite defaultSprite = null;
        protected Color normalColor;
        protected Button _button;
        protected Graphic _image;
        protected AudioSource _audioSource;
        protected bool holdingOnce = false;
        protected Dictionary<int, int> interactDataCount = new Dictionary<int, int>();
        protected int lostFrame = 0;

        public bool HasAnyInteractData
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<int, int> t in interactDataCount)
                {
                    count += t.Value;
                }
                return count > 0;
            }
        }

        // public Dictionary<int, InteractData> InteractingData { get => interactingData; }

        virtual public void Interact(bool isExit = false)
        {
            if (type == ButtonType.enter)
            {
                if (isExit)
                {
                    _image.color = normalColor;
                }
                else
                {
                    OnEntered.Invoke();
                    _button.onClick.Invoke();
                    _image.color = interactedColor;
                    PlayAudio(false);
                    EventSystem.current.SetSelectedGameObject(gameObject);
                }
            }
        }

        public void InteractEnter(InteractData data)
        {
            AddInterctCount(data.lidar);
            if (interactable)
            {
                OnInteractEnter.Invoke(data.id);
                Interact();
            }
        }

        public void InteractDown(InteractData data)
        {
            AddInterctCount(data.lidar);
            if (interactable)
            {
                OnInteractDown.Invoke(data.id);
                Interact();
            }
        }

        public void InteractHolding(InteractData data)
        {
            //interactDataCount[data.lidar]++;
            AddInterctCount(data.lidar);
            if (interactable)
            {
                OnInteractHolding.Invoke(data.id);
            }
        }

        public void InteractExit(InteractData data)
        {
            ReduceInterctCount(data.lidar);
            if (interactable)
            {
                Interact(true);
                OnInteractExit.Invoke(data.id);
            }
        }

        public void InteractUp(InteractData data)
        {
            ReduceInterctCount(data.lidar);
            if (interactable)
            {
                Interact(true);
                OnInteractUp.Invoke(data.id);
            }
        }

        public void InteractClick(InteractData data)
        {
            ReduceInterctCount(data.lidar);
            if (interactable)
            {
                Interact(true);
                OnInteractClick.Invoke(data.id);
            }
        }

        private void AddInterctCount(int lidar)
        {
            if (!interactDataCount.ContainsKey(lidar))
            {
                interactDataCount.Add(lidar, 0);
            }
            interactDataCount[lidar]++;
        }

        private void ReduceInterctCount(int lidar)
        {
            if (interactDataCount.ContainsKey(lidar))
            {
                interactDataCount[lidar] = Mathf.Clamp(interactDataCount[lidar] - 1, 0, 100);
            }
        }

        protected void PlayAudio(bool holding)
        {
            _audioSource.Stop();
            if (holding)
            {
                if (sfxHolding != null)
                {
                    _audioSource.clip = sfxHolding;
                    _audioSource.pitch = sfxHolding.length / VRSportsInputSystem.Instance.holdDelayOfVRSportsButton;
                }
            }
            else
            {
                if (sfxInteracted != null)
                {
                    _audioSource.pitch = 1;
                    _audioSource.clip = sfxInteracted;
                }
            }
            _audioSource.Play();
        }

        void Charging()
        {
            if (!holdingOnce)
            {
                if (img_holding.fillAmount == 0)
                {
                    if (holdFlip)
                    {
                        img_holding.fillAmount = holdedMaxOffset;
                    }
                    _image.color = interactedColor;
                    PlayAudio(true);
                    EventSystem.current.SetSelectedGameObject(gameObject);
                    interacting = this;
                    OnHoldBegin.Invoke();
                }
                OnHolding.Invoke();
                if(Time.timeScale == 0)
                    img_holding.fillAmount += ((holdFlip ? -1 : 1) * (Time.unscaledDeltaTime / VRSportsInputSystem.Instance.holdDelayOfVRSportsButton));
                else
                    img_holding.fillAmount += ((holdFlip ? -1 : 1) * (Time.deltaTime / VRSportsInputSystem.Instance.holdDelayOfVRSportsButton));

                if ((holdFlip && img_holding.fillAmount <= 0) ||
                    (!holdFlip && img_holding.fillAmount >= holdedMaxOffset))
                {
                    PlayAudio(false);
                    img_holding.fillAmount = 0;
                    _image.color = normalColor;
                    _button.onClick.Invoke();
                    holdingOnce = true;
                    interacting = null;
                    OnHolded.Invoke();
                }
            }
        }


        void Awake()
        {
            if (defaultSprite == null)
            {
                Texture2D texture = Texture2D.whiteTexture;
                defaultSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            _button = GetComponent<Button>();
            _image = _button.targetGraphic.gameObject.GetComponent<Graphic>();
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            normalColor = _image.color;
            if (type == ButtonType.hold)
            {
                if (img_holding == null)
                {
                    img_holding = new GameObject("HoldingImageMask").AddComponent<Image>();
                    Image holdingImage = new GameObject("HoldingImage").AddComponent<Image>();
                    holdingImage.tag = img_holding.tag = _image.tag;
                    holdingImage.rectTransform.localScale = img_holding.rectTransform.localScale = _image.rectTransform.localScale;
                    holdingImage.rectTransform.rotation = img_holding.rectTransform.rotation = _image.rectTransform.rotation;
                    holdingImage.rectTransform.position = img_holding.rectTransform.position = _image.rectTransform.position;
                    img_holding.transform.SetParent(transform);
                    holdingImage.transform.SetParent(img_holding.transform);
                    holdingImage.rectTransform.anchorMin = img_holding.rectTransform.anchorMin = new Vector2(0, 0);
                    holdingImage.rectTransform.anchorMax = img_holding.rectTransform.anchorMax = new Vector2(1, 1);
                    holdingImage.rectTransform.pivot = img_holding.rectTransform.pivot = _image.rectTransform.pivot;
                    holdingImage.rectTransform.anchoredPosition = img_holding.rectTransform.anchoredPosition = new Vector2(0, 0);
                    holdingImage.rectTransform.sizeDelta = img_holding.rectTransform.sizeDelta = new Vector2(0, 0);
                    img_holding.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                    if (_image.TryGetComponent(out Image img))
                    { 
                        holdingImage.preserveAspect = img.preserveAspect;
                        holdingImage.type = img.type;
                        holdingImage.sprite = img.sprite;
                    }

                    holdingImage.color = interactedColor;
                    holdingImage.raycastTarget = img_holding.raycastTarget = false;
                    img_holding.sprite = defaultSprite;
                    img_holding.type = Image.Type.Filled;
                }
                img_holding.fillMethod = fillMethod;
                img_holding.fillClockwise = fillClockWise;
                img_holding.raycastTarget = false;
                img_holding.fillAmount = 0;
            }
            VRSportsInputSystem.StartReceiveDataEvent += (t) =>
            {
                for (int i = 0; i < t.Count; ++i)
                {
                    interactDataCount[t[0]] = 0;
                }
            };
        }



        // Update is called once per frame
        virtual protected void Update()
        {
            int count = 0;
            foreach (KeyValuePair<int, int> t in interactDataCount)
            {
                count += t.Value;
            }
            if (VRSportsInputSystem.Instance.singleButtonInteract)
            {
                if (interacting != null && interacting != this)
                    return;
            }
            if (interactable && type == ButtonType.hold)
            {
                if (HasAnyInteractData)
                {
                    lostFrame = 0;
                    Charging();
                }
                else
                {
                    if (img_holding.fillAmount > 0)
                    {
                        lostFrame++;
                        if (lostFrame >= VRSportsInputSystem.Instance.receiveFrameDelay)
                        {
                            _image.color = normalColor;
                            img_holding.fillAmount = 0;
                            interacting = null;
                            _audioSource.Stop();
                            OnHoldLost.Invoke();
                        }
                        else
                        {
                            Charging();
                        }
                    }

                    if (holdingOnce)
                    {
                        holdingOnce = false;
                    }
                }
            }
        }
    }
}
