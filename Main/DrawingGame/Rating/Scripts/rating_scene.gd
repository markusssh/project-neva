extends Control

@export var curr_drawing: TextureRect
@export var stars_container: StarsContainer

var curr_player: int

func _ready() -> void:
	MultiplayerController.ImageToRateReceived.connect(_on_image_to_rate_received)
	stars_container.rated.connect(_on_rated)
	MultiplayerController.Client_NotifyNewSceneReady()

func _on_image_to_rate_received(player: int, image: Image):
	stars_container.clear_score()
	curr_player = player
	curr_drawing.texture = ImageTexture.create_from_image(image)

func _on_rated(score: int):
	MultiplayerController.Client_SendScore(curr_player, score)
