extends Control

func _ready() -> void:
	WebClient.action_controller.image_recieved.connect(update_texture)
	%BackgroundColorRect.size = Vector2(Params.CANV_W, Params.CANV_H)
	%BackgroundColorRect.position -= %BackgroundColorRect.size / 2
	%TextureRect.size = %BackgroundColorRect.size
	%TextureRect.position = %BackgroundColorRect.position

func update_texture(i: Image) -> void:
	%TextureRect.texture = ImageTexture.create_from_image(i)
