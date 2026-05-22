using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : IUpdateable
{
    private readonly Slider _healthSlider;
    private readonly Slider _manaSlider;
    private readonly TMP_Text _scoreText;
    private readonly Image _bulletCoolddownOverlay;
    private readonly Image _fireBallCoolddownOverlay;
    private readonly PlayerController _player;
    private readonly FireballAbility _fireball;
    private readonly ShootBulletAbility _bullet;

    public PlayerHUD(Slider healthSlider, Slider manaSlider, TMP_Text scoreText, Image bulletCooldownOverlay, Image fireBallCooldownOverlay, PlayerController player, FireballAbility fireball, ShootBulletAbility bullet)
    {
        _healthSlider = healthSlider;
        _manaSlider = manaSlider;
        _scoreText = scoreText;
        _bulletCoolddownOverlay = bulletCooldownOverlay;
        _fireBallCoolddownOverlay = fireBallCooldownOverlay;
        _player = player;
        _fireball = fireball;
        _bullet = bullet;

        _healthSlider.maxValue = _player.MaxHealth;
        _manaSlider.maxValue = _player.MaxMana;

        // Subscribe to GameHandler's update loop
        _player.HealthChanged += OnHealthChanged;
        _player.ManaChanged += OnManaChanged;
        GameHandler.instance.Subscribe(this);
    }

    public void Start()
    {
    }

    private void OnManaChanged((int previous, int next, int delta) e)
    {
        _manaSlider.value = e.next;
    }

    private void OnHealthChanged((int previous, int next, int delta) e)
    {
        _healthSlider.value = e.next;
    }

    public void Update()
    {
        _fireBallCoolddownOverlay.fillAmount = 1f - _fireball?.CooldownProgress ?? 0f;
        _scoreText.text = $"Score: {GameHandler.instance.Score}";

        if (_player.activeDecorator == null)
            _bulletCoolddownOverlay.fillAmount = 0f;
        else
        {
            _bulletCoolddownOverlay.fillAmount = 1f - _bullet?.CooldownProgress ?? 0f;
            _bulletCoolddownOverlay.color = _player.NextBulletColor;
        }       
    }

    public void Destroy()
    {
        _player.ManaChanged -= OnManaChanged;
        GameHandler.instance.UnSubscribe(this);
    }
}
