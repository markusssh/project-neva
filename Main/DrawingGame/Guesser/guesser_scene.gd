extends Control

func _ready() -> void:
	%BackgroundColorRect.size = Vector2(Params.CANV_W, Params.CANV_H)
	%BackgroundColorRect.position -= %BackgroundColorRect.size / 2
	%TextureRect.size = %BackgroundColorRect.size
	%TextureRect.position = %BackgroundColorRect.position
	MultiplayerController.ImageBytesReceived.connect(update_texture)

func update_texture(i: PackedByteArray) -> void:
	var image = Image.create_from_data(Params.CANV_W, Params.CANV_H, false, Image.FORMAT_RGBA8, i)
	%TextureRect.texture = ImageTexture.create_from_image(image)
	
