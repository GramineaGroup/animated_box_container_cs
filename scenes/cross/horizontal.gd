class_name Horizontal
extends PanelContainer

var _animated_box_container: Container
var _duration: float = 0.6

func _ready():
	_animated_box_container = %AnimatedBoxContainer

func _on_expand_button_pressed() -> void:
   	# _animated_box_container.ContainerDirection = 1
	_animated_box_container.SetEase(Tween.EASE_IN_OUT).SetDirection(1, _duration)

func _on_collapse_button_pressed() -> void:
   	# _animated_box_container.ContainerDirection = 0
	_animated_box_container.SetEase(Tween.EASE_IN_OUT).SetDirection(0, _duration)
