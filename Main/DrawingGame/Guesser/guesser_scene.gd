extends Control

@export var canvas_size: Vector2i = Vector2(Params.CANV_W, Params.CANV_H)

func _ready() -> void:
	%BackgroundColorRect.custom_minimum_size = canvas_size
	%TextureRect.custom_minimum_size = %BackgroundColorRect.size
	
	MultiplayerController.ImageBytesReceived.connect(update_texture)

func update_texture(i: PackedByteArray) -> void:
	var image = Image.create_from_data(Params.CANV_W, Params.CANV_H, false, Image.FORMAT_RGBA8, i)
	%TextureRect.texture = ImageTexture.create_from_image(image)
	
