extends Button

const TOP_PANEL_ITEM_SEPARATION = 15
const MAX_WIDTH = 400
const MAX_HEIGHT = 400

@export var image: CompressedTexture2D:
	set(new_image):
		image = new_image
		_on_image_changed()

var is_dragging: bool = false
var dragging_offset: Vector2
var contents_size: Vector2

func _ready() -> void:
	%PanelsVBoxContainer.add_theme_constant_override("separation", TOP_PANEL_ITEM_SEPARATION)
	get_viewport().size_changed.connect(_on_window_size_changed)

func _on_image_changed() -> void:
	await ready
	%ImageItem.texture = image
	position.y = get_viewport_rect().end.y - size.y
	contents_size = %PanelsVBoxContainer.size

func _on_window_size_changed() -> void:
	position.y = get_viewport_rect().end.y - size.y
	position.x = get_viewport_rect().position.x

func _process(delta: float) -> void:
	if is_dragging:
		var viewport_rect: Rect2 = get_viewport_rect()
		
		var new_position = get_global_mouse_position()  + dragging_offset
		
		new_position.x = clampf(
			new_position.x, 
			viewport_rect.position.x, 
			viewport_rect.end.x - contents_size.x
		)
		
		new_position.y = clampf(
			new_position.y, 
			viewport_rect.position.y + contents_size.y, 
			viewport_rect.end.y
		)
		
		position = new_position

func _on_button_down() -> void:
	is_dragging = true
	dragging_offset = position - get_global_mouse_position() 

func _on_button_up() -> void:
	is_dragging = false

func _on_minimize_button_pressed() -> void:
	%ImageItem.hide()

func _on_maximize_button_pressed() -> void:
	%ImageItem.show()
