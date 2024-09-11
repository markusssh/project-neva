extends Control

@export var drawing_canvas_size: Vector2i = Vector2i(826, 648)

func _ready() -> void:
	%DrawingCanvas.drawing_canvas_size = drawing_canvas_size
	%DrawingCanvas.allign_canvas()
