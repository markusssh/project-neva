extends Control

var canv_size: Vector2i = Vector2i(Params.CANV_W, Params.CANV_H)

func _ready() -> void:
	%DrawingCanvas.canv_size = canv_size
	%DrawingCanvas.allign_canvas()

func gamemode_cycle() -> void:
	for i in range(Params.rounds):
		var d_theme := pull_theme()

func _on_drawing_canvas_image_changed(i: PackedByteArray) -> void:
	WebClient.image_bus.add_image_data(i)

func pull_theme() -> DrawingTheme:
	return Params.drawing_theme_repo.pick_random()
