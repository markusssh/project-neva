class_name StarsContainer extends HBoxContainer

signal rated(score: int)

@export var stars_button_group: ButtonGroup
@export var full_star_texture: CompressedTexture2D
@export var empty_star_texture: CompressedTexture2D

var stars_array: Array

func _ready() -> void:
	stars_array.append($Star1)
	stars_array.append($Star2)
	stars_array.append($Star3)
	stars_array.append($Star4)
	stars_array.append($Star5)
	
	stars_button_group.pressed.connect(_on_stars_bg_pressed)

func _on_stars_bg_pressed(button: TextureButton):
	var score := button.name.right(1).to_int()
	rated.emit(score)
	
	for i in 5:
		if i < score:
			set_full_textures(stars_array[i])
		else:
			set_empty_textures(stars_array[i])

func clear_score() -> void:
	var pressed: TextureButton = stars_button_group.get_pressed_button()
	if pressed != null:
		pressed.button_pressed = false
	for i in 5:
		set_empty_textures(stars_array[i])

func set_full_textures(b: TextureButton) -> void:
	b.texture_normal = full_star_texture
	b.texture_hover = full_star_texture
	b.texture_pressed = full_star_texture

func set_empty_textures(b: TextureButton) -> void:
	b.texture_normal = empty_star_texture
	b.texture_hover = empty_star_texture
	b.texture_pressed = empty_star_texture
