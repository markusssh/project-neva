extends PanelContainer

const TOP_PANEL_ITEM_SEPARATION = 15
const MAX_WIDTH = 400
const MAX_HEIGHT = 400

@export var image: ImageTexture:
	set(new_image):
		image = new_image
		_on_image_changed()

func _ready() -> void:
	%PanelsVBoxContainer.add_theme_constant_override("separation", TOP_PANEL_ITEM_SEPARATION)

func _on_image_changed() -> void:
	%ImageItem.texture = image

func _on_minimize_button_pressed() -> void:
	%ImageItem.hide()

func _on_maximize_button_pressed() -> void:
	%ImageItem.show()
