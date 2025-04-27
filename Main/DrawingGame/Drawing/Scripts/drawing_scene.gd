extends Control

@export var drawing_manager: DrawingManager
@export var topic: String:
	set(val):
		topic = val
		%RoundTopicLabel.text = val
@export var scene_end_timer: Timer
@export var scene_end_progress: ProgressBar

var scene_time = MultiplayerController.Client_DrawTime
var canv_size: Vector2i = Vector2i(
	ImageHelper.GetCanvasWidth(), 
	ImageHelper.GetCanvasHeight()
)

func _ready() -> void:
	topic = MultiplayerController.Client_Topic
	MultiplayerController.DrawingGameStarted.connect(_on_server_ready)
	MultiplayerController.FinalImageRequested.connect(_on_drawing_image_requested)
	MultiplayerController.Client_NotifyNewSceneReady();

func _on_server_ready() -> void:
	scene_end_timer.start(scene_time)

func _on_drawing_image_requested() -> void:
	MultiplayerController.Client_SendFinalImage(
		drawing_manager.drawing_image.texture.get_image().get_data()
	)

func _process(_delta: float) -> void:
	scene_end_progress.value = scene_end_timer.time_left / scene_time * 100

func _on_player_ready_button_toggled(toggled_on: bool) -> void:
	var drawing_on: bool = not toggled_on
	drawing_manager.drawing_on = drawing_on
	MultiplayerController.Client_NotifyDrawingStateChanged(drawing_on)
