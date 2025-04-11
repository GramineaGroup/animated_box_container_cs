using Godot;
using System;

public partial class Vertical : PanelContainer
{
    private AnimatedPanelContainer _progressBarContainer;
    private AnimatedPanelContainer _contentLabelContainer;
    private AnimatedPanelContainer _leftButtonPanelContainer;
    private AnimatedPanelContainer _rightButtonPanelContainer;
    private AnimatedBoxContainer _animatedBoxContainer;
    private AnimatedPanelContainer _centerPanelContainer;
    private Vector2 _size;
    private bool _isExpanded = true;
    private double _duration = 0.6;
    public override void _Ready()
    {
        _progressBarContainer = GetNode<AnimatedPanelContainer>("%ProgressBarContainer");
        _contentLabelContainer = GetNode<AnimatedPanelContainer>("%ContentLabelContainer");
        _leftButtonPanelContainer = GetNode<AnimatedPanelContainer>("%LeftButtonPanelContainer");
        _rightButtonPanelContainer = GetNode<AnimatedPanelContainer>("%RightButtonPanelContainer");
        _animatedBoxContainer = GetNode<AnimatedBoxContainer>("%AnimatedBoxContainer");
        _centerPanelContainer = GetNode<AnimatedPanelContainer>("%CenterPanelContainer");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void OnExpandButtonPressed()
    {
        _leftButtonPanelContainer.CreateNewTween().AnimatedTransparentShow(_duration);
        _rightButtonPanelContainer.CreateNewTween().AnimatedTransparentShow(_duration);
        _contentLabelContainer.CreateNewTween().AnimatedShow(_duration);
        _progressBarContainer.CreateNewTween().AnimatedScale(new Vector2(1, 1), _duration);
        _centerPanelContainer.CreateNewTween().AnimatedExpand(_duration, new Vector2(303, 133));
        _animatedBoxContainer.SetEase(Tween.EaseType.InOut).SetDirection(AnimatedBoxContainerDirection.Vertical, _duration);
    }

    private void OnCollapseButtonPressed()
    {
        _leftButtonPanelContainer.CreateNewTween().AnimatedTransparentHide(_duration);
        _rightButtonPanelContainer.CreateNewTween().AnimatedTransparentHide(_duration);
        _contentLabelContainer.CreateNewTween().AnimatedHide(_duration);
        _progressBarContainer.CreateNewTween().AnimatedScale(new Vector2(0.34f, 0.34f), _duration);
        // setting a known size is recommended for propery animation
        _centerPanelContainer.CreateNewTween().AnimatedCollapse(_duration, new Vector2(224, 84));
        _animatedBoxContainer.SetEase(Tween.EaseType.InOut).SetDirection(AnimatedBoxContainerDirection.Horizontal, _duration);
    }
}
