extends Control

@export var drawing_canvas: DrawingCanvas
@export var topic: String:
	set(val):
		topic = val
		%RoundTopicLabel.text = val
@export var round_end_timer: Timer
@export var round_end_progress: ProgressBar

var round_time = MultiplayerController.DrawingRoundTimeSec
var canv_size: Vector2i = Vector2i(
	ImageHelper.GetCanvasWidth(), 
	ImageHelper.GetCanvasHeight()
)

func _ready() -> void:
	MultiplayerController.PlayerLoadedGameScene.emit()
	topic = MultiplayerController.Topic
	drawing_canvas.canv_size = canv_size
	drawing_canvas.allign_canvas()
	MultiplayerController.GameReady.connect(_on_game_ready)
	MultiplayerController.FinalImageRequested.connect(_on_drawing_image_requested)

func _on_game_ready() -> void:
	round_end_timer.start(round_time)

func _on_drawing_image_requested() -> void:
	MultiplayerController.HandleFinalImageResponseOnClient(
		drawing_canvas.painted_image.texture.get_image()
	)

func _process(delta: float) -> void:
	round_end_progress.value = round_end_timer.time_left / round_time * 100

func _on_finished_drawing_check_button_toggled(toggled_on: bool) -> void:
	var drawing: bool = not toggled_on
	drawing_canvas.drawing = drawing
	MultiplayerController.HandleDrawingStateChangeOnClient(drawing)
