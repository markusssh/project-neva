class_name BrushCircle
extends Control

var _circle_size: int = 0
var _circle_color := Color(1, 1, 1, 0.5)

func set_circle_size(c_size: int) -> void:
	_circle_size = c_size

func _process(_delta: float) -> void:
	position = get_global_mouse_position()
	queue_redraw()

func _draw() -> void:
	draw_circle(Vector2.ZERO, _circle_size, _circle_color)
