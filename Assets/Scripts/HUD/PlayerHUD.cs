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
    private readonly GameObject _itemPopupPanel;
    private readonly TMP_Text _popupItemName;
    private readonly TMP_Text _popupItemDescription;
    private readonly TMP_Text _popupStatChange;

    public PlayerHUD(Slider healthSlider, Slider manaSlider, TMP_Text scoreText, Image bulletCooldownOverlay, 
        Image fireBallCooldownOverlay, PlayerController player, FireballAbility fireball, ShootBulletAbility bullet, 
        GameObject itemPopupPanel, TMP_Text popupItemName,
        TMP_Text popupItemDescription, TMP_Text popupStatChange)
    {
        _healthSlider = healthSlider;
        _manaSlider = manaSlider;
        _scoreText = scoreText;
        _bulletCoolddownOverlay = bulletCooldownOverlay;
        _fireBallCoolddownOverlay = fireBallCooldownOverlay;
        _player = player;
        _fireball = fireball;
        _bullet = bullet;

        _itemPopupPanel = itemPopupPanel;
        _popupItemName = popupItemName;
        _popupItemDescription = popupItemDescription;
        _popupStatChange = popupStatChange;

        _itemPopupPanel.SetActive(false);

        _healthSlider.maxValue = _player.MaxHealth;
        _manaSlider.maxValue = _player.MaxMana;

        // Subscribe to GameHandler's update loop
        GameHandler.instance.Subscribe(this);
        Start();
    }

    public void Start()// Subscribe to events
    {        
        _player.HealthChanged += OnHealthChanged;
        _player.ManaChanged += OnManaChanged;
        _player.MaxHealthChanged += OnMaxHealthChanged;
        _player.MaxManaChanged += OnMaxManaChanged;
        GameHandler.instance.OnItemCollected += ShowItemPopup;
        GameHandler.instance.ScoreChanged += OnScoreChanged;
    }

    private void OnManaChanged(int newValue)
    {
        _manaSlider.value = newValue;
    }

    private void OnHealthChanged(int newValue)
    {
        _healthSlider.value = newValue;
    }

    private void OnMaxManaChanged(int newValue)
    {
        _manaSlider.maxValue = newValue;
    }

    private void OnMaxHealthChanged(int newValue)
    {
        _healthSlider.maxValue = newValue;
    }

    private void OnScoreChanged(int newValue)
    {
        _scoreText.text = $"Score: {newValue}";
    }

    private void ShowItemPopup(AbstractItem item)
    {
        _popupItemName.text = item.Name;
        _popupItemDescription.text = item.Description;
        _popupStatChange.text = item.StatChange;

        _itemPopupPanel.SetActive(true);
        GameHandler.instance.IsUIActive = true;
        Time.timeScale = 0;
    }
    public void Update()
    {
        if (_fireball.CooldownProgress > 0)
            _fireBallCoolddownOverlay.fillAmount = 1f - _fireball?.CooldownProgress ?? 0f;

        if (_player.activeDecorator == null)
        {
            _bulletCoolddownOverlay.fillAmount = 0f;
            return;
        }

        if (_bullet.CooldownProgress > 0)
            _bulletCoolddownOverlay.fillAmount = 1f - _bullet?.CooldownProgress ?? 0f;

        if (_bulletCoolddownOverlay.color != _player.NextBulletColor) // change to event to swap color after bullet is fired
            _bulletCoolddownOverlay.color = _player.NextBulletColor;
    }

    public void Destroy()
    {
        _player.ManaChanged -= OnManaChanged;
        _player.HealthChanged -= OnHealthChanged;
        GameHandler.instance.ScoreChanged -= OnScoreChanged;
        GameHandler.instance.UnSubscribe(this);
    }
}
