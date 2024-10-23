extends Control

var canv_size: Vector2i = Vector2i(Params.CANV_W, Params.CANV_H)
var rounds_left: int = Params.rounds

func _ready() -> void:
	%DrawingCanvas.canv_size = canv_size
	%DrawingCanvas.allign_canvas()
	gamemode_cycle()

func gamemode_cycle() -> void:
	for i in Params.rounds:
		var idx := Params.set_random_theme()
		%ThemeImage.texture = Params.drawing_theme.image_texture
		GameActionController.sync_theme(idx)
		WebActionBus.add_sync_theme_action(idx)
		%GameModeTimer.start(Params.round_time_sec)
		await %GameModeTimer.timeout

func _on_drawing_canvas_image_changed(i: PackedByteArray) -> void:
	WebImageBus.add_image_data(i)
