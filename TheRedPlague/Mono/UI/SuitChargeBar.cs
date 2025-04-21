using TheRedPlague.Mono.InfectionLogic;
using TheRedPlague.Mono.Insanity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheRedPlague.Mono.UI;

public class SuitChargeBar : MonoBehaviour
{
    // Based on UWE's beautiful copy-paste system
    private const float punchDamp = 100f;

    private const float puchFrequency = 5f;

    public static readonly Color NormalBarColor = new(0.86f, 1f, 0f);
    public static readonly Color NormalBorderColor = new(0.8f, 1f, 0.5f);
    private static readonly Color InsaneBarColor = new(0.7f, 0f, 0.8f);
    private static readonly Color InsaneBorderColor = new(0.8f, 0.4f, 0.8f);

    public uGUI_CircularBar bar;
    public RectTransform icon;
    public Image iconImage;
    public TextMeshProUGUI text;

    public Sprite normalSprite;
    public Sprite insaneSprite;

    public float dampSpeed = 0.1f;

    public float pulseReferenceCapacity = 100f;

    public AnimationCurve pulseDelayCurve;

    public AnimationCurve pulseTimeCurve;

    public Animation pulseAnimation;

    public float rotationSpringCoef = 240f;

    public float rotationVelocityDamp = 0.8f;

    public float rotationVelocityMax = -1f;

    public float rotationRandomVelocity = 1000f;

    private float _curr;

    private float _vel;

    private float _punchSeed;

    private float _punchMaxScale = 2f;

    private Vector3 _punchInitialScale;

    private Vector3 _punchScale = new(0f, 0f, 0f);

    private CoroutineTween _punchTween;

    private bool _subscribed;

    private CoroutineTween _pulseTween;

    private float _pulseDelay = -1f;

    private float _pulseTime = -1f;

    private AnimationState _pulseAnimationState;

    private int _cachedValue = int.MinValue;

    private float _rotationCurrent;

    private float _rotationVelocity;

    private bool _showNumbers;

    private float _lastFixedUpdateTime;

    private float _timeCanChangeInsanityDisplayAgain;
    private bool _insanityDisplayWasActive;
    
    private void Awake()
    {
        _punchTween = new CoroutineTween(this)
        {
            deltaTimeProvider = PDA.GetDeltaTime,
            mode = CoroutineTween.Mode.Once,
            onStart = OnPunchStart,
            onUpdate = OnPunchUpdate,
            onStop = OnPunchStop
        };
        _pulseTween = new CoroutineTween(this)
        {
            mode = CoroutineTween.Mode.Loop,
            duration = 0f,
            onUpdate = OnPulse
        };
        pulseAnimation.wrapMode = WrapMode.Loop;
        pulseAnimation.Stop();
        _pulseAnimationState = pulseAnimation.GetState(0);
        if (_pulseAnimationState != null)
        {
            _pulseAnimationState.blendMode = AnimationBlendMode.Blend;
            _pulseAnimationState.weight = 1f;
            _pulseAnimationState.layer = 0;
            _pulseAnimationState.speed = 0f;
        }
        text.enableCulling = true;
    }

    private void OnEnable()
    {
        _lastFixedUpdateTime = PDA.time;
        if (_pulseAnimationState != null)
        {
            _pulseAnimationState.enabled = true;
        }
        _pulseTween.Start();
    }

    private void LateUpdate()
    {
        var wasShowingNumbers = _showNumbers;
        _showNumbers = false;
        var main = Player.main;
        if (main != null)
        {
            var component = PlagueDamageStat.main;
            if (component != null)
            {
                if (!_subscribed)
                {
                    _subscribed = true;
                    component.OnChargeEvent += OnCharge;
                }
                float charge = 100f - component.InfectionPercent;
                SetValue(charge, 100f);
                float num2 = Mathf.Clamp01(charge / pulseReferenceCapacity);
                float time = 1f - num2;
                _pulseDelay = pulseDelayCurve.Evaluate(time);
                if (_pulseDelay < 0f)
                {
                    _pulseDelay = 0f;
                }
                _pulseTime = pulseTimeCurve.Evaluate(time);
                if (_pulseTime < 0f)
                {
                    _pulseTime = 0f;
                }
                float num3 = _pulseDelay + _pulseTime;
                if (_pulseTween.duration > 0f && num3 <= 0f)
                {
                    _pulseAnimationState.normalizedTime = 0f;
                }
                _pulseTween.duration = num3;
            }
            var pda = main.GetPDA();
            if (pda != null && pda.isInUse)
            {
                _showNumbers = true;
            }

            if (Time.time > _timeCanChangeInsanityDisplayAgain)
            {
                var useInsanityDisplay = UseInsanityDisplay();
                if (useInsanityDisplay != _insanityDisplayWasActive)
                {
                    bar.color = useInsanityDisplay ? InsaneBarColor : NormalBarColor;
                    bar.borderColor = useInsanityDisplay ? InsaneBorderColor : NormalBorderColor;
                    iconImage.sprite = useInsanityDisplay ? insaneSprite : normalSprite;
                    _insanityDisplayWasActive = useInsanityDisplay;
                    _timeCanChangeInsanityDisplayAgain = Time.time + 2f;
                }
            }
        }
        if (_pulseAnimationState != null && pulseAnimation.enabled)
        {
            icon.localScale += _punchScale;
        }
        else
        {
            icon.localScale = _punchScale;
        }
        if (wasShowingNumbers != _showNumbers)
        {
            _rotationVelocity += Random.Range(0f - rotationRandomVelocity, rotationRandomVelocity);
        }
        if (MathExtensions.CoinRotation(ref _rotationCurrent, _showNumbers ? 180f : 0f, ref _lastFixedUpdateTime, PDA.time, ref _rotationVelocity, rotationSpringCoef, rotationVelocityDamp, rotationVelocityMax))
        {
            icon.localRotation = Quaternion.Euler(0f, _rotationCurrent, 0f);
        }
    }
    
    private void OnCharge(float chargeAmount)
    {
        float maxScale = 1f + Mathf.Clamp01(chargeAmount / 100f);
        Punch(2.5f, maxScale);
    }
    
    private void Punch(float duration, float maxScale)
    {
        _punchTween.duration = duration;
        _punchMaxScale = maxScale;
        _punchTween.Start();
    }
    
    private void OnDisable()
    {
        _punchTween.Stop();
        _pulseTween.Stop();
        if (!_subscribed)
        {
            return;
        }
        _subscribed = false;
        Player main = Player.main;
        if (main != null)
        {
            var component = PlagueDamageStat.main;
            if (component != null)
            {
                component.OnChargeEvent -= OnCharge;
            }
        }
    }

    private void SetValue(float has, float capacity)
    {
        float target = has / capacity;
        _curr = Mathf.SmoothDamp(_curr, target, ref _vel, dampSpeed, float.PositiveInfinity, PDA.deltaTime);
        bar.value = _curr;
        int num = Mathf.CeilToInt(_curr * capacity);
        if (_cachedValue != num)
        {
            _cachedValue = num;
            text.text = IntStringCache.GetStringForInt(_cachedValue);
        }
    }


    private void OnPunchStart()
    {
        _punchInitialScale = icon.localScale;
        _punchSeed = Random.value;
    }

    private void OnPunchUpdate(float t)
    {
        float o = 0f;
        MathExtensions.Oscillation(punchDamp, puchFrequency, _punchSeed, t, out var o2, out o);
        _punchScale = new Vector3(o2 * _punchMaxScale, o * _punchMaxScale, 0f);
    }

    private void OnPunchStop()
    {
        _punchScale = new Vector3(0f, 0f, 0f);
        if (!(icon == null))
        {
            icon.localScale = _punchInitialScale;
        }
    }

    private void OnPulse(float scalar)
    {
        if (!(_pulseAnimationState == null))
        {
            _pulseAnimationState.normalizedTime = Mathf.Clamp01((_pulseTween.duration * scalar - _pulseDelay) / _pulseTime);
        }
    }

    private bool UseInsanityDisplay()
    {
        if (PlagueDamageStat.main && PlagueDamageStat.main.InfectionPercent >= 50)
        {
            return true;
        }

        if (InsanityManager.Main && InsanityManager.Main.Insanity >= 50)
        {
            return true;
        }

        return false;
    }
}