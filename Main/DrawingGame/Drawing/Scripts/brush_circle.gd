class_name BrushCircle
extends Control

var _circle_size: int = 0
var _circle_color := Color(0, 0, 0, 0.5)

func set_circle_size(c_size: int) -> void:
	_circle_size = c_size

func _process(_delta: float) -> void:
	position = get_viewport().get_mouse_position()
	queue_redraw()

func _draw() -> void:
	@warning_ignore("integer_division")
	draw_circle(Vector2.ZERO, _circle_size / 2, _circle_color, false, 1, true)
