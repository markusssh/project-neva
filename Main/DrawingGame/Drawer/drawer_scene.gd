extends Control

@export var drawing_canvas: DrawingCanvas

var canv_size: Vector2i = Vector2i(Params.CANV_W, Params.CANV_H)

func _ready() -> void:
	MultiplayerController.PlayerLoadedGameScene.emit()
	
	drawing_canvas.canv_size = canv_size
	drawing_canvas.allign_canvas()
	drawing_canvas.image_changed.connect(MultiplayerController.SendImageChangeByDrawer)
	MultiplayerController.TopicIdReceived.connect(_on_topic_id_received)
	await MultiplayerController.TopicIdReceived

func _on_topic_id_received(id: int) -> void:
	var data = Database.get_painting_by_id(id)
	var image: Image = Image.new()
	image.load_jpg_from_buffer(data.get("hint_image"))
	%HintImage.image = ImageTexture.create_from_image(image)
	var painting_name: String = data.get("name")
	var author_name: String = Database.get_author_by_id(
		data.get("author_id")
	).get("name")
	%PaitingLabel.text = author_name + ": " + painting_name
