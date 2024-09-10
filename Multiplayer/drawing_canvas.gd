extends Control

@export var drawing_line: Line2D
@export var paint_viewport: SubViewport
@export var painted_image: TextureRect

var e: float = 1
var mouse_pos: Vector2

func _process(_delta: float) -> void:
	if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
		mouse_pos = paint_viewport.get_mouse_position()
		handle_line_drawing()

func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.is_released():
		if event.button_index == MOUSE_BUTTON_LEFT:
			if drawing_line.points.size() > 0:
				bake_drawing()

func handle_line_drawing() -> void:
	if drawing_line.points.size() == 0:
		if mouse_pos.x < size.x and mouse_pos.y < size.y:
			drawing_line.add_point(mouse_pos)
			#Adding second point do make visible dot
			drawing_line.add_point(mouse_pos + Vector2.DOWN)
	elif absf(mouse_pos.x - drawing_line.points[-1].x) >= e \
		or absf(mouse_pos.y - drawing_line.points[-1].y) >= e:
		drawing_line.add_point(mouse_pos)

func bake_drawing() -> void:
	#Viewport texture can't be written and read at the same time by engine
	#So we should convert to image and back to texture
	#Doesn't even work with 'call_deferred()' method
	painted_image.texture = ImageTexture.create_from_image(paint_viewport.get_texture().get_image())
	drawing_line.clear_points()

func _on_color_picker_color_changed(color: Color) -> void:
	drawing_line.default_color = %ColorPicker.color

func _on_h_slider_value_changed(value: float) -> void:
	drawing_line.width = %Thickness.value
