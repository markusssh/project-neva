extends Control

@export var curr_drawing: TextureRect
@export var stars_container: StarsContainer
@export var scene_end_progress: ProgressBar
@export var scene_end_timer: Timer

var scene_time = MultiplayerController.Client_RateTime
var curr_player: int

func _ready() -> void:
	MultiplayerController.ImageToRateReceived.connect(_on_image_to_rate_received)
	stars_container.rated.connect(_on_rated)
	MultiplayerController.Client_NotifyNewSceneReady()
	scene_end_timer.start(scene_time)

func _on_image_to_rate_received(player: int, image: Image):
	stars_container.clear_score()
	curr_player = player
	curr_drawing.texture = ImageTexture.create_from_image(image)

func _on_rated(score: int):
	MultiplayerController.Client_SendScore(curr_player, score)

func _process(_delta: float) -> void:
	scene_end_progress.value = scene_end_timer.time_left / scene_time * 100
