using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : IUpdateable
{
    private readonly Slider _healthSlider;
    private readonly TMP_Text _scoreText;
    private readonly Image _bulletCoolddownOverlay;
    private readonly Image _fireBallCoolddownOverlay;
    private readonly PlayerController _player;
    private readonly FireballAbility _fireball;
    private readonly ShootBulletAbility _bullet;

    public PlayerHUD(Slider healthSlider, TMP_Text scoreText, Image bulletCooldownOverlay, Image fireBallCooldownOverlay, PlayerController player, FireballAbility fireball, ShootBulletAbility bullet)
    {
        _healthSlider = healthSlider;
        _scoreText = scoreText;
        _bulletCoolddownOverlay = bulletCooldownOverlay;
        _fireBallCoolddownOverlay = fireBallCooldownOverlay;
        _player = player;
        _fireball = fireball;
        _bullet = bullet;

        // Subscribe to GameHandler's update loop, same as PlayerController does
        GameHandler.instance.Subscribe(this);
    }

    public void Start()
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {
        _healthSlider.value = (float)_player.Health;
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
}
