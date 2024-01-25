using System;
using Godot;

[Tool]
public partial class SliderOption : Control
{
    private Label? _optionNameLabel;
    private HSlider? _slider;
    private LineEdit? _manualInput;
    private string _optionName = "Option name";
    private double _minValue = 0f;
    private double _maxValue = 100f;
    private double _currentValue = 100f;
    private bool _isPercent = false;

    [Export]
    public string OptionName
    {
        get => _optionName;
        set
        {
            _optionName = value;
            if (_optionNameLabel != null) _optionNameLabel.Text = value;
        }
    }

    [Export]
    public double MinValue
    {
        get => _minValue;
        set
        {
            _minValue = value;
            if (CurrentValue < _minValue) CurrentValue = _minValue;
            if (_slider != null) _slider.MinValue = _minValue;
        }
    }

    [Export]
    public double MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            if (CurrentValue > _maxValue) CurrentValue = _maxValue;
            if (_slider != null) _slider.MaxValue = _maxValue;
        }
    }

    [Export]
    public double CurrentValue
    {
        get => _currentValue;
        set
        {
            _currentValue = value;
            if (_currentValue > MaxValue) _currentValue = MaxValue;
            else if (_currentValue < MinValue) _currentValue = MinValue;
            
            if (_slider != null) _slider.Value = _currentValue;
            UpdateManualInputText(_currentValue);
            ValueChanged?.Invoke(_currentValue);
        }
    }

    [Export]
    public bool IsPercent
    {
        get => _isPercent;
        set
        {
            _isPercent = value;
            UpdateManualInputText(CurrentValue);
        }
    }

    public event Action<double>? ValueChanged;

    public override void _Ready()
    {
        _optionNameLabel = GetNode<Label>("HBoxContainer/OptionName");
        _optionNameLabel.Text = _optionName;

        _slider = GetNode<HSlider>("HBoxContainer/HSlider");
        _slider.ValueChanged += UpdateManualInputText;
        _slider.MinValue = MinValue;
        _slider.MaxValue = MaxValue;
        _slider.Value = CurrentValue;
        _slider.Step = IsPercent ? 0.01 : 1;

        _manualInput = GetNode<LineEdit>("HBoxContainer/ManualInput");
        _manualInput.TextSubmitted += text =>
        {
            var textSanitized = text.AsSpan().Trim();
            var endsWithPercent = textSanitized.EndsWith("%");
            
            if (!double.TryParse(endsWithPercent ? textSanitized[..^1] : textSanitized, out var newValue))
            {
                UpdateManualInputText(CurrentValue);
                return;
            }

            if (IsPercent || endsWithPercent) newValue *= 0.01;
            CurrentValue = newValue;
        };
        UpdateManualInputText(_currentValue);
    }

    private void UpdateManualInputText(double newValue)
    {
        if (_manualInput != null) _manualInput.Text = newValue.ToString(IsPercent ? "P0" : "N2");
    }
}