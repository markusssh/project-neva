extends Control

@export var canv_size: Vector2i = Vector2i(826, 648)

func _ready() -> void:
	%DrawingCanvas.canv_size = canv_size
	%DrawingCanvas.allign_canvas()
