extends Control

enum PaintingMode {
	BRUSH,
	ERASER,
	BUCKET,
	LINE,
	EMPTY_RECTANGLE,
	FULL_RECTANGLE,
	EMPTY_CIRCLE,
	FULL_CIRCLE
}

@export var drawing_line: Line2D
@export var paint_viewport: SubViewport
@export var painted_image: TextureRect
@export var background: Panel
@export var erase_mask_line: Line2D
@export var erase_mask_viewport: SubViewport
@export var drawing_background: StyleBoxFlat

var e: float = 1
var mouse_pos: Vector2
var mode: PaintingMode = PaintingMode.BRUSH 
var canv_size: Vector2i

@onready var draw_color: Color = drawing_line.default_color

func allign_canvas() -> void:
	for child in get_children():
		child.size = canv_size
		if child is SubViewportContainer:
			child.get_child(0).size = canv_size
	background.pivot_offset = canv_size / 2

func _process(_delta: float) -> void:
	match mode:
		PaintingMode.BRUSH:
			if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
				mouse_pos = paint_viewport.get_mouse_position()
				handle_brush_drawing()
		PaintingMode.ERASER:
			if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
				mouse_pos = paint_viewport.get_mouse_position()
				handle_erase()

func _input(event: InputEvent) -> void:
	match mode:
		PaintingMode.BRUSH:
			if event is InputEventMouseButton and event.is_released():
				if event.button_index == MOUSE_BUTTON_LEFT:
					if drawing_line.points.size() > 0:
						bake_drawing()
		PaintingMode.ERASER:
			if event is InputEventMouseButton and event.is_released():
				if event.button_index == MOUSE_BUTTON_LEFT:
					if drawing_line.points.size() > 0:
						bake_drawing()
		PaintingMode.BUCKET:
			if event is InputEventMouseButton and event.is_released():
				if event.button_index == MOUSE_BUTTON_LEFT:
					mouse_pos = paint_viewport.get_mouse_position()
					if mouse_pos.x < paint_viewport.size.x and mouse_pos.y < paint_viewport.size.y \
					and mouse_pos.x >= 0 and mouse_pos.y >= 0:
						bake_drawing()

func handle_brush_drawing() -> void:
	if drawing_line.points.size() == 0:
		if mouse_pos.x < paint_viewport.size.x and mouse_pos.y < paint_viewport.size.y \
		and mouse_pos.x >= 0 and mouse_pos.y >= 0:
			drawing_line.add_point(mouse_pos)
			#Adding second point do make visible dot
			drawing_line.add_point(mouse_pos + Vector2.DOWN)
	elif absf(mouse_pos.x - drawing_line.points[-1].x) >= e \
		or absf(mouse_pos.y - drawing_line.points[-1].y) >= e:
		drawing_line.add_point(mouse_pos)

func handle_erase() -> void:
	handle_brush_drawing()
	erase_mask_line.width = drawing_line.width
	erase_mask_line.points = drawing_line.points

#Viewport texture can't be written and read at the same time by engine
#So we should convert to image and back to texture
#Doesn't even work with 'call_deferred()' method
func bake_drawing() -> void:
	match mode:
		PaintingMode.ERASER:
			await RenderingServer.frame_post_draw
			var erase_mask: Image = erase_mask_viewport.get_texture().get_image()
			var picture: Image = painted_image.texture.get_image()
			for row in canv_size.y:
				for column in canv_size.x:
					if erase_mask.get_pixel(column, row) == Color.BLACK:
						picture.set_pixel(column, row, Color(0, 0, 0, 0))
			painted_image.texture = ImageTexture.create_from_image(picture)
			drawing_line.clear_points()
		PaintingMode.BUCKET:
			var picture: Image
			if painted_image.texture == null:
				picture = paint_viewport.get_texture().get_image()
			else:
				picture = painted_image.texture.get_image()
			var replace_color = picture.get_pixelv(mouse_pos)
			painted_image.texture = ImageTexture.create_from_image(
				AlgoUtil.FloodFill(picture, mouse_pos, replace_color, draw_color))
		_:
			painted_image.texture = ImageTexture.create_from_image(paint_viewport.get_texture().get_image())
			drawing_line.clear_points()

func _on_color_picker_color_changed(color: Color) -> void:
	draw_color = color
	if drawing_line.points.is_empty() and mode != PaintingMode.ERASER:
		drawing_line.default_color = %ColorPicker.color

func _on_h_slider_value_changed(value: float) -> void:
	drawing_line.width = %Thickness.value

func _on_brush_buttton_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.BRUSH

func _on_eraser_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.ERASER
		drawing_line.default_color = drawing_background.bg_color
	else:
		drawing_line.default_color = %ColorPicker.color

func _on_bucket_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.BUCKET

func _on_line_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.LINE

func _on_empty_rectangle_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.EMPTY_RECTANGLE

func _on_full_rectangle_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.FULL_RECTANGLE

func _on_empty_circle_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.EMPTY_CIRCLE

func _on_full_circle_button_toggled(toggled_on: bool) -> void:
	if toggled_on:
		mode = PaintingMode.FULL_CIRCLE
