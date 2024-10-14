extends Control

func _ready() -> void:
	WebClient.action_controller.image_recieved.connect(update_texture)

func update_texture(i: Image) -> void:
	%TextureRect.texture = ImageTexture.create_from_image(i)
